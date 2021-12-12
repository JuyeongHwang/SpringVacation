using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KidController_Ending : MonoBehaviour
{
    [Header ("애니메이터 설정")]
    // 시작하기 전에 사용자로부터 지정받아야 하도록 설정
    public Animator kidAnimator;
    public Animator[] kidToolAnimators; // 지정 받아야 하는 애니메이터
    protected Animator kidToolAnimator; // 인덱스를 이용한 현재 애니메이터
    public int kidToolIndex;

    void Start ()
    {
        SetChildToolByIndex ();
        SetAnimatorTrigger ("Bye");
    }

    public void SetAnimatorTrigger (string triggerName)
    {
        // 몸체 애니메이터
        if (kidAnimator == null)
            return;

        kidAnimator.SetTrigger (triggerName);

        // 툴 애니메이터
        if (kidToolAnimator == null)
            return;
        
        kidToolAnimator.SetTrigger (triggerName);
    }

    public void SetToolIndex (int index)
    {
        kidToolIndex = index;
    }

    public void SetChildToolByIndex ()
    {
        if (0 <= kidToolIndex && kidToolIndex < kidToolAnimators.Length
        && kidToolAnimators [kidToolIndex])
        {
            kidToolAnimator = kidToolAnimators [kidToolIndex];

            for (int i = 0; i < kidToolAnimators.Length; i++)
            {
                if (i == kidToolIndex)
                {
                    kidToolAnimators [i].gameObject.SetActive (true);
                }
                else
                {
                    kidToolAnimators [i].gameObject.SetActive (false);
                }
            }
        }
    }
}
