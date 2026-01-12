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
using Game.Utils;

namespace Game.Controller
{
    public class Motor : MonoBehaviour
    {
        [SerializeField] private InputOutput _cmdProducer;
        [SerializeField] private CharacterController _characterController;
        [SerializeField][Min(0)] private float _rotationSpeed = 60f;

        private ReadonlyQueue<MoveCommand> _moveCmds;
        private ReadonlyQueue<RotateCommand> _rotateCmds;
        private Vector3 _currVelocity;
        private Quaternion _targetRotation = Quaternion.identity;

        private void Awake()
        {
            _moveCmds = _cmdProducer.GetReader<MoveCommand>(nameof(BrainCommandChannelList.Move));
            _rotateCmds = _cmdProducer.GetReader<RotateCommand>(nameof(BrainCommandChannelList.Rotate));
        }

        private void Update()
        {
            if (_moveCmds.TryDequeue(out var moveCmd))
                _currVelocity = moveCmd.Velocity;

            _characterController.Move(_currVelocity * Time.deltaTime);

            if (_rotateCmds.TryDequeue(out var rotCmd)) _targetRotation = rotCmd.TargetRotation;

            if (!Mathf.Approximately(Quaternion.Dot(_targetRotation, _characterController.transform.rotation) - 1f, 0f))
                _characterController.transform.rotation = Quaternion.Slerp(
                        _characterController.transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
