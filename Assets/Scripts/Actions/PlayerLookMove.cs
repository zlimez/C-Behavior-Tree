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
using AI.BehaviorTree;
using Game.Utils;

namespace Game.Controller
{
    // Once entered only exit when lookDir goes back to 0,0, should evolve into shooting as per og game
    public class PlayerLookMove : Action
    {
        private readonly ReadonlyQueue<Vector2> _moveInputs;
        private readonly ReadonlyQueue<Vector2> _lookInputs;
        private readonly ReadonlyQueue<bool> _strikeInputs, _dodgeInputs;
        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<RotateCommand> _rotateCmdChannel;
        private readonly IChannel<SignalCommands.Look> _startStopLookCmdChannel;
        private readonly float _moveSpeed;

        private bool _isLooking;

        public PlayerLookMove(
            float moveSpeed,
            ReadonlyQueue<Vector2> moveInputs,
            ReadonlyQueue<Vector2> lookInputs,
            ReadonlyQueue<bool> strikeInputs,
            ReadonlyQueue<bool> dodgeInputs,
            IChannel<MoveCommand> moveCmdChannel,
            IChannel<RotateCommand> rotateCmdChannel,
            IChannel<SignalCommands.Look> startStopLookCmdChannel)
        {
            _moveSpeed = moveSpeed;
            _moveInputs = moveInputs;
            _lookInputs = lookInputs;
            _strikeInputs = strikeInputs;
            _dodgeInputs = dodgeInputs;
            _moveCmdChannel = moveCmdChannel;
            _rotateCmdChannel = rotateCmdChannel;
            _startStopLookCmdChannel = startStopLookCmdChannel;
        }

        protected override void Update(float deltaTime)
        {
            if (_lookInputs.Count > 0)
                Look();

            if (_moveInputs.Count > 0)
                Move();
        }

        private void Look()
        {
            var lastLook = Vector2.zero;
            while (_lookInputs.Count > 0)
                _lookInputs.TryDequeue(out lastLook);
            if (Mathf.Approximately(lastLook.sqrMagnitude, 0))
            {
                State = State.Failure;
                if (_isLooking)
                {
                    _strikeInputs.Clear();
                    _dodgeInputs.Clear();
                    _isLooking = false;
                    _startStopLookCmdChannel.Write(new SignalCommands.Look(false));
                    _moveCmdChannel.Write(MoveCommand.Stop);
                }
                return;
            }

            if (!_isLooking)
            {
                _startStopLookCmdChannel.Write(new SignalCommands.Look(true));
                _isLooking = true;
            }

            _rotateCmdChannel.Write(new RotateCommand(Quaternion.LookRotation(new Vector3(lastLook.x, 0f, lastLook.y), Vector3.up)));
        }

        private void Move()
        {
            var latestMove = Vector2.zero;
            while (_moveInputs.Count > 0)
                _moveInputs.TryDequeue(out latestMove);
            _moveCmdChannel.Write(new MoveCommand(_moveSpeed * new Vector3(latestMove.x, 0f, latestMove.y), latestMove.magnitude));
        }
    }
}
