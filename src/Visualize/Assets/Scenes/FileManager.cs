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
            reader = new StreamReader(fileName);
            // Handle file read here
        } catch(Exception e) {
            // Handle error here
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
}
