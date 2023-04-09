using PathfindAllDay.Structs;
using System;

namespace PathfindAllDayTest {
    internal struct QueueItem<T> : IPriorityQueueItem<QueueItem<T>> {
        public T Item { get; set; }
        public float Priority { get; set; }
        public int QueueIndex { get; set; }

        public QueueItem(T item, float priority) {
            Item = item; Priority = priority;
            QueueIndex = -1;
        }

        public int CompareTo(QueueItem<T> other) {
            return Priority.CompareTo(other.Priority);
        }
    }
}
