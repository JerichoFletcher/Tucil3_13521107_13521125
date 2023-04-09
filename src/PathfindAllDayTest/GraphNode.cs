using System;

namespace PathfindAllDayTest {
    public class GraphNode<T> {
        public T Value { get; set; }
        public (double x, double y) Position { get; set; }

        public GraphNode(T value, double x, double y) {
            Value = value;
            Position = (x, y);
        }

        public double DistanceTo(GraphNode<T> other) {
            double
                dx = other.Position.x - Position.x,
                dy = other.Position.y - Position.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString() {
            return Value.ToString();
        }

        public override bool Equals(object obj) {
            return obj is GraphNode<T> node && Value.Equals(node.Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}
