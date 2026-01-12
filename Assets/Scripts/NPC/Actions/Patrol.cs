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
    public class Patrol : Action
    {
        private readonly float _speed, _durationAtWaypoint;
        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<RotateCommand> _rotateCmdChannel;
        private readonly Transform _self;
        private readonly Transform[] _waypoints;
        
        private int _currWaypoint;
        private float _waitTimer;
        private bool _waiting;

        public Patrol(IChannel<MoveCommand> moveCmdChannel, IChannel<RotateCommand> rotateCmdChannel, Transform self, Transform[] waypoints, float speed, float durationAtWaypoint)
        {
            _self = self;
            _moveCmdChannel = moveCmdChannel;
            _rotateCmdChannel = rotateCmdChannel;
            _waypoints = waypoints;
            _speed = speed;
            _durationAtWaypoint = durationAtWaypoint;
        }
        
        protected override void Update(float deltaTime)
        {
            if (_waiting)
            {
                _waitTimer += deltaTime;
                if (_waitTimer >= _durationAtWaypoint)
                    _waiting = false;
            }
            else
            {
                var wp = _waypoints[_currWaypoint];
                if ((_self.position - wp.position).sqrMagnitude < 0.001f)
                {
                    _moveCmdChannel.Write(MoveCommand.Stop);
                    _waitTimer = 0f;
                    _waiting = true;
                    _currWaypoint = (_currWaypoint + 1) % _waypoints.Length;
                }
                else
                {
                    var nextPos = Vector3.MoveTowards(_self.position, wp.position, _speed * deltaTime);
                    var diff = nextPos - _self.position;
                    _moveCmdChannel.Write(new MoveCommand(diff / deltaTime, diff.magnitude / (_speed * deltaTime)));
                    _rotateCmdChannel.Write(new RotateCommand(Quaternion.LookRotation(diff)));
                }
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            _waiting = false;
            _waitTimer = 0f;
        }
    }
}
