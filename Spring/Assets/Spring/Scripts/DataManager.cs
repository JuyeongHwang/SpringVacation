using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header ("데이터 정보")]
    [SerializeField]
   // public int butterflyNum = 0;
   // public int butterflyMuch = 100;
    public static DataManager Inst = null;
    public GameObject Env;
    //public GameObject Kid;

    [SerializeField]
    protected DataPreset dataPreset;

    public float money = 0.0f;
    // 레벨
    // 이동속도증가
    // 공격속도증가
    // 하루시간증가
    // 채집시간감소

    //public int level = 1; //level * time
    public int levelIndex = 0;  // 0 ~ maxLevel-1

    public int day = 1;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <DataManager> ();
        }
        else
        {
            Destroy (gameObject);
        }

        // 데이터매니져같은 경우 다른 씬에서도 사용되기 때문에
        // 씬이 옮겨져도 유지되어야 한다
        DontDestroyOnLoad (this.gameObject);
    }

    // 간단한 시스템이고 싱글톤이기도하니 get set 같은 함수를 이용하지 않고 잠시 주석처리 해놓았습니다
    /*public void AddButterflyNumber (int i)
    {
        butterflyNum += i;
    }

    public int GetButterflyNumber ()
    {
        return butterflyNum;
    }*/

    // ================================ 데이터 프리셋 =================================

    public DataPreset GetDataPreset ()
    {
        return dataPreset;
    }
    
    // ================================== 머니 관련 ==================================

    public float GetMoney ()
    {
        return money;
    }

    public void AddMoney (float add)
    {
        money += add;
        money = Mathf.Max (money, 0f);
    }

    // ================================== 레벨 관련 =================================

    public int GetLevelIndex ()
    {
        return levelIndex;
    }

    public void IncreaseLevelIndex ()
    {
        if (DataManager.Inst == null)
            return;

        levelIndex++;
        levelIndex = Mathf.Min (levelIndex, dataPreset.DATAINFORMATIONS.Length);
    }
}
