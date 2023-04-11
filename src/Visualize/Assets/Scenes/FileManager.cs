using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathfindAllDay.Structs;
using PathfindAllDay.Algorithms;
using System.Text;

public class FileManager : MonoBehaviour {
    public Button ShowMap;
    public Button ShowPath;
    public TMP_InputField inputSimpulAwal;
    public TMP_InputField inputSimpulTujuan;
    public TMP_InputField inputFileGraph;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    private DirectedGraph<MapNode, double> graph = null;
    private readonly List<GameObject> nodeObjects = new List<GameObject>();
    private readonly List<GameObject> edgeObjects = new List<GameObject>();
    private readonly Dictionary<string, MapNode> nodes = new Dictionary<string, MapNode>();
    private readonly Dictionary<string, string> inputOptions = new Dictionary<string, string>();

    private const string
        optNodeFormat = "format",
        optEdgeFormat = "edge";
    private const string
        optNodeFormatXY = "xy",
        optNodeFormatYX = "yx",
        optNodeFormatLatLon = "latlon",
        optNodeFormatLonLat = "lonlat";
    private const string
        optEdgeFormatPair = "pair",
        optEdgeFormatMatrix = "matrix";
    private const string
        argEdgeDirected = "directed",
        argEdgeUndirected = "undirected";

    private void Update() {
        ShowMap.interactable = inputFileGraph.text?.Length > 0;
        ShowPath.interactable = graph != null
            && inputSimpulAwal.text?.Length > 0 && inputSimpulTujuan.text?.Length > 0
            && nodes.ContainsKey(inputSimpulAwal.text) && nodes.ContainsKey(inputSimpulTujuan.text);
    }

    void ReadFile(string fileName) {
        StreamReader reader = null;
        int lineNum = 1;

        static string errMsgHead(int i) => $"Invalid file format, error at line {i}";

        try {
            reader = new StreamReader(fileName);
            graph = new DirectedGraph<MapNode, double>();
            nodes.Clear();
            inputOptions.Clear();

            // Handle file read here
            while(!reader.EndOfStream) {
                string lineStr = reader.ReadLine().Trim();
                if(string.IsNullOrEmpty(lineStr) || lineStr.StartsWith('#')) continue;
                string[] columns = lineStr.Split(',');

                switch(columns[0]) {
                    case "O":
                        // Read columns as options with the format key=value
                        for(int i = 1; i < columns.Length; i++) {
                            string[] option = columns[i].Split('=');
                            if(option.Length == 2 && !string.IsNullOrEmpty(option[0]) && !string.IsNullOrEmpty(option[1])) {
                                inputOptions.Add(option[0], option[1]);
                            } else {
                                throw new InvalidDataException($"{errMsgHead(lineNum)}: Invalid option syntax '{columns[i]}'.");
                            }
                        }

                        break;
                    case "N":
                        if(!inputOptions.ContainsKey(optNodeFormat)) throw new InvalidDataException($"{errMsgHead(lineNum)}: Missing option '{optNodeFormat}' needed to parse node data.");

                        // Read node data in determined format
                        MapNode node = null;
                        string nodeFmt = inputOptions[optNodeFormat];
                        double lat = 0d, lon = 0d;
                        string nodeName = null;
                        switch(nodeFmt) {
                            case optNodeFormatXY:
                            case optNodeFormatLatLon:
                                lat = double.Parse(columns[1]);
                                lon = double.Parse(columns[2]);
                                nodeName = columns[3];
                                node = new MapNode(nodeName, lat, lon);
                                break;
                            case optNodeFormatYX:
                            case optNodeFormatLonLat:
                                lat = double.Parse(columns[2]);
                                lon = double.Parse(columns[1]);
                                nodeName = columns[3];
                                node = new MapNode(nodeName, lat, lon);
                                break;
                            default:
                                throw new InvalidDataException($"{errMsgHead(lineNum)}: Invalid option '{optNodeFormat}={nodeFmt}' while trying to parse node data.");
                        }

                        nodes.Add(nodeName, node);
                        graph.AddNode(node);

                        break;
                    case "E":
                        if(!inputOptions.ContainsKey(optEdgeFormat))throw new InvalidDataException($"{errMsgHead(lineNum)}: Missing option '{optEdgeFormat}' needed to parse edge data.");

                        // Read edge data
                        string edgeFmt = inputOptions[optEdgeFormat];

                        switch(edgeFmt) {
                            case optEdgeFormatPair:
                                string from = columns[1];
                                string to = columns[2];
                                double cost = double.Parse(columns[3]);
                                string kind = columns[4];

                                switch(kind) {
                                    case argEdgeDirected:
                                        graph.AddEdge(nodes[from], nodes[to], cost);
                                        break;
                                    case argEdgeUndirected:
                                        graph.AddEdge(nodes[from], nodes[to], cost);
                                        graph.AddEdge(nodes[to], nodes[from], cost);
                                        break;
                                    default:
                                        // Invalid type
                                        throw new InvalidDataException($"{errMsgHead(lineNum)}: Invalid edge type '{kind}'.");
                                }
                                break;
                            case optEdgeFormatMatrix:
                                // TODO: Add
                                break;
                        }

                        break;
                    default:
                        // Invalid line
                        throw new InvalidDataException($"{errMsgHead(lineNum)}: Invalid line ID '{columns[0]}'.");
                }
                lineNum++;
            }

            StringBuilder str = new StringBuilder(" | ");
            foreach(KeyValuePair<string, MapNode> pair in nodes) {
                str.Append(pair.Key + " | ");
            }
            Debug.Log($"Successfully read {graph.NodeCount} nodes and {graph.EdgeCount} edges:\n{str}");
        } catch(IOException e) {
            // Handle file read error here
            Debug.LogError($"Failed to read file: {e}");
            graph = null;
            nodes.Clear();
        } catch(InvalidDataException e) {
            // Handle invalid file format here
            Debug.LogException(e);
            graph = null;
            nodes.Clear();
        } catch(Exception e) {
            Debug.LogError($"{errMsgHead(lineNum)}: {e.Message}");
            graph = null;
            nodes.Clear();
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
        GraphTraversalAlgorithm<MapNode>
            ucs = new GraphTraversalAlgorithm<MapNode>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = info => 0d
            },
            astar = new GraphTraversalAlgorithm<MapNode>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = info => info.ExpandEdge.To.DistanceGreatCircle(info.End)
            };

