using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bugUI : MonoBehaviour
{
    public Camera mc;

    void Awake ()
    {
        mc = Camera.main.GetComponent <Camera> ();
    }
}
