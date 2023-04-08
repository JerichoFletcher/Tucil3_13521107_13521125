using System;
using System.Collections.Generic;
using PathfindAllDay.Structs;

namespace PathfindAllDay.Algorithms {
    /// <summary>
    /// A class that contains a traversal algorithm over a graph instance, with modifiable g- and h-function.
    /// </summary>
    /// <typeparam name="TNode">Type of the graph node.</typeparam>
    public class GraphTraversalAlgorithm<TNode> {
        /// <summary>The internal reference to the graph.</summary>
        readonly DirectedGraph<TNode, double> _graph;
        /// <summary>An internal queue of open nodes.</summary>
        readonly PriorityQueue<GraphTraversalNode<TNode>> _open;
        /// <summary>An internal map of visited nodes.</summary>
        readonly Dictionary<TNode, GraphTraversalNode<TNode>> _closed;

        /// <summary>The g-function of the traversal algorithm.</summary>
        public Func<Info, double> GFunction { get; set; } = info => 0d;
        /// <summary>The h-function of the traversal algorithm.</summary>
        public Func<Info, double> HFunction { get; set; } = info => 0d;

        /// <summary>
        /// Instantiates a traversal algorithm over the given graph.
        /// </summary>
        /// <param name="graph">The graph to traverse.</param>
        public GraphTraversalAlgorithm(DirectedGraph<TNode, double> graph) {
            _graph = graph;
            _open = new PriorityQueue<GraphTraversalNode<TNode>>(_graph.NodeCount);
            _closed = new Dictionary<TNode, GraphTraversalNode<TNode>>();
        }

        /// <summary>
        /// Performs a traversal search over the graph to find a path that leads from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        /// <param name="start">The start node of the path.</param>
        /// <param name="end">The end node of the path.</param>
        /// <returns>
        ///     An array containing the path from <paramref name="start"/> to <paramref name="end"/> if such path is found within the graph;
        ///     <see langword="null"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="start"/> or <paramref name="end"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="start"/> or <paramref name="end"/> is not contained in the graph.</exception>
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

        /// <summary>
        /// Contains information about a node expansion step within a search instance.
        /// </summary>
        public readonly struct Info {
            /// <summary>The start node of the searched path.</summary>
            public TNode Start { get; }
            /// <summary>The end node of the searched path.</summary>
            public TNode End { get; }
            /// <summary>The currently expanded node.</summary>
            public GraphTraversalNode<TNode> ExpandNode { get; }
            /// <summary>The edge over which the current node is expanded.</summary>
            public GraphEdge<TNode, double> ExpandEdge { get; }

            /// <summary>
            /// Constructs an <see cref="Info"/> instance that contains the given information.
            /// </summary>
            /// <param name="start">The start node of the searched path.</param>
            /// <param name="end">The end node of the searched path.</param>
            /// <param name="expandNode">The currently expanded node.</param>
            /// <param name="expandEdge">The edge over which the current node is expanded.</param>
            public Info(TNode start, TNode end, GraphTraversalNode<TNode> expandNode, GraphEdge<TNode, double> expandEdge) {
                Start = start; End = end; ExpandNode = expandNode; ExpandEdge = expandEdge;
            }
        }
    }

    /// <summary>
    /// Stores information about a node in the search tree.
    /// </summary>
    /// <typeparam name="T">Type of the graph node.</typeparam>
    public class GraphTraversalNode<T> : IPriorityQueueItem<GraphTraversalNode<T>> {
        /// <summary>The relevant graph node.</summary>
        public T Value { get; }
        /// <summary>The parent of this node in the search tree.</summary>
        public GraphTraversalNode<T> Parent { get; set; }
        /// <summary>The stored g-cost of the node.</summary>
        public double GCost { get; set; }
        /// <summary>The stored h-cost of the node.</summary>
        public double HCost { get; set; }
        public int QueueIndex { get; set; }

        /// <summary>The stored f-cost of the node.</summary>
        public double FCost => GCost + HCost;

        /// <summary>
        /// Constructs a traversal node.
        /// </summary>
        /// <param name="value">The relevant graph node.</param>
        /// <param name="parent">The parent of this node in the search tree.</param>
        public GraphTraversalNode(T value, GraphTraversalNode<T> parent) {
            Value = value; Parent = parent;
            GCost = 0f; HCost = 0f;
            QueueIndex = -1;
        }

        public int CompareTo(GraphTraversalNode<T> other) {
            return FCost.CompareTo(other.FCost);
        }

        /// <summary>
        /// Constructs a graph node array that leads from the root of the search tree to this node.
        /// </summary>
        /// <returns>An array containing the path from the search tree root to this node.</returns>
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