        MapNode[]
            ucsPath = ucs.FindPath(nodes[startNode], nodes[endNode]),
            astarPath = astar.FindPath(nodes[startNode], nodes[endNode]);

        Debug.Log(ucsPath != null ? $"Found path with UCS: {string.Join<MapNode>(" -> ", ucsPath)}" : "Found no path with UCS.");
        Debug.Log(astarPath != null ? $"Found path with A*: {string.Join<MapNode>(" -> ", astarPath)}" : "Found no path with A*.");
    }
}

class MapNode {
    public static double EarthRadius => 6378137d;

    public string Name { get; private set; }
    public (double x, double y) Coordinate { get; private set; }
    public MapNode(string name, double latitude, double longitude) {
        Name = name;
        Coordinate = (latitude, longitude);
    }

    public double DistanceEuclidean(MapNode other) {
        double dx = other.Coordinate.x - Coordinate.x;
        double dy = other.Coordinate.y - Coordinate.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double DistanceGreatCircle(MapNode other) {
        const double Deg2Rad = Math.PI / 180d;
        double dLat = (other.Coordinate.x - Coordinate.x) * Deg2Rad;
        double dLon = (other.Coordinate.y - Coordinate.y) * Deg2Rad;
        double sinLat = Math.Sin(dLat / 2d);
        double sinLon = Math.Sin(dLon / 2d);
        return 2d * EarthRadius * Math.Asin(Math.Sqrt(sinLat * sinLat + Math.Cos(Coordinate.x * Deg2Rad) * Math.Cos(other.Coordinate.y * Deg2Rad) * sinLon * sinLon));
    }

    public override bool Equals(object obj) {
        return obj is MapNode node && Name.Equals(node.Name);
    }

    public override int GetHashCode() {
        return Name.GetHashCode();
    }

    public override string ToString() {
        return Name;
    }
}
