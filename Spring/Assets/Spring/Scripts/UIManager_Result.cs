using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Result : MonoBehaviour
{
    [Header ("결과 UI 세팅")]
    public Text numberText;
    public Text dayText;

    [Header ("상점 UI 세팅")]
    protected List<UIShop_ItemGrid> itemGridUIs;
    public GameObject itemGridHolder;
    public GameObject itemGridPrefab;

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

        // Setting/DataPreset의 개수만큼 리스트 길이 설정 후 프리셋 생성

        if (itemGridUIs != null)
        {
            itemGridUIs.Clear ();
        }
        else
        {
            itemGridUIs = new List<UIShop_ItemGrid> ();
        }
        
        if (DataManager.Inst != null && itemGridHolder != null && itemGridPrefab != null)
        {
            int toolItemCount = DataManager.Inst.GetDataPreset ().DATAINFORMATIONS.Length;
            for (int i = 0; i < toolItemCount; i++)
            {
                GameObject grid = Instantiate (itemGridPrefab) as GameObject;
                grid.transform.SetParent (itemGridHolder.transform);
                grid.transform.localScale = Vector3.one;

                UIShop_ItemGrid gridUI = grid.GetComponent <UIShop_ItemGrid> ();

                if (gridUI == null)
                    continue;

                gridUI.SetLevelIndex (i);
                gridUI.UpdateUI (DataManager.Inst.GetLevelIndex (), DataManager.Inst.GetMoney ());

                itemGridUIs.Add (gridUI);
            }
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
            //numberText.text = "채집한 나비: " + DataManager.Inst.butterflyNum.ToString () + " 마리";

            numberText.text =  DataManager.Inst.money.ToString () + " 재화";
        }
    }

    public void UpdateDayText ()
    {
        if (dayText.text != null && DataManager.Inst != null)
        {
            dayText.text = "DAY " + DataManager.Inst.day.ToString ();
        }
    }

    public void UpdateItemGrids ()
    {
        for (int i = 0; i < itemGridUIs.Count; i++)
        {
            itemGridUIs [i].UpdateUI (DataManager.Inst.GetLevelIndex (), DataManager.Inst.GetMoney ());
        }
    }
}
