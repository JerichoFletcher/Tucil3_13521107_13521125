using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FileManager : MonoBehaviour
{
    public Button ShowMap;
    public Button ShowPath;
    public Dropdown DropdownAlgorithm;
    public InputField inputSimpulAwal;
    public InputField inputSimpulTujuan;
    public InputField inputFileGraph;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    private int numNodes;
    private List<GameObject> nodeObjects = new List<GameObject>();
    private List<GameObject> edgeObjects = new List<GameObject>();

    void Start()
    {
        ShowMap.onClick.AddListener(onShowMap);
        ShowPath.onClick.AddListener(onShowPath);
    }

    void ReadFile(string fileName)
    {
        StreamReader reader = new StreamReader(fileName);

        numNodes = int.Parse(reader.ReadLine());

        // tambahkan semua edge ke dalam graph
        for (int i = 0; i < numNodes; i++) {
            string[] line = reader.ReadLine().Split(' ');

            for (int j = 0; j < numNodes; j++){
                if (int.Parse(line[j]) == 1)
                {

                    // tambahkan GameObject untuk edge
                    GameObject edgeObject = Instantiate(edgePrefab, Vector3.zero, Quaternion.identity);
                    LineRenderer lineRenderer = edgeObject.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, nodeObjects[i].transform.position);
                    lineRenderer.SetPosition(1, nodeObjects[j].transform.position);

                    edgeObjects.Add(edgeObject);
                }
            }
        }

        reader.Close();
    }


    void onShowMap()
    {
        string fileName = "test.txt";
        ReadFile(fileName);
    }

    void onShowPath()
    {
        int startNode = int.Parse(inputSimpulAwal.text);
        int endNode = int.Parse(inputSimpulTujuan.text);

        if (DropdownAlgorithm.value == 0)
        {
            // UCS
        }
        else if (DropdownAlgorithm.value == 1)
        {
            // kode untuk menampilkan lintasan terpendek menggunakan A*
            // ...
        }
    }
}
