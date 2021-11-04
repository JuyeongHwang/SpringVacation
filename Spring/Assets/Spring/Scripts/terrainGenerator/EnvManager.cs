using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum NearTerrainDir2
{
    NONE = -1
    , UP, DOWN, LEFT, RIGHT
}

public class EnvManager : MonoBehaviour
{
    [Header("�� ȯ�� ����")]
    public int terrainUnitSize = 100;
    public GameObject customTerrainPrefab;

    [Space(10)]
    public KidController kidController;

    [Space(10)]
    public float checkDelay = 1f;
    [Header("�ͷ��� ����")]
    public myDel_Terrain currentCustomTerrain;
    public List<myDel_Terrain> customTerrains;

    [Header("�ͷ��� ������ ����: ����")]
    public float terrainNoiseScale = 1f;
    public float terrainNoisePow = 1f;

    [Header("�ͷ��� ������ ����: ����")]
    [Tooltip("�� ����� �ٲ�� Texture")]
    public Texture2D riverNoiseTexture2D;
    public int riverNoiseSize = 1024;
    public int riverRegionAmount = 50;
    public float riverNoiseScale = 1f;
    public float riverNoisePow = 1f;

    public static EnvManager Inst = null;

    [Header("Navmesh")]

    IEnumerator icheck;

    void Awake()
    {
        // �̱���
        if (Inst == null)
        {
            Inst = gameObject.GetComponent<EnvManager>();
        }
        else
        {
            Destroy(gameObject);
        }

        // Ű�� ã��
        if (kidController == null)
            kidController = FindObjectOfType<KidController>();

        // ����Ʈ �ʱ�ȭ
        customTerrains = new List<myDel_Terrain>();
        // ���γ���
        riverNoiseTexture2D = GetDiagramByDistance();
        currentCustomTerrain = InstantiateCustomTerrain(Vector3.zero, NearTerrainDir2.NONE);
        currentCustomTerrain.GenerateNearTerrain();

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

        //NavMeshSurface[] surfaces = gameObject.GetComponentsInChildren<NavMeshSurface>();

        // �ֱ⸶�� ĳ���� ��ġ üũ �� ���� ����
        //CheckKidPosition();
        //foreach (var s in surfaces)
        //{
        //    s.RemoveData();
        //    s.BuildNavMesh();
        //}
    }

    
    // ȯ�� �Ŵ������� �ν��Ͻ� ����
    // �׸��� �ش� ������Ʈ�� ��ȯ
    public myDel_Terrain InstantiateCustomTerrain(Vector3 pivotTerrainPos, NearTerrainDir2 terrainDir)
    {
        myDel_Terrain ret = null;

        if (customTerrainPrefab != null)
        {
            Vector3 instTerrainPos = pivotTerrainPos;

            switch (terrainDir)
            {
                case NearTerrainDir2.UP:
                    instTerrainPos += Vector3.forward * terrainUnitSize;
                    break;

                case NearTerrainDir2.DOWN:
                    instTerrainPos -= Vector3.forward * terrainUnitSize;
                    break;

                case NearTerrainDir2.LEFT:
                    instTerrainPos -= Vector3.right * terrainUnitSize;
                    break;

                case NearTerrainDir2.RIGHT:
                    instTerrainPos += Vector3.right * terrainUnitSize;
                    break;

            }

            /*
             * 
             * 1. detect near terrain
             * 2. generate edge met terrain 'ct'
             * 3. bind the edge in 'g'
             */
            // �ش� �ͷ��� ����
            GameObject g = Instantiate(customTerrainPrefab, instTerrainPos, Quaternion.identity);

            ret = g.GetComponent<myDel_Terrain>();


            foreach (myDel_Terrain ct in customTerrains)
            {

                if (ret.meetDown && ret.meetLeft&&ret.meetRight && ret.meetUp) { break; }

                if (!ret.meetRight&& instTerrainPos + Vector3.right * terrainUnitSize == ct.gameObject.transform.position)
                {
                    ret.meetRight = true;
                    ct.generateLeft();

                    ret.bindingRightVertex = ct.leftVert;
                    ret.bindingRightElev = ct.hashleftElev;

                }
                if (!ret.meetLeft && instTerrainPos - Vector3.right * terrainUnitSize == ct.gameObject.transform.position)
                {
                    ret.meetLeft = true;
                    ct.generateRight();

                    ret.bindingLeftVertex = ct.rightVert;
                    ret.bindingLeftElev = ct.hashrightElev;
                }
                if (!ret.meetUp && instTerrainPos + Vector3.forward * terrainUnitSize == ct.gameObject.transform.position)
                {
                    ret.meetUp = true;
                    ct.generateDown(); //info

                    ret.bindingUpVertex = ct.downVert;
                    ret.bindingUpElev = ct.hashDownElev;
                }
                if (!ret.meetDown && instTerrainPos - Vector3.forward * terrainUnitSize == ct.gameObject.transform.position)
                {

                    ret.meetDown = true;
                    ct.generateUp();

                    ret.bindingDownVertex = ct.upVert;
                    ret.bindingDownElev = ct.hashupElev;
                }
            }

            // �ش� �ͷ��� ��ġ �ε��� ���� (�ε��� = �ش� ��ġ / ������)
            //ret.customPos = gameObject.transform.position;
            //ret.indexX = (int)pivotTerrainPos.x / GetTerrainUnitSize_Original ();
            //ret.indexY = (int)pivotTerrainPos.y / GetTerrainUnitSize_Original ();

            if (!ret.meetDown && !ret.meetLeft && !ret.meetRight && !ret.meetUp)
            {
                ret.Generate();
            }
            else //�Ҵ�޾ƾ��� ������ �ϳ��� �ִ� ���
            {
                ret.GenerateForNear();
            }



            // �θ� �ش� �Ŵ����� �����Ѵ�
            g.gameObject.transform.SetParent(this.gameObject.transform);

            // ��ġ ���� (����)
            //g.gameObject.transform.position = instTerrainPos;

            // ����Ʈ�� �߰�
            customTerrains.Add(ret);
        }

        return ret;
    }


