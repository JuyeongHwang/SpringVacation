using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header ("데이터 정보")]
    [SerializeField]
    public int butterflyNum = 0;
    public int butterflyMuch = 100;
    public static DataManager Inst = null;


    public float money = 0.0f;
    public int level = 1; //level * time


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

    public void AddButterflyNumber (int i)
    {
        butterflyNum += i;
    }

    public int GetButterflyNumber ()
    {
        return butterflyNum;
    }
}
