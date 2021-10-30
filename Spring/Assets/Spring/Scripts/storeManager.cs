using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class storeManager : MonoBehaviour
{
    public Button levelBtn;
    public void ClickSell()
    {
        if (DataManager.Inst != null)
        {
            DataManager.Inst.money += DataManager.Inst.butterflyNum * DataManager.Inst.butterflyMuch;
            DataManager.Inst.butterflyNum = 0;
        }
        
        if (UIManager_Result.Inst != null)
        {
            UIManager_Result.Inst.UpdateButterflyNum ();
        }
    }

    public void ClickLevelUp()
    {
        if (DataManager.Inst != null)
        {
            // µ¥ÀÌÅÍ¸Å´ÏÁ®¸¦ ÅëÇÑ ¿ä±¸ ¸Ó´Ï¿Í ÇöÀç ¸Ó´Ï¸¦ °¡Á®¿Í¼­ ÆÇ´Ü
            float currentMoney = DataManager.Inst.GetMoney ();
            float requirementMoney = DataManager.Inst.GetDataPreset ()
            .DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].REQUIREMENTMONEY;

            if(currentMoney >=  requirementMoney)
            {
                DataManager.Inst.IncreaseLevelIndex ();
                DataManager.Inst.AddMoney (requirementMoney);
            }
            else
            {
                levelBtn.interactable = false;
                //ï¿½ï¿½ï¿½ï¿½ ï¿½Ò°ï¿½. 
            }
        }
    }

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
