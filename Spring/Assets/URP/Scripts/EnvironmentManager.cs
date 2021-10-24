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

    [Space (10)]
    public float checkDelay = 1f;
    [Header ("터레인 정보")]
    public CustomDelaunayTerrain currentCustomTerrain;
    public List <CustomDelaunayTerrain> customTerrains;

    [Header ("터레인 노이즈 설정: 지형")]
    public float terrainNoiseScale = 1f;
    public float terrainNoisePow = 1f;
    
    [Header ("터레인 노이즈 설정: 강물")]
    [Tooltip ("매 실행시 바뀌는 Texture")]
    public Texture2D riverNoiseTexture2D;
    public int riverNoiseSize = 1024;
	public int riverRegionAmount = 50;
    public float riverNoiseScale = 1f;
    public float riverNoiseCutoff = 0.3f;
    public float riverNoisePow = 1f;


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

        // 보로노이
        riverNoiseTexture2D = GetDiagramByDistance ();
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

    // ============================================ 터레인 사이즈 ====================================================
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

    // ========================================== 노이즈 관련 ==================================================

    // -0.5 ~ 0.5
    public float GetTerrainNoise (Vector2 uv)
    {
        float ret = Mathf.PerlinNoise (uv.x * terrainNoiseScale, uv.y * terrainNoiseScale) - 0.5f;
        ret = Mathf.Pow (ret, terrainNoisePow);
        return ret;
    }

    // 0 or 1
    public float GetRiverNoise (Vector2 uv)
    {
        Vector2 trueUV = new Vector2 (riverNoiseSize * 0.5f + (uv.x) * riverNoiseScale, riverNoiseSize * 0.5f + (uv.y) * riverNoiseScale);
        //trueUV.x = (trueUV.x + riverNoiseSize) % riverNoiseSize;
        //trueUV.y = (trueUV.y + riverNoiseSize) % riverNoiseSize; 

        Color col = riverNoiseTexture2D.GetPixel ((int)trueUV.x, (int)trueUV.y);
        float ret = col.r;
        //print (ret);

        if (ret > riverNoiseCutoff)
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
				distances[index] = Vector2.Distance(new Vector2Int(x,y), centroids[GetClosestCentroidIndex(new Vector2Int(x,y), centroids)]);
				if(distances[index] > maxDst)
				{
					maxDst = distances[index];
				}
			}	
		}

		for(int i = 0; i < distances.Length; i++)
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
		for(int i = 0; i < centroids.Length; i++)
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
