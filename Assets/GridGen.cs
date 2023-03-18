using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class GridGen : MonoBehaviour
{
    public int rows = 10;
    public int columns = 10;
    public float cellSize = 1.0f;
    public float ks = 0.5f;
    public float kd = 0.7f;
    public Material material;
    public List<int> pinVtxs;
    public float particleMass = 0.1f;
    private ClothMesh clothMesh;



    void Start() {
        Mesh mesh = new Mesh();
        clothMesh = new ClothMesh(rows, columns, ks, kd);
        clothMesh.pinList = pinVtxs;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[(rows + 1) * (columns + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rows * columns * 6];

        for (int i = 0, y = 0; y <= rows; y++) {
            for (int x = 0; x <= columns; x++, i++) {
                vertices[i] = new Vector3(x * cellSize, 0 , y * cellSize);
                uv[i] = new Vector2((float)x / columns, (float)y / rows);

                //cloth config
                ClothVertex nvtx = new ClothVertex(i, new Vector2(x, y), vertices[i], particleMass);
                //Debug.Log("i is " + i + "coord is  " + new Vector2(x, y));
                clothMesh.vtxList.Add(nvtx);

            }
        }

       clothMesh.InitializeNeighbor();

        for (int ti = 0, vi = 0, y = 0; y < rows; y++, vi++) {  
            for (int x = 0; x < columns; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + columns + 1;
                triangles[ti + 5] = vi + columns + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


    
    private void FixedUpdate() {
        ApplyForce();
        Advect();
        ShowOnMesh();
    }
    

    void ShowOnMesh() {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[(rows + 1) * (columns + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rows * columns * 6];

        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = clothMesh.vtxList[i].pos;
        }

        for (int ti = 0, vi = 0, y = 0; y < rows; y++, vi++) {
            for (int x = 0; x < columns; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + columns + 1;
                triangles[ti + 5] = vi + columns + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    void ApplyForce() {
        clothMesh.ApplyForce();
    }
    void Advect() {
        clothMesh.AdvectAndUpdate(Time.fixedDeltaTime);
    }
}




//-----------------------------------------------------------------------------------------------------------------------------------------------Cloth Mesh--------------------------------------------------------------------------------------------------------------------------------------------------------------


public class ClothMesh
{
    public List<ClothVertex> vtxList;
    public int rows;
    public int cols;
    public float ks;
    public float kd;
    public List<int> pinList;
    public ClothMesh(int rows, int cols, float ks, float kd) {
        this.ks = ks;
        this.kd = kd;
        this.rows = rows;
        this.cols = cols;
        this.vtxList = new List<ClothVertex>();
        this.pinList = new List<int>();
    }


    public void ApplyForce() {
        foreach(ClothVertex vtx in this.vtxList) {
            vtx.AccumulateForce();
        }
    }

    public void AdvectAndUpdate(float deltaTime) {
        foreach (ClothVertex vtx in this.vtxList) {
            if(!this.pinList.Contains(vtx.idx) ){
                vtx.vel += vtx.force/vtx.mass * deltaTime;
                vtx.pos = vtx.pos + vtx.vel * deltaTime;
            }
          
        }
    }
    
    private void setLink(ClothVertex vtx,  Vector2 neibor, Dictionary<int, ClothLink> dict) {
        if (FindVtx(neibor) >= 0) {
            int neiboridx = FindVtx(neibor);
            ClothVertex neiborvtx = this.vtxList[neiboridx];
            ClothLink link = new ClothLink(vtx, neiborvtx, this.ks, this.kd);
            dict[neiboridx] = link;
        }
    }
    public void InitializeNeighbor() {
        foreach(ClothVertex vtx in vtxList) {
            int x = (int)vtx.coord.x;
            int y = (int)vtx.coord.y;

            setLink(vtx, new Vector2(x + 1, y), vtx.stretchNeiDict);
            setLink(vtx, new Vector2(x - 1, y), vtx.stretchNeiDict);
            setLink(vtx, new Vector2(x, y + 1), vtx.stretchNeiDict);
            setLink(vtx, new Vector2(x, y-1), vtx.stretchNeiDict);

            //shear

            setLink(vtx, new Vector2(x + 1, y+1), vtx.shearNeiDict);
            setLink(vtx, new Vector2(x - 1, y-1), vtx.shearNeiDict);
            setLink(vtx, new Vector2(x-1, y + 1), vtx.shearNeiDict);
            setLink(vtx, new Vector2(x+1, y - 1), vtx.shearNeiDict);

            //bend
            setLink(vtx, new Vector2(x + 2, y), vtx.bendNeiDict);
            setLink(vtx, new Vector2(x - 2, y), vtx.bendNeiDict);
            setLink(vtx, new Vector2(x, y + 2), vtx.bendNeiDict);
            setLink(vtx, new Vector2(x, y - 2), vtx.bendNeiDict);
        }
    }


    private int FindVtx(Vector2 coord) {
        if ((coord.x >= 0) && (coord.x <= this.rows) && (coord.y >=0) &&(coord.y <= this.cols)) {
            return (int)coord.y * (rows+1) + (int)coord.x;
        }
        return -1;
    }
    private Vector3 FindVtxPos(Vector2 coord) {
        if ((coord.x >= 0) && (coord.x <=this.rows) && (coord.y >= 0) && (coord.y <= this.cols)) {
            return vtxList[(int)coord.y * (rows+1) + (int)coord.x].pos;
        }
        return new Vector3(-10000, -10000, -10000);
    }
}


//-----------------------------------------------------------------------------------------------------------------------------------------------Cloth Vertex--------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ClothVertex
{
    public int idx;
    public float mass;
    public Vector3 vel;
    public Vector3 force;
    public Vector2 coord;
    public Vector3 pos;
    public Dictionary<int, ClothLink> neighborDict;
    public Dictionary<int, ClothLink> stretchNeiDict;
    public Dictionary<int, ClothLink> shearNeiDict;
    public Dictionary<int, ClothLink> bendNeiDict;

    public ClothVertex(int index, Mesh mesh) {
        this.idx = index;
        this.pos = mesh.vertices[index];
        this.neighborDict = new Dictionary<int, ClothLink>();

        FindNeighbor(mesh);
    }

    public ClothVertex(int index, Vector2 coord, Vector3 pos, float mass) {
        this.idx = index;
        this.coord = coord;
        this.pos = pos;
        this.vel = new Vector3(0, 0, 0);
        this.mass = mass;
        this.force = mass * new Vector3(0, -10, 0);
        this.neighborDict = new Dictionary<int, ClothLink>();
        this.stretchNeiDict = new Dictionary<int, ClothLink>();
        this.shearNeiDict = new Dictionary<int, ClothLink>();
        this.bendNeiDict = new Dictionary<int, ClothLink>();
    }


    public void AccumulateForce() {
        //Apply stretch
        Vector3 force_new = this.mass * new Vector3(0, -10, 0);
       foreach(var neibor in stretchNeiDict) { 
            Vector3 force_nei = CalculateForce(neibor.Value);
            force_new += force_nei;
        }
        foreach (var neibor in bendNeiDict) {
            Vector3 force_nei = CalculateForce(neibor.Value);
            force_new += force_nei;
        }
        foreach (var neibor in shearNeiDict) {
            Vector3 force_nei = CalculateForce(neibor.Value);
            force_new += force_nei;
        }
        //Debug.Log("vtx " + this.idx + "force is " + force_new);
        this.force = force_new;
    }

    public Vector3 CalculateForce(ClothLink link) {
        ClothVertex vtx_nei = link.vtx_1;
        Vector3 p1 = this.pos;
        Vector3 p2 = vtx_nei.pos;
        Vector3 v1 = this.vel;
        Vector3 v2 = vtx_nei.vel;
        float l0 = link.distance;
        float ks = link.ks;
        float kd = link.kd;
        Vector3 forceSpring = -ks * ((p2 - p1).magnitude - l0) * ((p1 - p2) / (p1 - p2).magnitude);
        Vector3 forceDamping =- ( kd * (Vector3.Dot((v1 - v2), (p1 - p2) )/ (p1 - p2).magnitude)) * ((p1 - p2) / (p1 - p2).magnitude);
        //Debug.Log("check links " + link.vtx_0.idx + " with " + link.vtx_1.idx + " force spring is " + forceSpring + " and damping is " + forceDamping);
        Vector3 force = forceSpring + forceDamping;
        return force;
    }

    private void FindNeighbor(Mesh mesh) {
        for (int i = 0; i < mesh.triangles.Length; i += 3) {
            List<int> triIndex = new List<int>();
            triIndex.Add(mesh.triangles[i]);
            triIndex.Add(mesh.triangles[i + 1]);
            triIndex.Add(mesh.triangles[i + 2]);

            if (triIndex.IndexOf(this.idx) >= 0) {
                foreach (int j in triIndex) {
                    if (j != idx) {
                        if (!neighborDict.ContainsKey(j)) {
                            //neighborDict[j] = mesh.vertices[j];
                        }
                    }
                }
            }

        }
    }



}
//-----------------------------------------------------------------------------------------------------------------------------------------------Cloth Link--------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ClothLink
{
    public float ks;
    public float kd;
    public ClothVertex vtx_0;
    public ClothVertex vtx_1;
    //original distance
    public float distance;

    public ClothLink(ClothVertex start_vtx, ClothVertex end_vtx, float ks, float kd ){
        this.ks = ks;
        this.kd = kd;
        this.vtx_0 = start_vtx;
        this.vtx_1 = end_vtx;
        this.distance = (start_vtx.pos -end_vtx.pos).magnitude;
    }
}