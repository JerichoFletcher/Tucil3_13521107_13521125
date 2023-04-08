using System;
using System.Collections.Generic;

namespace PathfindAllDay.Structs {
    /// <summary>
    /// Represents a graph with monodirectional edges.
    /// </summary>
    /// <typeparam name="TNode">Type of each node.</typeparam>
    /// <typeparam name="TEdgeData">Type of data stored in each edge.</typeparam>
    public class DirectedGraph<TNode, TEdgeData> {
        private readonly Dictionary<TNode, LinkedList<TNode>> _adjacencyLists;
        private readonly Dictionary<(TNode, TNode), TEdgeData> _edges;

        /// <summary>The number of nodes stored in the graph.</summary>
        public int NodeCount => _adjacencyLists.Count;
        /// <summary>The number of edges stored in the graph.</summary>
        public int EdgeCount => _edges.Count;

        /// <summary>Constructs an empty graph.</summary>
        public DirectedGraph() {
            _edges = new Dictionary<(TNode, TNode), TEdgeData>();
            _adjacencyLists = new Dictionary<TNode, LinkedList<TNode>>();
        }

        /// <summary>
        /// Checks whether a node exists in the graph.
        /// </summary>
        /// <param name="node">The node to be checked.</param>
        /// <returns>Whether <paramref name="node"/> is contained in the graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is <see langword="null"/>.</exception>
        public bool ContainsNode(TNode node) {
            if(node == null) throw new ArgumentNullException();
            return _adjacencyLists.ContainsKey(node);
        }

        /// <summary>
        /// Checks whether the edge (<paramref name="from"/>, <paramref name="to"/>) exists in the graph.
        /// </summary>
        /// <param name="from">The source node of the edge.</param>
        /// <param name="to">The destination node of the edge.</param>
        /// <returns>Whether the edge (<paramref name="from"/>, <paramref name="to"/>) is contained in the graph.</returns>
        /// <inheritdoc cref="ContainsNode(TNode)"/>
        public bool ContainsEdge(TNode from, TNode to) {
            return ContainsNode(from) && ContainsNode(to) && _edges.ContainsKey((from, to));
        }

        /// <summary>
        /// Attempts to find the edge (<paramref name="from"/>, <paramref name="to"/>) in the graph.
        /// </summary>
        /// <param name="from">The source node of the edge.</param>
        /// <param name="to">The destination node of the edge.</param>
        /// <param name="data">The data stored in the edge.</param>
        /// <returns>Whether the edge (<paramref name="from"/>, <paramref name="to"/>) is contained in the graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.</exception>
        public bool TryGetEdge(TNode from, TNode to, out TEdgeData data) {
            if(from == null || to == null) {
                data = default;
                throw new ArgumentNullException();
            }
            return _edges.TryGetValue((from, to), out data);
        }

        /// <summary>
        /// Adds <paramref name="node"/> to the graph.
        /// </summary>
        /// <param name="node">The node to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> already exists in the graph.</exception>
        public void AddNode(TNode node) {
            if(node == null) throw new ArgumentNullException();
            if(_adjacencyLists.ContainsKey(node)) throw new ArgumentException($"Node {node} already exists in this graph.");

            _adjacencyLists.Add(node, null);
        }

        /// <summary>
        /// Adds the edge (<paramref name="from"/>, <paramref name="to"/>) to the graph.
        /// </summary>
        /// <param name="from">The source node of the edge.</param>
        /// <param name="to">The destination node of the edge.</param>
        /// <param name="data">The data stored in the edge.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="from"/> equals <paramref name="to"/>.</exception>
        public void AddEdge(TNode from, TNode to, TEdgeData data) {
            if(from == null || to == null) throw new ArgumentNullException();
            if(from.Equals(to)) throw new ArgumentException();

            if(!ContainsNode(from)) AddNode(from);
            if(!ContainsNode(to)) AddNode(to);

            _edges.Add((from, to), data);
            (_adjacencyLists[from] ?? (_adjacencyLists[from] = new LinkedList<TNode>())).AddFirst(to);
        }

        /// <summary>
        /// Adds the edge described by <paramref name="edge"/> to the graph.
        /// </summary>
        /// <param name="edge">The edge description.</param>
        /// <inheritdoc cref="AddEdge(TNode, TNode, TEdgeData)"/>
        public void AddEdge(GraphEdge<TNode, TEdgeData> edge) {
            AddEdge(edge.From, edge.To, edge.Data);
        }

        /// <summary>
        /// Enumerates through all the nodes stored in the graph.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> that enumerates through all the nodes stored in the graph.</returns>
        public IEnumerable<TNode> Nodes() {
            foreach(TNode node in _adjacencyLists.Keys)
                yield return node;
        }

        /// <summary>
        /// Enumerates through all edges leading out of <paramref name="from"/>.
        /// </summary>
        /// <param name="from">The source node of the edges.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that enumerates through all edges leading out of <paramref name="from"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="from"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="from"/> doesn't exist in the graph.</exception>
        public IEnumerable<GraphEdge<TNode, TEdgeData>> OutEdges(TNode from) {
            if(from == null) throw new ArgumentNullException();
            if(!ContainsNode(from)) throw new ArgumentException($"Graph doesn't contain the node {from}");

            foreach(TNode to in _adjacencyLists[from])
                yield return new GraphEdge<TNode, TEdgeData>(from, to, _edges[(from, to)]);
        }
    }

    /// <summary>
    /// Stores all information relating to a graph edge.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdgeData"></typeparam>
    public readonly struct GraphEdge<TNode, TEdgeData> {
        /// <summary>The source node of the edge.</summary>
        public TNode From { get; }
        /// <summary>The destination node of the edge.</summary>
        public TNode To { get; }
        /// <summary>The data stored in the edge.</summary>
        public TEdgeData Data { get; }

        /// <summary>
        /// Constructs a <see cref="GraphEdge{TNode, TEdgeData}"/> that describes the edge (<paramref name="from"/>, <paramref name="to"/>).
        /// </summary>
        /// <param name="from">The source node of the edge.</param>
        /// <param name="to">The destination node of the edge.</param>
        /// <param name="data">The data stored in the edge.</param>
        public GraphEdge(TNode from, TNode to, TEdgeData data) {
            From = from; To = to; Data = data;
        }
    }
}
