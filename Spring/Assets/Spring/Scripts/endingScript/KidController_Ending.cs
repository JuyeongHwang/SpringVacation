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

    void Awake ()
    {
        // 위치 세팅
        RaycastHit hit;

        if (Physics.Raycast (gameObject.transform.position, Vector3.down, out hit, EnvManager.Inst.GetLayermaskValue_Ground ()))
        {
            gameObject.transform.position = hit.point;
        } 

        // 방향 세팅
        CameraHolder_Ending cam = FindObjectOfType <CameraHolder_Ending> ();

        Vector3 dir = cam.transform.position - gameObject.transform.position;
        dir.y = 0;
        dir = dir.normalized;

        gameObject.transform.forward = dir;
    }

    void Start ()
    {
        if (DataManager.Inst != null)
        {
            SetToolIndex (DataManager.Inst.GetLevelIndex ());
        }

        SetChildToolByIndex ();
        SetAnimatorTrigger ("Idle");

        if (EnvManager.Inst != null)
        {
            gameObject.transform.position = EnvManager.Inst.GetKidStartPoint ();
        }
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
