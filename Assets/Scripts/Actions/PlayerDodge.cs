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
using Game.Utils;

// NOTE: For actions that can persist for a period, make sure to clear the other input queues to prevent stale data from being processed
namespace Game.Controller
{
    public class PlayerDodge : Action
    {
        private readonly ReadonlyQueue<bool> _dodgeInputs, _strikeInputs;
        private readonly ReadonlyQueue<Vector2> _lookInputs, _moveInputs;

        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<DodgeCommand> _dodgeCmdChannel;
        private readonly IChannel<SignalCommands.DodgeDone> _signalDodgeDoneChannel;
        private readonly IChannel<SignalCommands.MoveMemory> _moveMemoryChannel;

        private readonly CollisionNotifier _collisionNotifier;
        private readonly float _duration;

        private bool _hasCollided;
        private float _timer;

        public PlayerDodge(
            ReadonlyQueue<bool> dodgeInputs,
            ReadonlyQueue<bool> strikeInputs,
            ReadonlyQueue<Vector2> moveInputs,
            ReadonlyQueue<Vector2> lookInputs,
            IChannel<MoveCommand> moveCmdChannel,
            IChannel<DodgeCommand> dodgeCmdChannel,
            IChannel<SignalCommands.DodgeDone> signalDodgeDoneChannel,
            IChannel<SignalCommands.MoveMemory> moveMemoryChannel,
            CollisionNotifier collisionNotifier,
            float duration)
        {
            _dodgeInputs = dodgeInputs;
            _strikeInputs = strikeInputs;
            _lookInputs = lookInputs;
            _moveInputs = moveInputs;

            _moveCmdChannel = moveCmdChannel;
            _dodgeCmdChannel = dodgeCmdChannel;
            _signalDodgeDoneChannel = signalDodgeDoneChannel;
            _moveMemoryChannel = moveMemoryChannel;
#if UNITY_EDITOR
            Assert.IsFalse(Mathf.Approximately(duration, 0f), $"Dodge has zero duration, which may cause undefined behavior.");
#endif
            _duration = duration;
            _collisionNotifier = collisionNotifier;
        }

        protected override void Update(float deltaTime)
        {
            if (_timer >= _duration || _hasCollided)
            {
                State = State.Success;
                return;
            }

            _timer += deltaTime;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _timer = 0f;
            _hasCollided = false;
            _dodgeCmdChannel.Write(new (_duration));
            _moveMemoryChannel.Write(SignalCommands.MoveMemory.Memorize);
            _moveCmdChannel.Write(MoveCommand.Stop);
            _collisionNotifier.SubscribeControllerColliderHit(OnCollisionEnter);
        }

        public override void Done()
        {
            base.Done();
            _signalDodgeDoneChannel.Write(default);
            _strikeInputs.Clear();
            if (_moveInputs.Count == 0)
                _moveMemoryChannel.Write(SignalCommands.MoveMemory.Resume);
            _lookInputs.Clear();
            _dodgeInputs.Clear();
            _collisionNotifier.UnsubscribeControllerColliderHit(OnCollisionEnter);
        }

        private void OnCollisionEnter(ControllerColliderHit _) => _hasCollided = true;
    }
}
