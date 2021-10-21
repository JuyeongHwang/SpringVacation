using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KidController : MonoBehaviour
{
    public GameObject kidBody;
    protected Animator kidAnimator;
    protected CharacterController kidCharacterController;
    public float kidCharacterController_maxSpeed = 2.5f;
    public float kidCharacterController_maxSpeedRangeDist = 5f;
    public Vector3 kidCharacterController_velocity;
    public bool kidCharacterController_isGrounded;
    protected const float kidCharacterController_gravity = -10f;
    protected float kidCharacterController_currentGravity = -10f;
    protected const float kidCharacterController_detectGroundDist = 0.25f;
    protected const string kidCharacterController_groundName = "Ground";


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

    void Awake ()
    {
        if (kidBody != null)
        {
            kidAnimator = kidBody.GetComponent <Animator> ();
        }

        kidCharacterController = GetComponent <CharacterController> ();
    }

    private void Start()
    {
        detectBug();
        //conditionText.text = "탐색 중";
        UIManager_Gameplay.Inst.SetConditionText_Finding ();
    }

    private void Update()
    {
        // 찾은 버그가 있으면
        if (findObject != null) 
        {
            //Debug.Log(butterfly.name);
            
            // 감지된 버그의 주변에 도달했을 때
            if (isArrived)
            {
                attackBug();
            }
            // 이동한다
            else
            {
                Move ();
                MoveY ();
            }   
        }
        // 찾은 버그가 없으면
        else
        {
            //conditionText.text = "탐색 중";
            detectBug();
            UIManager_Gameplay.Inst.SetConditionText_Finding ();
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

    public void SetAnimatorTrigger (string triggerName)
    {
        if (kidAnimator == null)
            return;

        kidAnimator.SetTrigger (triggerName);
    }


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
        //Debug.Log("스페이스바를 눌러 공격하세요~!");
        //conditionText.text = "스페이스바를 눌러 곤충을 잡으세요!";
        UIManager_Gameplay.Inst.SetConditionText_Finded ();

        if (Input.GetKeyDown(KeyCode.Space))
        {

            findObject.GetComponent<bugController>().bug.hp -= attackPower;
        }

        if (findObject.GetComponent<bugController>().bug.hp <= 0.0f)
        {
            isArrived = false;
            Destroy(findObject.gameObject);

            // 데이터 매니져의 butterflyNum 추가
            if (DataManager.Inst != null)
            {
                DataManager.Inst.AddButterflyNumber (1);
            }
            
            // UI 갱신
            if (UIManager_Gameplay.Inst != null)
            {
                UIManager_Gameplay.Inst.UpdateButterflyNum ();
            }
        }
    }
}
