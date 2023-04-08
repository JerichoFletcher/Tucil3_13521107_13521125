using System;

namespace PathfindAllDay.Structs {
    public class PriorityQueue<T> where T : IPriorityQueueElement<T> {
        private readonly T[] _buffer;

        public int Capacity => _buffer.Length;
        public int Count { get; private set; }

        public PriorityQueue(int capacity) {
            if(capacity < 0)throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new T[capacity];
            Count = 0;
        }

        public void Clear() {
            for(int i = 0; i < Capacity; i++) _buffer[i] = default;
            Count = 0;
        }

        public bool Contains(T element) {
            if(element == null) throw new ArgumentNullException();
            return element.Equals(_buffer[element.QueueIndex]);
        }

        public bool TryEnqueue(T element) {
            if(element == null)throw new ArgumentNullException();
            if(Count == Capacity) return false;

            (_buffer[Count] = element).QueueIndex = Count;
            SortUp(element);
            Count++;

            return true;
        }

        public bool TryDequeue(out T element) {
            if(Count == 0) {
                element = default;
                return false;
            }

            element = _buffer[0];
            Count--;
            (_buffer[0] = _buffer[Count]).QueueIndex = 0;
            SortDown(_buffer[0]);

            return true;
        }

        private void SortUp(T element) {
            int parentIndex = (element.QueueIndex - 1) / 2;
            
            while(parentIndex >= 0) {
                T parentItem = _buffer[parentIndex];
                if(element.CompareTo(parentItem) > 0) {
                    Swap(element, parentItem);
                } else {
                    return;
                }
                parentIndex = (element.QueueIndex - 1) / 2;
            }
        }

        private void SortDown(T element) {
            while(true) {
                int childIndexLeft = element.QueueIndex * 2 + 1;
                int childIndexRight = element.QueueIndex * 2 + 2;
                int swapIndex;

                if(childIndexLeft < Count) {
                    swapIndex = childIndexLeft;
                    if(childIndexRight < Count) {
                        if(_buffer[childIndexLeft].CompareTo(_buffer[childIndexRight]) < 0) {
                            swapIndex = childIndexRight;
                        }
                    }

                    if(element.CompareTo(_buffer[swapIndex]) < 0) {
                        Swap(element, _buffer[swapIndex]);
                    } else {
                        return;
                    }
                } else {
                    return;
                }
            }
        }

        private void Swap(T a, T b) {
            _buffer[a.QueueIndex] = b;
            _buffer[b.QueueIndex] = a;

            int temp = a.QueueIndex;
            a.QueueIndex = b.QueueIndex;
            b.QueueIndex = temp;
        }
    }

    public interface IPriorityQueueElement<TElement> : IComparable<TElement> {
        int QueueIndex { get; set; }
    }
}
