using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class CustomDelaunayTerrain : DelaunayTerrain
{
    [Header ("커스텀")]

    //public Vector3 gameObject.transform.position;
    //public int indexX = 0;
    //public int indexY = 0;

    [Space (10)]
    public int terrainLOD = 0;
    
    [Space (10)]
    public CustomDelaunayTerrain nearTerrainHolder_u = null;
    public CustomDelaunayTerrain nearTerrainHolder_l = null;
    public CustomDelaunayTerrain nearTerrainHolder_r = null;
    public CustomDelaunayTerrain nearTerrainHolder_d = null;

    bool bridgeExist = false;
    bool haveLake = false;
    private void Awake ()
    {
        // 생성에 필요한 수치들을 세팅후 Generate
        SetSize ();
    }

    void Start ()
    {

    }



    public void SetSize ()
    {
        xsize = EnvironmentManager.Inst.GetTerrainUnitSize_Original();
        ysize = EnvironmentManager.Inst.GetTerrainUnitSize_Original();
    }


    public bool meetLeft = false;
    public bool meetRight = false;
    public bool meetDown = false;
    public bool meetup = false;

    //public Dictionary<Vertex, float> myDownElevation = new Dictionary<Vertex, float>();

    public Dictionary<double, float> nearRightEdgeInfo = new Dictionary<double, float>();
    public Dictionary<double, float> nearLeftEdgeInfo = new Dictionary<double, float>();

    public Dictionary<double, float> nearUpEdgeInfo = new Dictionary<double, float>();
    public Dictionary<double, float> nearDownEdgeInfo = new Dictionary<double, float>();


    public override void Generate() 
    {
        
        UnityEngine.Random.InitState(Random.Range(0,50));

        elevations = new List<float>();

        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++) {
            seed[i] = Random.Range(0.0f, 100.0f);
        }
        
        PoissonDiscSampler sampler = new PoissonDiscSampler(xsize, ysize, minPointRadius);

        Polygon polygon = new Polygon();


        polygon.Add(new Vertex(xsize, ysize));


        // Add some randomly sampled points
        float xRange = Random.Range(8.0f, xsize-8.0f);
        float yRange = Random.Range(8.0f, ysize - 8.0f);

        //edge별로 설졍 1

        if (meetRight)
        {
            foreach(double bindY in nearRightEdgeInfo.Keys)
            {
                polygon.Add(new Vertex(xsize, bindY));
            }
            yRange = Random.Range(8.0f, ysize - 8f);
        }
        if (meetLeft)
        {
            foreach(double bindY in nearLeftEdgeInfo.Keys)
            {
                polygon.Add(new Vertex(0, bindY));
            }
            yRange = Random.Range(8.0f, ysize - 8.0f);
        }


        if (meetup)
        {
            foreach (double bindX in nearUpEdgeInfo.Keys)
            {
                polygon.Add(new Vertex(bindX, ysize));

            }
            xRange = Random.Range(8.0f, ysize - 8f);
            
        }

        if (meetDown)
        {
            foreach (double bindX in nearDownEdgeInfo.Keys)
            {

                polygon.Add(new Vertex(bindX, 0.0f));
            }
            yRange = Random.Range(8.0f, ysize - 8.0f);

        }

        polygon.Add(new Vertex(xsize, 0));


        // Add uniformly-spaced points
        foreach (Vector2 sample in sampler.Samples())
        {
            polygon.Add(new Vertex((double)sample.x, (double)sample.y));

        }
        polygon.Add(new Vertex(0, ysize));
        
        for (int i = 0; i < 5; i++)
        {
            polygon.Add(new Vertex(xRange,yRange));
        }
        polygon.Add(new Vertex(0, 0));



        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        


        bin = new TriangleBin(mesh, xsize, ysize, minPointRadius * 2.0f);

        int x = xsize;
        int y = ysize ;
        //float r = 2f;

        float lowY = -0.5f;

        // Sample perlin noise to get elevations
        foreach (Vertex vert in mesh.Vertices) {
            float elevation = 0.0f;
            float amplitude = Mathf.Pow(persistence, octaves);
            float frequency = 1.0f;
            float maxVal = 0.0f;

            for (int o = 0; o < octaves; o++) 
            {
                float sample = (Mathf.PerlinNoise(seed[o] + (float)vert.x * sampleSize / (float)xsize * frequency,
                                                 seed[o] + (float)vert.y * sampleSize / (float)ysize * frequency) - 0.5f) * amplitude;

                elevation += sample;
                maxVal += amplitude;
                amplitude /= persistence;
                frequency *= frequencyBase;

                elevation += sample;
                maxVal += amplitude;
                amplitude /= persistence;
                frequency *= frequencyBase;
            }


            elevation = elevation / maxVal * elevationScale;

            //edge별로 설졍 2
            if (meetRight)
            {
                if (nearRightEdgeInfo.ContainsKey(vert.y))
                {
                    elevation = nearRightEdgeInfo[vert.y];
                }

            }
            if (meetLeft)
            {
                if (nearLeftEdgeInfo.ContainsKey(vert.y))
                {
                    elevation = nearLeftEdgeInfo[vert.y];
                }
            }

            if (meetup)
            {
                if (nearUpEdgeInfo.ContainsKey(vert.x))
                {
                    elevation = nearUpEdgeInfo[vert.x];
                }
            }

            if (meetDown)
            {
                if (nearDownEdgeInfo.ContainsKey(vert.x))
                {
                    elevation = nearDownEdgeInfo[vert.x];
                }
            }

            elevations.Add(elevation);

        }

        nearRightEdgeInfo.Clear();
        nearLeftEdgeInfo.Clear();
        nearUpEdgeInfo.Clear();
        nearDownEdgeInfo.Clear();
        MakeMesh();
        ScatterDetailMeshes();
    }


    public void GenRightNearEdge()
    {//nearEdgeInfo
        nearRightEdgeInfo.Clear();
        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.x <= 0.0f)
            {
                if (nearRightEdgeInfo.ContainsKey(vert.y))
                { 
                    nearRightEdgeInfo.Add(vert.y, elevations[vert.id]);
                }
            }
        }

    }

    public void GenLeftNearEdge()
    {//nearEdgeInfo
        nearLeftEdgeInfo.Clear();

        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.x >= xsize)
            {
                if (nearLeftEdgeInfo.ContainsKey(vert.y))  nearLeftEdgeInfo.Add(vert.y, elevations[vert.id]);
            }
        }

    }

    public void GenUpNearEdge()
    {//nearEdgeInfo

        nearDownEdgeInfo.Clear();

        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.y <= 0.0f)
            {
                if (nearDownEdgeInfo.ContainsKey(vert.x))  nearDownEdgeInfo.Add(vert.x, elevations[vert.id]);
            }

        }

    }

    public void GenDownNearEdge()
    {//nearEdgeInfo
        nearUpEdgeInfo.Clear();
        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.y >= ysize)
            {
                if (nearUpEdgeInfo.ContainsKey(vert.x))
                    nearUpEdgeInfo.Add(vert.x, elevations[vert.id]);
            }
        }

    }


    public override void MakeMesh()
    {
        IEnumerator<Triangle> triangleEnumerator = mesh.Triangles.GetEnumerator();

        for (int chunkStart = 0; chunkStart < mesh.Triangles.Count; chunkStart += trianglesInChunk)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            int chunkEnd = chunkStart + trianglesInChunk;
            for (int i = chunkStart; i < chunkEnd; i++)
            {
                if (!triangleEnumerator.MoveNext())
                {
                    break;
                }

                Triangle triangle = triangleEnumerator.Current;

                // For the triangles to be right-side up, they need
                // to be wound in the opposite direction
                // triangle.vertices != vertices
                Vector3 v0 = GetPoint3D(triangle.vertices[2].id);
                Vector3 v1 = GetPoint3D(triangle.vertices[1].id);
                Vector3 v2 = GetPoint3D(triangle.vertices[0].id);

                List<Vector3> _vertices = new List<Vector3>();
                List<Vector3> _normals = new List<Vector3>();
                List<Vector2> _uvs = new List<Vector2>();
                List<int> _triangles = new List<int>();

                MakeMeshLOD(v0, v1, v2
                , ref _triangles, ref _vertices, ref _normals, ref _uvs
                , vertices.Count, terrainLOD);

                for (int j = 0; j < _triangles.Count; j++)
                {
                    triangles.Add(_triangles[j]);
                    vertices.Add(_vertices[j]);
                    normals.Add(_normals[j]);
                    uvs.Add(_uvs[j]);
                }
            }

            Mesh chunkMesh = new Mesh();
            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.normals = normals.ToArray();

            Transform chunk = Instantiate<Transform>(chunkPrefab, transform.position, transform.rotation);
            chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
            chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            chunk.transform.parent = transform;
        }
    }

    void MakeMeshLOD (Vector3 v0, Vector3 v1, Vector3 v2
    , ref List<int> triangles_ref, ref List<Vector3> vertices_ref, ref List<Vector3> normals_ref, ref List<Vector2> uvs_ref
    , int verticesNum, int LOD)
    {
        if (LOD == 0)
        {
            // 버텍스, 노멀, uv를 가져오기 위한 인덱스 값
            triangles_ref.Add(verticesNum + vertices_ref.Count);
            triangles_ref.Add(verticesNum + vertices_ref.Count + 1);
            triangles_ref.Add(verticesNum + vertices_ref.Count + 2);

            vertices_ref.Add(v0);
            vertices_ref.Add(v1);
            vertices_ref.Add(v2);

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
            normals_ref.Add(normal);
            normals_ref.Add(normal);
            normals_ref.Add(normal);

            // 이것땜시 uv가 제대로 작동 안한거임,,,
            uvs_ref.Add(new Vector2(0.0f, 0.0f));
            uvs_ref.Add(new Vector2(0.0f, 0.0f));
            uvs_ref.Add(new Vector2(0.0f, 0.0f));
        }
        // 메쉬를 쪼개기 위한 재귀함수
        else
        {
            Vector3 v01 = (v0 + v1) / 2;
            Vector3 v12 = (v1 + v2) / 2;
            Vector3 v20 = (v2 + v0) / 2;

            MakeMeshLOD (v12, v2, v20
            , ref triangles_ref, ref vertices_ref, ref normals_ref, ref uvs_ref
            , verticesNum, LOD-1);

            MakeMeshLOD (v01, v20, v0
            , ref triangles_ref, ref vertices_ref, ref normals_ref, ref uvs_ref
            , verticesNum, LOD-1);

            MakeMeshLOD (v20, v01, v12
            , ref triangles_ref, ref vertices_ref, ref normals_ref, ref uvs_ref
            , verticesNum, LOD-1);

            MakeMeshLOD (v1, v12, v01
            , ref triangles_ref, ref vertices_ref, ref normals_ref, ref uvs_ref
            , verticesNum, LOD-1);
        }
    }

    public void UpdateNearTerrain ()
    {
        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvironmentManager.Inst.GetTerrainHolderByPosition (gameObject.transform.position, NearTerrainDir.UP);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvironmentManager.Inst.GetTerrainHolderByPosition (gameObject.transform.position, NearTerrainDir.LEFT);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvironmentManager.Inst.GetTerrainHolderByPosition (gameObject.transform.position, NearTerrainDir.RIGHT);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvironmentManager.Inst.GetTerrainHolderByPosition (gameObject.transform.position, NearTerrainDir.DOWN);
        }
    }

    public void GenerateNearTerrain (int level)
    {
        UpdateNearTerrain ();
        
        if (level <= 0)
            return;

        // =======================================================

        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.UP);
            nearTerrainHolder_u.GenerateNearTerrain (level-1);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.LEFT);
            nearTerrainHolder_l.GenerateNearTerrain (level-1);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.RIGHT);
            nearTerrainHolder_r.GenerateNearTerrain (level-1);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.DOWN);
            nearTerrainHolder_d.GenerateNearTerrain (level-1);
        }
    }

    public void OnDrawGizmos() {
        if (mesh == null) {
            // Probably in the editor
            return;
        }

        Gizmos.color = Color.red;

        foreach(Vertex vert in mesh.Vertices)
        {
            Gizmos.DrawSphere(new Vector3((float)vert.x + this.transform.position.x, 
                elevations[vert.id],
                 (float)vert.y + this.transform.position.z), 0.1f);
        }
        //foreach (Edge edge in mesh.Edges) {
        //    Vertex v0 = mesh.vertices[edge.P0];
        //    Vertex v1 = mesh.vertices[edge.P1];
        //    Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
        //    Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
        //    Gizmos.DrawLine(p0, p1);
        //}
    }
}
