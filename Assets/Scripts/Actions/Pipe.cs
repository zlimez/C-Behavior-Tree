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
using AI.BehaviorTree;
using Action = AI.BehaviorTree.Action;
using Game.Utils;

namespace Game.Controller
{
    public class Pipe<T1, T2> : Action where T1 : struct where T2 : struct
    {
        private readonly ReadonlyQueue<T1> _inputQueue;
        private readonly IChannel<T2> _outputChannel;
        private readonly Func<T1, float, (T2, bool)> _converter;

        public Pipe(ReadonlyQueue<T1> inputQueue, IChannel<T2> outputChannel, Func<T1, float, (T2, bool)> converter)
        {
            _inputQueue = inputQueue;
            _outputChannel = outputChannel;
            _converter = converter;
        }

        protected override void Update(float deltaTime)
        {
            if (_inputQueue.Count > 0)
            {
                var data = default(T1);
                while (_inputQueue.Count > 0) _inputQueue.TryDequeue(out data);
                var (convertedData, success) = _converter(data, deltaTime);
                if (success) _outputChannel.Write(convertedData);
            }

            State = State.Success;
        }
    }

    public class BranchPipe<T1, T2, T3> : Action where T1 : struct where T2 : struct where T3 : struct
    {
        private readonly ReadonlyQueue<T1> _inputQueue;
        private readonly IChannel<T2> _outputChannel1;
        private readonly IChannel<T3> _outputChannel2;
        private readonly Func<T1, float, (T2, bool)> _converter1;
        private readonly Func<T1, float, (T3, bool)> _converter2;

        public BranchPipe(
            ReadonlyQueue<T1> inputQueue,
            IChannel<T2> outputChannel1,
            IChannel<T3> outputChannel2,
            Func<T1, float, (T2, bool)> converter1,
            Func<T1, float, (T3, bool)> converter2)
        {
            _inputQueue = inputQueue;
            _outputChannel1 = outputChannel1;
            _outputChannel2 = outputChannel2;
            _converter1 = converter1;
            _converter2 = converter2;
        }

        protected override void Update(float deltaTime)
        {
            if (_inputQueue.Count > 0)
            {
                var data = default(T1);
                while (_inputQueue.Count > 0) _inputQueue.TryDequeue(out data);
                var (convertedData1, success1) = _converter1(data, deltaTime);
                var (convertedData2, success2) = _converter2(data, deltaTime);
                if (success1)
                    _outputChannel1.Write(convertedData1);
                if (success2)
                    _outputChannel2.Write(convertedData2);
            }

            State = State.Success;
        }
    }
}
