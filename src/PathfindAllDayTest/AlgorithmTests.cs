using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathfindAllDay.Structs;
using PathfindAllDay.Algorithms;
using System.Linq;
using System.Diagnostics;
using System;

namespace PathfindAllDayTest {
    [TestClass]
    public class AlgorithmTests {
        [TestMethod]
        public void TestAstar() {
            // Graph information
            GraphNode<int>
                node1 = new GraphNode<int>(1, 1d, 1d),
                node2 = new GraphNode<int>(2, 4d, 1d),
                node3 = new GraphNode<int>(3, 1.5d, 3d),
                node4 = new GraphNode<int>(4, 5d, 2d),
                node5 = new GraphNode<int>(5, 1d, 6d),
                node6 = new GraphNode<int>(6, 4.5d, 0.5d);

            GraphNode<int>[] nodes = new GraphNode<int>[] { node1, node2, node3, node4, node5, node6 };
            (int, int, double)[] edges = new (int, int, double)[] {
                (1, 2, 3d),
                (1, 3, 2d),
                (2, 4, 1d),
                (3, 4, 5d),
                (1, 5, 7d),
                (3, 5, 4d),
                (6, 2, 1d)
            };

            // Graph construction
            DirectedGraph<GraphNode<int>, double> graph = new DirectedGraph<GraphNode<int>, double>();
            for(int i = 0; i < edges.Length; i++) {
                (int a, int b, double w) = edges[i];
                (GraphNode<int> from, GraphNode<int> to) = (nodes[a - 1], nodes[b - 1]);
                graph.AddEdge(from, to, w);
            }

            // Astar instance construction test
            GraphTraversalAlgorithm<GraphNode<int>> astar = new GraphTraversalAlgorithm<GraphNode<int>>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = info => info.ExpandEdge.To.DistanceTo(info.End)
            };

            // Path search success test
            GraphNode<int>[] expectedPath, foundPath;

            expectedPath = new GraphNode<int>[] { node1, node2, node4 };
            foundPath = astar.FindPath(node1, node4);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<int>[] { node1, node3, node5 };
            foundPath = astar.FindPath(node1, node5);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<int>[] { node6, node2, node4 };
            foundPath = astar.FindPath(node6, node4);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<int>[] { node3 };
            foundPath = astar.FindPath(node3, node3);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            // Path search fail test
            GraphNode<int> node7 = new GraphNode<int>(7, 0f, 0f);
            Assert.ThrowsException<ArgumentException>(() => astar.FindPath(node1, node7));

            Assert.IsNull(astar.FindPath(node1, node6));
            Assert.IsNull(astar.FindPath(node6, node3));
            Assert.IsNull(astar.FindPath(node3, node2));
        }
    }
}
