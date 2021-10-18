using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NearTerrainDir 
{
    NONE = -1
    , UP, DOWN, LEFT, RIGHT
}

public class EnvironmentManager : MonoBehaviour
{
    [Header ("씬 환경 설정")]
    public int terrainUnitSize = 100;
    public int terrainUnitSize_gap = 25;
    public GameObject customTerrainPrefab;

    [Space (10)]
    public KidController kidController;
    //public float kidGenerationRange = 100;

    [Space (10)]
    public float checkDelay = 1f;

    [Header ("터레인 정보")]
    public CustomDelaunayTerrain currentCustomTerrain;
    public List <CustomDelaunayTerrain> customTerrains;

    public static EnvironmentManager Inst = null;

    IEnumerator icheck;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <EnvironmentManager> ();
        }
        else
        {
            Destroy (gameObject);
        }

        // 키드 찾기
        if (kidController == null)
            kidController = FindObjectOfType <KidController> ();

        // 리스트 초기화
        customTerrains = new List<CustomDelaunayTerrain> ();
    }

    void Start ()
    {
        currentCustomTerrain = InstantiateCustomTerrain (Vector3.zero, NearTerrainDir.NONE);
        currentCustomTerrain.GenerateNearTerrain ();

        // 주기마다 캐릭터 위치 체크 후 지형 생성
        CheckKidPosition ();
    }

    // 환경 매니져에서 인스턴스 수행
    // 그리고 해당 컴포넌트를 반환
    public CustomDelaunayTerrain InstantiateCustomTerrain (Vector3 pivotTerrainPos, NearTerrainDir terrainDir)
    {
        CustomDelaunayTerrain ret = null;

        if (customTerrainPrefab != null)
        {
            Vector3 instTerrainPos = pivotTerrainPos;

            switch (terrainDir)
            {
                case NearTerrainDir.UP:
                instTerrainPos += Vector3.forward * terrainUnitSize;
                break;

                case NearTerrainDir.DOWN:
                instTerrainPos -= Vector3.forward * terrainUnitSize;
                break;

                case NearTerrainDir.LEFT:
                instTerrainPos -= Vector3.right * terrainUnitSize;
                break;

                case NearTerrainDir.RIGHT:
                instTerrainPos += Vector3.right * terrainUnitSize;
                break;

            }  

            // 해당 터레인 설정
            GameObject g = Instantiate (customTerrainPrefab, instTerrainPos, Quaternion.identity);
            ret = g.GetComponent <CustomDelaunayTerrain> ();

            // 해당 터레인 위치 인덱스 설정 (인덱스 = 해당 위치 / 사이즈)
            //ret.customPos = gameObject.transform.position;
            //ret.indexX = (int)pivotTerrainPos.x / GetTerrainUnitSize_Original ();
            //ret.indexY = (int)pivotTerrainPos.y / GetTerrainUnitSize_Original ();

            // 부모를 해당 매니저로 설정한다
            g.gameObject.transform.SetParent (this.gameObject.transform);

            // 위치 설정 (로컬)
            //g.gameObject.transform.position = instTerrainPos;

            // 리스트에 추가
            customTerrains.Add (ret);
        }

        return ret;
    }

    public CustomDelaunayTerrain GetTerrainHolderByPosition (Vector3 pos, NearTerrainDir dir)
    {
        float e = 0.01f;
        CustomDelaunayTerrain ret = null;

        switch (dir)
        {
            case NearTerrainDir.UP:
            pos += Vector3.forward * GetTerrainUnitSize_Original ();
            break;

            case NearTerrainDir.LEFT:
            pos -= Vector3.right * GetTerrainUnitSize_Original ();
            break;

            case NearTerrainDir.RIGHT:
            pos += Vector3.right * GetTerrainUnitSize_Original ();
            break;

            case NearTerrainDir.DOWN:
            pos -= Vector3.forward * GetTerrainUnitSize_Original ();
            break;
        }

        for (int i = 0; i < customTerrains.Count; i++)
        {
            float dist = Vector3.Distance (pos, customTerrains [i].gameObject.transform.position);
            if (dist < e)
            {
                ret = customTerrains [i];
                break;
            }
        }

        return ret;
    }

    // 터레인 유닛 사이즈
    public int GetTerrainUnitSize_Original ()
    {
        return terrainUnitSize;
    }

    public int GetTerrainUnitSize ()
    {
        return terrainUnitSize + terrainUnitSize_gap;
    }

    void CheckKidPosition ()
    {
        if (icheck != null)
            StopCoroutine (icheck);

        icheck = ICheckKidPosition ();
        StartCoroutine (icheck);
    }

    IEnumerator ICheckKidPosition ()
    {
        bool gen = false;

        while (kidController != null && EnvironmentManager.Inst != null)
        {
            gen = false;

            float posXMin = currentCustomTerrain.gameObject.transform.position.x;
            float posXMax = posXMin + EnvironmentManager.Inst.GetTerrainUnitSize_Original ();

            float posZMin = currentCustomTerrain.gameObject.transform.position.z;
            float posZSMax = posZMin + EnvironmentManager.Inst.GetTerrainUnitSize_Original ();

            Vector3 kidPos = kidController.gameObject.transform.position;

            if (kidPos.x < posXMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_l;
                currentCustomTerrain.GenerateNearTerrain ();
                gen = true;
            }
            else if (kidPos.x > posXMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_r;
                currentCustomTerrain.GenerateNearTerrain ();
                gen = true;
            }
            else if (kidPos.z < posZMin)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_d;
                currentCustomTerrain.GenerateNearTerrain ();
                gen = true;
            }
            else if (kidPos.z > posZSMax)
            {
                currentCustomTerrain = currentCustomTerrain.nearTerrainHolder_u;
                currentCustomTerrain.GenerateNearTerrain ();
                gen = true;
            }

            if (gen)
            {
                foreach (CustomDelaunayTerrain cdt in customTerrains)
                {
                    cdt.UpdateNearTerrain ();
                }
            }

            yield return new WaitForSeconds (checkDelay);
        }
    }
}   
