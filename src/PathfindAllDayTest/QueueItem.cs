using PathfindAllDay.Structs;
using System;

namespace PathfindAllDayTest {
    internal class QueueItem<T> : IPriorityQueueItem<QueueItem<T>> {
        public T Item { get; set; }
        public float Priority { get; set; }
        public int QueueIndex { get; set; } = -1;

        public QueueItem(T item, float priority) {
            Item = item; Priority = priority;
        }

        public int CompareTo(QueueItem<T> other) {
            return Priority.CompareTo(other.Priority);
        }

        public override bool Equals(object obj) {
            return obj is QueueItem<T> item && Item.Equals(item.Item);
        }

        public override int GetHashCode() => Item.GetHashCode();

        public override string ToString() {
            return $"{Priority}:{Item} at {QueueIndex}";
        }
    }
}
