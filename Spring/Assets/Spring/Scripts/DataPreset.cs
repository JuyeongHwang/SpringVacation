using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    // 레벨
    // 이동속도증가
    // 채집시간감소
    // 하루시간증가

[System.Serializable]
public class DataInformation 
{
    [Header ("데이터 정보")]
    [SerializeField]
    protected int level;
    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected float catchPower;
    [SerializeField]
    protected float dayDuration;
    [SerializeField]
    protected float requirementMoney;
    [SerializeField]
    protected int toolIndex;
    [SerializeField]
    protected Sprite toolImage;

    public int LEVEL
    {
        get {return level;}
    }

    public float MOVESPEED
    {
        get {return moveSpeed;}
    }

    public float CATCHPOWER
    {
        get {return catchPower;}
    }

    public float DAYDURATION
    {
        get {return dayDuration;}
    }

    public float REQUIREMENTMONEY
    {
        get {return requirementMoney;}
    }

    public int TOOLINDEX
    {
        get {return toolIndex;}
    }

    public Sprite TOOLIMAGE
    {
        get {return toolImage;}
    }
}

[System.Serializable]
[CreateAssetMenu(fileName = "Data Preset", menuName = "Scriptables/Data Preset")]
public class DataPreset : ScriptableObject
{
    [Header ("데이터 프리셋")]
    [SerializeField]
    protected DataInformation[] dataInformations;

    public DataInformation[] DATAINFORMATIONS
    {
        get {return dataInformations;}
    }
}
