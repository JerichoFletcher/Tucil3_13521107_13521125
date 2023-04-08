using System;
using System.Collections.Generic;
using PathfindAllDay.Structs;

namespace PathfindAllDay.Algorithms {
    public class GraphTraversalAlgorithm<TNode> {
        readonly DirectedGraph<TNode, double> _graph;
        readonly PriorityQueue<GraphTraversalNode<TNode>> _open;
        readonly Dictionary<TNode, GraphTraversalNode<TNode>> _closed;

        public Func<Info, double> GFunction { get; set; } = info => 0d;
        public Func<Info, double> HFunction { get; set; } = info => 0d;

        public GraphTraversalAlgorithm(DirectedGraph<TNode, double> graph) {
            _graph = graph;
            _open = new PriorityQueue<GraphTraversalNode<TNode>>(_graph.NodeCount);
            _closed = new Dictionary<TNode, GraphTraversalNode<TNode>>();
        }

        public TNode[] FindPath(TNode start, TNode end) {
            if(start == null || end == null) throw new ArgumentNullException();
            if(!_graph.ContainsNode(start) || !_graph.ContainsNode(end)) throw new ArgumentException("Node is not in the graph.");

            _open.Clear(); _closed.Clear();

            bool success = false;
            GraphTraversalNode<TNode> expandNode = new GraphTraversalNode<TNode>(start, null);
            _open.TryEnqueue(expandNode);

            while(_open.TryDequeue(out expandNode)) {
                _closed.Add(expandNode.Value, expandNode);

                success = expandNode.Value.Equals(end);
                if(success) break;

                foreach(GraphEdge<TNode, double> expandEdge in _graph.OutEdges(expandNode.Value)) {
                    if(_closed.ContainsKey(expandEdge.To)) continue;

                    double gCost = GFunction(new Info(start, end, expandNode, expandEdge));
                    GraphTraversalNode<TNode> neighborNode = _open.Find(node => node.Value.Equals(expandEdge.To));
                    if(neighborNode == null || neighborNode.GCost < gCost) {
                        if(neighborNode == null) {
                            neighborNode = new GraphTraversalNode<TNode>(expandEdge.To, expandNode);
                            _open.TryEnqueue(neighborNode);
                        }
                        neighborNode.GCost = gCost;
                        neighborNode.HCost = HFunction(new Info(start, end, expandNode, expandEdge));
                        neighborNode.Parent = expandNode;
                    }
                }
            }

            return success ? expandNode.Backtrack() : null;
        }

        public readonly struct Info {
            public TNode Start { get; }
            public TNode End { get; }
            public GraphTraversalNode<TNode> ExpandNode { get; }
            public GraphEdge<TNode, double> ExpandEdge { get; }

            public Info(TNode start, TNode end, GraphTraversalNode<TNode> expandNode, GraphEdge<TNode, double> expandEdge) {
                Start = start; End = end; ExpandNode = expandNode; ExpandEdge = expandEdge;
            }
        }
    }

    public class GraphTraversalNode<T> : IPriorityQueueElement<GraphTraversalNode<T>> {
        public T Value { get; }
        public GraphTraversalNode<T> Parent { get; set; }
        public double GCost { get; set; }
        public double HCost { get; set; }
        public int QueueIndex { get; set; }

        public double FCost => GCost + HCost;

        public GraphTraversalNode(T value, GraphTraversalNode<T> parent) {
            Value = value; Parent = parent;
            GCost = 0f; HCost = 0f;
            QueueIndex = -1;
        }

        public int CompareTo(GraphTraversalNode<T> other) {
            return FCost.CompareTo(other.FCost);
        }

        public T[] Backtrack() {
            List<T> path = new List<T>();
            GraphTraversalNode<T> current = this;

            while(current != null) {
                path.Add(current.Value);
                current = current.Parent;
            }

            path.Reverse();
            return path.ToArray();
        }
    }
}
