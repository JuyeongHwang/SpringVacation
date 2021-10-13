using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Intro : MyGameManager
{
    [Header ("인트로 설정")]
    public float introDuration = 3f;

    void Start ()
    {
        StartCoroutine (IIntro ());
    }

    IEnumerator IIntro ()
    {
        yield return new WaitForSecondsRealtime (introDuration);

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }
}
