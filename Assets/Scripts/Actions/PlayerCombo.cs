/* (C)2025 Rayark Inc. - All Rights Reserved
 * Rayark Confidential
 *
 * NOTICE: The intellectual and technical concepts contained herein are
 * proprietary to or under control of Rayark Inc. and its affiliates.
 * The information herein may be covered by patents, patents in process,
 * and are protected by trade secret or copyright law.
 * You may not disseminate this information or reproduce this material
 * unless otherwise prior agreed by Rayark Inc. in writing.
 */

using UnityEngine;
using UnityEngine.Assertions;
using AI.BehaviorTree;
using Action = AI.BehaviorTree.Action;
using Game.Utils;

namespace Game.Controller
{
    public class PlayerCombo : Action
    {
        private readonly Combo.ComboStep[] _comboSteps;
        private readonly ReadonlyQueue<bool> _strikeInputs, _dodgeInputs;
        private readonly ReadonlyQueue<Vector2> _moveInputs, _lookInputs;

        private readonly IChannel<StrikeCommand> _strikeCmdChannel;
        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<RotateCommand> _rotateCmdChannel;
        private readonly IChannel<SignalCommands.StrikeDone> _strikeDoneCmdChannel;
        private readonly IChannel<SignalCommands.MoveMemory> _moveMemoryChannel;

        private readonly float[] _cumStrikeDurations;
        private readonly float _moveSpeed;

        private int _currCombStep;
        private float _timer;
        private bool _hasMoveChange;
        private Vector2 _latestMove;

        public PlayerCombo(
            Combo.ComboStep[] comboSteps,
            float moveSpeed,
            ReadonlyQueue<bool> strikeInputs,
            ReadonlyQueue<bool> dodgeInputs,
            ReadonlyQueue<Vector2> moveInputs,
            ReadonlyQueue<Vector2> lookInputs,
            IChannel<StrikeCommand> strikeCmdChannel,
            IChannel<MoveCommand> moveCmdChannel,
            IChannel<RotateCommand> rotateCmdChannel,
            IChannel<SignalCommands.StrikeDone> strikeDoneCmdChannel,
            IChannel<SignalCommands.MoveMemory> moveMemoryChannel)
        {
            _moveSpeed = moveSpeed;
            _comboSteps = comboSteps;
            _strikeInputs = strikeInputs;
            _moveInputs = moveInputs;
            _lookInputs = lookInputs;
            _dodgeInputs = dodgeInputs;

            _strikeCmdChannel = strikeCmdChannel;
            _moveCmdChannel = moveCmdChannel;
            _rotateCmdChannel = rotateCmdChannel;
            _strikeDoneCmdChannel = strikeDoneCmdChannel;
            _moveMemoryChannel = moveMemoryChannel;

            _cumStrikeDurations = new float[_comboSteps.Length];
            _cumStrikeDurations[0] = _comboSteps[0].StrikeDuration;
            for (var i = 1; i < _comboSteps.Length; i++)
            {
#if UNITY_EDITOR
                Assert.IsFalse(Mathf.Approximately(_comboSteps[i].StrikeDuration, 0f), $"Combo step {i} has zero strike duration, which may cause undefined behavior.");
#endif
                _cumStrikeDurations[i] = _cumStrikeDurations[i - 1] + _comboSteps[i].StrikeDuration;
            }
        }

        protected override void Update(float deltaTime)
        {
            if (_currCombStep == -1 || _currCombStep < _comboSteps.Length - 1 && _timer > _cumStrikeDurations[_currCombStep])
            {
                if (_strikeInputs.TryDequeue(out var strike) && strike) // new strike input detected, proceed to next strike step
                {

                    _currCombStep++;
#if UNITY_EDITOR
                    Debug.Log("Player comb step " + _currCombStep);
#endif
                    _strikeCmdChannel.Write(new StrikeCommand(_comboSteps[_currCombStep].StrikeId, _comboSteps[_currCombStep].StrikeDuration));
                    _strikeInputs.Clear();

                    // Between strikes, allow movement input to adjust facing
                    if (_moveInputs.Count > 0)
                    {
                        _hasMoveChange = true;
                        _latestMove = Vector2.zero;
                        while (_moveInputs.Count > 0)
                            _moveInputs.TryDequeue(out _latestMove);
                        if (!Mathf.Approximately(_latestMove.sqrMagnitude, 0))
                            _rotateCmdChannel.Write(new RotateCommand(Quaternion.LookRotation(new Vector3(_latestMove.x, 0f, _latestMove.y), Vector3.up)));
                    }

                    if (_currCombStep == 0)
                    {
                        _moveMemoryChannel.Write(SignalCommands.MoveMemory.Memorize);
                        _moveCmdChannel.Write(MoveCommand.Stop);
                    }
                } else // strike step completed but no new strike input, end combo
                {
                    _strikeDoneCmdChannel.Write(new SignalCommands.StrikeDone(_comboSteps[_currCombStep].StrikeId));
                    State = State.Success;
                }
            } else if (_currCombStep == _comboSteps.Length - 1 && _timer > _cumStrikeDurations[^1]) // combo completed
            {
                _strikeDoneCmdChannel.Write(new SignalCommands.StrikeDone(_comboSteps[_currCombStep].StrikeId));
                _strikeInputs.Clear();
                State = State.Success;
            }

            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _currCombStep = -1;
            _timer = 0f;
            _hasMoveChange = false;
        }

        public override void Done()
        {
            base.Done();
            if (_hasMoveChange)
                _moveCmdChannel.Write(new MoveCommand(_moveSpeed * new Vector3(_latestMove.x, 0f, _latestMove.y), _latestMove.magnitude));
            else
                _moveMemoryChannel.Write(SignalCommands.MoveMemory.Resume);
            _lookInputs.Clear(); // rid of stale data
            _dodgeInputs.Clear();
        }
    }
}
