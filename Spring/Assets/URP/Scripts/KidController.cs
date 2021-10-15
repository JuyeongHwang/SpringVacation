using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KidController : MonoBehaviour
{
    public GameObject kidBody;
    protected Animator kidAnimator;

    void Awake ()
    {
        if (kidBody != null)
        {
            kidAnimator = kidBody.GetComponent <Animator> ();
        }
    }

    public void SetAnimatorTrigger (string triggerName)
    {
        if (kidAnimator == null)
            return;

        kidAnimator.SetTrigger (triggerName);
    }
}
