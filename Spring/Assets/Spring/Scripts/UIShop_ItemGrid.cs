using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 아이템 구매 상태 (레벨부족, 구매하기, 구매됨)
public enum ItemGridState
{
    NOLEVEL, NOMONEY, BUY, HAVE
}

public class UIShop_ItemGrid : MonoBehaviour
{
    //[Header ("요구 레벨 세팅")]
    protected int itemGridLevelIndex;  // dataSetting의 levelIndex로부터 복사

    [Header ("컴포넌트 세팅")]
    public Image itemImage;
    public Text levelText;
    public Text moneyText;
    public Text buttonText;

    public ItemGridState itemGridState;

    private const string text_needMoreLevel = "레벨부족!";
    private const string text_needMoreMoney = "재화부족!";
    private const string text_buy = "구매하기!";
    private const string text_alreayHave = "구매함!";

    void Start ()
    {
        
    }

    public void SetLevelIndex (int i)
    {
        itemGridLevelIndex = i;
    }

    public void UpdateUI (int currentLevelIndex, float currentMoney)
    {
        SetItemImage (DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [itemGridLevelIndex].TOOLIMAGE);
        SetLevelText (DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [itemGridLevelIndex].LEVEL);
        SetMoneyText (DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [itemGridLevelIndex].REQUIREMENTMONEY);
        SetItemGridState (currentLevelIndex, currentMoney);
    }

    public void SetItemImage (Sprite img)
    {
        if (itemImage == null)
            return;

        itemImage.sprite = img;
    }

    public void SetLevelText (int level)
    {
        if (moneyText == null)
            return;

        levelText.text = "Level " + level.ToString ();
    }

    public void SetMoneyText (float money)
    {
        if (moneyText == null)
            return;

        moneyText.text = money.ToString ();
    }

    public void SetItemGridState (int levelIndex, float currentMoney)
    {
        // 레벨 부족
        if (levelIndex+1 < itemGridLevelIndex)
        {
            itemGridState = ItemGridState.NOLEVEL;
        }
        else if (levelIndex+1 == itemGridLevelIndex)
        {
            float money = DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [itemGridLevelIndex].REQUIREMENTMONEY;
            // 구매하기
            if (currentMoney >= money)
            {
                itemGridState = ItemGridState.BUY;
            }
            // 재화 부족
            else
            {
                itemGridState = ItemGridState.NOMONEY;
            }
        }
        // 구매함
        else
        {
            itemGridState = ItemGridState.HAVE;
        }

        if (buttonText == null)
            return;

        switch (itemGridState)
        {
            case ItemGridState.NOLEVEL:
            buttonText.text = text_needMoreLevel;
            break;

            case ItemGridState.NOMONEY:
            buttonText.text = text_needMoreMoney;
            break;

            case ItemGridState.BUY:
            buttonText.text = text_buy;
            break;

            case ItemGridState.HAVE:
            buttonText.text = text_alreayHave;
            break;

        }
    }

    // =================================== 클릭 이벤트 ===============================================

    public void ClickLevelUp ()
    {
        if (itemGridState != ItemGridState.BUY)
        {
            Debug.Log ("조건 불충족!");
            return;
        }
            
        // 구매요청
        if (DataManager.Inst != null)
        {
            DataManager.Inst.IncreaseLevelIndex ();
            DataManager.Inst.AddMoney (-1 * DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [itemGridLevelIndex].REQUIREMENTMONEY);
        }

        // UI 갱신 요청
        if (UIManager_Result.Inst != null)
        {
            UIManager_Result.Inst.UpdateItemGrids ();
            UIManager_Result.Inst.UpdateButterflyNum ();
        }

        // 캐릭터 애니메이션 요청
        if (MyGameManager_Result.Inst != null)
        {
            MyGameManager_Result.Inst.UpgradeKid ();
        }
    }
}
