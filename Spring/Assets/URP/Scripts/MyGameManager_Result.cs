using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Result : MyGameManager
{
    [Header ("인트로 설정")]
    public float introDuration = 3f;

    void Start ()
    {
        // 코루틴 수행
        StartCoroutine (IResult ());

        // 캐릭터 애니메이션 수행
        if (kidController != null)
            kidController.SetAnimatorTrigger ("Idle");
    }

    IEnumerator IResult ()
    {
        yield return new WaitForSecondsRealtime (introDuration);

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }
}
