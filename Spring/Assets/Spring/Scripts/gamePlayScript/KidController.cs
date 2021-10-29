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
    // 애니메이션 관련 변수들
    public GameObject kidBody;

    protected Animator kidAnimator;

    public KidState currentKidState;
    public KidState nextKidState;

    protected IEnumerator icatching;

    // 물리 관련 변수들
    protected CharacterController kidCharacterController;
    public float kidCharacterController_maxSpeed = 2.5f;
    public float kidCharacterController_maxSpeedRangeDist = 5f;
    public Vector3 kidCharacterController_velocity;
    public bool kidCharacterController_isGrounded;
    protected const float kidCharacterController_gravity = -10f;
    protected float kidCharacterController_currentGravity = -10f;
    protected const float kidCharacterController_detectGroundDist = 0.25f;
    protected const string kidCharacterController_groundName = "Ground";

    NavMeshAgent agent;

    [SerializeField]
    private List<GameObject> FoundBug;
    [SerializeField]
    private GameObject findObject; //나중에 struct로 더 다양한 곤충 관리 예정 //target
    public string TagName;
    [SerializeField]
    private float shortDis;


    //characterInfo
    //level, 수집물 리스트,, 등등
    float attackPower = 10f;
    bool isArrived = false;

    [SerializeField]
    private float speed;

    float time = 0.0f;
    void Awake ()
    {
        if (kidBody != null)
        {
            kidAnimator = kidBody.GetComponent <Animator> ();
        }

        kidCharacterController = GetComponent <CharacterController> ();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        UIManager_Gameplay.Inst.SetConditionText_Finding ();

        // 상태 설정
        currentKidState = KidState.NONE;
        nextKidState = KidState.IDLE;
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
            if (findObject != null) 
            {
                // 감지된 버그의 주변에 도달했을 때
                if (isArrived)
                {
                    attackBug();

                    // 상태 변화: 채집
                    nextKidState = KidState.CATCHING;
                }
                // 이동한다
                else
                {
                    agent.SetDestination(findObject.transform.position);
                    //Move ();
                    //MoveY ();
                }   
            }
            // 찾은 버그가 없으면
            else
            {
                detectBug();
                UIManager_Gameplay.Inst.SetConditionText_Finding ();
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
    }

    void LateUpdate ()
    {
        // 회전 후처리
        // 캐릭터의 버벅거림을 줄여주기 위함
        float rotY = gameObject.transform.eulerAngles.y;
        gameObject.transform.rotation = Quaternion.Euler (Vector3.up * rotY);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("충돌 시작!");
        
        if (collision.gameObject.CompareTag(TagName))
        {
            isArrived = true;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        Debug.Log("충돌 시작!");
        
        if (other.gameObject.CompareTag(TagName))
        {
            isArrived = true;
        }
    }

    // ================================== kid 관련 함수 =========================================

    void Move ()
    {
        // 회전
        Vector3 targetDir = findObject.transform.position - transform.position;
        float step = 1.0f * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
        Debug.DrawRay(transform.position, newDir, Color.red);
        transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);

        float dist = Mathf.Sqrt(Mathf.Abs(findObject.transform.position.x-transform.position.x)+
            Mathf.Abs(findObject.transform.position.y - transform.position.y)+
            Mathf.Abs(findObject.transform.position.z - transform.position.z)
            );
        //speed = 0.004f * (dist);
        //transform.position = Vector3.MoveTowards(transform.position, findObject.transform.position, speed);

        // 앞으로 이동
        float trueSpeed = kidCharacterController_maxSpeed;
        //float trueRatio = dist / kidCharacterController_maxSpeedRangeDist;
        //trueRatio = Mathf.Min (trueRatio, 1f);
        //trueSpeed = Mathf.Lerp (0, trueSpeed, trueRatio);

        kidCharacterController_velocity = newDir * trueSpeed;
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
        if (kidAnimator == null)
            return;

        kidAnimator.SetTrigger (triggerName);
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


    // ================================== bug 관련 함수 ============================================
    void detectBug()
    {
        FoundBug = new List<GameObject>(GameObject.FindGameObjectsWithTag(TagName));
        shortDis = Vector3.Distance(gameObject.transform.position,
            FoundBug[0].transform.position);

        findObject = FoundBug[0];

        foreach(GameObject found in FoundBug)
        {
            float Distance = Vector3.Distance(gameObject.transform.position, found.transform.position);
            if (Distance < shortDis)
            {
                findObject = found;
                shortDis = Distance;
            }
        }
        
        if(FoundBug.Count == 0)
        {
            makeBug();
        }
    }
    void makeBug()
    {
        Debug.Log("자동생성");
    }

    void attackBug()
    {
        UIManager_Gameplay.Inst.SetConditionText_Finded ();

        findObject.GetComponent<bugController>().bug.hp -= attackPower * Time.deltaTime *DataManager.Inst.level;

        if (findObject.GetComponent<bugController>().bug.hp <= 0.0f)
        {
            isArrived = false;
            Destroy(findObject.gameObject);

            // 데이터 매니져의 butterflyNum 추가
            if (DataManager.Inst != null)
            {
                DataManager.Inst.butterflyNum += 1;
            }

            // UI 갱신
            if (UIManager_Gameplay.Inst != null)
            {
                UIManager_Gameplay.Inst.UpdateButterflyNum();
            }
        }
    }
}
