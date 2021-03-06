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
    public GameObject envObjectHolder;  // 자연물
    public GameObject artObjectHolder;  // 인공물
    public GameObject bugHolder;

    // 꼬마 시작 위치
    public GameObject kidStartpoint;
    

    [Header ("터레인 프리팹 설정")]     // 나무, 버그 등 프리팹은 EnvManger에서 관리
    public GameObject customTerrainPrefab;
    public int treeSeed = 30;
    public GameObject[] treePrefabs;
    public int flowerSeed = 100;
    public GameObject[] flowerPrefabs;
    public int rockSeed = 27;
    public GameObject[] rockPrefabs;
    public int bugSeed = 3;
    public GameObject[] bugPrefabs;
    public GameObject cliffPrefab;
    //public GameObject townPrefab;
    public GameObject housePrefab;
    public int houseNum = 7;
    public GameObject bridgePrefab;
    public int bridgeNum;   // 큰 강줄기
    public int bridgeNum1;  // 작은 강줄기
    public int bridgeRelaxNum;
    private List <GameObject> bridges;

    [Header("터레인 현재 정보")]
    public myDel_Terrain currentCustomTerrain;
    public List<myDel_Terrain> customTerrains;
    public List <EnvObject> envObjects;
    //public int nearTerrainDepth = 8;    // 재귀함수로 지형 생성을 위한 수치

    [Header("터레인 노이즈 설정: 지형")]
    public float terrainNoiseScale = 1f;
    public float terrainNoisePow = 1f;

    /*
    [Header("터레인 노이즈 설정: 강물")]
    [Tooltip("매 실행시 바뀌는 Texture")]
    public Texture2D riverNoiseTexture2D;
    public int riverNoiseSize = 1024;
    public int riverRegionAmount = 50;
    public float riverNoiseScale = 1f;
    public float riverNoisePow = 1f;
    */
    // 베지어 곡선을 사용하므로 보로노이를 이용한 생성은 주석처리하였습니다

    [Header ("터레인 기타 설정")]
    public KidController kidController;
    //int townNum = 2;
    
    [HideInInspector]
    public GameObject waterPlane;
    public static EnvManager Inst = null;
    

    [Header("Navmesh")]
    IEnumerator icheck;

    // 진행도를 체크하기 위한 변수
    public int totalGenTerrain;
    public int currentGenTerrain;

    protected const string groundName = "Ground";
    protected const string waterName = "Water";

    protected int layermask_ground;
    protected int layermask_water;

    public List <myDel_Terrain> instTerrains;
    //public List <myDel_Terrain> instTerrains_old;
    public List <myDel_Terrain> instTerrains_new;

    private void OnEnable() 
    {
        
    }

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

        // 리스트 초기화
        customTerrains = new List<myDel_Terrain>();
        // 보로노이
        //riverNoiseTexture2D = GetDiagramByDistance();
        
        DontDestroyOnLoad(gameObject);

        // 레이어마스크 인트 설정
        layermask_ground = 1 << LayerMask.NameToLayer (groundName);
        layermask_water = 1 << LayerMask.NameToLayer (waterName);
    }


    void Start()
    {
        BezierRiver();
        //InstantiateBridgeByBezierRiver (bridgeNum, bridgeNum1, bridgeRelaxNum);

        totalGenTerrain = (int)envSetting.GetMaxBoundarySize().x + (int)terrainUnitSize - (int)envSetting.GetMinBoundarySize().x;
        totalGenTerrain /= terrainUnitSize;
        totalGenTerrain *= totalGenTerrain;

        // 지형생성
        // 딜레이 생성
        StartInstTerrains ();
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


    public Vector3 P1;
    public Vector3 P2;
    public Vector3 P3;
    public Vector3 P4;
    //public Vector4 RiverRange = new Vector4(55, 75, 100, -50); //start x, end x, start z, end z
    public List<Vector3> BezierPoints = new List<Vector3>();
    public List<Vector3> BezierPoints2 = new List<Vector3>();
    public void BezierRiver()
    {
        List<Vector3> forRandom = new List<Vector3>();
        //P1 = new Vector3(RiverRange[0], 0, RiverRange[2]);
        //P2 = new Vector3(RiverRange[0], 0, RiverRange[3]);
        //P3 = new Vector3(RiverRange[1], 0, RiverRange[2]);
        //P4 = new Vector3(RiverRange[1], 0, RiverRange[3]);
        Vector3 n1 = new Vector3(envSetting.boundryCoord_min.x, 0,
            Random.Range(envSetting.boundryCoord_min.y, envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        Vector3 n2 = new Vector3(envSetting.boundryCoord_max.x + GetTerrainUnitSize (), 0,
            Random.Range(envSetting.boundryCoord_min.y, envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        Vector3 n3 = new Vector3(Random.Range(envSetting.boundryCoord_min.x, envSetting.boundryCoord_max.x + GetTerrainUnitSize ()), 0,
            (envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        Vector3 n4 = new Vector3(Random.Range(envSetting.boundryCoord_min.x, envSetting.boundryCoord_max.x + GetTerrainUnitSize ()), 0,
            (envSetting.boundryCoord_min.y));

        forRandom.Add(n1);forRandom.Add(n2); forRandom.Add(n3); forRandom.Add(n4);


        int[] index = { 0, 1, 2, 3 };
        for(int i = 0; i<10; i++)
        {
            int j1 = Random.Range(0, 4);
            int j2 = Random.Range(0, 4);
            int temp = index[j1];
            index[j1] = index[j2];
            index[j2] = temp;
        }

        P1 = forRandom[index[0]];
        P2 = forRandom[index[1]];
        P3 = forRandom[index[2]];
        P4 = forRandom[index[3]];


        for (float i = 0.0f; i < 1.0f; i += 0.01f)
        {
            Vector3 A = Vector3.Lerp(P1, P2, i);
            Vector3 B = Vector3.Lerp(P2, P3, i);
            Vector3 C = Vector3.Lerp(P3, P4, i);

            Vector3 D = Vector3.Lerp(A, B, i);
            Vector3 E = Vector3.Lerp(B, C, i);

            //최종 위치
            Vector3 F = Vector3.Lerp(D, E, i);

            BezierPoints.Add(F);
        }


        P1 = new Vector3(envSetting.boundryCoord_min.x, 0,
            Random.Range(envSetting.boundryCoord_min.y, envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        P3 = new Vector3(envSetting.boundryCoord_max.x + GetTerrainUnitSize (), 0,
            Random.Range(envSetting.boundryCoord_min.y, envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        P4 = new Vector3(Random.Range(envSetting.boundryCoord_min.x, envSetting.boundryCoord_max.x + GetTerrainUnitSize ()), 0,
            (envSetting.boundryCoord_max.y + GetTerrainUnitSize ()));
        P3 = new Vector3(Random.Range(envSetting.boundryCoord_min.x, envSetting.boundryCoord_max.x + GetTerrainUnitSize ()), 0,
            (envSetting.boundryCoord_min.y));
        for (float i = 0.0f; i < 1.0f; i += 0.005f)
        {
            Vector3 A = Vector3.Lerp(P1, P2, i);
            Vector3 B = Vector3.Lerp(P2, P3, i);
            Vector3 C = Vector3.Lerp(P3, P4, i);

            Vector3 D = Vector3.Lerp(A, B, i);
            Vector3 E = Vector3.Lerp(B, C, i);

            //최종 위치
            Vector3 F = Vector3.Lerp(D, E, i);

            BezierPoints2.Add(F);
        }
    }

    // 다리 만들기
    void InstantiateBridgeByBezierRiver (int num0, int num1, int relax)
    {
        // 생성
        int bridgeIndexPer = (BezierPoints.Count) / num0;
        bridges = new List<GameObject> ();

        for (int i = 0; i < num0; i ++)
        {
            //int bridgeIndex = Random.Range(50, BezierPoints.Count-50);
            //Vector3 bridgePos = BezierPoints[bridgeIndex];
            //Vector3 bridgePos2 = BezierPoints[bridgeIndex+1];

            int bridgeIndex = Random.Range (bridgeIndexPer * i, bridgeIndexPer * (i+1));
            bridgeIndex = Mathf.Min (bridgeIndex, (BezierPoints.Count));

            Vector3 bridgePos = Vector3.zero;
            Vector3 bridgePos2 = Vector3.zero;

            // 마지막 인덱스라면 다음 인덱스를 이용한 강의 방향을 구할 수 없다
            if (bridgeIndex == BezierPoints.Count-1)
            {
                bridgeIndex -= 1;
            }

            bridgePos = BezierPoints [bridgeIndex];
            bridgePos2 = BezierPoints [bridgeIndex + 1];

            //Vector3 bridge = bridgePos2 - bridgePos;
            //Vector3 angle_bridge = new Vector3(bridge.y, -bridge.x, bridge.z); ]

            // 오른쪽 방향
            Vector3 bridgeRight = bridgePos2 - bridgePos;
            bridgeRight = bridgeRight.normalized;

            // 앞 방향
            Vector3 bridgeDir = Vector3.Cross (Vector3.up, bridgeRight);

            GameObject bridgeObj;
            //float rotY = Random.Range(0, 360f);
            //rotY /= 90;

            bridgeObj = Instantiate(bridgePrefab, new Vector3(bridgePos.x, bridgePos.y, bridgePos.z), Quaternion.identity, artObjectHolder.transform);
            bridgeObj.transform.forward = bridgeDir;

            bridges.Add (bridgeObj);

            //Debug.Log(bridgePos);
        }   

        bridgeIndexPer = (BezierPoints2.Count) / num1;
        
        for (int i = 0; i < num1; i ++)
        {
            //int bridgeIndex = Random.Range(50, BezierPoints.Count-50);
            //Vector3 bridgePos = BezierPoints[bridgeIndex];
            //Vector3 bridgePos2 = BezierPoints[bridgeIndex+1];

            int bridgeIndex = Random.Range (bridgeIndexPer * i, bridgeIndexPer * (i+1));
            bridgeIndex = Mathf.Min (bridgeIndex, (BezierPoints2.Count));

            Vector3 bridgePos = Vector3.zero;
            Vector3 bridgePos2 = Vector3.zero;

            // 마지막 인덱스라면 다음 인덱스를 이용한 강의 방향을 구할 수 없다
            if (bridgeIndex == BezierPoints2.Count-1)
            {
                bridgeIndex -= 1;
            }

            bridgePos = BezierPoints2 [bridgeIndex];
            bridgePos2 = BezierPoints2 [bridgeIndex + 1];

            //Vector3 bridge = bridgePos2 - bridgePos;
            //Vector3 angle_bridge = new Vector3(bridge.y, -bridge.x, bridge.z); ]

            // 오른쪽 방향
            Vector3 bridgeRight = bridgePos2 - bridgePos;
            bridgeRight = bridgeRight.normalized;

            // 앞 방향
            Vector3 bridgeDir = Vector3.Cross (Vector3.up, bridgeRight);

            GameObject bridgeObj;
            //float rotY = Random.Range(0, 360f);
            //rotY /= 90;

            bridgeObj = Instantiate(bridgePrefab, new Vector3(bridgePos.x, bridgePos.y, bridgePos.z), Quaternion.identity, artObjectHolder.transform);
            bridgeObj.transform.forward = bridgeDir;

            bridges.Add (bridgeObj);

            //Debug.Log(bridgePos);
        }   

        float closeDist = 10f;
        float moveDist = 1f;

        bool exefor = false;

        // 릴렉스
        for (int i = 0; i < relax && exefor == false; i++)
        {
            exefor = false;

            for (int i0 = 0; i0 < bridges.Count; i0++)
            {
                Vector3 bpos0 = bridges [i0].transform.position;
                Vector3 bright0 = bridges [i0].transform.right;
                for (int i1 = 0; i1 < bridges.Count; i1++)
                {
                    if (i0 == i1)
                        continue;

                    Vector3 bpos1 = bridges [i1].transform.position;
                    Vector3 bright1 = bridges [i1].transform.right;

                    if (Vector3.Distance (bpos0, bpos1) <= closeDist)
                    {
                        Vector3 midpos = (bpos0 + bpos1) / 2;

                        Vector3 b0midDir = (bpos0 - midpos).normalized;
                        float b0sign = Mathf.Sign (Vector3.Dot (b0midDir, bright0));

                        Vector3 b1midDir = (bpos1 - midpos).normalized;
                        float b1sign = Mathf.Sign (Vector3.Dot (b1midDir, bright1));

                        bridges [i0].transform.position += bright0 * b0sign * moveDist;
                        bridges [i1].transform.position += bright1 * b1sign * moveDist;

                        exefor = true;
                    }
                }
            }
        }
    }

    void InstantiateHouse ()
    {
        // 생성 가능한 터레인 찾기
        List <myDel_Terrain> houseTerrains = new List<myDel_Terrain> ();

        for (int i = 0; i < customTerrains.Count; i++)
        {
            if (customTerrains [i].hasBeach == false
            && customTerrains [i].hasMountain == false
            && customTerrains [i].hasCliff == false
            && customTerrains [i].hasRiver == false)
            {
                houseTerrains.Add (customTerrains [i]);
            }
        }

        int houseNum_max = Mathf.Min (houseNum, houseTerrains.Count);

        while (houseNum_max > 0)
        {
            int randIndex = Random.Range (0, houseTerrains.Count);
            myDel_Terrain t = houseTerrains [randIndex];

            GameObject gtown;

            Vector3 pos = t.gameObject.transform.position;
            pos +=  new Vector3 (GetTerrainUnitSize () * 0.5f, 0, GetTerrainUnitSize () * 0.5f);

            float rotY = Random.Range(0, 360f);
            Quaternion rot = Quaternion.Euler (Vector3.up * rotY);
            //rotY /= 90;

            gtown = Instantiate(housePrefab, pos, rot, artObjectHolder.transform);

            // 마을로 시작 위치 설정
            if (kidStartpoint != null)
            {
                float upGap = 1f;
                kidStartpoint.gameObject.transform.position = pos + Vector3.up * upGap;
                kidStartpoint.gameObject.transform.position += gtown.gameObject.transform.forward * GetTerrainUnitSize () * 0.5f;
            }

            houseNum_max--;
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
                // 세팅이 없으면 생성하지 않음
                canGenerate = false;
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


                int itown = Random.Range(0, 2);
                bool hasTown = (itown == 0 ? false : true);

                if (!hasTown)
                {
                    int mountain = Random.Range(0, 2);
                    ret.hasMountain = (mountain == 0 ? false : true);
                }
                
                if (!ret.meetDown && !ret.meetLeft && !ret.meetRight && !ret.meetUp)
                {
                    ret.Generate();
                }
                else //할당받아야할 엣지가 하나라도 있는 경우
                {
                    
                    ret.GenerateForNear();
                }

                // 좌표
                float minX = ret.gameObject.transform.position.x ;
                float maxX = ret.gameObject.transform.position.x + GetTerrainUnitSize ();
                float minZ = ret.gameObject.transform.position.z ;
                float maxZ = ret.gameObject.transform.position.z + GetTerrainUnitSize ();

                //bool envRiver = false;
                ret.hasRiver = false;
                foreach (Vector3 point in BezierPoints)
                {
                    //float x = 0;
                    //float z = 0;

                    //x = (float)point.x / 50;
                    ///z = (float)point.z / 50;

                    //Debug.Log(Mathf.FloorToInt(x) + "     " + Mathf.FloorToInt(z));

                    //Vector3 pos = new Vector3(Mathf.FloorToInt(x) * 50, 0, Mathf.FloorToInt(z) * 50);
                    //Debug.Log(pos);

                    if (minX <= point.x && point.x < maxX
                    && minZ <= point.z && point.z < maxZ)
                    {
                        // print (point);
                        //envRiver = true;
                        ret.hasRiver = true;

                        //Instantiate (cliffPrefab, point, Quaternion.identity, ret.gameObject.transform);
                        break;
                    }
                }

                foreach (Vector3 point in BezierPoints2)
                {
                    if (ret.hasRiver == true)
                        break;
                    //float x = 0;
                    //float z = 0;

                    //x = (float)point.x / 50;
                    //z = (float)point.z / 50;

                    //Debug.Log(Mathf.FloorToInt(x) + "     " + Mathf.FloorToInt(z));

                    //Vector3 pos = new Vector3(Mathf.FloorToInt(x) * 50, 0, Mathf.FloorToInt(z) * 50);
                    //Debug.Log(pos);
                    //if (ret.transform.position.Equals(pos))
                    //{
                        //envRiver = true;
                    //   ret.hasRiver = true;
                    //}

                    if (minX <= point.x && point.x < maxX
                    && minZ <= point.z && point.z < maxZ)
                    {
                        //print (point);
                        ret.hasRiver = true;
                        //Instantiate (cliffPrefab, point, Quaternion.identity, ret.gameObject.transform);
                        break;
                    }
                }
                //Debug.Log(envRiver);

                //bool beach = false;
                //ret.hasBeach = false;
                // 해변가 판단
                if (ret.gameObject.transform.position.x < envSetting.boundryCoord_min.x + 30
                || ret.gameObject.transform.position.z < envSetting.boundryCoord_min.y + 30)
                {
                    //beach = true;
                    ret.hasBeach = true;
                }
                
                //int itown = Random.Range(0, 2);
                //bool hasTown = (itown == 0 ? false : true);
                //if (hasTown && townNum > 0 && !ret.hasRiver)
                //{
                //    townNum--;
                //    ret.canEdit = false;

                //    GameObject gtown;
                //    float rotY = Random.Range(0, 360f);

                //    gtown = Instantiate(townPrefab, instTerrainPos, Quaternion.Euler(Vector3.up * rotY), bugHolder.transform);
                //}

                // 부모를 해당 매니저로 설정한다 -> 청크 홀더로 수정
                g.gameObject.transform.SetParent(this.chunkHolder.transform);

                // 리스트에 추가
                customTerrains.Add(ret);
            }
        }

        return ret;
    }

    public myDel_Terrain GetTerrainHolderByPosition(Vector3 pos, NearTerrainDir2 dir)
    {
        float e = 0.01f;
        myDel_Terrain ret = null;

        switch (dir)
        {
            case NearTerrainDir2.UP:
                pos += Vector3.forward * GetTerrainUnitSize();
                break;

            case NearTerrainDir2.LEFT:
                pos -= Vector3.right * GetTerrainUnitSize();
                break;

            case NearTerrainDir2.RIGHT:
                pos += Vector3.right * GetTerrainUnitSize();
                break;

            case NearTerrainDir2.DOWN:
                pos -= Vector3.forward * GetTerrainUnitSize();
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
    public int GetTerrainUnitSize()
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
        //bool gen = false;

        while (true)
        {
            yield return new WaitForSeconds(checkDelay);

            // 조건을 while에서 검사하도록 수정
            if (kidController == null || EnvManager.Inst == null || currentCustomTerrain == null)
            {
                continue;
            }

            // 자기 자신의 지형 탐사
            if (currentCustomTerrain.isDiscovered == false)
            {
                currentCustomTerrain.isDiscovered = true;
                IncreaseTerrainNum_Current ();
            }

            //gen = false;

            float posXMin = currentCustomTerrain.gameObject.transform.position.x;
            float posXMax = posXMin + EnvManager.Inst.GetTerrainUnitSize();

            float posZMin = currentCustomTerrain.gameObject.transform.position.z;
            float posZSMax = posZMin + EnvManager.Inst.GetTerrainUnitSize();

            Vector3 kidPos = kidController.gameObject.transform.position;

            if (kidPos.x < posXMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_l;
                //currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                //gen = true;
            }
            else if (kidPos.x > posXMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_r;
                //currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                //gen = true;
            }
            else if (kidPos.z < posZMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_d;
               // currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                //gen = true;
            }
            else if (kidPos.z > posZSMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_u;
                //currentCustomTerrain.GenerateNearTerrain (nearTerrainDepth);
                //gen = true;
            }

            if (currentCustomTerrain.isDiscovered == false)
            {
                currentCustomTerrain.isDiscovered = true;
                IncreaseTerrainNum_Current ();
            }

            /*if (gen)
            {
                foreach (myDel_Terrain cdt in customTerrains)
                {
                    cdt.UpdateNearTerrain();
                }
            }*/
        }
    }

    // ========================================== 노이즈 관련 ==================================================

    // -0.5 ~ 0.5
    /*public float GetTerrainNoise(Vector2 uv)
    {
        float ret = Mathf.PerlinNoise(uv.x * terrainNoiseScale, uv.y * terrainNoiseScale) - 0.5f;
        ret = Mathf.Pow(ret, terrainNoisePow);
        return ret;
    }*/

    // 0 or 1
    /*public float GetRiverNoise(Vector2 uv)
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
    }*/

    // ========================================== 보로노이 ========================================================

    /*Texture2D GetDiagramByDistance()
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
	}
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
    }*/

    // ==================================== 진행도 관련 =======================================

    public void IncreaseTerrainNum_Current ()
    {
        currentGenTerrain += 1;
        UpdateProcessRate ();
    }

    public void UpdateProcessRate ()
    {
        if (DataManager.Inst != null)
        {
            DataManager.Inst.SetProgress (currentGenTerrain, totalGenTerrain);

            if (UIManager_Gameplay.Inst != null)
            {
                UIManager_Gameplay.Inst.SetProgressText(DataManager.Inst.GetProgress ());
            }
        }
    }

    // ======================================= EnvObject 추가 ==================================

    public void AddEnvObject (EnvObject envO)
    {  
        if (envO == null)
            return;

        envObjects.Add (envO);
    }

    public void DeleteEnvObject (EnvObject env0)
    {
        if (env0 == null)
            return;

        if (envObjects.Contains (env0))
        {
            envObjects.Remove (env0);
        }
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

    public List <EnvObject> GetEnvObjectsByPointAndRange (Vector3 point, float range)
    {
        List <EnvObject> ret = new List<EnvObject> ();

        foreach (EnvObject eo in envObjects)
        {
            if (Vector3.Distance (point, eo.gameObject.transform.position) < range)
            {
                ret.Add (eo);
            }
        }

        return ret;
    }

    // ====================================== 프리팹 관련 =========================================

    public GameObject Instantiate_EnvObject_Tree (Vector3 pos, bool isbeach)
    {
        GameObject ret = null;

        if (treePrefabs.Length > 0)
        {
            if (!isbeach)
            {
                int index = Random.Range(0, treePrefabs.Length - 1);

                ret = Instantiate(treePrefabs[index], pos, Quaternion.Euler(Vector3.zero), envObjectHolder.transform);
            }
            else
            {
                ret = Instantiate(treePrefabs[treePrefabs.Length-1], pos, Quaternion.Euler(Vector3.zero), envObjectHolder.transform);
            }
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

    public GameObject Instantiate_Bug (bool beach,Vector3 pos)
    {
        GameObject ret = null;
        int index = -1;
        if (bugPrefabs.Length > 0)
        {
            // 일반 땅 : Random.Range (0, bugPrefabs.Length-3), 
            // 해변가 : Random.Range (4, bugPrefabs.Length)
            if (beach)
            {
                index = Random.Range(4, bugPrefabs.Length);
            }
            else
            {
                index = Random.Range(0, bugPrefabs.Length-3);
            }
            
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

    // =================================== 레이어마스크 =======================================

    public int GetLayermaskValue_Ground ()
    {
        return layermask_ground;
    }

    public int GetLayermaskValue_Water ()
    {
        return layermask_water;
    }
    public LayerMask GetLayermask_Water ()
    {
        return LayerMask.GetMask (waterName);
    }

    // =================================== 시작지점 ===========================================

    public Vector3 GetKidStartPoint ()
    {
        Vector3 ret = Vector3.zero;

        if (kidStartpoint != null)
        {
            ret = kidStartpoint.gameObject.transform.position;
        }

        return ret;
    }

    // ==================================== 엔딩 확인 ==========================================

    public bool GetIsEnding ()
    {
        if (envSetting == null)
            return false;

        if (DataManager.Inst == null)
            return false;

        if (envSetting.GetIsEndingByCurrentProgress (DataManager.Inst.GetProgress ()))
        {
            return true;
        }

        return false;
    }

    // ===================================== 순차적 절차 생성 ===================================

    public void AddInstTerrainToList (myDel_Terrain mdt)
    {
        if (mdt == null)
            return;

        if (instTerrains_new.Contains (mdt) == false)
        {
            instTerrains_new.Add (mdt);
            //print ("Add");
        }
    }

    /*public void RemoveInstTerrainFromList (myDel_Terrain mdt)
    {
        if (mdt == null)
            return;

        if (instTerrains.Contains (mdt) == true)
        {
            instTerrains.Remove (mdt);
            print ("Remove");
        }
    }*/

    void StartInstTerrains ()
    {
        StartCoroutine (IStartInstTerrains ());
    }

    IEnumerator IStartInstTerrains ()
    {
        float firstDelay = 1f;
        float delay = .1f;

        yield return new WaitForSeconds (firstDelay);

        instTerrains = new List<myDel_Terrain> ();
        //instTerrains_old = new List<myDel_Terrain> ();
        instTerrains_new = new List<myDel_Terrain> ();

        currentCustomTerrain = InstantiateCustomTerrain (Vector3.zero, NearTerrainDir2.NONE);
        AddInstTerrainToList (currentCustomTerrain);

        for (int j = 0; j < instTerrains_new.Count; j++)
        {
            instTerrains.Add (instTerrains_new [j]);
        }

        while (instTerrains.Count > 0)
        {
            instTerrains_new.Clear ();

            yield return new WaitForSeconds (delay);

            for (int i = 0; i < instTerrains.Count; i++)
            {
                instTerrains [i].GenerateNearTerrain ();
            }

            instTerrains.Clear ();
            for (int j = 0; j < instTerrains_new.Count; j++)
            {
                instTerrains.Add (instTerrains_new [j]);
            }
        }

        instTerrains.Clear ();
        instTerrains_new.Clear ();

        // ================================ 다리 생성 ===================================

        yield return new WaitForSeconds (delay);

        InstantiateBridgeByBezierRiver (bridgeNum, bridgeNum1, bridgeRelaxNum);

        // ================================== 집 생성 =======================================

        yield return new WaitForSeconds (delay);

        InstantiateHouse ();
    }
}
