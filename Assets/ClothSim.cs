using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSim : MonoBehaviour
{
    // Start is called before the first frame update
    public MeshFilter m_meshfilter;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class ClothVertex
{
    int idx;
    Vector3 pos;
    List<ClothVertex> neighborList;
    List<float> DistanceList;

    public ClothVertex(int index, Mesh mesh)
    {
        idx = index;
        pos = mesh.vertices[index];
        int[] connectedVertices = mesh.GetConnectedVertices(index);
        for (int i = 0; i < connectedVertices.Length; i++)
        {
            if (connectedVertices[i] != vertexIndex)
            {
                neighborIndices.Add(connectedVertices[i]);
            }
        }
    }
}

