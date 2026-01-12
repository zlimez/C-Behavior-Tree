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

using AI.BehaviorTree;
using Game.Utils;

namespace Game.Controller
{
    public class CheckChannelEmpty<T> : Action where T : struct
    {
        private readonly ReadonlyQueue<T> _readQueue;

        public CheckChannelEmpty(ReadonlyQueue<T> readQueue) => _readQueue = readQueue;

        protected override void Update(float deltaTime)
            => State = _readQueue.Count > 0 ? State.Failure : State.Success;
    }
}
