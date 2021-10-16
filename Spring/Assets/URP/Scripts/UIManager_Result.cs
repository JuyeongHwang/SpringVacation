using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Result : MonoBehaviour
{
    [Header ("결과 UI 세팅")]
    public Text numberText;

    public static UIManager_Result Inst = null;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <UIManager_Result> ();
        }
        else
        {
            Destroy (gameObject);
        }
    }

    void Start ()
    {
        // 데이터 매니져로부터 데이터를 가져온다
        if (DataManager.Inst != null)
        {
            SetButterflyNumber (DataManager.Inst.GetButterflyNumber ());
        }   
        else
        {
            SetButterflyNumber (0);
        }
    }

    public void SetButterflyNumber (int num)
    {
        if (numberText != null)
        {
            numberText.text = "채집한 나비: " + num.ToString () + " 마리";
        }
    }
}
