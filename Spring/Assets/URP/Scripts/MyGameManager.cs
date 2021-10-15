using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Inst = null;

    protected KidController kidController;

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

        // 씬 내에 있는 꼬마를 불러온다
        kidController = FindObjectOfType <KidController> ();
    }
}
