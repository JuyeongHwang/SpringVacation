using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Inst = null;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MyGameManager> ();
        }
        else
        {
            Destroy (gameObject);
        }
    }
}
