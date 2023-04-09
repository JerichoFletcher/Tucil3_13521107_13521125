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
            GraphNode<char>
                node1 = new GraphNode<char>('A', 1d, 5d),
                node2 = new GraphNode<char>('B', 4d, 4d),
                node3 = new GraphNode<char>('C', 2d, 2d),
                node4 = new GraphNode<char>('D', 5d, 2d),
                node5 = new GraphNode<char>('E', 1d, 1d),
                node6 = new GraphNode<char>('F', 6d, 5d);

            GraphNode<char>[] nodes = new GraphNode<char>[] { node1, node2, node3, node4, node5, node6 };
            (int, int, double)[] edges = new (int, int, double)[] {
                (1, 2, 3.5d),
                (1, 3, 4d),
                (2, 4, 2.5d),
                (3, 4, 3d),
                (1, 5, 6d),
                (3, 5, 1.5d),
                (6, 2, 2.5d)
            };

            // Graph construction
            DirectedGraph<GraphNode<char>, double> graph = new DirectedGraph<GraphNode<char>, double>();
            for(int i = 0; i < edges.Length; i++) {
                (int a, int b, double w) = edges[i];
                (GraphNode<char> from, GraphNode<char> to) = (nodes[a - 1], nodes[b - 1]);
                graph.AddEdge(from, to, w);
            }

            // Astar instance construction test
            GraphTraversalAlgorithm<GraphNode<char>> astar = new GraphTraversalAlgorithm<GraphNode<char>>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = info => info.ExpandEdge.To.DistanceTo(info.End)
            };

            // Path search success test
            GraphNode<char>[] expectedPath, foundPath;            

            expectedPath = new GraphNode<char>[] { node1, node2, node4 };
            foundPath = astar.FindPath(node1, node4);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<char>[] { node6, node2, node4 };
            foundPath = astar.FindPath(node6, node4);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<char>[] { node1, node3, node5 };
            foundPath = astar.FindPath(node1, node5);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            expectedPath = new GraphNode<char>[] { node3 };
            foundPath = astar.FindPath(node3, node3);
            Assert.IsTrue(Enumerable.SequenceEqual(expectedPath, foundPath));

            // Path search fail test
            GraphNode<char> node7 = new GraphNode<char>('G', 0f, 0f);
            Assert.ThrowsException<ArgumentException>(() => astar.FindPath(node1, node7));

            Assert.IsNull(astar.FindPath(node1, node6));
            Assert.IsNull(astar.FindPath(node6, node3));
            Assert.IsNull(astar.FindPath(node3, node2));
        }
    }
}
