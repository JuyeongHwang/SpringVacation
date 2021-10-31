using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI���� UIManager_Result, UIShop_ItemGrid .. �� ������ ������ �ִ� ������Ʈ

public class storeManager : MonoBehaviour
{
    public Button levelBtn;
    public void ClickSell()
    {
        if (DataManager.Inst != null)
        {
            //DataManager.Inst.money += DataManager.Inst.butterflyNum * DataManager.Inst.butterflyMuch;
            //DataManager.Inst.butterflyNum = 0;
        }
        
        if (UIManager_Result.Inst != null)
        {
            UIManager_Result.Inst.UpdateButterflyNum ();
            UIManager_Result.Inst.UpdateItemGrids ();
        }
    }

    /*public void ClickLevelUp(bool ableToBuy)
    {
        if (DataManager.Inst != null)
        {
            // ???????????? ???? ?? ???? ???? ???? ??????? ???
            float currentMoney = DataManager.Inst.GetMoney ();
            float requirementMoney = DataManager.Inst.GetDataPreset ()
            .DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].REQUIREMENTMONEY;

            if(currentMoney >=  requirementMoney)
            {
                DataManager.Inst.IncreaseLevelIndex ();
                DataManager.Inst.AddMoney (-requirementMoney);
            }
            else
            {
                levelBtn.interactable = false;
                //???? ???. 
            }
        }
    }*/

    public void ClickNextGame ()
    {
        if (DataManager.Inst != null)
        {
            DataManager.Inst.day += 1;
        }

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }

}
