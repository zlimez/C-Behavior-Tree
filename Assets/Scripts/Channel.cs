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

namespace Game.Utils
{
    public class ReadonlyQueue<T> where T : struct
    {
        private readonly Queue<T> _queue;
        public ReadonlyQueue(Queue<T> queue) => _queue = queue;
        public int Count => _queue.Count;

        public void Clear() => _queue.Clear();
        public bool TryPeek(out T target) => _queue.TryPeek(out target);

        public bool TryDequeue(out T target)
        {
            if (_queue.Count == 0)
            {
                target = default;
                return false;
            }
            target = _queue.Dequeue();
            return true;
        }
    }

    public interface IChannel<T> where T : struct
    {
        public void Clear();
        public void Write(T data);
        public ReadonlyQueue<T> Subscribe();
    }

    public class Channel<T> : IChannel<T> where T : struct
    {
        private readonly List<Queue<T>> _readers = new();

        public void Clear()
        {
            foreach (var reader in _readers)
                reader.Clear();
        }

        public void Write(T data)
        {
            foreach (var reader in _readers)
                reader.Enqueue(data);
        }

        public ReadonlyQueue<T> Subscribe()
        {
            var queue = new Queue<T>();
            _readers.Add(queue);
            return new ReadonlyQueue<T>(queue);
        }
    }

    public class Channels
    {
        private readonly Dictionary<string, object> _channels = new();

        private Channel<T> Create<T>(string channelName) where T : struct
        {
            var newChannel = new Channel<T>();
            _channels.Add(channelName, newChannel);
            return newChannel;
        }

        public Channel<T> Get<T>(string channelName) where T : struct
        {
            if (_channels.TryGetValue(channelName, out var channel))
            {
                if (channel is not Channel<T> typedChannel) throw new Exception("Channel type mismatch");
                return typedChannel;
            }

            return Create<T>(channelName);
        }

        public ReadonlyQueue<T> Subscribe<T>(string channelName) where T : struct
        {
            Channel<T> channel;
            if (!_channels.ContainsKey(channelName)) channel = Create<T>(channelName);
            else if (_channels[channelName] is Channel<T> typedChannel) channel = typedChannel;
            else throw new Exception("Channel type mismatch");
            return channel.Subscribe();
        }
    }
}
