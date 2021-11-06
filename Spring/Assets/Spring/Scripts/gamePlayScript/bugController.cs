using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bugController : MonoBehaviour
{
    //[HideInInspector]
    //public bugInfo bug;

    public bugInfo bugInfo_start;

    public float bugHP_current;
    public float bugHP_max;

    public bool bugAlive = true;
    public Slider hpBar;

    protected Animator bugAnimator;

    void Awake ()
    {
        bugAnimator = GetComponentInChildren <Animator> ();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 첫 활성화 시 1로 설정
        if (bugInfo_start != null)
        {
            bugHP_max = bugInfo_start.GetBugHP ();
        }
        else
        {   // 기본값
            bugHP_max = 100;
        }

        bugHP_current = bugHP_max;
        
        hpBar.value = 1f;
        bugAlive = true;

        SetBugAnimatorTrigger ("Idle");
    }

    private void Update()
    {
 
    }


    void LateUpdate ()
    {
        // 만약 죽으면
        if (bugAlive == false)
        {
            // 스스로 삭제하기
            Destroy (this.gameObject);
        }
    }

    public void SetBugAnimatorTrigger (string trigger)
    {
        if (bugAnimator == null)
            return;

        bugAnimator.SetTrigger (trigger);
    }

    // =============================== 이름 관련 ==================================

    public string GetBugName ()
    {
        if (bugInfo_start != null)
        {
            return bugInfo_start.GetBugName (); 
        }

        // 기본값
        return "UnknownBug";
    }
    // =============================== 체력 관련 ==================================

    // HP 설정은 아래 함수를 통해 설정
    public bool AddBugHP (float hp)
    {
        bugHP_current += hp;

        // hp바 값 설정
        if (hpBar != null)
        {
            hpBar.value = bugHP_current / bugHP_max;
        }

        // 만약 HP가 0이하라면 fales를 반환하고 isAlive를 false로 하기
        if (bugHP_current <= 0)
        {
            bugAlive = false;
            return false;
        }

        return true;
    }

    // HP 값 얻기
    // 생각해보니 유니티  c#에서 제공하는 get set 을 쓰면 되는데
    // c++ 폐해의 부작용로써 직접 get set 함수를 만들어서 사용하는것이기 때문에,,, 참고해주세요,,,,,,

    // 그리고 get set 쓰는 부분은 DataManager와 같이 싱글톤 매니저에서 사용하고 있습니다
    // --> private value;   public VALUE
    // 그 외 나머지 스크립트에서는 get set 함수를 만들어서 사용하고 있습니다
    public float GetBugHP ()
    {
        return bugHP_current;
    }
}
