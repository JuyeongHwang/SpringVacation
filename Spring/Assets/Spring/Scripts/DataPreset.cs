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

public enum BugGrade
{
    S = 0
    , A = 1
    , B = 2
    , C = 3
    , D = 4
}

[System.Serializable]
public class DataBugGrade 
{
    [Header ("곤충 등급 정보")]
    [SerializeField]
    private BugGrade bugGrade;
    [SerializeField]
    private Color bugGradeColor;
    [SerializeField]
    private float bugSpeedRatio = 1f;
    [SerializeField]
    private float bugDelayRatio = 1f;
    [SerializeField]
    private float bugMoneyRatio = 1f;

    [SerializeField]
    private int bugGradePercentage = 10; 

    public BugGrade BUGGRADE
    {
        get {return bugGrade;}
    }

    public Color BUGGRADECOLOR
    {
        get {return bugGradeColor;}
    }

    public float BUGSPEEDRATIO
    {
        get {return bugSpeedRatio;}
    }

    public float BUGDELAYRATIO
    {
        get {return bugDelayRatio;}
    }

    public float BUGMONEYRATIO 
    {
        get {return bugMoneyRatio;}
    }

    public int BUGGRADEPERCENTAGE
    {
        get {return bugGradePercentage;}
    }
}

[System.Serializable]
[CreateAssetMenu(fileName = "Data Preset", menuName = "Scriptables/Data Preset")]
public class DataPreset : ScriptableObject
{
    [Header ("데이터 프리셋")]
    [SerializeField]
    protected DataInformation[] dataInformations;
    [Header ("곤충 등급 프리셋")]
    [SerializeField]
    protected DataBugGrade [] dataBugGrades;

    public DataInformation[] DATAINFORMATIONS
    {
        get {return dataInformations;}
    }

    public DataBugGrade[] DATABUGGRADES
    {
        get {return dataBugGrades;}
    }
}
