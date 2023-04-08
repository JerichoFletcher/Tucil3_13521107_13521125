using System;
using System.Collections.Generic;

namespace PathfindAllDay.Structs {
    public class DirectedGraph<TNode, TEdgeData> {
        private readonly Dictionary<TNode, LinkedList<TNode>> _adjacencyLists;
        private readonly Dictionary<(TNode, TNode), TEdgeData> _edges;

        public int NodeCount => _adjacencyLists.Count;
        public int EdgeCount => _edges.Count;

        public DirectedGraph() {
            _edges = new Dictionary<(TNode, TNode), TEdgeData>();
            _adjacencyLists = new Dictionary<TNode, LinkedList<TNode>>();
        }

        public bool ContainsNode(TNode node) {
            if(node == null) throw new ArgumentNullException();
            return _adjacencyLists.ContainsKey(node);
        }

        public bool ContainsEdge(TNode from, TNode to) {
            return ContainsNode(from) && ContainsNode(to) && _edges.ContainsKey((from, to));
        }

        public bool TryGetEdge(TNode from, TNode to, out TEdgeData data) {
            if(from == null || to == null) {
                data = default;
                throw new ArgumentNullException();
            }
            return _edges.TryGetValue((from, to), out data);
        }

        public void AddNode(TNode node) {
            if(node == null) throw new ArgumentNullException();
            if(_adjacencyLists.ContainsKey(node)) throw new ArgumentException($"Node {node} already exists in this graph.");

            _adjacencyLists.Add(node, null);
        }

        public void AddEdge(TNode from, TNode to, TEdgeData data) {
            if(from == null || to == null) throw new ArgumentNullException();
            if(from.Equals(to)) throw new ArgumentException();

            if(!ContainsNode(from)) AddNode(from);
            if(!ContainsNode(to)) AddNode(to);

            _edges.Add((from, to), data);
            (_adjacencyLists[from] ?? (_adjacencyLists[from] = new LinkedList<TNode>())).AddFirst(to);
        }

        public IEnumerable<TNode> Nodes() {
            foreach(TNode node in _adjacencyLists.Keys)
                yield return node;
        }

        public IEnumerable<GraphEdge<TNode, TEdgeData>> OutEdges(TNode from) {
            if(from == null) throw new ArgumentNullException();
            if(!ContainsNode(from)) throw new ArgumentException($"Graph doesn't contain the node {from}");

            foreach(TNode to in _adjacencyLists[from])
                yield return new GraphEdge<TNode, TEdgeData>(from, to, _edges[(from, to)]);
        }
    }

    public readonly struct GraphEdge<TNode, TEdgeData> {
        public TNode From { get; }
        public TNode To { get; }
        public TEdgeData Data { get; }

        public GraphEdge(TNode from, TNode to, TEdgeData data) {
            From = from; To = to; Data = data;
        }
    }
}
