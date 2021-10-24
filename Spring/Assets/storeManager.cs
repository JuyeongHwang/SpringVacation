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
            if(DataManager.Inst.money > 100 * (DataManager.Inst.level / 2))
            {
                DataManager.Inst.level += 1;
                DataManager.Inst.money -= 100 * (DataManager.Inst.level / 2);
            }
            else
            {
                levelBtn.interactable = false;
                //���� �Ұ�. 
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
