using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class bugController : MonoBehaviour
{
    //[HideInInspector]
    //public bugInfo bug;
    [Header ("곤충 설정")]
    public bugInfo bugInfo_start;
    public bugUI bugUII;    // 클래스명과 혼동을 피하기 위한 오타
    //public Slider hpBar;
    protected Animator bugAnimator;

    public BoxCollider bugCollider_min;
    public BoxCollider bugCollider_max;

    //public Material bugOutlineMaterial;
    public Renderer bugRenderer;    //[0]: 본체, [1]: 아웃라인


    [Header ("곤충 현재 상태")]

    public float bugHP_current;
    public float bugHP_max;
    public bool bugAlive = true;
    public bool bugCollider = false;    // in: true, out: false
    public BugGrade bugGrade;
    public Color bugGradeColor;
    public float bugGradeMoveRatio;
    public float bugGradeDelayRatio;
    public float bugGradeMoneyRatio;
    

    // 이동관련함수
    public float bugMoveSpeed;
    //public float bugMoveDist;
    public float bugMoveDelay;
    //public float bugFloatingDist;
    public float bugRotSpeed;
    public EnvObject currentEnvObject;

    private IEnumerator ibugMove;

    protected const string groundLayerName = "Ground";

    void Awake ()
    {
        bugAnimator = GetComponentInChildren <Animator> ();
    }

    private void OnEnable() 
    {
        // 이동
        BugMove ();
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

        // 기본 정보 설정
        bugHP_current = bugHP_max;
        bugAlive = true;
        
        // 등급 설정
        if (DataManager.Inst != null)
        {
            bugGrade = DataManager.Inst.GetBugGradeRandomly ();
            bugGradeColor = DataManager.Inst.GetDataPreset ().DATABUGGRADES [(int)bugGrade].BUGGRADECOLOR;
            bugGradeMoveRatio = DataManager.Inst.GetDataPreset ().DATABUGGRADES [(int)bugGrade].BUGSPEEDRATIO;
            bugGradeDelayRatio = DataManager.Inst.GetDataPreset ().DATABUGGRADES [(int)bugGrade].BUGDELAYRATIO;
            bugGradeMoneyRatio = DataManager.Inst.GetDataPreset ().DATABUGGRADES [(int)bugGrade].BUGMONEYRATIO;

            // 등급에 따라 색 설정
            Material retMat = Instantiate (bugRenderer.materials [1]);
            bugRenderer.materials [1].SetColor ("_OutlineColor", bugGradeColor);
        }
        
        // 콜라이더 설정
        SetColliderByBoolean (bugCollider = false);

        // UI 설정
        if (bugUII != null)
        {
            bugUII.SetBugUIInfo (bugInfo_start.GetBugName (), bugGrade);
        }
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

        float posY = 2f;
        float detectY = 5f;
        int hitlayermask = 1 << LayerMask.NameToLayer (groundLayerName);

        // 위에서 레이를 쏴서 y 위치 조절
        Vector3 pos = gameObject.transform.position;
        Vector3 rayPos = pos + Vector3.up * posY;

        RaycastHit hit;
        if (Physics.Raycast (rayPos, Vector3.down, out hit, detectY, hitlayermask))
        {
            // 위치 설정
            gameObject.transform.position = hit.point;
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
        if (bugUII != null)
        {
            bugUII.SetBugHPValue (bugHP_current / bugHP_max);
            bugUII.SetActiveBugUI_On ();
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

    bool HaveEnvObject ()
    {
        if (currentEnvObject != null)
            return true;

        return false;
    }

    void BugMove ()
    {
        if (ibugMove != null )
            StopCoroutine (ibugMove);

        ibugMove = IBugMove ();
        StartCoroutine (ibugMove);
    }

    IEnumerator IBugMove ()
    {
        // envObjec를 가질때 까지 대기
        yield return HaveEnvObject ();

        while (true)
        {
            if (bugInfo_start != null)
            {   
                bugMoveSpeed = bugInfo_start.GetBugMoveSpeed ();
                //bugMoveDist = bugInfo_start.GetBugMoveDistance ();
                bugMoveDelay = bugInfo_start.GetBugMoveDelay ();
               // bugFloatingDist = bugInfo_start.GetBugFlyingDistanceFromGround ();
                bugRotSpeed = bugInfo_start.GetBugRotationSpeed ();
            }
            else
            {
                // 기본값
                bugMoveSpeed = 2.5f;
                //bugMoveDist = 5f;
                bugMoveDelay = 3f;
                //bugFloatingDist = 0.5f;
                bugRotSpeed = 0.25f;
            }

            // 등급에 따른 계산
            bugMoveSpeed *= bugGradeMoveRatio;
            bugMoveDelay *= bugGradeDelayRatio;

            // 위치 설정
            Vector3 currentPos = gameObject.transform.position;
            Vector3 targetPos = currentPos;

            // 이동 과정 수정
            if (EnvManager.Inst != null)
            {
                currentEnvObject = EnvManager.Inst.GetEnvObjectByCase (bugInfo_start.GetBugFavoriteEnvObjectType (), currentEnvObject, currentPos);
            }
            
            if (currentEnvObject != null)
            {
                targetPos = currentEnvObject.gameObject.transform.position;
            }

            // 애니메이션
            SetBugAnimatorTrigger ("Move");
            
            while (currentEnvObject != null && Vector3.Distance (currentPos, targetPos) > 0.1f)
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

            // 애니메이션
            SetBugAnimatorTrigger ("Idle");

            // 딜레이만큼 대기
            yield return new WaitForSeconds (bugMoveDelay);
        }
    }

    // ============================= 돈 ===========================================

    public float GetBugMoney ()
    {
        if (bugInfo_start != null)
        {
            // 등급 계산
            return bugInfo_start.GetBugMoney () * bugGradeMoneyRatio;
        }

        return 250 * bugGradeMoneyRatio;
    }

    // ============================ 콜라이더 설정 =====================================

    // 첫 시작 Start 함수
    // 혹은 KidCollider에서 수행    
    public void SetColliderByBoolean (bool b)
    {
        if (bugCollider_max == null || bugCollider_min == null)
            return;

        // IN
        if (b == true)
        {
            bugCollider_max.enabled = true;
            bugCollider_min.enabled = false;
        }
        // OUT
        else
        {
            bugCollider_max.enabled = false;
            bugCollider_min.enabled = true;
        }
    }

    // ============================ 마우스 클릭 =========================================

    // 마우스 클릭으로 타킷 변경
    private void OnMouseDown() 
    {
        if (MyGameManager_Gameplay.Inst != null)
        {
            MyGameManager_Gameplay.Inst.ClickFromBug (this);
        }
    }
}
