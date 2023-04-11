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

        
    }


    void onShowMap()
    {
        string fileName = "../test.txt";
        ReadFile(fileName);
    }

    void onShowPath()
    {
        int startNode = int.Parse(inputSimpulAwal.text);
        int endNode = int.Parse(inputSimpulTujuan.text);

        if (DropdownAlgorithm.value == 0)
        {
            
        }
        else if (DropdownAlgorithm.value == 1)
        {
           
        }
    }   
}

class MapNode
{
    public string Name { get; private set; }

}
