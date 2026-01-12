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
using System.Collections.Generic;
using UnityEngine;
using AI.BehaviorTree;
using Action = AI.BehaviorTree.Action;
using Game.Utils;

namespace Game.Controller
{
    public class GoToBest : Action
    {
        private readonly Transform _self;
        private readonly List<Transform> _targetCandidates;
        private readonly Func<Transform, Transform, float> _prioritizer;
        private readonly float _speed;
        private readonly IChannel<MoveCommand> _moveCmdChannel;
        private readonly IChannel<RotateCommand> _rotateCmdChannel;

        public GoToBest(
            Transform self,
            List<Transform> targetCandidates,
            Func<Transform, Transform, float> prioritizer,
            float speed,
            IChannel<MoveCommand> moveCmdChannel,
            IChannel<RotateCommand> rotateCmdChannel)
        {
            _self = self;
            _targetCandidates = targetCandidates;
            _prioritizer = prioritizer;
            _speed = speed;
            _moveCmdChannel = moveCmdChannel;
            _rotateCmdChannel = rotateCmdChannel;
        }

        protected override void Update(float deltaTime)
        {
            Transform target = null;
            var bestPriority = float.PositiveInfinity;
            foreach (var candidate in _targetCandidates)
            {
                var score = _prioritizer(_self, candidate);
                if (score < bestPriority)
                {
                    bestPriority = score;
                    target = candidate;
                }
            }

            if (!target)
            {
                State = State.Failure;
                return;
            }

            var nextPos = Vector3.MoveTowards(_self.position, target!.position, _speed * deltaTime);
            var diff = nextPos - _self.position;
            _moveCmdChannel.Write(new MoveCommand(diff / deltaTime, diff.magnitude / (_speed * deltaTime)));
            _rotateCmdChannel.Write(new RotateCommand(Quaternion.LookRotation(diff)));
            State = State.Success;
        }
    }
}
