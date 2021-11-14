using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
public enum KidState
{
    NONE = -1, IDLE, CATCHING
}

public class KidController : MonoBehaviour
{    
    //[Header ("모습 설정")]
    // 애니메이션 관련 변수들
    //public GameObject kidBody;

    [Header ("꼬마 시작 설정")]
    // 시작하기 전에 사용자로부터 지정받아야 하도록 설정
    public Animator kidAnimator;
    public Animator[] kidToolAnimators; // 지정 받아야 하는 애니메이터
    protected Animator kidToolAnimator; // 인덱스를 이용한 현재 애니메이터
    public GameObject kidTargetPoint;

    [Header ("상태 설정")]
    public KidState currentKidState;
    public KidState nextKidState;
    public bugController currentBugController;  // 현재 목표로하는 버그 컨트롤러 (일정거리에 상관없음)
    public Vector3 currentClickPos;             // 가장 마지막에 클릭된 위치
    public Button StopCatching;
    //bool stopcatching = false;

    protected IEnumerator icatching;

    // 물리 관련 변수들
    protected CharacterController kidCharacterController;
    public float kidCharacterController_moveSpeed = 2.5f;   // 기본속도
    //public float kidCharacterController_maxSpeedRangeDist = 5f;
    public float kidCharacterController_rotSpeed = 5f;
    public Vector3 kidCharacterController_velocity;
    public bool kidCharacterController_isGrounded;
    protected const float kidCharacterController_gravity = -10f;
    protected float kidCharacterController_currentGravity = -10f;
    protected const float kidCharacterController_detectGroundDist = 0.25f;
    protected const string kidCharacterController_groundName = "Ground";

    NavMeshAgent agent;

    //[SerializeField]
    //private List<GameObject> FoundBug;
    //[SerializeField]
    //private GameObject findObject; //나중에 struct로 더 다양한 곤충 관리 예정 //target
    //public string TagName;
    //[SerializeField]
    //private float shortDis;

    protected const string bugTagName = "Bug";


    //characterInfo
    //level, 수집물 리스트,, 등등
    float attackPower = 10f;    // 기본 잡기
    bool isArrived = false;

    [SerializeField]
    private float speed;

