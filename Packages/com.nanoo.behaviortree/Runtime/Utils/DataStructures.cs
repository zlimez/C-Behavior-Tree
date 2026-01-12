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

            var item = _buffer[_tail];

            _buffer[_tail] = default;

            Count--;

            if (_buffer.Length > MinimumCapacity && Count < _buffer.Length / 4)
                Resize(_buffer.Length / 2);

            return item;
        }

        public T PeekFirst() => Count == 0 ? throw new InvalidOperationException("Deque is empty.") : _buffer[_head];

        public T PeekLast()
        {
            if (Count == 0)
                throw new InvalidOperationException("Deque is empty.");

            var lastIndex = (_tail - 1 + _buffer.Length) % _buffer.Length;
            return _buffer[lastIndex];
        }


        public Enumerator GetEnumerator() =>  new(this);

        // Explicit interface implementations are required for compatibility,
        // but these will Box the struct (allocate) if cast to IEnumerable.
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Deque<T> _deque;
            private int _index;

            internal Enumerator(Deque<T> deque)
            {
                _deque = deque;
                _index = -1;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_index + 1 < _deque.Count)
                {
                    _index++;
                    Current = _deque._buffer[_deque.GetIndex(_index)];
                    return true;
                }

                _index = _deque.Count;
                Current = default;
                return false;
            }

            public T Current { get; private set; }
            object IEnumerator.Current => Current;
            public void Dispose() { }

            public void Reset()
            {
                _index = -1;
                Current = default;
            }
        }

        public ReverseEnumerable Reverse => new(this);

        public readonly struct ReverseEnumerable
        {
            private readonly Deque<T> _deque;
            public ReverseEnumerable(Deque<T> deque) => _deque = deque;
            public ReverseEnumerator GetEnumerator() => new(_deque);
        }

        public struct ReverseEnumerator : IEnumerator<T>
        {
            private readonly Deque<T> _deque;
            private int _index;

            internal ReverseEnumerator(Deque<T> deque)
            {
                _deque = deque;
                _index = deque.Count;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_index > 0)
                {
                    _index--;
                    Current = _deque._buffer[_deque.GetIndex(_index)];
                    return true;
                }

                _index = -1;
                Current = default;
                return false;
            }

            public T Current { get; private set; }
            object IEnumerator.Current => Current;
            public void Dispose() { }

            public void Reset()
            {
                _index = _deque.Count;
                Current = default;
            }
        }
    }
}
