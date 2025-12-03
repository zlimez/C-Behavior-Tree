/* (C)2022 Rayark Inc. - All Rights Reserved
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
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    [Serializable]
    public class RefPair<T1,T2>
    {
        public T1 Head;
        public T2 Tail;
        public RefPair(T1 head, T2 tail)
        {
            Head = head;
            Tail = tail;
        }
    }

    [Serializable]
    public struct Pair<T1, T2>
    {
        public T1 Head;
        public T2 Tail;
    }

    [Serializable]
    public struct Triplet<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
    }

    public class Deque<T> : IEnumerable<T>
    {
        private T[] _buffer;
        private int _head;
        private int _tail;
        private const int MinimumCapacity = 8;

        public int Count { get; private set; }
        public int Capacity => _buffer.Length;

        public Deque(int capacity)
        {
            var initialCapacity = Math.Max(capacity, MinimumCapacity);
            _buffer = new T[initialCapacity];
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        public Deque() : this(MinimumCapacity) { }

        private int GetIndex(int offset) => (_head + offset) % _buffer.Length;

        private void Resize(int newCapacity)
        {
            var newBuffer = new T[newCapacity];

            for (var i = 0; i < Count; i++)
            {
                var oldIndex = GetIndex(i);
                newBuffer[i] = _buffer[oldIndex];
            }

            _buffer = newBuffer;
            _head = 0;
            _tail = Count;
        }

        public void AddLast(T item)
        {
            if (Count == _buffer.Length)
                Resize(_buffer.Length * 2);

            _buffer[_tail] = item;
            _tail = (_tail + 1) % _buffer.Length;
            Count++;
        }

        public void AddFirst(T item)
        {
            if (Count == _buffer.Length)
                Resize(_buffer.Length * 2);

            _head = (_head - 1 + _buffer.Length) % _buffer.Length;
            _buffer[_head] = item;
            Count++;
        }

        public T RemoveFirst()
        {
            if (Count == 0)
                throw new InvalidOperationException("Deque is empty.");

            var item = _buffer[_head];
            _buffer[_head] = default;

            _head = (_head + 1) % _buffer.Length;
            Count--;

            // Shrink logic: If the buffer is large and utilization is below 25% (Count < Capacity / 4)
            if (_buffer.Length > MinimumCapacity && Count < _buffer.Length / 4)
                Resize(_buffer.Length / 2);

            return item;
        }

        public T RemoveLast()
        {
            if (Count == 0)
                throw new InvalidOperationException("Deque is empty.");

            _tail = (_tail - 1 + _buffer.Length) % _buffer.Length;

            T item = _buffer[_tail];

            _buffer[_tail] = default;

            Count--;

            if (_buffer.Length > MinimumCapacity && Count < _buffer.Length / 4)
                Resize(_buffer.Length / 2);

            return item;
        }

        public T PeekFirst()
        {
            if (Count == 0)
                throw new InvalidOperationException("Deque is empty.");

            return _buffer[_head];
        }

        public T PeekLast()
        {
            if (Count == 0)
                throw new InvalidOperationException("Deque is empty.");

            var lastIndex = (_tail - 1 + _buffer.Length) % _buffer.Length;
            return _buffer[lastIndex];
        }


        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return _buffer[GetIndex(i)];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
