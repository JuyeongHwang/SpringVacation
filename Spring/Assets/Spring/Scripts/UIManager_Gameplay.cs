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
    public GameObject progressCircle;
    public string[] tips;

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
        SetProgressCircle(0);
        UpdateButterflyNum ();

        if (DataManager.Inst != null)
        {
            SetDayText (DataManager.Inst.day);
            SetProgressText(DataManager.Inst.GetProgress ());
        }
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
        int index = Random.Range(0, tips.Length);
        
        if (conditionText != null)
        {
            conditionText.text = "채집 중! \n" + tips[0];
        }
    }

    public void SetTimeText (float time)
    {
        if (timeText != null)
        {
            timeText.text = "남은 시간: " + ((int)time).ToString ();
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
        SetProgressCircle(percent);
    }

    public void SetProgressCircle(float percent)
    {
        progressCircle.GetComponent<Image>().fillAmount = percent/100;
    }

    public void UpdateButterflyNum ()
    {
        if (DataManager.Inst == null)
            return;

        if (numText != null)
        {
            //numberText.text = "채집한 나비: " + DataManager.Inst.butterflyNum.ToString () + " 마리";

            numText.text =   "채집한 곤충 가치: " + DataManager.Inst.money.ToString () + " ";
        }
    }
}
