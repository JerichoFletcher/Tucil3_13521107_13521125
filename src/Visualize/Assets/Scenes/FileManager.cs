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
    [Header("Events")]
    public Button ShowMap;
    public Button ShowPath;
    [Header("Text Fields")]
    public TMP_InputField inputSimpulAwal;
    public TMP_InputField inputSimpulTujuan;
    public TMP_InputField inputFileGraph;
    public TMP_Text outputJarakHasil;
    [Header("Draw Bounds")]
    public Boundary ucsDrawArea;
    public Boundary astarDrawArea;
    [Header("Draw Options")]
    public Color defaultEdgeColor;
    public Gradient pathEdgeColor;
    [Header("Prefabs")]
    public NodePrefab nodePrefab;
    public EdgePrefab edgePrefab;

    private DirectedGraph<MapNode, double> graph = null;
    private readonly Dictionary<string, GameObject> ucsNodeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> astNodeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> ucsEdgeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> astEdgeObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, MapNode> nodes = new Dictionary<string, MapNode>();
    private readonly List<string> nodeNames = new List<string>();
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

    private string EdgeHash(MapNode from, MapNode to) => $"{from.Name}-{to.Name}";

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
            nodeNames.Clear();
            inputOptions.Clear();

            // Handle file read here
            bool isReadingMatrix = false;
            int matrixRowsRead = 0;
            while(!reader.EndOfStream) {
                string lineStr = reader.ReadLine().Trim();
                if(string.IsNullOrEmpty(lineStr) || lineStr.StartsWith('#')) {
                    lineNum++;
                    continue;
                }
                string[] columns = lineStr.Split(',');

                switch(columns[0]) {
                    case "O":
                        if(isReadingMatrix) throw new InvalidDataException($"{errMsgHead(lineNum)}: Interrupted matrix reading.");

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
                        if(isReadingMatrix) throw new InvalidDataException($"{errMsgHead(lineNum)}: Interrupted matrix reading.");
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
                        nodeNames.Add(nodeName);
                        graph.AddNode(node);

                        break;
                    case "E":
                        if(!inputOptions.ContainsKey(optEdgeFormat)) throw new InvalidDataException($"{errMsgHead(lineNum)}: Missing option '{optEdgeFormat}' needed to parse edge data.");

                        // Read edge data in determined format
                        string edgeFmt = inputOptions[optEdgeFormat];

                        double cost = 0d;
                        switch(edgeFmt) {
                            case optEdgeFormatPair:
                                if(isReadingMatrix) throw new InvalidDataException($"{errMsgHead(lineNum)}: Interrupted matrix reading.");

                                string from = columns[1];
                                string to = columns[2];
                                cost = double.Parse(columns[3]);
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
                                if(isReadingMatrix && matrixRowsRead >= nodes.Count) throw new InvalidDataException($"{errMsgHead(lineNum)}: Wrong number of matrix rows (expected {nodes.Count}).");

                                if(!isReadingMatrix) isReadingMatrix = true;
                                string[] matrixCols = columns[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if(matrixCols.Length == nodes.Count) {
                                    for(int i = 0; i < matrixCols.Length; i++) {
                                        cost = double.Parse(matrixCols[i]);
                                        if(cost > 0d) graph.AddEdge(nodes[nodeNames[matrixRowsRead]], nodes[nodeNames[i]], cost);
                                    }
                                } else {
                                    throw new InvalidDataException($"{errMsgHead(lineNum)}: Wrong number of matrix columns (expected {nodes.Count}).");
                                }

                                matrixRowsRead++;
                                break;
                        }

                        break;
                    default:
                        // Invalid line
                        throw new InvalidDataException($"{errMsgHead(lineNum)}: Invalid line ID '{columns[0]}'.");
                }
                lineNum++;
            }

            outputJarakHasil.text = "Berhasil membuka file!";
            outputJarakHasil.color = Color.green;

            Visualize();

            StringBuilder str = new StringBuilder(" | ");
            foreach(KeyValuePair<string, MapNode> pair in nodes) {
                str.Append(pair.Key + " | ");
            }
            Debug.Log($"Successfully read {graph.NodeCount} nodes and {graph.EdgeCount} edges:\n{str}");
        } catch(IOException e) {
            // Handle file read error here
            Debug.LogError($"Failed to read file: {e}");
            outputJarakHasil.text = "Gagal membaca file!";
            outputJarakHasil.color = Color.red;
            graph = null;
            ClearVisualization();
        } catch(InvalidDataException e) {
            // Handle invalid file format here
            Debug.LogException(e);
            outputJarakHasil.text = "File tidak valid!";
            outputJarakHasil.color = Color.red;
            graph = null;
            ClearVisualization();
        } catch(Exception e) {
            Debug.LogError($"{errMsgHead(lineNum)}: {e.Message}\n{e.StackTrace}");
            outputJarakHasil.text = "Terjadi kesalahan!";
            outputJarakHasil.color = Color.red;
            graph = null;
            ClearVisualization();
        } finally {
            reader?.Close();
        }
    }

    private void Visualize() {
        ClearVisualization();
        static GameObject CreateNode(NodePrefab nodePrefab, MapNode node, Boundary bound, Rect range, bool flipped, string name) {
            GameObject nodeObj = Instantiate(nodePrefab.gameObject);
            NodePrefab nodePref = nodeObj.GetComponent<NodePrefab>();
            nodePref.Bound = bound.ToRect;
            nodePref.Range = range;
            if(flipped) {
                nodePref.Set(node.Name, (float)node.Coordinate.y, (float)node.Coordinate.x);
            } else {
                nodePref.Set(node.Name, (float)node.Coordinate.x, (float)node.Coordinate.y);
            }

            nodeObj.name = $"NodeObj{name}:{node.Name}";
            return nodeObj;
        }
        static GameObject CreateEdge(EdgePrefab edgePrefab, GraphEdge<MapNode, double> edge, Boundary bound, Rect range, bool flipped, string name) {
            GameObject edgeObj = Instantiate(edgePrefab.gameObject);
            EdgePrefab edgePref = edgeObj.GetComponent<EdgePrefab>();
            edgePref.Bound = bound.ToRect;
            edgePref.Range = range;
            if(flipped) {
                edgePref.SetPosition((float)edge.From.Coordinate.y, (float)edge.From.Coordinate.x, (float)edge.To.Coordinate.y, (float)edge.To.Coordinate.x);
            } else {
                edgePref.SetPosition((float)edge.From.Coordinate.x, (float)edge.From.Coordinate.y, (float)edge.To.Coordinate.x, (float)edge.To.Coordinate.y);
            }

            edgeObj.name = $"EdgeObj{name}:{edge.From}->{edge.To}";
            return edgeObj;
        }

        // Calculate range
        (double x, double y) min = (double.MaxValue, double.MaxValue), max = (double.MinValue, double.MinValue);
        bool flipped = inputOptions[optNodeFormat].Equals(optNodeFormatLonLat) || inputOptions[optNodeFormat].Equals(optNodeFormatYX);
        foreach(MapNode node in graph.Nodes()) {
            if(flipped) {
                if(min.x > node.Coordinate.y) min.x = node.Coordinate.y;
                if(min.y > node.Coordinate.x) min.y = node.Coordinate.x;
                if(max.x < node.Coordinate.y) max.x = node.Coordinate.y;
                if(max.y < node.Coordinate.x) max.y = node.Coordinate.x;
            } else {
                if(min.x > node.Coordinate.x) min.x = node.Coordinate.x;
                if(min.y > node.Coordinate.y) min.y = node.Coordinate.y;
                if(max.x < node.Coordinate.x) max.x = node.Coordinate.x;
                if(max.y < node.Coordinate.y) max.y = node.Coordinate.y;
            }
        }
        Rect range = new Rect((float)min.x, (float)min.y, (float)(max.x - min.x), (float)(max.y - min.y));

        // Instantiate objects
        foreach(MapNode node in graph.Nodes()) {
            GameObject ucsNodeObj = CreateNode(nodePrefab, node, ucsDrawArea, range, flipped, "UCS");
            GameObject astNodeObj = CreateNode(nodePrefab, node, astarDrawArea, range, flipped, "Astar");
            ucsNodeObjects.Add(node.Name, ucsNodeObj);
            astNodeObjects.Add(node.Name, astNodeObj);

            foreach(GraphEdge<MapNode, double> edge in graph.OutEdges(node)) {
                if(!ucsEdgeObjects.ContainsKey(EdgeHash(edge.From, edge.To)) && !ucsEdgeObjects.ContainsKey(EdgeHash(edge.To, edge.From))) {
                    GameObject ucsEdgeObj = CreateEdge(edgePrefab, edge, ucsDrawArea, range, flipped, "UCS");
                    //Debug.Log($"UCSEDGE: {ucsEdgeObj}");
                    ucsEdgeObjects.Add(EdgeHash(edge.From, edge.To), ucsEdgeObj);
                }
                if(!astEdgeObjects.ContainsKey(EdgeHash(edge.From, edge.To)) && !astEdgeObjects.ContainsKey(EdgeHash(edge.To, edge.From))) {
                    GameObject astEdgeObj = CreateEdge(edgePrefab, edge, astarDrawArea, range, flipped, "Astar");
                    //Debug.Log($"ASTEDGE: {astEdgeObj}");
                    astEdgeObjects.Add(EdgeHash(edge.From, edge.To), astEdgeObj);
                }
            }
        }

        //Debug.Log($"UCSEDGES: {ucsEdgeObjects.Count}, ASTEDGES: {astEdgeObjects.Count}");
        ResetEdgeColors();
        Debug.Log($"Instantiated {ucsNodeObjects.Count + astNodeObjects.Count} node and {ucsEdgeObjects.Count + astEdgeObjects.Count} edge objects.");
    }

    public void OnShowMap() {
        string fileName = inputFileGraph.text;
        ReadFile(fileName);
    }

    public void OnShowPath() {
        string startNode = inputSimpulAwal.text;
        string endNode = inputSimpulTujuan.text;

        // Handle show path here
        bool greatCircle = inputOptions[optNodeFormat].Equals(optNodeFormatLatLon) || inputOptions[optNodeFormat].Equals(optNodeFormatLonLat);
        GraphTraversalAlgorithm<MapNode>
            ucs = new GraphTraversalAlgorithm<MapNode>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = info => 0d
            },
            astar = new GraphTraversalAlgorithm<MapNode>(graph, true) {
                GFunction = info => info.ExpandNode.GCost + info.ExpandEdge.Data,
                HFunction = greatCircle ? info => info.ExpandEdge.To.DistanceGreatCircle(info.End) : info => info.ExpandEdge.To.DistanceEuclidean(info.End)
            };

        MapNode[]
            ucsPath = ucs.FindPath(nodes[startNode], nodes[endNode]),
            astarPath = astar.FindPath(nodes[startNode], nodes[endNode]);
        double ucsPathCost = 0d, astarPathCost = 0d;
        if(ucsPath?.Length > 1) {
            for(int i = 0; i < ucsPath.Length - 1; i++) {
                graph.TryGetEdge(ucsPath[i], ucsPath[i + 1], out double cost);
                ucsPathCost += cost;
            }
        }
        if(astarPath?.Length > 1) {
            for(int i = 0; i < astarPath.Length - 1; i++) {
                graph.TryGetEdge(astarPath[i], astarPath[i + 1], out double cost);
                astarPathCost += cost;
            }
        }
        
        outputJarakHasil.text = $"UCS: {(ucsPath != null ? ucsPathCost : "Not found")}\nA*: {(astarPath != null ? astarPathCost : "Not found")}";
        outputJarakHasil.color = Color.black;

        ResetEdgeColors();
        if(ucsPath != null) ColorPath(ucsEdgeObjects, ucsPath);
        if(astarPath != null) ColorPath(astEdgeObjects, astarPath);

        Debug.Log(ucsPath != null ? $"Found path with UCS, cost {ucsPathCost}: {string.Join<MapNode>(" -> ", ucsPath)}" : "Found no path with UCS.");
        Debug.Log(astarPath != null ? $"Found path with A*, cost {astarPathCost}: {string.Join<MapNode>(" -> ", astarPath)}" : "Found no path with A*.");
    }

    private void ClearVisualization() {
        foreach(GameObject obj in ucsNodeObjects.Values) Destroy(obj);
        foreach(GameObject obj in astNodeObjects.Values) Destroy(obj);
        foreach(GameObject obj in ucsEdgeObjects.Values) Destroy(obj);
        foreach(GameObject obj in astEdgeObjects.Values) Destroy(obj);
        ucsNodeObjects.Clear();
        astNodeObjects.Clear();
        ucsEdgeObjects.Clear();
        astEdgeObjects.Clear();
    }

    private void ResetEdgeColors() {
        foreach(GameObject edgeObj in ucsEdgeObjects.Values) {
            LineRenderer lineRender = edgeObj.GetComponent<EdgePrefab>().line;
            lineRender.startColor = defaultEdgeColor;
            lineRender.endColor = defaultEdgeColor;
        }
        foreach(GameObject edgeObj in astEdgeObjects.Values) {
            LineRenderer lineRender = edgeObj.GetComponent<EdgePrefab>().line;
            lineRender.startColor = defaultEdgeColor;
            lineRender.endColor = defaultEdgeColor;
        }
    }

    private void ColorPath(Dictionary<string, GameObject> edgeObjects, MapNode[] path) {
        if(path.Length <= 1) return;
        for(int i = 0; i < path.Length - 1; i++) {
            GameObject lineObj = null;
            if(edgeObjects.TryGetValue(EdgeHash(path[i], path[i + 1]), out lineObj) || edgeObjects.TryGetValue(EdgeHash(path[i + 1], path[i]), out lineObj)) {
                LineRenderer lineRender = lineObj.GetComponent<EdgePrefab>().line;
                Color
                    c1 = pathEdgeColor.Evaluate((float)i / path.GetUpperBound(0)),
                    c2 = pathEdgeColor.Evaluate((float)(i + 1) / path.GetUpperBound(0));
                lineRender.startColor = c1;
                lineRender.endColor = c2;
            } else {
                Debug.LogError($"Missing edge object {path[i]} -> {path[i + 1]}");
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Rect
            ucsRect = ucsDrawArea.ToRect,
            astRect = astarDrawArea.ToRect;
        Gizmos.DrawWireCube(ucsRect.center, ucsRect.size);
        Gizmos.DrawWireCube(astRect.center, astRect.size);
    }

    [Serializable]
    public class Boundary {
        public Transform bottomLeftCorner, topRightCorner;
        public Rect ToRect => new Rect(bottomLeftCorner.position, (topRightCorner.position - bottomLeftCorner.position));
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
