using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Smoothing;

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
    //public GameObject bridge;

    [Space(10)]
    public myDel_Terrain nearTerrainHolder_u = null;
    public myDel_Terrain nearTerrainHolder_l = null;
    public myDel_Terrain nearTerrainHolder_r = null;
    public myDel_Terrain nearTerrainHolder_d = null;
    public List<Vertex> faildBindingEdge = new List<Vertex>();

    public bool hasMountain;
    public bool hasCliff;
    public bool hasRiver = false;
    public bool hasBeach = false;
     // 테스트
    SimpleSmoother simpleSmoother = new SimpleSmoother ();


    public Dictionary<Vertex, GameObject> manageSpawnObject = new Dictionary<Vertex, GameObject>();
    float cliffRangeX;
    float cliffRangeY;

    // 꼬마 위치 체크를 위한 수치
    float terrainMinX;
    float terrainMaxX;
    float terrainMinZ;
    float terrainMaxZ;

    public bool isDiscovered = false;
    //IEnumerator idiscover;

    /*private void OnEnable() 
    {
        if (idiscover != null)
            StopCoroutine (idiscover);

        idiscover = IDiscover ();
        StartCoroutine (idiscover);
    }

    private void OnDisable ()
    {
        if (idiscover != null)
            StopCoroutine (idiscover);
    }*/

    private void Awake()
    {
        cliffRangeX = EnvManager.Inst.envSetting.boundryCoord_min.x + 30;
        cliffRangeY = EnvManager.Inst.envSetting.boundryCoord_min.y + 30;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        isDiscovered = false;

        terrainMinX = gameObject.transform.position.x;
        terrainMinZ = gameObject.transform.position.z;
        terrainMaxX = terrainMinX + EnvManager.Inst.GetTerrainUnitSize ();
        terrainMaxZ = terrainMinZ + EnvManager.Inst.GetTerrainUnitSize ();
    }

    private RaycastHit hit;
    public float updownRange = 15;
    public bool canEdit = true;
    private void Update()
    {
           
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.name == "ChunkPrefab(Clone)")
                    {
                        UpDownTerrain(false);
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
                        UpDownTerrain(true);
                    }

                }
            }

        }
    }

    bool checkInside(Vertex v)
    {

        if (v.x >= 0 && v.x <= xsize)
        {
            if (v.y >= 0 && v.y <= ysize)
            {

                return true;

            }
        }
        return false;
    }

    float movingrange = 0.4f;

    void UpDownTerrain(bool up)
    {
        List<Vector3> myArea = new List<Vector3>();

        bool inside = false;
        foreach (Vertex ver in mesh.Vertices)
        {

            float dist = Mathf.Sqrt(Mathf.Pow((hit.point.x - ((int)ver.x + transform.position.x)), 2)
                    + Mathf.Pow((hit.point.z - ((int)ver.y + transform.position.z)), 2));

            if (dist <= updownRange)
            {
                if (up)
                {
                    elevations[ver.id] += movingrange;
                }
                else
                {
                    elevations[ver.id] -= movingrange;
                }
                //int x = 0;
                //int z = 0;

                ////음수라면
                //if ((float)ver.x + transform.position.x < 0)
                //{
                //    x = (((int)ver.x + (int)transform.position.x)) / 50 - 1;
                //}
                //else
                //{
                //    x = (((int)ver.x + (int)transform.position.x)) / 50;
                //}

                //if ((float)ver.y + transform.position.z < 0)
                //{
                //    z = (((int)ver.y + (int)transform.position.z)) / 50 - 1;
                //}
                //else
                //{
                //    z = (((int)ver.y + (int)transform.position.z)) / 50;
                //}

                //Vector3 vec = new Vector3(x * 50, 0, z * 50);
                //if (!myArea.Contains(vec))
                //{
                //    myArea.Add(vec);
                //}
                if (checkInside(ver))
                {
                    inside = true;
                }
                if (manageSpawnObject.ContainsKey(ver))
                {
                    GameObject g = manageSpawnObject[ver];
                    if(g != null)
                    {
                        if (up)
                        {
                            g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y + movingrange, g.transform.position.z);
                        }
                        else
                        {

                            g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y - movingrange, g.transform.position.z);
                        }
                    }
                    
                }

            }
        }

        if (inside)
        {
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

        //foreach (Vector3 area in myArea)
        //{
        //    if (area.Equals(this.transform.position))
        //    {
        //        for (int j = 0; j < this.gameObject.transform.childCount; j++)
        //        {
        //            if (transform.GetChild(j) != null)
        //            {
        //                if (transform.GetChild(j).transform.name == "ChunkPrefab(Clone)" || transform.GetChild(j).transform.name == ("RockDetail(Clone)"))
        //                    Destroy(this.gameObject.transform.GetChild(j).gameObject);


        //            }
        //        }

        //        MakeMesh();
        //        ScatterDetailMeshes();
        //    }
        //}
        inside = false;
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

        //simpleSmoother.Smooth (mesh);

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
            
            if (MakeRiver(vert))
            {
                riverVert.Add(vert);
                elevation -= 7.0f;
            }
            elevations.Add(elevation);
        }

        //GameObject water = Instantiate(water_plane,
        //    new Vector3(transform.position.x + 25,
        //    this.transform.position.y - 0.8f, transform.position.z + 25),
        //    Quaternion.identity);

        //water.transform.SetParent(this.gameObject.transform);

        spawnArcifact();
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

    List<Vertex> riverVert = new List<Vertex>();


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

        //simpleSmoother.Smooth (mesh);


        bin = new TriangleBin(mesh, (int)maxX, (int)maxX, minPointRadius * 2.0f);

        bool makeWaterPlane = false;

        float minPoint = EnvManager.Inst.envSetting.boundryCoord_min.x;
        float maxPoint = EnvManager.Inst.envSetting.boundryCoord_max.y;

        float Radius = Random.Range(5, 11);
        float RandomCenterPointX = Random.Range(Radius, xsize - Radius);
        float RandomCenterPointY = Random.Range(Radius, ysize - Radius);

        float Radius2 = Random.Range(3, 7);
        float RandomCenterPointX2 = Random.Range(Radius2, xsize - Radius);
        float RandomCenterPointY2 = Random.Range(Radius2, ysize - Radius);

        int grass_seed = 0;
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

            //해변가
            if (transform.position.x + vert.x <= cliffRangeX ||
                transform.position.z + vert.y <= cliffRangeY)
            {
                elevation = -3.5f;
            }

            if (transform.position.x + vert.x <= EnvManager.Inst.envSetting.boundryCoord_min.x+0.3 ||
                transform.position.z + vert.y <= EnvManager.Inst.envSetting.boundryCoord_min.y+0.3)
            {
                elevation = -4.3f;
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

            if (MakeRiver(vert))
            {
                riverVert.Add(vert);
                elevation -= 7.0f;
            }
            else
            {
                if (transform.position.x + vert.x >= cliffRangeX &&
                    transform.position.z + vert.y >= cliffRangeY)
                {
                    if (hasMountain)// && this.transform.position.x >=0)
                    {
                        elevation += MakeMountainElevation(RandomCenterPointX + transform.position.x, RandomCenterPointY + transform.position.z, Radius, vert);
                        elevation += MakeMountainElevation(RandomCenterPointX2 + transform.position.x, RandomCenterPointY2 + transform.position.z, Radius2, vert);
                    }
                }
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

            if (transform.position.x + vert.x >= cliffRangeX &&
                transform.position.z + vert.y >= cliffRangeY)
            {
                if (grass_seed % 3 == 0)
                {

                    if (EnvManager.Inst.flowerPrefabs.Length > 0)
                    {
                        int index = Random.Range(0, EnvManager.Inst.flowerPrefabs.Length);

                        GameObject grass = Instantiate(EnvManager.Inst.flowerPrefabs[0], new Vector3((float)vert.x + transform.position.x,
                            elevation, (float)vert.y + transform.position.z),
                            Quaternion.Euler(Vector3.zero), this.gameObject.transform);
                    }
                }

            }

            grass_seed++;

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

        // 절벽 생성

        hasCliff = false;

        if (this.transform.position.z >= EnvManager.Inst.envSetting.boundryCoord_max.y)
        {
            GameObject cliff = Instantiate(EnvManager.Inst.cliffPrefab,
            new Vector3(transform.position.x + 25,
            this.transform.position.y , this.transform.position.z + 17.5f),
            Quaternion.Euler(new Vector3(0, 90, 0)));

            cliff.transform.SetParent(this.gameObject.transform);

            hasCliff = true;
        }

        if (this.transform.position.x >= EnvManager.Inst.envSetting.boundryCoord_max.x)
        {
            GameObject cliff = Instantiate(EnvManager.Inst.cliffPrefab,
            new Vector3(transform.position.x + 22.5f,
            this.transform.position.y, transform.position.z + 25),
            Quaternion.identity);

            cliff.transform.SetParent(this.gameObject.transform);

            hasCliff = true;
        }

        spawnArcifact();
        ClearUsedEdge();

        MakeMesh();

        ScatterDetailMeshes();
    }

    bool MakeRiver(Vertex ver)
    {
        foreach(Vector3 point in EnvManager.Inst.BezierPoints)
        {
            float dist = Mathf.Sqrt(Mathf.Pow((point.x - ((float)ver.x + transform.position.x)), 2)
                                    + Mathf.Pow((point.z - ((float)ver.y + transform.position.z)), 2));

            if (dist <= 9)
            {
                
                return true;

            }
        }

        foreach (Vector3 point in EnvManager.Inst.BezierPoints2)
        {
            float dist = Mathf.Sqrt(Mathf.Pow((point.x - ((float)ver.x + transform.position.x)), 2)
                                    + Mathf.Pow((point.z - ((float)ver.y + transform.position.z)), 2));

            if (dist <= 4)
            {

                return true;

            }
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
            if (riverVert.Contains(ver)) continue;
            //산 지역이냐
            if (hasMountain) continue;

            //해변?
            if (ver.x +transform.position.x> cliffRangeX && ver.y+transform.position.z>cliffRangeY) //아니요
            {
                // 나무 생성
                if (i % EnvManager.Inst.GetTreeSeed() == 0)
                {
                    //print ("해변가 나무");

                    g = EnvManager.Inst.Instantiate_EnvObject_Tree(new Vector3((float)ver.x + transform.position.x, elevations[ver.id], (float)ver.y + transform.position.z), false);

                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }

                // 꽃 생성
                else if (i % EnvManager.Inst.GetFlowerSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        if(elevations[ver.id] - 0.18f > 0.0)
                        {
                            g = EnvManager.Inst.Instantiate_EnvObject_Flower(new Vector3((float)ver.x + transform.position.x, elevations[ver.id] - 0.18f, (float)ver.y + transform.position.z));

                            if (!manageSpawnObject.ContainsKey(ver))
                                manageSpawnObject.Add(ver, g);
                        }
                        
                    }
                }

                // 돌 생성
                else if (i % EnvManager.Inst.GetRockSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        g = EnvManager.Inst.Instantiate_EnvObject_Rock(new Vector3((float)ver.x + transform.position.x, elevations[ver.id] -1f, (float)ver.y + transform.position.z));

                        if (!manageSpawnObject.ContainsKey(ver))
                            manageSpawnObject.Add(ver, g);
                    }
                }

                // 곤충 생성
                else if (i % EnvManager.Inst.GetBugSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        g = EnvManager.Inst.Instantiate_Bug(false, new Vector3((float)ver.x + transform.position.x, elevations[ver.id], (float)ver.y + transform.position.z));
                    }
                }
            }

            else//예
            {
                // 나무 생성
                if (i % EnvManager.Inst.GetTreeSeed() == 0)
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Tree(new Vector3((float)ver.x + transform.position.x, elevations[ver.id], (float)ver.y + transform.position.z),true);

                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }

                //해변용 추가??하면 해변용 곤충까지 같이 추가해야할듯,,,,
                // 꽃 생성
                else if (i % EnvManager.Inst.GetFlowerSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        if (elevations[ver.id] >= -0.5f)
                        {
                            g = EnvManager.Inst.Instantiate_EnvObject_Flower(new Vector3((float)ver.x + transform.position.x, elevations[ver.id] - 0.18f, (float)ver.y + transform.position.z));

                            if (!manageSpawnObject.ContainsKey(ver))
                                manageSpawnObject.Add(ver, g);
                        }
                    }
                }

                //해변용 추가??
                // 돌 생성
                else if (i % EnvManager.Inst.GetRockSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        g = EnvManager.Inst.Instantiate_EnvObject_Rock(new Vector3((float)ver.x + transform.position.x, elevations[ver.id] - 1f, (float)ver.y + transform.position.z));

                        if (!manageSpawnObject.ContainsKey(ver))
                            manageSpawnObject.Add(ver, g);
                    }
                }

                else if (i % EnvManager.Inst.GetBugSeed() == 0)
                {
                    if (EnvManager.Inst != null)
                    {
                        g = EnvManager.Inst.Instantiate_Bug(true,new Vector3((float)ver.x + transform.position.x, elevations[ver.id], (float)ver.y + transform.position.z));
                    }
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

                // 여태 생성된 트라이앵글 인덱스를 순회화며 중복 찾기
                for (int j = 0; j < triangles.Count - 3; j++)
                {
                    if (vertices [triangles.Count-3] == vertices [j])
                    {
                        triangles [triangles.Count-3] = triangles [j];
                        normals [j] = (normals [j] + normals [triangles.Count-3]) / 2;
                    }

                    if (vertices [triangles.Count-2] == vertices [j])
                    {
                        triangles [triangles.Count-2] = triangles [j];
                        normals [j] = (normals [j] + normals [triangles.Count-2]) / 2;
                    }

                    if (vertices [triangles.Count-1] == vertices [j])
                    {
                        triangles [triangles.Count-1] = triangles [j];
                        normals [j] = (normals [j] + normals [triangles.Count-1]) / 2;
                    }
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
        
        // 1차 -> 겹치는 버텍스의 인덱스 찾기
        /*List<List <int>> overlapTri = new List<List<int>> ();
        for (int i = 0; i < triangles.Count; i++)
        {
            int j = 0;
            for (; j < i; j++)
            {
                if (vertices [i] == vertices [j])
                {
                    break;
                }
            }

            // 검사한 적이 없는 첫 인덱스
            if (j == i)
            {
                // 새로 만들어 삽입
                List <int> _l = new List<int> ();
                _l.Add (i);

                overlapTri.Add (_l);
            }
            // 검사한 적이 있는 인덱스
            else
            {
                // 리스트에 있는 리스트에서 삽입할 리스트 찾기
                foreach (List<int> ll in overlapTri)
                {
                    if (ll.Contains (j))
                    {
                        ll.Add (i);
                    }
                }
            }
        }

        // 2차 순회하며 겹치는 버텍스들은 걸러내기
        for (int i = 0; i < triangles.Count; i++)
        {
            foreach (List<int> ll in overlapTri)
            {
                if (ll.Contains (i))
                {
                    triangles [i] = triangles [ll [0]];
                }
            }
        }*/
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

    /*public void GenerateNearTerrain (int depth)
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
    }*/

    public void GenerateNearTerrain ()
    {   
        if (EnvManager.Inst == null)
            return;

        // 자기 자신 주변 업데이트
        UpdateNearTerrain ();

        // =======================================================
        // 1단계: 일단 생성

        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.UP);
            EnvManager.Inst.AddInstTerrainToList (nearTerrainHolder_u);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.LEFT);
            EnvManager.Inst.AddInstTerrainToList (nearTerrainHolder_l);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.RIGHT);
            EnvManager.Inst.AddInstTerrainToList (nearTerrainHolder_r);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvManager.Inst.InstantiateCustomTerrain(gameObject.transform.position, NearTerrainDir2.DOWN);
            EnvManager.Inst.AddInstTerrainToList (nearTerrainHolder_d);
        }

        // =======================================================

        // 자기 자신 주변 업데이트
        UpdateNearTerrain ();

        // 2단계: 주변지형 갱신
        if (nearTerrainHolder_u != null)
        {
            nearTerrainHolder_u.UpdateNearTerrain(); 
        }

        if (nearTerrainHolder_l != null)
        {
            nearTerrainHolder_l.UpdateNearTerrain(); 
        }

        if (nearTerrainHolder_r != null)
        {
            nearTerrainHolder_r.UpdateNearTerrain(); 
        }

        if (nearTerrainHolder_d != null)
        {
            nearTerrainHolder_d.UpdateNearTerrain(); 
        }

        //print ("B");
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


    // ============================== 꼬마위치 체크 ==========================

    /*IEnumerator IDiscover ()
    {
        float checkDur = 10f;

        yield return null;

        while (isDiscovered == false 
        && MyGameManager_Gameplay.Inst != null 
        && EnvManager.Inst != null)
        {
            Vector3 kidPos = MyGameManager_Gameplay.Inst.GetKidPosition ();

            if (terrainMinX <= kidPos.x && kidPos.x <= terrainMaxX
            && terrainMinZ <= kidPos.z && kidPos.z <= terrainMaxZ)
            {
                isDiscovered = true;
                EnvManager.Inst.IncreaseTerrainNum_Current ();
            }

            yield return new WaitForSeconds (checkDur);
        }
    }*/
}
