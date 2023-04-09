using System;

namespace PathfindAllDay.Structs {
    /// <summary>
    /// Represents a queue where items are ordered by priority, internally implemented using a heap.
    /// </summary>
    /// <typeparam name="T">Type of queue items.</typeparam>
    public class PriorityQueue<T> where T : IPriorityQueueItem<T> {
        /// <summary>Internal buffer that stores queue items.</summary>
        private readonly T[] _buffer;

        /// <summary>The maximum capacity of the queue.</summary>
        public int Capacity => _buffer.Length;
        /// <summary>The number of items in the queue.</summary>
        public bool IsMinHeap { get; }
        public int Count { get; private set; }

        /// <summary>
        /// Constructs an empty queue.
        /// </summary>
        /// <param name="capacity">The maximum capacity of the queue.</param>
        /// <param name="minHeap">Whether to sort the queue using min-heap rule.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is negative.</exception>
        public PriorityQueue(int capacity, bool minHeap = false) {
            if(capacity < 0)throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new T[capacity];
            IsMinHeap = minHeap;
            Count = 0;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear() {
            for(int i = 0; i < Capacity; i++) _buffer[i] = default;
            Count = 0;
        }

        /// <summary>
        /// Checks if <paramref name="item"/> is contained in the queue.
        /// </summary>
        /// <param name="item">The item to be checked.</param>
        /// <returns>Whether <paramref name="item"/> is contained in the queue.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is <see langword="null"/>.</exception>
        public bool Contains(T item) {
            if(item == null) throw new ArgumentNullException();
            return _buffer.GetLowerBound(0) <= item.QueueIndex && item.QueueIndex < Count && item.Equals(_buffer[item.QueueIndex]);
        }

        /// <summary>
        /// Checks whether any item in the queue satisfies a condition.
        /// </summary>
        /// <param name="pred">The condition to check.</param>
        /// <returns>Whether at least one item satisfies <paramref name="pred"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pred"/> is <see langword="null"/>.</exception>
        public bool Any(Predicate<T> pred) {
            if(pred == null) throw new ArgumentNullException();
            for(int i = 0; i < Count; i++) if(pred(_buffer[i])) return true;
            return false;
        }

        /// <summary>
        /// Checks whether all item in the queue satisfies a condition.
        /// </summary>
        /// <param name="pred">The condition to check.</param>
        /// <returns>Whether all item satisfies <paramref name="pred"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pred"/> is <see langword="null"/>.</exception>
        public bool All(Predicate<T> pred) {
            if(pred == null) throw new ArgumentNullException();
            for(int i = 0; i < Count; i++) if(pred(_buffer[i])) return false;
            return true;
        }

        /// <summary>
        /// Finds the first item in the queue that satisfies a condition.
        /// </summary>
        /// <param name="pred">The condition to check.</param>
        /// <returns>The first item in the queue that satisfies <paramref name="pred"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pred"/> is <see langword="null"/>.</exception>
        public T Find(Predicate<T> pred) {
            if(pred == null) throw new ArgumentNullException();
            for(int i = 0; i < Count; i++) if(pred(_buffer[i])) return _buffer[i];
            return default;
        }

        /// <summary>
        /// Attempts to enqueue an item into the queue.
        /// </summary>
        /// <param name="item">The item to be enqueued.</param>
        /// <returns>Whether <paramref name="item"/> is successfully enqueued.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is <see langword="null"/>.</exception>
        public bool TryEnqueue(T item) {
            if(item == null)throw new ArgumentNullException();
            if(Count == Capacity) return false;

            (_buffer[Count] = item).QueueIndex = Count;
            SortUp(item);
            Count++;

            return true;
        }

        /// <summary>
        /// Attempts to dequeue an item from the queue.
        /// </summary>
        /// <param name="item">The dequeued item.</param>
        /// <returns>Whether <paramref name="item"/> is succesfully dequeued.</returns>
        public bool TryDequeue(out T item) {
            if(Count == 0) {
                item = default;
                return false;
            }

            item = _buffer[0];
            Count--;
            (_buffer[0] = _buffer[Count]).QueueIndex = 0;
            SortDown(_buffer[0]);

            return true;
        }

        /// <summary>
        /// Sorts <paramref name="item"/> upwards in the heap.
        /// </summary>
        /// <param name="item">The item to be sorted upwards.</param>
        private void SortUp(T item) {
            int parentIndex = (item.QueueIndex - 1) / 2;
            
            while(parentIndex >= 0) {
                T parentItem = _buffer[parentIndex];
                int compare = item.CompareTo(parentItem);
                if(!IsMinHeap ? compare > 0 : compare < 0) {
                    Swap(item, parentItem);
                } else return;
                parentIndex = (item.QueueIndex - 1) / 2;
            }
        }

        /// <summary>
        /// Sorts <paramref name="item"/> downwards in the heap.
        /// </summary>
        /// <param name="item">The item to be sorted downwards.</param>
        private void SortDown(T item) {
            while(true) {
                int childIndexLeft = item.QueueIndex * 2 + 1;
                int childIndexRight = item.QueueIndex * 2 + 2;
                int swapIndex;

                if(childIndexLeft < Count) {
                    swapIndex = childIndexLeft;
                    if(childIndexRight < Count) {
                        int compareChild = _buffer[childIndexLeft].CompareTo(_buffer[childIndexRight]);
                        if(!IsMinHeap ? compareChild < 0 : compareChild > 0) {
                            swapIndex = childIndexRight;
                        }
                    }

                    int compare = item.CompareTo(_buffer[swapIndex]);
                    if(!IsMinHeap ? compare < 0 : compare > 0) {
                        Swap(item, _buffer[swapIndex]);
                    } else return;
                } else return;
            }
        }

        /// <summary>
        /// Swaps <paramref name="a"/> and <paramref name="b"/> in the heap.
        /// </summary>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        private void Swap(T a, T b) {
            _buffer[a.QueueIndex] = b;
            _buffer[b.QueueIndex] = a;

            (a.QueueIndex, b.QueueIndex) = (b.QueueIndex, a.QueueIndex);
        }
    }

    /// <summary>
    /// Denotes that a type can be treated as a queue item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPriorityQueueItem<T> : IComparable<T> {
        /// <summary>The buffer index of the queue item.</summary>
        int QueueIndex { get; set; }
    }
}
