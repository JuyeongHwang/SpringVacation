using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Result : MonoBehaviour
{
    //[Header ("인트로 설정")]
    //public float introDuration = 3f;

    [Header ("캐릭터 설정")]
    public KidController_Result kidController_result;

    public static MyGameManager_Result Inst = null;
    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MyGameManager_Result> ();
        }
        else
        {
            Destroy (gameObject);
        }

        // kidController가 없으면 씬 내에서 찾는다
        if (kidController_result == null)
            kidController_result = FindObjectOfType <KidController_Result> ();
    }

    void Start ()
    {
        // 코루틴 수행
        //StartCoroutine (IResult ());

        // 캐릭터 애니메이션 수행
        //if (kidController != null)
        //    kidController.SetAnimatorTrigger ("Idle");
    
    }

    /*IEnumerator IResult ()
    {
        yield return new WaitForSecondsRealtime (introDuration);

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }*/

    public void UpgradeKid ()
    {
        kidController_result.SetToolIndex (DataManager.Inst.GetLevelIndex ());
        kidController_result.SetChildToolByIndex ();
        SetKidAnimatorTrigger ("Upgrade");
    }

    public void SetKidAnimatorTrigger (string trigger)
    {
        if (kidController_result == null)
            return;

        kidController_result.SetAnimatorTrigger (trigger);
    }
}
