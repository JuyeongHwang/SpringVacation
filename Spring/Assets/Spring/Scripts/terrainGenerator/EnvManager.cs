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
    [Header("터레인 설정")]
    public int terrainUnitSize = 100;
    public EnvSetting envSetting;   // 프리셋 개념
    public float checkDelay = 1f;   // 체크 주기


    [Header ("터레인 홀더 설정")]       // 생성된 프리팹의 인스턴스를 모아주는 역할
    public GameObject chunkHolder;
    public GameObject envObjectHolder;
    public GameObject bugHolder;
    

    [Header ("터레인 프리팹 설정")]     // 나무, 버그 등 프리팹은 EnvManger에서 관리
    public GameObject customTerrainPrefab;
    public int treeSeed = 37;
    public GameObject[] treePrefabs;
    public int flowerSeed = 17;
    public GameObject[] flowerPrefabs;
    public int rockSeed = 51;
    public GameObject[] rockPrefabs;
    public int bugSeed = 43;
    public GameObject[] bugPrefabs;
    public GameObject cliffPrefab;

    [Header("터레인 현재 정보")]
    public myDel_Terrain currentCustomTerrain;
    public List<myDel_Terrain> customTerrains;
    public List <EnvObject> envObjects;
    public int nearTerrainDepth = 2;    // 재귀함수로 지형 생성을 위한 수치

    [Header("터레인 노이즈 설정: 지형")]
    public float terrainNoiseScale = 1f;
    public float terrainNoisePow = 1f;

    [Header("터레인 노이즈 설정: 강물")]
    [Tooltip("매 실행시 바뀌는 Texture")]
    public Texture2D riverNoiseTexture2D;
    public int riverNoiseSize = 1024;
    public int riverRegionAmount = 50;
    public float riverNoiseScale = 1f;
    public float riverNoisePow = 1f;

    [Header ("터레인 기타 설정")]
    public KidController kidController;
    
    [HideInInspector]
    public GameObject waterPlane;
    public static EnvManager Inst = null;
    

    [Header("Navmesh")]
    IEnumerator icheck;

    public int totalGenTerrain;

    void Awake()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent<EnvManager>();
        }
        else
        {
            Destroy(gameObject);
        }

        // 키드 찾기
        if (kidController == null)
            kidController = FindObjectOfType<KidController>();

        // 리스트 초기화
        customTerrains = new List<myDel_Terrain>();
        // 보로노이
        riverNoiseTexture2D = GetDiagramByDistance();
        
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {

        totalGenTerrain = (int)envSetting.GetMaxBoundarySize().x + (int)terrainUnitSize - (int)envSetting.GetMinBoundarySize().x;
        totalGenTerrain /= terrainUnitSize;
        totalGenTerrain *= totalGenTerrain;


        // 지형생성
        currentCustomTerrain = InstantiateCustomTerrain(Vector3.zero, NearTerrainDir2.NONE);
        currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);

        // 주기마다 캐릭터 위치 체크 후 지형 생성
        if (kidController != null)
        {
            CheckKidPosition();
        }
    }

    // 환경 매니져에서 인스턴스 수행
    // 그리고 해당 컴포넌트를 반환
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

            // 지역변수로 설정
            bool canGenerate;

            if (envSetting != null)
            {
                canGenerate = envSetting.GetIsAbleToGenerate (new Vector2 (instTerrainPos.x, instTerrainPos.z));
            }
            else
            {
                // 세팅이 없으면 무제한
                canGenerate = true;
            }

            if (canGenerate)
            {
                // 해당 터레인 설정
                GameObject g = Instantiate(customTerrainPrefab, instTerrainPos, Quaternion.identity);

                ret = g.GetComponent<myDel_Terrain>();


                foreach (myDel_Terrain ct in customTerrains)
                {

                    if (ret.meetDown && ret.meetLeft && ret.meetRight && ret.meetUp) { break; }

                    if (!ret.meetRight && instTerrainPos + Vector3.right * terrainUnitSize == ct.gameObject.transform.position)
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

                int mountain = Random.Range(0, 2);
                //Debug.Log(mountain);
                ret.hasMountain = (mountain == 0 ? false : true);

                if (!ret.meetDown && !ret.meetLeft && !ret.meetRight && !ret.meetUp)
                {
                    ret.Generate();
                }
                else //할당받아야할 엣지가 하나라도 있는 경우
                {
                    
                    ret.GenerateForNear();
                }

                // 부모를 해당 매니저로 설정한다 -> 청크 홀더로 수정
                g.gameObject.transform.SetParent(this.chunkHolder.transform);

                // 리스트에 추가
                customTerrains.Add(ret);
            }
        }

        Debug.Log(customTerrains.Count  +  " " +totalGenTerrain);
        UIManager_Gameplay.Inst.SetProgressText(customTerrains.Count / (float)totalGenTerrain * 100);

        return ret;
    }

    private void Update()
    {
        // 컨트롤러를 찾을 때 까지 반복
        if (kidController == null)
        {
            kidController = FindObjectOfType <KidController> ();

            // 찾은 프레임에서 코루틴 수행
            if (kidController != null)
            {
                CheckKidPosition ();
            }
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

    // ============================================ 터레인 사이즈 ====================================================
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

        while (true)
        {
            // 조건을 while에서 검사하도록 수정
            if (kidController == null || EnvManager.Inst == null || currentCustomTerrain == null)
            {
                continue;
            }

            gen = false;

            float posXMin = currentCustomTerrain.gameObject.transform.position.x;
            float posXMax = posXMin + EnvManager.Inst.GetTerrainUnitSize_Original();

            float posZMin = currentCustomTerrain.gameObject.transform.position.z;
            float posZSMax = posZMin + EnvManager.Inst.GetTerrainUnitSize_Original();

            Vector3 kidPos = kidController.gameObject.transform.position;

            if (kidPos.x < posXMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_l;
                currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                gen = true;
            }
            else if (kidPos.x > posXMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_r;
                currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                gen = true;
            }
            else if (kidPos.z < posZMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_d;
                currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                gen = true;
            }
            else if (kidPos.z > posZSMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_u;
                currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
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

    // ========================================== 노이즈 관련 ==================================================

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

    // ========================================== 보로노이 ========================================================

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

    // ======================================= EnvObject 추가 ==================================

    public void AddEnvObject (EnvObject envO)
    {  
        if (envO == null)
            return;

        envObjects.Add (envO);
    }

    public EnvObject GetEnvObjectByCase (EnvObjectType fav, EnvObject originEnvObject, Vector3 originPos)
    {
        float distMax = 10;
        float dist0 = 1000;

        // 가장 가까운 것 및 리턴할 것
        EnvObject ee = null;

        // 후보 리스트
        List <EnvObject> ees = new List<EnvObject> ();

        // 모든 ???? 환경 오브젝트를 순회하며 검사한다
        foreach (EnvObject e in envObjects)
        {
            // 혹시라도 모를 null값이 들어왔을 경우
            if (e == null)
                continue;

            // 조건 부적합
            if (e.GetEnvObjectType () != fav)
            {
                continue;
            }

            // 조건 부적합
            if (originEnvObject == e)
            {
                continue;
            }

            float dist = Vector3.Distance (e.gameObject.transform.position, originPos);

            // 후보 등록
            if (distMax > dist)
            {
                ees.Add (e);
            }

            // 가장 가까운거
            if (dist0 > dist)
            {
                dist0 = dist;
                ee = e;  
            }
        }

        // 후보리스트가 있으면 랜덤
        if (ees.Count != 0)
        {
            ee = ees [Random.Range (0, ees.Count)];
        }

        // 없으면 가장 가까운것
        
        return ee;
    }

    // ====================================== 프리팹 관련 =========================================

    public GameObject Instantiate_EnvObject_Tree (Vector3 pos)
    {
        GameObject ret = null;

        if (treePrefabs.Length > 0)
        {
            int index = Random.Range (0, treePrefabs.Length);

            ret = Instantiate (treePrefabs [index], pos, Quaternion.Euler (Vector3.zero), envObjectHolder.transform);
        }

        return ret;
    }

    public GameObject Instantiate_EnvObject_Flower (Vector3 pos)
    {
        GameObject ret = null;

        if (flowerPrefabs.Length > 0)
        {
            int index = Random.Range (0, flowerPrefabs.Length);

            ret = Instantiate (flowerPrefabs [index], pos, Quaternion.Euler (Vector3.zero), envObjectHolder.transform);
        }

        return ret;
    }

    public GameObject Instantiate_EnvObject_Rock (Vector3 pos)
    {
        GameObject ret = null;

        if (rockPrefabs.Length > 0)
        {
            int index = Random.Range (0, rockPrefabs.Length);

            ret = Instantiate (rockPrefabs [index], pos, Quaternion.Euler (Vector3.zero), envObjectHolder.transform);
        }

        return ret;
    }

    public GameObject Instantiate_Bug (Vector3 pos)
    {
        GameObject ret = null;

        if (bugPrefabs.Length > 0)
        {
            int index = Random.Range (0, bugPrefabs.Length);
            float rotY = Random.Range (0, 360f);

            ret = Instantiate (bugPrefabs [index], pos, Quaternion.Euler (Vector3.up * rotY), bugHolder.transform);
        }

        return ret;
    }

    public int GetTreeSeed ()
    {
        return treeSeed;
    }

    public int GetFlowerSeed ()
    {
        return flowerSeed;
    }

    public int GetRockSeed ()
    {
        return rockSeed;
    }

    public int GetBugSeed ()
    {
        return bugSeed;
    }
}