    private void Update()
    {
        if (MyGameManager_Gameplay.Inst.isLoadGameScene == true)
        {
            this.gameObject.SetActive(true);
            CheckKidPosition();
            MyGameManager_Gameplay.Inst.isLoadGameScene = false;
        }
    }


    public myDel_Terrain GetTerrainHolderByPosition(Vector3 pos, NearTerrainDir2 dir)
    {
        float e = 0.01f;
        myDel_Terrain ret = null;

        switch (dir)
        {
            case NearTerrainDir2.UP:
                pos += Vector3.forward * GetTerrainUnitSize_Original();
                break;

            case NearTerrainDir2.LEFT:
                pos -= Vector3.right * GetTerrainUnitSize_Original();
                break;

            case NearTerrainDir2.RIGHT:
                pos += Vector3.right * GetTerrainUnitSize_Original();
                break;

            case NearTerrainDir2.DOWN:
                pos -= Vector3.forward * GetTerrainUnitSize_Original();
                break;
        }

        for (int i = 0; i < customTerrains.Count; i++)
        {
            float dist = Vector3.Distance(pos, customTerrains[i].gameObject.transform.position);
            if (dist < e)
            {
                ret = customTerrains[i];
                break;
            }
        }

        return ret;
    }

    // ============================================ �ͷ��� ������ ====================================================
    public int GetTerrainUnitSize_Original()
    {
        return terrainUnitSize;
    }

    void CheckKidPosition()
    {
        if (icheck != null)
            StopCoroutine(icheck);

        icheck = ICheckKidPosition();
        StartCoroutine(icheck);
    }

