using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Result : MyGameManager
{
    [Header ("인트로 설정")]
    public float introDuration = 3f;

    void Start ()
    {
        StartCoroutine (IResult ());
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
