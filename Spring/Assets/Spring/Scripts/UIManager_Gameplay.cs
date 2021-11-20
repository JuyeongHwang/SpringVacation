using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Gameplay : MonoBehaviour
{
    [Header ("게임플레이 UI 세팅")]
    public Text conditionText;
    public Text timeText;
    public Text numText;
    public Text dayText;
    public Text progressText;

    public static UIManager_Gameplay Inst = null;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <UIManager_Gameplay> ();
        }
        else
        {
            Destroy (gameObject);
        }
    }

    void Start ()
    {
        SetConditionText_Finding ();
        SetTimeText (0);
        SetDayText (DataManager.Inst.day);
        SetProgressText(0);
        UpdateButterflyNum ();
    }

    public void SetConditionText_Finding ()
    {
        if (conditionText != null)
        {
            conditionText.text = "탐색 중";
        }
    }

    public void SetConditionText_Finded ()
    {
        if (conditionText != null)
        {
            conditionText.text = "스페이스바를 눌러 곤충을 잡으세요!";
        }
    }

    public void SetTimeText (float time)
    {
        if (timeText != null)
        {
            timeText.text = "남은시간: " + ((int)time).ToString ();
        }
    }

    public void SetDayText (int day)
    {
        if (dayText != null)
        {
            dayText.text = "DAY " + day.ToString ();
        }
    }

    public void SetProgressText(float percent)
    {
        if(progressText != null)
        {
            progressText.text = "진행율 : " + percent;
        }
    }

    public void UpdateButterflyNum ()
    {
        if (DataManager.Inst == null)
            return;

        if (numText != null)
        {
            //numberText.text = "채집한 나비: " + DataManager.Inst.butterflyNum.ToString () + " 마리";

            numText.text =  DataManager.Inst.money.ToString () + " 단위";
        }
    }
}