    IEnumerator ICheckKidPosition()
    {
        bool gen = false;

        while (kidController != null && EnvManager.Inst != null)
        {
            gen = false;

            float posXMin = currentCustomTerrain.gameObject.transform.position.x;
            float posXMax = posXMin + EnvManager.Inst.GetTerrainUnitSize_Original();

            float posZMin = currentCustomTerrain.gameObject.transform.position.z;
            float posZSMax = posZMin + EnvManager.Inst.GetTerrainUnitSize_Original();

            Vector3 kidPos = kidController.gameObject.transform.position;

            if (kidPos.x < posXMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_l;
                currentCustomTerrain.GenerateNearTerrain();
                gen = true;
            }
            else if (kidPos.x > posXMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_r;
                currentCustomTerrain.GenerateNearTerrain();
                gen = true;
            }
            else if (kidPos.z < posZMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_d;
                currentCustomTerrain.GenerateNearTerrain();
                gen = true;
            }
            else if (kidPos.z > posZSMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_u;
                currentCustomTerrain.GenerateNearTerrain();
                gen = true;
            }

            if (gen)
            {
                foreach (myDel_Terrain cdt in customTerrains)
                {
                    cdt.UpdateNearTerrain();
                }
            }

            yield return new WaitForSeconds(checkDelay);
        }
    }

    // ========================================== ������ ���� ==================================================

    // -0.5 ~ 0.5
    public float GetTerrainNoise(Vector2 uv)
    {
        float ret = Mathf.PerlinNoise(uv.x * terrainNoiseScale, uv.y * terrainNoiseScale) - 0.5f;
        ret = Mathf.Pow(ret, terrainNoisePow);
        return ret;
    }

    // 0 or 1
    public float GetRiverNoise(Vector2 uv)
    {
        Vector2 trueUV = new Vector2(riverNoiseSize * 0.5f + (uv.x) * riverNoiseScale, riverNoiseSize * 0.5f + (uv.y) * riverNoiseScale);
        //trueUV.x = (trueUV.x + riverNoiseSize) % riverNoiseSize;
        //trueUV.y = (trueUV.y + riverNoiseSize) % riverNoiseSize; 

        Color col = riverNoiseTexture2D.GetPixel((int)trueUV.x, (int)trueUV.y);
        float ret = col.r;
        //print (ret);

        if (ret > 0.5f)
            return 0;
        return 1;
    }

    // ========================================== ���γ��� ========================================================

    Texture2D GetDiagramByDistance()
    {
        Vector2Int[] centroids = new Vector2Int[riverRegionAmount];

        for (int i = 0; i < riverRegionAmount; i++)
        {
            centroids[i] = new Vector2Int(Random.Range(0, riverNoiseSize), Random.Range(0, riverNoiseSize));
        }
        Color[] pixelColors = new Color[riverNoiseSize * riverNoiseSize];
        float[] distances = new float[riverNoiseSize * riverNoiseSize];

        //you can get the max distance in the same pass as you calculate the distances. :P oops!
        float maxDst = float.MinValue;
        for (int x = 0; x < riverNoiseSize; x++)
        {
            for (int y = 0; y < riverNoiseSize; y++)
            {
                int index = x * riverNoiseSize + y;
                distances[index] = Vector2.Distance(new Vector2Int(x, y), centroids[GetClosestCentroidIndex(new Vector2Int(x, y), centroids)]);
                if (distances[index] > maxDst)
                {
                    maxDst = distances[index];
                }
            }
        }

        for (int i = 0; i < distances.Length; i++)
        {
            float colorValue = distances[i] / maxDst;
            pixelColors[i] = new Color(colorValue, colorValue, colorValue, 1f);
        }
        return GetImageFromColorArray(pixelColors);
    }
    /* didn't actually need this
	float GetMaxDistance(float[] distances)
	{
		float maxDst = float.MinValue;
		for(int i = 0; i < distances.Length; i++)
		{
			if(distances[i] > maxDst)
			{
				maxDst = distances[i];
			}
		}
		return maxDst;
	}*/
    int GetClosestCentroidIndex(Vector2Int pixelPos, Vector2Int[] centroids)
    {
        float smallestDst = float.MaxValue;
        int index = 0;
        for (int i = 0; i < centroids.Length; i++)
        {
            if (Vector2.Distance(pixelPos, centroids[i]) < smallestDst)
            {
                smallestDst = Vector2.Distance(pixelPos, centroids[i]);
                index = i;
            }
        }
        return index;
    }
    Texture2D GetImageFromColorArray(Color[] pixelColors)
    {
        Texture2D tex = new Texture2D(riverNoiseSize, riverNoiseSize);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixelColors);
        tex.Apply();
        return tex;
    }
}
