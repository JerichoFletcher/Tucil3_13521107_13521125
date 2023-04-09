using System;
using System.Collections.Generic;
using PathfindAllDay.Structs;

namespace PathfindAllDay.Algorithms {
    /// <summary>
    /// A class that contains a traversal algorithm over a graph instance, with modifiable g- and h-function.
    /// </summary>
    /// <typeparam name="T">Type of the graph node.</typeparam>
    public class GraphTraversalAlgorithm<T> {
        /// <summary>The internal reference to the graph.</summary>
        readonly DirectedGraph<T, double> _graph;
        /// <summary>An internal queue of open nodes.</summary>
        readonly PriorityQueue<GraphTraversalNode<T>> _open;
        /// <summary>An internal map of visited nodes.</summary>
        readonly Dictionary<T, GraphTraversalNode<T>> _closed;

        /// <summary>The g-function of the traversal algorithm.</summary>
        public Func<Info, double> GFunction { get; set; } = info => 0d;
        /// <summary>The h-function of the traversal algorithm.</summary>
        public Func<Info, double> HFunction { get; set; } = info => 0d;

        /// <summary>
        /// Instantiates a traversal algorithm over the given graph.
        /// </summary>
        /// <param name="graph">The graph to traverse.</param>
        /// <param name="minHeap">Whether to use min-heap to sort expand nodes.</param>
        public GraphTraversalAlgorithm(DirectedGraph<T, double> graph, bool minHeap = false) {
            _graph = graph;
            _open = new PriorityQueue<GraphTraversalNode<T>>(_graph.NodeCount, minHeap);
            _closed = new Dictionary<T, GraphTraversalNode<T>>();
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
        public T[] FindPath(T start, T end) {
            if(start == null || end == null) throw new ArgumentNullException();
            if(!_graph.ContainsNode(start) || !_graph.ContainsNode(end)) throw new ArgumentException("Node is not in the graph.");

            Console.WriteLine($">>> Beginning search, from: {start}, to: {end}"); 
            _open.Clear(); _closed.Clear();

            // Add the starting node to the open queue.
            bool success = false;
            GraphTraversalNode<T> expandNode = new GraphTraversalNode<T>(start, null);
            _open.TryEnqueue(expandNode);

            Console.WriteLine($"  Queue: {_open}");
            // Dequeue a node from the open queue and add it to the closed set. Repeat until the open set is empty.
            while(_open.TryDequeue(out expandNode)) {
                Console.WriteLine($"    Selected: {expandNode.Value}");
                _closed.Add(expandNode.Value, expandNode);

                // If the end node is reached, then a path is found: bail out early from the loop.
                success = expandNode.Value.Equals(end);
                if(success) break;

                // If not, then enumerate through the node's open neighbors...
                foreach(GraphEdge<T, double> expandEdge in _graph.OutEdges(expandNode.Value)) {
                    if(_closed.ContainsKey(expandEdge.To)) continue;

                    // ...and calculate its g-cost.
                    double gCost = GFunction(new Info(start, end, expandNode, expandEdge));
                    GraphTraversalNode<T> neighborNode = _open.Find(node => node.Value.Equals(expandEdge.To));
                    Console.WriteLine($"      Evaluating node {expandEdge.To} -> {expandEdge.From}, got {gCost}{(neighborNode != null ? " versus " + neighborNode.GCost : "")}");

                    // If the neighbor hasn't been visited yet or the calculated g-cost is smaller than the stored g-cost...
                    if(neighborNode == null || gCost < neighborNode.GCost) {
                        if(neighborNode == null)
                            neighborNode = new GraphTraversalNode<T>(expandEdge.To, expandNode);

                        // ...update the neighbor's g- and h-cost...
                        neighborNode.GCost = gCost;
                        neighborNode.HCost = HFunction(new Info(start, end, expandNode, expandEdge));
                        neighborNode.Parent = expandNode;

                        // ...and update the neighbor's order in the open queue.
                        if(!_open.Contains(neighborNode))
                            _open.TryEnqueue(neighborNode);
                        else
                            _open.Update(neighborNode);
                    }
                }
                Console.WriteLine($"  Queue: {_open}");
            }

            T[] result = success ? expandNode.Backtrack() : null;

            Console.WriteLine($"<<< Result: {(success ? $"success [{string.Join(", ", result)}]" : "fail")}");
            return result;
        }

        /// <summary>
        /// Contains information about a node expansion step within a search instance.
        /// </summary>
        public readonly struct Info {
            /// <summary>The start node of the searched path.</summary>
            public T Start { get; }
            /// <summary>The end node of the searched path.</summary>
            public T End { get; }
            /// <summary>The currently expanded node.</summary>
            public GraphTraversalNode<T> ExpandNode { get; }
            /// <summary>The edge over which the current node is expanded.</summary>
            public GraphEdge<T, double> ExpandEdge { get; }

            /// <summary>
            /// Constructs an <see cref="Info"/> instance that contains the given information.
            /// </summary>
            /// <param name="start">The start node of the searched path.</param>
            /// <param name="end">The end node of the searched path.</param>
            /// <param name="expandNode">The currently expanded node.</param>
            /// <param name="expandEdge">The edge over which the current node is expanded.</param>
            public Info(T start, T end, GraphTraversalNode<T> expandNode, GraphEdge<T, double> expandEdge) {
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

        public override string ToString() {
            return $"({Value}{(Parent != null ? " -> " + Parent : "")}: {FCost})";
        }
    }
}
