using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class storeManager : MonoBehaviour
{

    public Button levelBtn;
    public void ClickSell()
    {
        DataManager.Inst.money += DataManager.Inst.butterflyNum * DataManager.Inst.butterflyMuch;
        DataManager.Inst.butterflyNum = 0;
        UIManager_Result.Inst.SetButterflyNumber(0);
    }

    public void ClickLevelUp()
    {
        
        if(DataManager.Inst.money > 100 * (DataManager.Inst.level / 2))
        {
            DataManager.Inst.level += 1;
            DataManager.Inst.money -= 100 * (DataManager.Inst.level / 2);
        }
        else
        {
            levelBtn.interactable = false;
            //레벨 불가. 
        }
    }

}