    float time = 0.0f;
    void Awake ()
    {
        //if (kidBody != null)
        //{
            //kidAnimator = kidBody.GetComponent <Animator> ();
        //}

        kidCharacterController = GetComponent <CharacterController> ();
        agent = GetComponent<NavMeshAgent>();

        SetChildToolByIndex (DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()]
        .TOOLINDEX);
    }

    private void OnEnable() 
    {
        // 활성화 시키면 달리는 상태로 시작
        SetAnimatorTrigger ("Run");
    }

    private void Start()
    {
        UIManager_Gameplay.Inst.SetConditionText_Finding ();

        // 상태 설정
        currentKidState = KidState.NONE;
        nextKidState = KidState.IDLE;

        // 이동 속도 및 공격 속도 설정
        if (DataManager.Inst != null)
        {
            kidCharacterController_moveSpeed = DataManager.Inst.GetDataPreset ()
            .DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].MOVESPEED;

            attackPower = DataManager.Inst.GetDataPreset ()
            .DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].CATCHPOWER;
        }

        // 곤충 찾기
        detectBug();
        UIManager_Gameplay.Inst.SetConditionText_Finding ();
    }

    private void Update()
    {
        time += Time.deltaTime;
        // 상태 설정에 따른 애니메이션 트리거 작동
        if (nextKidState != KidState.NONE && nextKidState != currentKidState)
        {
            currentKidState = nextKidState;
            nextKidState = KidState.NONE;

            switch (currentKidState)
            {
                case KidState.IDLE:
                SetAnimatorTrigger ("Run");

                if (icatching != null)
                    StopCoroutine (icatching);
                break;

                case KidState.CATCHING:
                // 코루틴으로 애니메이션설정
                if (icatching != null)
                    StopCoroutine (icatching);

                icatching = ISetAnimatorTrigger_Catching ();
                StartCoroutine (icatching);
                break;
            }
        }

        // 상태에 따른 행동요령
        switch (currentKidState)
        {
            // 아이들/달리기
            case KidState.IDLE:
            // 찾은 버그가 있으면
            if (currentBugController != null) 
            {
                // 감지된 버그의 주변에 도달했을 때
                if (isArrived)
                {
                    // 상태 변화: 채집
                    nextKidState = KidState.CATCHING;
                }
                // 이동한다
                else
                {
                    //agent.SetDestination(findObject.transform.position);
                    //Move();
                    //MoveY();
                }
            }
            // 찾은 버그가 없으면
            else
            {
                //detectBug();
                //UIManager_Gameplay.Inst.SetConditionText_Finding ();
            }
            break;

            // 채집중
            case KidState.CATCHING:
            // 채집중이라면
            if (isArrived) 
            {   
                attackBug ();
            }
            // 채집 완료하면
            else
            {   
                // 상태 변화: 이동
                nextKidState = KidState.IDLE;
            }
            break;
        }

        // 이동 검사를 위한 계산
        float distClose = 1f;
        float dist = Vector3.Distance (kidTargetPoint.transform.position, gameObject.transform.position);

        // 목표와 너무 가까이 있있거나 채집중이면 이동하지 않는다
        if (dist > distClose && !isArrived)
        {
            MoveY();
            Move();

            SetAnimatorMoveBlend (1f);
        }
        else
        {
            SetAnimatorMoveBlend (0f);
        }
        // 타킷 포인트 설정
        // 곤충으로 설정
        if (currentBugController != null)
        {
            kidTargetPoint.gameObject.transform.position = currentBugController.transform.position;
        }
        // 현재 쫒는 곤충이 없으면 마지막에 클린된 위치로 설정
        else
        {
            kidTargetPoint.gameObject.transform.position = currentClickPos;
        }
    }

    void LateUpdate ()
    {
        // 회전 후처리
        // 캐릭터의 버벅거림을 줄여주기 위함
        float rotY = gameObject.transform.eulerAngles.y;
        gameObject.transform.rotation = Quaternion.Euler (Vector3.up * rotY);
    }

    private void OnTriggerEnter(Collider other) 
    {
        bugController b = other.gameObject.GetComponent <bugController> ();

        if (b != null
            && currentBugController != null
            && other.gameObject == currentBugController.gameObject)
        {
            Debug.Log ("채집시작!");

            isArrived = true;
            b.SetColliderByBoolean (true);
        }
    }

    private void OnTriggerStay (Collider other) 
    {
        bugController b = other.gameObject.GetComponent <bugController> ();

        if (b != null
            && currentBugController != null
            && other.gameObject == currentBugController.gameObject)
        {
            Debug.Log ("채집시작!");

            isArrived = true;
            b.SetColliderByBoolean (true);
        }
    }

    private void OnTriggerExit (Collider other)
    {
        bugController b = other.gameObject.GetComponent <bugController> ();

        if (b != null
            && currentBugController != null
            && other.gameObject == currentBugController.gameObject)
        {
            Debug.Log ("도망간다!");

            isArrived = false;
            b.SetColliderByBoolean (false);
        }
    }

    // ================================== kid 관련 함수 =========================================

    // kidTargetPoint를 따라가도록 수정
    void Move ()
    {
        Vector3 newDir = gameObject.transform.forward;

        if (kidTargetPoint != null)
        {
            // 회전
            Vector3 targetDir = kidTargetPoint.transform.position - transform.position;
            targetDir.y = 0f;
            targetDir = targetDir.normalized;

            float step = kidCharacterController_rotSpeed * Time.deltaTime;
            newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
        }

        kidCharacterController_velocity = newDir * kidCharacterController_moveSpeed;
        kidCharacterController_velocity.y = kidCharacterController_currentGravity;

        // 최종 이동
        kidCharacterController.Move (kidCharacterController_velocity * Time.deltaTime);
    }

    void MoveY ()
    {
        // 탐지
        RaycastHit hit;
        int hitlayermask = 1 << LayerMask.NameToLayer (kidCharacterController_groundName);
        if (Physics.Raycast (transform.position, Vector3.down, out hit, kidCharacterController_detectGroundDist, hitlayermask))
        {
            kidCharacterController_isGrounded = true;
        }
        else
        {
            kidCharacterController_isGrounded = false;
        }

        // 탐지따라서 중력을 더해준다
        if (kidCharacterController_isGrounded == false)
        {
            kidCharacterController_currentGravity += Time.deltaTime * kidCharacterController_gravity;
        }
        else
        {
           kidCharacterController_currentGravity = 0f;
        }
    }

    // ================================= kid 관련 애니메이션 함수 ===========================================

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
    
    IEnumerator ISetAnimatorTrigger_Catching ()
    {
        float catchDelay = 1.5f;

        // 주기마다 스윙 트리거 작동
        while (true)
        {
            int rand = Random.Range (0, 2);
            if (rand == 0)
                SetAnimatorTrigger ("SwingA");
            else
                SetAnimatorTrigger ("SwingB");

            yield return new WaitForSeconds (catchDelay);
        }
    }

    public void SetAnimatorMoveBlend (float b)
    {
        // 몸체 애니메이터
        if (kidAnimator == null)
            return;

        kidAnimator.SetFloat ("MoveBlend", b);

        // 툴 애니메이터
        if (kidToolAnimator == null)
            return;

        kidToolAnimator.SetFloat ("MoveBlend", b);
    }


    // ================================== bug 관련 함수 ============================================

    // 사실상 한 프레임에서만 수행 될거기 때문에
    // 기존의 FoundBug 리스트 shortDist .. 등을 지역변수로 설정하였습니다
    void detectBug()
    {
        List<bugController> FoundBug = new List<bugController>(GameObject.FindObjectsOfType <bugController> ());
        float shortDist = Vector3.Distance(gameObject.transform.position,
            FoundBug[0].transform.position);

        currentBugController = FoundBug[0];

        foreach(bugController found in FoundBug)
        {
            float Distance = Vector3.Distance(gameObject.transform.position, found.transform.position);
            if (Distance < shortDist)
            {
                currentBugController = found;
                shortDist = Distance;
            }
        }
        
        /*if(FoundBug.Count == 0)
        {
            makeBug();
        }*/
    }

    /*void makeBug()
    {
        Debug.Log("자동생성");
    }*/

    void attackBug()
    {
        UIManager_Gameplay.Inst.SetConditionText_Finded ();


        if (currentBugController != null)
        { 
            // 체력을 깎고 살아있는지 bool 타입으로 리턴받음
            bool bugAlive = currentBugController.AddBugHP  (-attackPower * Time.deltaTime);

            // 리턴값이 false라면
            if (bugAlive == false)
            {
                isArrived = false;

                // 데이터 매니져의 재화 추가
                if (DataManager.Inst != null)
                {
                    DataManager.Inst.money += currentBugController.GetBugMoney ();
                }

                // UI 갱신
                if (UIManager_Gameplay.Inst != null)
                {
                    UIManager_Gameplay.Inst.UpdateButterflyNum();
                }
            }
        }
    }

    // ======================================= 도구관련 함수 ===================================================

    public void SetChildToolByIndex (int index)
    {
        if (0 <= index && index < kidToolAnimators.Length
        && kidToolAnimators [index] != null)
        {
            kidToolAnimator = kidToolAnimators [index];

            for (int i = 0; i < kidToolAnimators.Length; i++)
            {
                if (i == index)
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

    // ===================================== 클릭 (터치) 입력 관련 함수 ==============================================

    public void ClickFromBug (bugController bc)
    {
        if (bc == null)
            return;

        // 현재 곤충 초기화
        if (currentBugController != null)
            currentBugController.SetColliderByBoolean (false);

        currentBugController = bc;

        // 상태 초기화
        nextKidState = KidState.IDLE;
        isArrived = false;
    }

    public void ClickFromTerrain (Vector3 pos)
    {
         // 현재 곤충 초기화
        if (currentBugController != null)
            currentBugController.SetColliderByBoolean (false);

        // 곤충 없음
        currentBugController = null;

        currentClickPos = pos;

        // 상태 초기화
        nextKidState = KidState.IDLE;
        isArrived = false;
    }
}
