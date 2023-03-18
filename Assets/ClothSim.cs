using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;


public class ClothSim : MonoBehaviour
{
    // Start is called before the first frame update
    public MeshFilter m_meshfilter;
    private List<ClothVertex> clothVertices;


    void Start()
    {
        m_meshfilter = this.GetComponent<MeshFilter>();
        clothVertices = new List<ClothVertex> ();
        Mesh m_mesh = m_meshfilter.sharedMesh;
        for(int i=0; i< m_mesh.vertices.Length; i++) {
            //Debug.Log(i);
            clothVertices.Add(new ClothVertex(i, m_mesh));
        }
        //Debug.Log(clothVertices.Count);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



class Links
{
    public List<int> connections = new List<int>();
}

