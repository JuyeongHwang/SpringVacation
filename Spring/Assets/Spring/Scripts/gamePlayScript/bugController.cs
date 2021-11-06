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

    // 이동관련함수
    public float bugMoveSpeed;
    public float bugMoveDist;
    public float bugMoveDelay;
    public float bugFloatingDist;
    public float bugRotSpeed;

    public IEnumerator ibugMove;

    protected const string groundLayerName = "Ground";

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

        // 이동
        BugMove ();
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

    // ============================== 이동 관련 =====================================
    // 전체적인 움직임은 코루틴으로 작동하도록 코드를 작성하였습니다

    void BugMove ()
    {
        if (ibugMove != null )
            StopCoroutine (ibugMove);

        ibugMove = IBugMove ();
        StartCoroutine (ibugMove);
    }

    IEnumerator IBugMove ()
    {
        while (true)
        {
            if (bugInfo_start != null)
            {   
                bugMoveSpeed = bugInfo_start.GetBugMoveSpeed ();
                bugMoveDist = bugInfo_start.GetBugMoveDistance ();
                bugMoveDelay = bugInfo_start.GetBugMoveDelay ();
                bugFloatingDist = bugInfo_start.GetBugFlyingDistanceFromGround ();
                bugRotSpeed = bugInfo_start.GetBugRotationSpeed ();
            }
            else
            {
                // 기본값
                bugMoveSpeed = 2.5f;
                bugMoveDist = 5f;
                bugMoveDelay = 3f;
                bugFloatingDist = 0.5f;
                bugRotSpeed = 0.25f;
            }

            // 애니메이션
            SetBugAnimatorTrigger ("Idle");

            // 딜레이만큼 대기
            yield return new WaitForSeconds (bugMoveDist);

            // 위치 설정
            Vector3 currentPos = gameObject.transform.position;
            Vector3 targetPos = currentPos;

            // 최대 n번 검사
            int checkN = 10;
            for (int i = 0; i < checkN; i++)
            {
                bool targetPosOK = false;

                // 현재 위치를 중앙으로 bugMoveDist만큼 사각형을 구역으로 가정하고
                // 그 구역안의 한 위치를 무작위로 설정한다
                targetPos.x += Random.Range (-bugMoveDist, bugMoveDist);
                targetPos.z += Random.Range (-bugMoveDist, bugMoveDist);

                // 위에서 레이를 쏴서 y 값 설정
                float detectLength = 100;
                int hitlayermask = 1 << LayerMask.NameToLayer (groundLayerName);

                Vector3 rayPos = targetPos;
                rayPos.y = detectLength;

                RaycastHit hit;
                if (Physics.Raycast (rayPos, Vector3.down, out hit, detectLength * 1.1f, hitlayermask))
                {
                    targetPos.y = hit.point.y + bugFloatingDist;
                }

                // 추후에 지형에 따른 검사 과정 추가
                {
                    targetPosOK = true;
                }

                if (targetPosOK)
                {
                    break;
                }
            }

            // 애니메이션
            SetBugAnimatorTrigger ("Move");
            
            while (Vector3.Distance (currentPos, targetPos) > 0.1f)
            {
                Vector3 moveDir = targetPos - currentPos;
                moveDir = moveDir.normalized;

                // 이동
                gameObject.transform.position += moveDir * bugMoveSpeed * Time.deltaTime;
                
                // 회전
                gameObject.transform.forward += moveDir * Time.deltaTime * bugRotSpeed;

                // 후처리
                currentPos = gameObject.transform.position;

                yield return null;
            }
        }
    }
}
