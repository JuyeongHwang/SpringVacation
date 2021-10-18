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

    //[Space (10)]

    public CustomDelaunayTerrain nearTerrainHolder_u = null;
    public CustomDelaunayTerrain nearTerrainHolder_l = null;
    public CustomDelaunayTerrain nearTerrainHolder_r = null;
    public CustomDelaunayTerrain nearTerrainHolder_d = null;

    int xsizeOffset = 0;
    int ysizeOffset = 0;

    int i = 0;
    bool bridgeExist = false;

    private void Awake ()
    {
        // 생성에 필요한 수치들을 세팅후 Generate
        SetSize ();
        SetSizeOffset ();
    }

    void Start ()
    {
        Generate ();

        //다른 방법을 강구해야함...임시로,,
        foreach (Vertex ver in mesh.Vertices)
        {
            //Debug.Log(ver.x+" " + ver.y+" " + elevations[i]);
            if (ver.x <= (double)70.0f && ver.x >= (double)50.0f)
            {
                if(bridgeExist ==false)
                {
                    Instantiate(Bridge,
                           new Vector3((float)ver.x + transform.position.x, elevations[i] + 2.0f, (float)ver.y + transform.position.z),
                           Quaternion.identity);

                    bridgeExist = true;

                }
            }
            else
            {
                if (i % 12 == 0)
                {
                    Instantiate(myPrefab_butterfly,
                        new Vector3((float)ver.x + transform.position.x, elevations[i] + 1.0f, (float)ver.y + transform.position.z),
                        Quaternion.identity);

                }
                else if (i % 34 == 0)
                {
                    Instantiate(myPrefab_tree1,
                        new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z),
                        Quaternion.identity);
                }
                else if (i % 51 == 0)
                {
                    Instantiate(myPrefab_tree2,
                        new Vector3((float)ver.x + transform.position.x, elevations[i] , (float)ver.y + transform.position.z),
                        Quaternion.identity);
                }
            }
            

            i++;
        }
    }

    public void SetSize ()
    {
        //xsize = x;
        //ysize = y;

        xsize = EnvironmentManager.Inst.GetTerrainUnitSize ();
        ysize = EnvironmentManager.Inst.GetTerrainUnitSize ();
    }

    public void SetSizeOffset ()
    {
        //xSizeOffset = x;
        //ySizeOffset = y;

        xsizeOffset = (int)gameObject.transform.position.x;
        ysizeOffset = (int)gameObject.transform.position.z;
    }

    public override void Generate() 
    {
        UnityEngine.Random.InitState(0);

        elevations = new List<float>();

        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++) {
            seed[i] = Random.Range(0.0f, 100.0f);
        }
        
        PoissonDiscSampler sampler = new PoissonDiscSampler(xsize, ysize, minPointRadius);

        Polygon polygon = new Polygon();

        // Add uniformly-spaced points
        foreach (Vector2 sample in sampler.Samples()) {
            polygon.Add(new Vertex((double)sample.x, (double)sample.y));
        }

        // Add some randomly sampled points
        for (int i = 0; i < randomPoints; i++) {
            polygon.Add(new Vertex(Random.Range(0.0f, xsize), Random.Range(0.0f, ysize)));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        
        bin = new TriangleBin(mesh, xsize, ysize, minPointRadius * 2.0f);

        // Sample perlin noise to get elevations
        foreach (Vertex vert in mesh.Vertices) {
            float elevation = 0.0f;
            float amplitude = Mathf.Pow(persistence, octaves);
            float frequency = 1.0f;
            float maxVal = 0.0f;

            for (int o = 0; o < octaves; o++) 
            {
                // offset을 주어 연결이 자연스럽게 보이도록 한다
                Vector2 sampleUV = new Vector2 (seed[o] + (float)(vert.x+xsizeOffset)*sampleSize / (float)xsize * frequency,
                                                  seed[o] + (float)(vert.y+ysizeOffset)*sampleSize / (float)ysize * frequency);

                float sample = (Mathf.PerlinNoise (sampleUV.x, sampleUV.y) - 0.5f) * amplitude;
                elevation += sample;
                maxVal += amplitude;
                amplitude /= persistence;
                frequency *= frequencyBase;
            }

            elevation = elevation / maxVal;

            if (vert.x <= (double)70.0f && vert.x >=(double) 50.0f)
            {
                elevation = -0.5f;
            }

            else if(vert.x >= 160.0f)
            {
                elevation = 3.0f;
            }

            elevations.Add(elevation * elevationScale);
        }

        MakeMesh();

        ScatterDetailMeshes();
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

    public void GenerateNearTerrain ()
    {
        UpdateNearTerrain ();

        // =======================================================

        if (nearTerrainHolder_u == null)
        {
            nearTerrainHolder_u = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.UP);
        }

        if (nearTerrainHolder_l == null)
        {
            nearTerrainHolder_l = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.LEFT);
        }

        if (nearTerrainHolder_r == null)
        {
            nearTerrainHolder_r = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.RIGHT);
        }

        if (nearTerrainHolder_d == null)
        {
            nearTerrainHolder_d = EnvironmentManager.Inst.InstantiateCustomTerrain (gameObject.transform.position, NearTerrainDir.DOWN);
        }
    }
}
