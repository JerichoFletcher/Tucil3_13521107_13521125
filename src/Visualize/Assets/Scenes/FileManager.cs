using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathfindAllDay.Structs;

public class FileManager : MonoBehaviour {
    public Button ShowMap;
    public Button ShowPath;
    public TMP_InputField inputSimpulAwal;
    public TMP_InputField inputSimpulTujuan;
    public TMP_InputField inputFileGraph;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    private List<GameObject> nodeObjects = new List<GameObject>();
    private List<GameObject> edgeObjects = new List<GameObject>();
    private DirectedGraph<MapNode, double> graph = null;
    private Dictionary<string, MapNode> nodes = new Dictionary<string, MapNode>();

    private void Update() {
        ShowMap.interactable = inputFileGraph.text?.Length > 0;
        ShowPath.interactable = graph != null && inputSimpulAwal.text?.Length > 0 && inputSimpulTujuan.text?.Length > 0;
    }

    void ReadFile(string fileName) {
        StreamReader reader = null;
        try {
            int lineNum = 1;
            try {
                reader = new StreamReader(fileName);
                graph = new DirectedGraph<MapNode, double>();
                nodes.Clear();

                // Handle file read here
                while(!reader.EndOfStream) {
                    string lineStr = reader.ReadLine();
                    string[] columns = lineStr.Split(',');

                    //bool matrixInput = false;
                    switch(columns[0]) {
                        case "N":
                            // Read node data in lon-lat format
                            double lat = double.Parse(columns[2]);
                            double lon = double.Parse(columns[1]);
                            string name = columns[3];

                            MapNode node = new MapNode(name, lat, lon);
                            nodes.Add(name, node);
                            graph.AddNode(node);

                            break;
                        case "E":
                            // Read edge data
                            string from = columns[1];
                            string to = columns[2];
                            double cost = double.Parse(columns[3]);
                            string kind = columns[4];

                            switch(kind) {
                                case "directed":
                                    graph.AddEdge(nodes[from], nodes[to], cost);
                                    break;
                                case "undirected":
                                    graph.AddEdge(nodes[from], nodes[to], cost);
                                    graph.AddEdge(nodes[to], nodes[from], cost);
                                    break;
                                default:
                                    // Invalid line
                                    throw new InvalidDataException($"Invalid file format, error at line {lineNum}: Invalid edge type '{kind}'.");
                            }

                            break;
                        case "M":
                            // Read matrix row
                            // TODO: Add
                            break;
                        case "#":
                            // Comment line
                            break;
                        default:
                            // Invalid line
                            throw new InvalidDataException($"Invalid file format, error at line {lineNum}: Invalid line ID '{columns[0]}'.");
                    }
                    lineNum++;
                }

                Debug.Log($"nodes: {graph.NodeCount}, edges: {graph.EdgeCount}");
                foreach(MapNode n in graph.Nodes()) Debug.Log(n.ToString());
            } catch(FileNotFoundException e) {
                // Handle file not found here
                Debug.LogException(e);
            } catch(Exception e) {
                throw e is InvalidDataException ? e : new InvalidDataException($"Invalid file format, error at line {lineNum}: {e.Message}");
            }
        } catch(InvalidDataException e) {
            // Handle invalid file format here
            graph = null;
            nodes.Clear();
            Debug.LogException(e);
        } finally {
            reader?.Close();
        }
    }


    public void OnShowMap() {
        string fileName = inputFileGraph.text;
        ReadFile(fileName);
    }

    public void OnShowPath() {
        string startNode = inputSimpulAwal.text;
        string endNode = inputSimpulTujuan.text;

        // Handle show path here
    }
}

class MapNode {
    public string Name { get; private set; }
    public (double x, double y) Coordinate { get; private set; }
    public MapNode(string name, double latitude, double longitude) {
        Name = name;
        Coordinate = (latitude, longitude);
    }

    public override bool Equals(object obj) {
        return obj is MapNode node && Name.Equals(node.Name);
    }

    public override int GetHashCode() {
        return Name.GetHashCode();
    }

    public override string ToString() {
        return $"{Coordinate}: {Name}";
    }
}
