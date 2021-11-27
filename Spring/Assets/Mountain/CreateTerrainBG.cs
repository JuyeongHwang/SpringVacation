using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class CreateTerrainBG : MonoBehaviour
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

    public bool forz;
    // Elevations at each point in the mesh
    private List<float> elevations;

    // Fast triangle querier for arbitrary points
    private TriangleBin bin;

    // The delaunay mesh
    private TriangleNet.Mesh mesh = null;

    int totalsize;

    public Dictionary<Vertex, GameObject> manageSpawnObject = new Dictionary<Vertex, GameObject>();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        totalsize = (int)EnvManager.Inst.envSetting.boundryCoord_max.x - (int)EnvManager.Inst.envSetting.boundryCoord_min.x;
        Debug.Log(totalsize);
        xsize = totalsize ;
        ysize = totalsize ;

        if (forz)
        {
            this.transform.position = new Vector3(EnvManager.Inst.envSetting.boundryCoord_min.x, 0, EnvManager.Inst.envSetting.boundryCoord_max.y + 50);
        }
        else
        {
            this.transform.position = new Vector3(EnvManager.Inst.envSetting.boundryCoord_max.x + 50, 0, EnvManager.Inst.envSetting.boundryCoord_min.y );
        }

        Generate();      
    }

    private RaycastHit hit;
    public float updownRange = 3.0f;


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

            elevation += MakeMountainElevation(totalsize+150, totalsize+75, 50,vert);
            elevation += MakeMountainElevation(totalsize+50, totalsize+30, 20, vert);
            elevation += MakeMountainElevation(totalsize+220, totalsize+100, 55, vert);
            //elevation += MakeMountainElevation(totalsize+15, totalsize+120, 45, vert);


            elevations.Add(elevation);
        }

        MakeMesh();

        ScatterDetailMeshes();
    }

    float MakeMountainElevation(float cx, float cy, float r, Vertex vert)
    {
        float mountainElev = 0;

        //吝痢栏肺何磐 冻绢柳 芭府
        float dist = Mathf.Sqrt(Mathf.Pow((cx - ((float)vert.x+ totalsize)), 2)
            + Mathf.Pow((cy - ((float)vert.y+ totalsize)), 2));

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

            // 唱公 积己
            if (i % EnvManager.Inst.GetTreeSeed() == 0)
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
                    g = EnvManager.Inst.Instantiate_EnvObject_Tree(new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));

                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 采 积己
            else if (i % EnvManager.Inst.GetFlowerSeed() == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Flower(new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));

                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 倒 积己
            else if (i % EnvManager.Inst.GetRockSeed() == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_EnvObject_Rock(new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));

                    if (!manageSpawnObject.ContainsKey(ver))
                        manageSpawnObject.Add(ver, g);
                }
            }

            // 帮面 积己
            else if (i % EnvManager.Inst.GetBugSeed() == 0)
            {
                if (EnvManager.Inst != null)
                {
                    g = EnvManager.Inst.Instantiate_Bug(new Vector3((float)ver.x + transform.position.x, elevations[i], (float)ver.y + transform.position.z));
                }
            }

            i++;
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

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (Triangle tri in mesh.Triangles)
        {

        }
        foreach (Edge edge in mesh.Edges)
        {

        }

        foreach (Vertex vert in mesh.Vertices)
        {

        }
    }

}
