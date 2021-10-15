using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KidController : MonoBehaviour
{
    public GameObject kidBody;
    protected Animator kidAnimator;


    [SerializeField]
    private List<GameObject> FoundBug;
    [SerializeField]
    private GameObject butterfly; //나중에 struct로 더 다양한 곤충 관리 예정 //target
    public string TagName;
    [SerializeField]
    private float shortDis;

    //characterInfo
    //level, 수집물 리스트,, 등등
    float attackPower = 10f;
    float butterflyHP = 100.0f;
    bool isArrived = false;

    void Awake ()
    {
        if (kidBody != null)
        {
            kidAnimator = kidBody.GetComponent <Animator> ();
        }
    }

    private void Start()
    {
        detectBug();
    }

    private void Update()
    {
        if (butterfly != null) 
        {
            //Debug.Log(butterfly.name);
            
            if (isArrived)
            {
                attackBug();
            }
            else
            {

                //Vector3 targetDir = butterfly.transform.position - transform.position;
                //float step = 1.0f * Time.deltaTime;
                //Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
                //Debug.DrawRay(transform.position, newDir, Color.red);
                //transform.rotation = Quaternion.LookRotation(newDir);

                transform.position = Vector3.MoveTowards(transform.position, butterfly.transform.position, 0.01f);

            }
        }
        else
        {
            butterflyHP = 100.0f;
            detectBug();
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("충돌 시작!");
        if (collision.gameObject.CompareTag(TagName))
        {
            isArrived = true;
        }
        

    }

    public void SetAnimatorTrigger (string triggerName)
    {
        if (kidAnimator == null)
            return;

        kidAnimator.SetTrigger (triggerName);
    }

    void detectBug()
    {
        FoundBug = new List<GameObject>(GameObject.FindGameObjectsWithTag(TagName));
        shortDis = Vector3.Distance(gameObject.transform.position,
            FoundBug[0].transform.position);

        butterfly = FoundBug[0];

        foreach(GameObject found in FoundBug)
        {
            float Distance = Vector3.Distance(gameObject.transform.position, found.transform.position);
            if (Distance < shortDis)
            {
                butterfly = found;
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
    //임시로... 나중에 버터플라이 객체 만들 예정
    void attackBug()
    {
        Debug.Log("스페이스바를 눌러 공격하세요~!");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            butterflyHP -= attackPower;

        }

        if (butterflyHP <= 0.0f)
        {
            isArrived = false;
            Destroy(butterfly.gameObject);
        }
    }
}
