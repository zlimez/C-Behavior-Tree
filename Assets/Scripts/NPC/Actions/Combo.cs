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

using System;
using UnityEngine;
using UnityEngine.Assertions;
using AI.BehaviorTree;
using Action = AI.BehaviorTree.Action;
using Game.Utils;

namespace Game.Controller
{
    public class Combo : Action
    {
        [Serializable]
        public struct ComboStep
        {
            [Min(0)] public int StrikeId;
            public float StrikeDuration;
        }

        private readonly ComboStep[] _comboSteps;
        private readonly string _targetVarName;
        private readonly Transform _self;
        private readonly Func<Transform, bool> _canContinueCombo;

        private readonly IChannel<StrikeCommand> _strikeCmdChannel;
        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<RotateCommand> _rotateCmdChannel;
        private readonly IChannel<SignalCommands.StrikeDone> _strikeDoneCmdChannel;

        private readonly float[] _cumStrikeDurations;
        private int _currCombStep;
        private float _timer;
        private Transform _target;

        public Combo(
            ComboStep[] comboSteps,
            Transform self,
            string targetVarName,
            Func<Transform, bool> canContinueCombo,
            IChannel<StrikeCommand> strikeCmdChannel,
            IChannel<MoveCommand> moveCmdChannel,
            IChannel<RotateCommand> rotateCmdChannel,
            IChannel<SignalCommands.StrikeDone> strikeDoneCmdChannel)
        {
            _self = self;
            _targetVarName = targetVarName;
            _comboSteps = comboSteps;
            _canContinueCombo = canContinueCombo;

            _strikeCmdChannel = strikeCmdChannel;
            _moveCmdChannel = moveCmdChannel;
            _rotateCmdChannel = rotateCmdChannel;
            _strikeDoneCmdChannel = strikeDoneCmdChannel;

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
                if (!_canContinueCombo(_target))
                {
                    if (_currCombStep >= 0) _strikeDoneCmdChannel.Write(new SignalCommands.StrikeDone(_comboSteps[_currCombStep].StrikeId));
                    State = State.Failure;
                    return;
                }

                _currCombStep++;
#if UNITY_EDITOR
                Debug.Log("Enemy comb step " + _currCombStep);
#endif
                _strikeCmdChannel.Write(new StrikeCommand(_comboSteps[_currCombStep].StrikeId, _comboSteps[_currCombStep].StrikeDuration));
                _rotateCmdChannel.Write(new RotateCommand(Quaternion.LookRotation(_target.position - _self.position)));

                if (_currCombStep == 0)
                    _moveCmdChannel.Write(MoveCommand.Stop);
            } else if (_currCombStep == _comboSteps.Length - 1 && _timer > _cumStrikeDurations[_currCombStep])
            {
                _strikeDoneCmdChannel.Write(new SignalCommands.StrikeDone(_comboSteps[_currCombStep].StrikeId));
                State = State.Success;
                return;
            }

            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _currCombStep = -1;
            _timer = 0f;
            _target = Tree.GetDatum<Transform>(_targetVarName);
        }
    }
}
