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

namespace Game.Controller
{
    // REVIEW: Many inputs will be passed to action nodes which can continuously run for extended frames just so that they are cleared when the action completes (to prevent old input from being used)
    public class PlayerBrain : InputOutput
    {
        [SerializeField] private InputOutput _inputProducer;
        [SerializeField] private Combo.ComboStep[] _claymoreCombo;
        [SerializeField][Min(0)] private float _moveSpeed = 8f, _dodgeDuration = 0.75f;
        [SerializeField] private CollisionNotifier _bodyCollider;

        private BehaviorTree _bt;

        private void Awake()
        {
            var moveDirReader = _inputProducer.GetReader<Vector2>(nameof(InputChannelList.MoveDirection));
            var strikeSignalReader = _inputProducer.GetReader<bool>(nameof(InputChannelList.StrikeSignal));
            var lookDirReader = _inputProducer.GetReader<Vector2>(nameof(InputChannelList.LookDirection));
            var dodgeSignalReader = _inputProducer.GetReader<bool>(nameof(InputChannelList.DodgeSignal));

            var moveCmdChannel = GetChannel<MoveCommand>(nameof(BrainCommandChannelList.Move));
            var strikeCmdChannel = GetChannel<StrikeCommand>(nameof(BrainCommandChannelList.Strike));
            var rotateCmdChannel = GetChannel<RotateCommand>(nameof(BrainCommandChannelList.Rotate));
            var dodgeCmdChannel = GetChannel<DodgeCommand>(nameof(BrainCommandChannelList.Dodge));

            var signalLookChannel = GetChannel<SignalCommands.Look>(nameof(BrainCommandChannelList.LookSignal));
            var signalStrikeDoneChannel = GetChannel<SignalCommands.StrikeDone>(nameof(BrainCommandChannelList.StrikeDoneSignal));
            var signalDodgeDoneChannel = GetChannel<SignalCommands.DodgeDone>(nameof(BrainCommandChannelList.DodgeDoneSignal));
            var signalMoveMemChannel = GetChannel<SignalCommands.MoveMemory>(nameof(BrainCommandChannelList.MoveMemorySignal));

            _bt = new BehaviorTree(new Selector(new Node[]
            {
                new Sequence(new Node[] // Claymore Combo
                {
                    new Inverter(new CheckChannelEmpty<bool>(strikeSignalReader)),
                    new PlayerCombo(_claymoreCombo, _moveSpeed, strikeSignalReader, dodgeSignalReader, moveDirReader, lookDirReader,
                              strikeCmdChannel, moveCmdChannel, rotateCmdChannel, signalStrikeDoneChannel, signalMoveMemChannel)
                }),
                new Sequence(new Node[] // Dodge
                {
                   new Inverter(new CheckChannelEmpty<bool>(dodgeSignalReader)),
                   new PlayerDodge(dodgeSignalReader, strikeSignalReader, moveDirReader, lookDirReader,
                             moveCmdChannel, dodgeCmdChannel, signalDodgeDoneChannel, signalMoveMemChannel, _bodyCollider, _dodgeDuration)
                }),
                new Sequence(new Node[] // Look and Move
                {
                    new Inverter(new CheckChannelEmpty<Vector2>(lookDirReader)),
                    new PlayerLookMove(_moveSpeed, moveDirReader, lookDirReader, strikeSignalReader, dodgeSignalReader,
                                 moveCmdChannel, rotateCmdChannel, signalLookChannel),
                }),
                new BranchPipe<Vector2, MoveCommand, RotateCommand>( // Normal Move
                    moveDirReader,
                    moveCmdChannel,
                    rotateCmdChannel,
                    (moveDir, _) => (new MoveCommand( _moveSpeed * new Vector3(moveDir.x, 0f, moveDir.y), moveDir.magnitude), true),
                    (moveDir, _) => Mathf.Approximately(moveDir.sqrMagnitude, 0)
                        ? (default, false)
                        : (new RotateCommand(Quaternion.LookRotation(new Vector3(moveDir.x, 0f, moveDir.y), Vector3.up)), true))
            }));
        }

        private void Update() => _bt.Tick(Time.deltaTime);
        private void OnDestroy() => _bt.Teardown();
    }
}
