using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Env Setting", menuName = "Scriptables/Env Setting")]
public class EnvSetting : ScriptableObject
{
    [Header ("지형 설정")]
    [SerializeField]
    public Vector2 boundryCoord_min = new Vector2 (-100f, -100f);
    [SerializeField]
    public Vector2 boundryCoord_max = new Vector2 (50f, 50f);

    [Header ("엔딩 설정")]
    public float progressRate_ending = 75f;

    // 외부 값을 통해 생성 가능한지의 여부만 리턴하도록 설정
    public bool GetIsAbleToGenerate (Vector2 curtPos)
    {
        // 세팅값이 잘못됬으면
        if (boundryCoord_min.x >= boundryCoord_max.x
        || boundryCoord_min.y >= boundryCoord_max.y)
        {
            return true;
        }

        // 조건을 만족하면
        if (boundryCoord_min.x <= curtPos.x && curtPos.x <= boundryCoord_max.x
        && boundryCoord_min.y <= curtPos.y && curtPos.y <= boundryCoord_max.y)
        {
            return true;
        }

        return false;
    }

    public Vector2 GetMinBoundarySize()
    {
        return boundryCoord_min;
    }

    public Vector2 GetMaxBoundarySize()
    {
        return boundryCoord_max;
    }

    public bool GetIsEndingByCurrentProgress (float currentRate)
    {
        if (currentRate >= progressRate_ending)
            return true;

        return false;
    }
}
