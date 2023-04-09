using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathfindAllDay.Structs;
using System;
using System.Collections.Generic;

namespace PathfindAllDayTest {
    [TestClass]
    public class StructTests {
        [TestMethod]
        public void TestPriorityQueue() {
            // Queue construction test
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new PriorityQueue<QueueItem<int>>(-1));
            PriorityQueue<QueueItem<int>> queue = new PriorityQueue<QueueItem<int>>(8);
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(8, queue.Capacity);

            // Dequeue fail test
            Assert.IsFalse(queue.TryDequeue(out QueueItem<int> _));
            Assert.AreEqual(0, queue.Count);

            // Enqueue success test
            for(int i = 0; i < queue.Capacity; i++) {
                QueueItem<int> item = new QueueItem<int>(i, i);

                // Pre-enqueue
                Assert.IsFalse(queue.Contains(item));

                // Enqueue
                Assert.IsTrue(queue.TryEnqueue(item));

                // Post enqueue
                Assert.AreEqual(i + 1, queue.Count);
                Assert.IsTrue(queue.Contains(item));
            }

            // Enqueue fail test
            Assert.IsFalse(queue.TryEnqueue(new QueueItem<int>(8, 8)));
            Assert.AreEqual(queue.Capacity, queue.Count);

            // Dequeue success test
            for(int i = queue.Capacity - 1; i >= 0; i--) {
                // Dequeue
                Assert.IsTrue(queue.TryDequeue(out QueueItem<int> item));

                // Post dequeue
                Assert.AreEqual(i, item.Item);
                Assert.AreEqual(i, queue.Count);
                Assert.IsFalse(queue.Contains(item));
            }
        }

        [TestMethod]
        public void TestDirectedGraph() {
            HashSet<int> uncheckedNodes = new HashSet<int>();

            // Graph initialization test
            DirectedGraph<int, int> graph = new DirectedGraph<int, int>();
            Assert.AreEqual(0, graph.NodeCount);
            Assert.AreEqual(0, graph.EdgeCount);

            int nodeCount = 5;

            // Graph node add success and fail test
            for(int i = 1; i <= nodeCount; i++) {
                // Pre-add
                Assert.IsFalse(graph.ContainsNode(i));

                // Add
                graph.AddNode(i);
                uncheckedNodes.Add(i);

                // Post-add
                Assert.IsTrue(graph.ContainsNode(i));
                Assert.AreEqual(i, graph.NodeCount);
                Assert.ThrowsException<ArgumentException>(() => graph.AddNode(i));
            }

            // Graph edge add success and fail test
            (int, int)[] pairs = new (int, int)[] { (1, 2), (1, 3), (3, 4), (2, 4), (1, 5), (3, 5) };
            int[] weights = new int[] { 3, 2, 5, 1, 7, 4 };
            for(int i = 0; i < pairs.Length; i++) {
                (int a, int b) = pairs[i];
                int weight = weights[i];

                // Pre-add
                Assert.IsFalse(graph.ContainsEdge(a, b));
                Assert.IsFalse(graph.TryGetEdge(a, b, out int _));

                // Add
                graph.AddEdge(a, b, weight);

                // Post-add
                Assert.IsTrue(graph.ContainsEdge(a, b));
                Assert.IsTrue(graph.TryGetEdge(a, b, out int storedWeight));
                Assert.AreEqual(weight, storedWeight);
                Assert.AreEqual(i + 1, graph.EdgeCount);
                Assert.ThrowsException<ArgumentException>(() => graph.AddEdge(a, b, weight));
            }

            // Graph node and neighbor enumeration test
            HashSet<int> uncheckedNeighbors = new HashSet<int>();
            int[][] neighbors = new int[][] {
                new int[] { 2, 3, 5 },
                new int[] { 4 },
                new int[] { 4, 5 },
                new int[] { },
                new int[] { }
            };
            foreach(int node in graph.Nodes()) {
                // Ignore irrelevant nodes
                if(1 <= node && node <= nodeCount) {
                    // List all neighbors to be checked
                    foreach(int n in neighbors[node - 1]) uncheckedNeighbors.Add(n);

                    // Iterate neighbors
                    foreach(GraphEdge<int, int> edge in graph.OutEdges(node)) {
                        Assert.IsTrue(uncheckedNeighbors.Remove(edge.To));
                    }
                    Assert.AreEqual(0, uncheckedNeighbors.Count);
                }

                // Remove checked node from hash map
                Assert.IsTrue(uncheckedNodes.Remove(node));
            }
            Assert.AreEqual(0, uncheckedNodes.Count);
        }
    }
}
