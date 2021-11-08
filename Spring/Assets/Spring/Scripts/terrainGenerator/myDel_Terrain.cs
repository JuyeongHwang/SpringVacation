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
    [Space(10)]
   // public GameObject Butterfly;
    public GameObject[] bugsPrefabs;

    [Space (10)]
    public GameObject myPrefab_tree2;
    public GameObject water_plane;

    [Space(10)]
    public myDel_Terrain nearTerrainHolder_u = null;
    public myDel_Terrain nearTerrainHolder_l = null;
    public myDel_Terrain nearTerrainHolder_r = null;
    public myDel_Terrain nearTerrainHolder_d = null;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        
        //GameObject g = Instantiate(Butterfly, new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100)), Quaternion.identity);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            for(int i = 0; i<elevations.Count; i++)
            {
                elevations[i] += 5.0f;
            }

            for(int j = 0; j < this.gameObject.transform.childCount; j++)
            {
                if(transform.GetChild(j) != null)
                {
                    Destroy(this.gameObject.transform.GetChild(j).gameObject);
                }
                
            }

            MakeMesh();
            ScatterDetailMeshes();
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


            elevation += MakeMountainElevation(60, 50, 20, vert);
            elevation += MakeMountainElevation(0, 0, 10, vert);


            elevations.Add(elevation);
        }
        MakeMesh();

        ScatterDetailMeshes();
    }


    public List<Vertex> upVert = new List<Vertex>();
    public Dictionary<double, float> hashupElev = new Dictionary<double, float>();
    public List<float> forshow = new List<float>();
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
                forshow.Add(elevations[vert.id]);
            }
        }

    }


    public List<Vertex> downVert = new List<Vertex>();
    public Dictionary<double, float> hashDownElev = new Dictionary<double, float>();
    public List<float> forshow2 = new List<float>();
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

                forshow2.Add(elevations[vert.id]);
            }
        }
    }


    public List<Vertex> leftVert = new List<Vertex>();
    public Dictionary<double, float> hashleftElev = new Dictionary<double, float>();
    public List<float> forshow3 = new List<float>();
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

                forshow3.Add(elevations[vert.id]);
            }
        }

    }


    public List<Vertex> rightVert = new List<Vertex>();
    public Dictionary<double, float> hashrightElev = new Dictionary<double, float>();
    public List<float> forshow4 = new List<float>();
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

                forshow4.Add(elevations[vert.id]);
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

    public List<Vertex> faildBindingEdge = new List<Vertex>();
    float minX = 0, minY = 0, maxX = 0, maxY = 0;
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

        // Add uniformly-spaced points
        // 재귀함수 구현 중 아래 반복문이 문제가 되어 주석처리하였습니다
        /*foreach (Vector2 sample in sampler.Samples())
        {
            polygon.Add(new Vertex((double)sample.x, (double)sample.y));
        }*/

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



            // uplift ************
            if (vert.x + this.transform.position.x >= 60)
            {
                elevation += (this.transform.position.x) / 20;
            }
            if (vert.y + this.transform.position.z >= 60)
            {
                elevation += (this.transform.position.z) / 20;
            }

            //(100,100) 한정 _ 예시 // 못움직이게
            if (vert.x + this.transform.position.x >= 100)
            {
                elevation += (this.transform.position.x) / 5;
            }
            if (vert.y + this.transform.position.z >= 100)
            {
                elevation += (this.transform.position.z) / 5;
            }

            //절벽
            if (vert.x + this.transform.position.x < -30)
            {
                elevation -= 12.0f;
            }
            else if (vert.y + this.transform.position.z < -90)
            {
                elevation -= 12.0f;
            }


            // 강가 ***********
            //if (transform.position.x + vert.x >= 55 && transform.position.x + vert.x <= 80)
            //{
            //    if (transform.position.z + vert.y <= 10)
            //    {
            //        elevation = -1.0f * elevationScale;
            //    }

            //}

            if (transform.position.z + vert.y <= 10 && transform.position.z + vert.y >= -10)
            {
                if(transform.position.x + vert.x < 0)
                {
                    elevation -= 5f;
                }
            }

            //make Mountain****************************


            elevation += MakeMountainElevation(60, 50, 20, vert);
            elevation += MakeMountainElevation(0, 0, 10, vert);

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
                    //elevation = -3;
                    //Instantiate(water_plane,
                    //new Vector3((float)vert.x + transform.position.x, -0.5f, (float)vert.y + transform.position.z),
                    //Quaternion.identity);
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
                    //elevation = -3;
                    //Instantiate(water_plane,
                    //new Vector3((float)vert.x + transform.position.x, -0.5f, (float)vert.y + transform.position.z),
                    //Quaternion.identity);
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
                    //elevation = -3;
                    //Instantiate(water_plane,
                    //new Vector3((float)vert.x + transform.position.x, -0.5f, (float)vert.y + transform.position.z),
                    //Quaternion.identity);
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
                    //elevation = -3;
                    //Instantiate(water_plane,
                    //new Vector3((float)vert.x + transform.position.x, -0.5f, (float)vert.y + transform.position.z),
                    //Quaternion.identity);
                }
            }

            elevations.Add(elevation);

        }
        //예시

        //if(vert.x + transform.position.x <= -70)
        //{
        //    elevation += 0.3f;
        //}

        //else if (vert.x <= (double)70.0f && vert.x >=(double) 50.0f)
        //{
        //    elevation = -0.5f;
        //}

        //else if(vert.x + transform.position.x >= 10)
        //{
        //    elevation -= 0.3f;
        //}
        SpawnButterFly();
        //MakeWaterLoad();
        ClearUsedEdge();

        MakeMesh();

        ScatterDetailMeshes();
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

    void MakeWaterLoad()
    {
        foreach(Vertex ver in faildBindingEdge)
        {
            elevations[ver.id] = -5;
            Instantiate(water_plane,
            new Vector3((float)ver.x + transform.position.x, -0.5f, (float)ver.y + transform.position.z),
            Quaternion.identity);
        }
    }

    int i = 0;
    public void SpawnButterFly()
    {
        //foreach(Triangle tri in mesh.Triangles)
        //{
        //    if(i % 55 == 0)
        //    {
        //        float a = ((float)tri.vertices[0].x + transform.position.x*3 + (float)tri.vertices[1].x + (float)tri.vertices[2].x )/ 3;
        //        float b = ((float)tri.vertices[0].y + transform.position.y * 3 + (float)tri.vertices[1].y + (float)tri.vertices[2].y) / 3;
        //        float c = (elevations[tri.vertices[0].id] + elevations[tri.vertices[1].id] + elevations[tri.vertices[2].id]) / 3;

        //        GameObject g = Instantiate(Butterfly, new Vector3(a,c,b), Quaternion.identity);
        //    }
        //}

        
        GameObject g;


        //waterplane size : 1
        foreach(Triangle tri in mesh.Triangles)
        {
            float gx = (float)tri.vertices[0].x + (float)tri.vertices[1].x + (float)tri.vertices[2].x;
            gx /= 3;

            float gy = (float)tri.vertices[0].y + (float)tri.vertices[1].y + (float)tri.vertices[2].y;
            gy /= 3;


            Vector3 v0 = GetPoint3D(tri.vertices[2].id);
            Vector3 v1 = GetPoint3D(tri.vertices[1].id);
            Vector3 v2 = GetPoint3D(tri.vertices[0].id);

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
            
            if (transform.position.z + gy < 8 && transform.position.z + gy > -8)
            {
                
                if (transform.position.x + gx < -5)
                {
                    float gelev = elevations[tri.vertices[0].id] + elevations[tri.vertices[1].id] + elevations[tri.vertices[2].id];
                    gelev /= 3;

                    g = Instantiate(water_plane,
                        new Vector3(gx + transform.position.x, gelev + 1.0f, gy + transform.position.z),
                        Quaternion.identity);
                    //Quaternion.Euler(new Vector3(normal.x, normal.y, normal.z))
                    g.transform.parent = this.transform;
                }
            }
        }

        foreach (Vertex ver in mesh.Vertices)
        {

            //if (transform.position.x + ver.x > 55 && transform.position.x + ver.x < 80)
            //{
            //    if (transform.position.z + ver.y <= 7)
            //    {
            //        g = Instantiate(water_plane,
            //        new Vector3((float)ver.x + transform.position.x, elevations[i] + 2f, (float)ver.y + transform.position.z),
            //        Quaternion.identity);
            //        g.transform.parent = this.transform;
            //    }

            //}
            if (transform.position.z + ver.y <= 10 && transform.position.z + ver.y >= -10)
            {
                //절벽 지점 알아오기
                //if (transform.position.x + ver.x < 0)
                //{
                //    if (ver.x == -30)
                //    {
                //        g = Instantiate(water_plane,
                //        new Vector3((float)ver.x + transform.position.x, elevations[i] + 2f, (float)ver.y + transform.position.z),
                //        Quaternion.Euler(new Vector3(0, 0, 45)));
                        
                //    }
                //    else
                //    {
                //        g = Instantiate(water_plane,
                //        new Vector3((float)ver.x + transform.position.x, elevations[i] + 2f, (float)ver.y + transform.position.z),
                //        Quaternion.identity);
                //    }
                    
                //    g.transform.parent = this.transform;
                //}
            }
            else
            {
                if (i % 39 == 0)
                {
                    g=Instantiate(myPrefab_tree2,
                        new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z),
                        Quaternion.identity);
                    g.transform.parent = this.transform;
                }

                else if (i % 51 == 0 && bugsPrefabs.Length > 0)
                {
                    // 랜덤으로 곤충 생성
                    int bugIndex = Random.Range (0, bugsPrefabs.Length);

                    g=Instantiate(bugsPrefabs [bugIndex],
                        new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z),
                        Quaternion.identity);
                    g.transform.parent = this.transform;
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
}
