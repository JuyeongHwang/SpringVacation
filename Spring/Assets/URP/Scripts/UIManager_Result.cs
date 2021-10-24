using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Result : MonoBehaviour
{
    [Header ("결과 UI 세팅")]
    public Text numberText;
    public Text dayText;

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
        UpdateButterflyNum ();
        UpdateDayText ();
    }

    public void UpdateButterflyNum ()
    {
        if (numberText != null && DataManager.Inst != null)
        {
            numberText.text = "채집한 나비: " + DataManager.Inst.butterflyNum.ToString () + " 마리";
        }
    }

    public void UpdateDayText ()
    {
        if (dayText.text != null && DataManager.Inst != null)
        {
            dayText.text = "DAY " + DataManager.Inst.day.ToString ();
        }
    }
}
