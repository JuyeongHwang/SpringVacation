using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Intro : MonoBehaviour
{
    [Header ("인트로 설정")]
    public float introDuration = 3f;

    public static MyGameManager_Intro Inst = null;
    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MyGameManager_Intro> ();
        }
        else
        {
            Destroy (gameObject);
        }
    }

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
