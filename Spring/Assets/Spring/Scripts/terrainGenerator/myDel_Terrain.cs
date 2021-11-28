using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class myDel_Terrain : MonoBehaviour
{
    // Maximum size of the terrain.
    public int xsize = 50;
    public int ysize = 50;

    // Minimum distance the poisson-disc-sampled points are from each other.
    public float minPointRadius = 4.0f;

    // Number of random points to generate.
    public int randomPoints = 100;

    // Triangles in each chunk.
    public int trianglesInChunk = 20000;

    // Perlin noise parameters
    public float elevationScale = 100.0f;
    public float sampleSize = 1.0f;
    public int octaves = 8;
    public float frequencyBase = 2;
    public float persistence = 1.1f;

    public Transform detailMesh;
    public int detailMeshesToGenerate = 50;

    // Prefab which is generated for each chunk of the mesh.
    public Transform chunkPrefab = null;

    // Elevations at each point in the mesh
    private List<float> elevations;

    // Fast triangle querier for arbitrary points
    private TriangleBin bin;


    // The delaunay mesh
    private TriangleNet.Mesh mesh = null;
    //[Space(10)]
   // public GameObject Butterfly;
    //public GameObject[] bugsPrefabs;

    //[Space (10)]
    //public GameObject myPrefab_tree2;
    public GameObject water_plane;
    public GameObject bridge;

    [Space(10)]
    public myDel_Terrain nearTerrainHolder_u = null;
    public myDel_Terrain nearTerrainHolder_l = null;
    public myDel_Terrain nearTerrainHolder_r = null;
    public myDel_Terrain nearTerrainHolder_d = null;
    public List<Vertex> faildBindingEdge = new List<Vertex>();

    public bool hasMountain;
    public bool hasRiver = true;
    public bool hasCliff;


    public Dictionary<Vertex, GameObject> manageSpawnObject = new Dictionary<Vertex, GameObject>();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        //GameObject g = Instantiate(Butterfly, new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100)), Quaternion.identity);
    }

    private RaycastHit hit;
    public float updownRange = 3.0f;
    private void Update()
    {
        //if(Input.GetKey(KeyCode.A))
        //강
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.name == "ChunkPrefab(Clone)")
                    {
                        //Debug.Log(hit.point);
                        foreach (Vertex ver in mesh.Vertices)
                        {

                            float dist = Mathf.Sqrt(Mathf.Pow((hit.point.x - ((float)ver.x + transform.position.x)), 2)
                                    + Mathf.Pow((hit.point.z - ((float)ver.y + transform.position.z)), 2));

                            if (dist <= updownRange)
                            {
                                elevations[ver.id] -= 0.5f;

                                if (manageSpawnObject.ContainsKey(ver))
                                {
                                    GameObject g = manageSpawnObject[ver];
                                    g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y - 0.5f, g.transform.position.z);

                                }

                            }



                        }
                        for (int j = 0; j < this.gameObject.transform.childCount; j++)
                        {
                            if (transform.GetChild(j) != null)
                            {
                                if (transform.GetChild(j).transform.name == "ChunkPrefab(Clone)" || transform.GetChild(j).transform.name == ("RockDetail(Clone)"))
                                    Destroy(this.gameObject.transform.GetChild(j).gameObject);


                            }
                        }
                        MakeMesh();
                        ScatterDetailMeshes();
                        
                    }
                }
            }

        }

        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.name == "ChunkPrefab(Clone)")
                    {
                        //Debug.Log(hit.point);
                        foreach (Vertex ver in mesh.Vertices)
                        {

                            float dist = Mathf.Sqrt(Mathf.Pow((hit.point.x - ((float)ver.x + transform.position.x)), 2)
                                    + Mathf.Pow((hit.point.z - ((float)ver.y + transform.position.z)), 2));

                            if (dist <= updownRange)
                            {
                                elevations[ver.id] += 0.5f;

                                if (manageSpawnObject.ContainsKey(ver))
                                {
                                    GameObject g = manageSpawnObject[ver];
                                    g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y + 0.5f, g.transform.position.z);

                                }
                            }


                        }
                        for (int j = 0; j < this.gameObject.transform.childCount; j++)
                        {
                            if (transform.GetChild(j) != null)
                            {
                                if (transform.GetChild(j).transform.name == "ChunkPrefab(Clone)" || transform.GetChild(j).transform.name == ("RockDetail(Clone)"))
                                    Destroy(this.gameObject.transform.GetChild(j).gameObject);


                            }
                        }
                        MakeMesh();
                        ScatterDetailMeshes();

                    }
                }
            }

        }

    }

    public bool meetRight;
    public bool meetLeft;
    public bool meetUp;
    public bool meetDown;
    public virtual void Generate()
    {
        int rseed = Random.Range(0, 50);
        UnityEngine.Random.InitState(rseed);

        elevations = new List<float>();

        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++)
        {
            seed[i] = Random.Range(0.0f, 100.0f);
        }

        PoissonDiscSampler sampler = new PoissonDiscSampler(0, 0, xsize, ysize, minPointRadius);
        Polygon polygon = new Polygon();

        polygon.Add(new Vertex(xsize, ysize));
        polygon.Add(new Vertex(xsize, 0));
        polygon.Add(new Vertex(0, ysize));
        polygon.Add(new Vertex(0, 0));

        // Add uniformly-spaced points
        foreach (Vector2 sample in sampler.Samples())
        {
            polygon.Add(new Vertex((double)sample.x, (double)sample.y));
        }

        // Add some randomly sampled points
        for (int i = 0; i < randomPoints; i++)
        {

            polygon.Add(new Vertex(Random.Range(0.0f, xsize), Random.Range(0.0f, ysize)));

        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);


        bin = new TriangleBin(mesh, xsize, ysize, minPointRadius * 2.0f);


        //// Sample perlin noise to get elevations
        foreach (Vertex vert in mesh.Vertices)
        {
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
            }

            elevation = elevation / maxVal * elevationScale;
            elevations.Add(elevation);
        }

        GameObject water = Instantiate(water_plane,
            new Vector3(transform.position.x + 25,
            this.transform.position.y - 0.8f, transform.position.z + 25),
            Quaternion.identity);

        water.transform.SetParent(this.gameObject.transform);

        MakeMesh();

        ScatterDetailMeshes();
    }


    public List<Vertex> upVert = new List<Vertex>();
    public Dictionary<double, float> hashupElev = new Dictionary<double, float>();

    public void generateUp()
    {

        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.y >= xsize)
            {
                upVert.Add(vert);
                if (!hashupElev.ContainsKey(vert.x))
                {
                    hashupElev.Add(vert.x, elevations[vert.id]);
                }
            }
        }

    }


    public List<Vertex> downVert = new List<Vertex>();
    public Dictionary<double, float> hashDownElev = new Dictionary<double, float>();

    public void generateDown()
    {


        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.y <= 0)
            {
                downVert.Add(vert);
                if (!hashDownElev.ContainsKey(vert.x))
                {
                    hashDownElev.Add(vert.x, elevations[vert.id]);
                }
            }
        }
    }


    public List<Vertex> leftVert = new List<Vertex>();
    public Dictionary<double, float> hashleftElev = new Dictionary<double, float>();

    public void generateLeft()
    {

        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.x <= 0)
            {
                leftVert.Add(vert);
                if (!hashleftElev.ContainsKey(vert.y))
                {
                    hashleftElev.Add(vert.y, elevations[vert.id]);
                }
            }
        }

    }


    public List<Vertex> rightVert = new List<Vertex>();
    public Dictionary<double, float> hashrightElev = new Dictionary<double, float>();
    public void generateRight()
    {

        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.x >= xsize)
            {
                rightVert.Add(vert);
                if (!hashrightElev.ContainsKey(vert.y))
                {
                    hashrightElev.Add(vert.y, elevations[vert.id]);
                }
            }
        }

    }


    public List<Vertex> bindingRightVertex = new List<Vertex>();
    public Dictionary<double, float> bindingRightElev = new Dictionary<double, float>();

    public List<Vertex> bindingLeftVertex = new List<Vertex>();
    public Dictionary<double, float> bindingLeftElev = new Dictionary<double, float>();

    public List<Vertex> bindingUpVertex = new List<Vertex>();
    public Dictionary<double, float> bindingUpElev = new Dictionary<double, float>();

    public List<Vertex> bindingDownVertex = new List<Vertex>();
    public Dictionary<double, float> bindingDownElev = new Dictionary<double, float>();


    float minX = 0, minY = 0, maxX = 0, maxY = 0;

    float width;
    float depth;
    float startPoint;
    public virtual void GenerateForNear()
    {

        maxX = xsize;
        maxY = ysize;

        int rseed = Random.Range(0, 50);
        UnityEngine.Random.InitState(rseed);

        elevations = new List<float>();

        float[] seed = new float[octaves];

        for (int ii = 0; ii < octaves; ii++)
        {
            seed[ii] = Random.Range(0.0f, 100.0f);
        }

        if (meetDown) { minY += 3; }
        if (meetUp) { maxY -= 3; }
        if (meetLeft) { minX += 3; }
        if (meetRight){ maxX -= 3; }

        PoissonDiscSampler sampler = new PoissonDiscSampler(minX, minY, maxX, maxY, minPointRadius);
        Polygon polygon = new Polygon();

        polygon.Add(new Vertex(xsize, ysize));
        polygon.Add(new Vertex(xsize, 0));
        polygon.Add(new Vertex(0, ysize));
        polygon.Add(new Vertex(0, 0));

        if (meetRight)
        {
            foreach (Vertex ver in bindingRightVertex)
            {
                polygon.Add(new Vertex(xsize, ver.y));
            }
        }

        if (meetLeft)
        {
            foreach (Vertex ver in bindingLeftVertex)
            {
                polygon.Add(new Vertex(0, ver.y));
            }
        }

        if (meetUp)
        {
            foreach (Vertex ver in bindingUpVertex)
            {
                polygon.Add(new Vertex(ver.x, xsize));
            }
        }
        if (meetDown)
        {
            foreach (Vertex ver in bindingDownVertex)
            {
                polygon.Add(new Vertex(ver.x, 0));
            }
        }

        // Add some randomly sampled points
        for (int i = 0; i < randomPoints; i++)
        {
            polygon.Add(new Vertex(Random.Range(minX, maxX), Random.Range(minY, maxY)));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);


        bin = new TriangleBin(mesh, (int)maxX, (int)maxX, minPointRadius * 2.0f);

        bool makeWaterPlane = false;
        bool makeBridge = false;
        width = Random.Range(15.0f, 20.0f);
        depth = Random.Range(-4.7f, -13f);
        startPoint = Random.Range(0.0f + width, 50.0f - width);
        float minPoint = EnvManager.Inst.envSetting.boundryCoord_min.x;
        float maxPoint = EnvManager.Inst.envSetting.boundryCoord_max.y;
        //// Sample perlin noise to get elevations
        foreach (Vertex vert in mesh.Vertices)
        {
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
            }

            elevation = elevation / maxVal * elevationScale;


            ////절벽
            hasCliff = true;
            if (hasCliff)
            {

                //절벽
                if (transform.position.x + vert.x <= minPoint +50 )
                {
                    elevation -=4.5f;
                }
                if (transform.position.z <= minPoint + 50) elevation -= 4.5f;

            }

            //해안가쪽
            if (transform.position.x + vert.x <= minPoint+30)
            {
                float weight = (transform.position.x + (float)vert.x);
                elevation += weight / 75;

            }
            if (transform.position.z + vert.y <= minPoint + 30)
            {
                float weight = (transform.position.z + (float)vert.y);
                elevation += weight / 75;

            }

            //벽쪽
            if (transform.position.x + vert.x >=maxPoint + 20)
            {
                float weight = (transform.position.x + (float)vert.x);
                elevation += weight / Random.Range(65.0f, 75.0f);

            }
            if (transform.position.z + vert.y >= maxPoint + 20)
            {
                float weight = (transform.position.z + (float)vert.y);
                elevation += weight / Random.Range(65.0f, 75.0f);

            }

            if (hasMountain)// && this.transform.position.x >=0)
            {
                float Radius = Random.Range(10, 20);
                float RandomCenterPointX = Random.Range(Radius, 50 - Radius);
                float RandomCenterPointY = Random.Range(Radius, 50 - Radius);
                elevation += MakeMountainElevation(RandomCenterPointX + transform.position.x, RandomCenterPointY + transform.position.z, Radius, vert);
            }


            //hasRiver = true;
            ////// 강가 ***********
            //if (hasRiver)
            //{

            //    if(transform.position.x + vert.x >=RiverRange[0] && transform.position.x + vert.x <= RiverRange[1])
            //    {
            //        if(transform.position.z <= RiverRange[2])
            //            elevation -= 5.0f;
            //    }

            //}
            if (MakeRiver(vert))
            {
                elevation -= 5.0f;
            }

            //edge 연결*****************************

            if (vert.x >= xsize && meetRight)
            {
                if (bindingRightElev.ContainsKey(vert.y))
                {
                    elevation = (bindingRightElev[vert.y]);
                   
                }
                else
                {
                    faildBindingEdge.Add(vert);
                }

            }

            if (vert.x <= 0 && meetLeft)    // meet 관련 조건 추가
            {
                if (bindingLeftElev.ContainsKey(vert.y))
                {
                    elevation = (bindingLeftElev[vert.y]);

                    if (vert.x + transform.position.x <= -40)
                    {
                        elevation += 2.5f;
                    }
                    
                }
                else
                {
                    faildBindingEdge.Add(vert);
                }
            }

            if (vert.y >= ysize && meetUp)  // meet 관련 조건 추가
            {
                if (bindingUpElev.ContainsKey(vert.x))
                {
                    elevation = bindingUpElev[vert.x];
                }
                else
                {
                    faildBindingEdge.Add(vert);
                }

            }

            if (vert.y <= 0 && meetDown)    // meet 관련 조건 추가
            {
                if (bindingDownElev.ContainsKey(vert.x))
                {
                    elevation = bindingDownElev[vert.x];
                }
                else
                {
                    faildBindingEdge.Add(vert);
                }
            }

            elevations.Add(elevation);



            if (makeWaterPlane)
            {
                GameObject water = Instantiate(water_plane,
                new Vector3(transform.position.x + 25,
                elevation -1.5f, transform.position.z + 25),
                Quaternion.identity);

                water.transform.SetParent(this.gameObject.transform);

                makeWaterPlane = true;

            }

        }


        if (this.transform.position.z >= EnvManager.Inst.envSetting.boundryCoord_max.y)
        {
            GameObject cliff = Instantiate(EnvManager.Inst.cliffPrefab,
            new Vector3(transform.position.x + 50,
            this.transform.position.y , this.transform.position.z + 35),
            Quaternion.Euler(new Vector3(0, 90, 0)));

            cliff.transform.SetParent(this.gameObject.transform);
        }

        if (this.transform.position.x >= EnvManager.Inst.envSetting.boundryCoord_max.x)
        {
            GameObject cliff = Instantiate(EnvManager.Inst.cliffPrefab,
            new Vector3(transform.position.x + 45,
            this.transform.position.y, transform.position.z + 50),
            Quaternion.identity);

            cliff.transform.SetParent(this.gameObject.transform);
        }

        spawnArcifact();
        //MakeWaterLoad();
        ClearUsedEdge();

        MakeMesh();

        ScatterDetailMeshes();
    }


    //EnvManager로 넘겨야할듯
   

    bool MakeRiver(Vertex ver)
    {
        foreach(Vector3 point in EnvManager.Inst.BezierPoints)
        {
            float dist = Mathf.Sqrt(Mathf.Pow((point.x - ((float)ver.x + transform.position.x)), 2)
                                    + Mathf.Pow((point.z - ((float)ver.y + transform.position.z)), 2));

            if (dist <= 15)
            {
                Debug.Log("Hello?");
                return true;

            }

            Debug.Log(";;;;");
        }

        return false;
    }
    float MakeMountainElevation(float cx, float cy, float r, Vertex vert)
    {
        float mountainElev = 0;

        //중점으로부터 떨어진 거리
        float dist = Mathf.Sqrt(Mathf.Pow((cx - ((float)vert.x + transform.position.x)), 2)
            + Mathf.Pow((cy - ((float)vert.y + transform.position.z)), 2));

        if (dist <= r)
        {
            mountainElev = r - dist;
        }

        return mountainElev;
    }


    public void spawnArcifact()
    {
        int i = 0;

        GameObject g;

        foreach (Vertex ver in mesh.Vertices)
        {
            if (EnvManager.Inst == null)
                break;

            //강이냐
            if(transform.position.x + ver.x >= EnvManager.Inst.RiverRange[0] && transform.position.x + ver.x <= EnvManager.Inst.RiverRange[1])
            {
                if (transform.position.z <= EnvManager.Inst.RiverRange[2]) continue;
            }

            //산 지역이냐
            if (hasMountain)
            {
                continue;
            }
            // 나무 생성
            if (i % EnvManager.Inst.GetTreeSeed () == 0)
            {
                if (transform.position.z + ver.y <= 40 && transform.position.z + ver.y >= 30)
                {
                    if (transform.position.x + ver.x < 0)
                    {
                        continue;
                    }
                }
                else
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Tree (new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));
                    
                    if(!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 꽃 생성
            else if (i % EnvManager.Inst.GetFlowerSeed () == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Flower (new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));
                    
                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 돌 생성
            else if (i % EnvManager.Inst.GetRockSeed () == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Rock (new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));
                    
                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 곤충 생성
            else if (i % EnvManager.Inst.GetBugSeed () == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_Bug (new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));
                }
            }

            i++;
        }
    }


    public void ClearUsedEdge()
    {
        if (meetRight)
        {
            bindingRightVertex.Clear();
            bindingRightElev.Clear();
        }

        if (meetLeft)
        {
            bindingLeftElev.Clear();
            bindingLeftVertex.Clear();
        }

        if (meetUp)
        {
            bindingUpVertex.Clear();
            bindingUpElev.Clear();
        }

        if (meetDown)
        {
            bindingDownVertex.Clear();
            bindingDownElev.Clear();
        }

        
    }

    public void MakeMesh()
    {
        IEnumerator<Triangle> triangleEnumerator = mesh.Triangles.GetEnumerator();

        //Debug.Log(mesh.Triangles.Count + "  " + elevations.Count);

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
                Vector3 v0 = GetPoint3D(triangle.vertices[2].id);
                Vector3 v1 = GetPoint3D(triangle.vertices[1].id);
                Vector3 v2 = GetPoint3D(triangle.vertices[0].id);

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
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

    /* Returns a point's local coordinates. */
    public Vector3 GetPoint3D(int index)
    {
        Vertex vertex = mesh.vertices[index];
        float elevation = elevations[index];
        return new Vector3((float)vertex.x, elevation, (float)vertex.y);
    }

    /* Returns the triangle containing the given point. If no triangle was found, then null is returned.
       The list will contain exactly three point indices. */
    public List<int> GetTriangleContainingPoint(Vector2 point)
    {
        Triangle triangle = bin.getTriangleForPoint(new Point(point.x, point.y));
        if (triangle == null)
        {
            return null;
        }

        return new List<int>(new int[] { triangle.vertices[0].id, triangle.vertices[1].id, triangle.vertices[2].id });
    }

    /* Returns a pretty good approximation of the height at a given point in worldspace */
    public float GetElevation(float x, float y)
    {
        if (x < 0 || x > xsize ||
                y < 0 || y > ysize)
        {
            return 0.0f;
        }

        Vector2 point = new Vector2(x, y);
        List<int> triangle = GetTriangleContainingPoint(point);

        if (triangle == null)
        {
            // This can happen sometimes because the triangulation does not actually fit entirely within the bounds of the grid;
            // not great error handling, but let's return an invalid value
            return float.MinValue;
        }

        Vector3 p0 = GetPoint3D(triangle[0]);
        Vector3 p1 = GetPoint3D(triangle[1]);
        Vector3 p2 = GetPoint3D(triangle[2]);

        Vector3 normal = Vector3.Cross(p0 - p1, p1 - p2).normalized;
        float elevation = p0.y + (normal.x * (p0.x - x) + normal.z * (p0.z - y)) / normal.y;

        return elevation;
    }

    /* Scatters detail meshes within the bounds of the terrain. */
    public void ScatterDetailMeshes()
    {
        for (int i = 0; i < detailMeshesToGenerate; i++)
        {
            // Obtain a random position
            float x = Random.Range(0, xsize);
            float z = Random.Range(0, ysize);
            float elevation = GetElevation(x, z);
            Vector3 position = new Vector3(x, elevation, z);

            if (elevation == float.MinValue)
            {
                // Value returned when we couldn't find a triangle, just skip this one
                continue;
            }

            // We always want the mesh to remain upright, so only vary the rotation in the x-z plane
            float angle = Random.Range(0, 360.0f);
            Quaternion randomRotation = Quaternion.AngleAxis(angle, Vector3.up);

            Instantiate<Transform>(detailMesh, position, randomRotation, this.transform);
        }
    }

    public void UpdateNearTerrain()
    {
        if (EnvManager.Inst == null)
            return;

        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvManager.Inst.GetTerrainHolderByPosition(gameObject.transform.position, NearTerrainDir2.UP);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvManager.Inst.GetTerrainHolderByPosition(gameObject.transform.position, NearTerrainDir2.LEFT);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvManager.Inst.GetTerrainHolderByPosition(gameObject.transform.position, NearTerrainDir2.RIGHT);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvManager.Inst.GetTerrainHolderByPosition(gameObject.transform.position, NearTerrainDir2.DOWN);
        }
    }

    public void GenerateNearTerrain (int depth)
    //public void GenerateNearTerrain ()
    {   
        if (EnvManager.Inst == null)
            return;

        // 자기 자신 주변 업데이트
        UpdateNearTerrain ();

        // 재귀함수를 끝낼 조건
        if (depth <= 0)
            return;

        // =======================================================
        // 1단계: 일단 생성

        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.UP);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.LEFT);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.RIGHT);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.DOWN);
        }

        // =======================================================
        // 2단계: 확인 및 생성 반복

        if (nearTerrainHolder_u != null)
        {
            nearTerrainHolder_u.UpdateNearTerrain(); 
            nearTerrainHolder_u.GenerateNearTerrain (depth-1);
        }

        if (nearTerrainHolder_l != null)
        {
            nearTerrainHolder_l.UpdateNearTerrain(); 
            nearTerrainHolder_l.GenerateNearTerrain (depth-1);
        }

        if (nearTerrainHolder_r != null)
        {
            nearTerrainHolder_r.UpdateNearTerrain(); 
            nearTerrainHolder_r.GenerateNearTerrain (depth-1);
        }

        if (nearTerrainHolder_d != null)
        {
            nearTerrainHolder_d.UpdateNearTerrain(); 
            nearTerrainHolder_d.GenerateNearTerrain (depth-1);
        }
       // UpdateNearTerrain();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (Triangle tri in mesh.Triangles)
        {

        }
        foreach (Edge edge in mesh.Edges)
        {

        }

        foreach(Vertex vert in mesh.Vertices)
        {
            
        }
    }

    // 마우스 클릭은 EnvChunk 에서 처리
}
