using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KidController : MonoBehaviour
{
    public GameObject kidBody;
    protected Animator kidAnimator;


    [SerializeField]
    private List<GameObject> FoundBug;
    [SerializeField]
    private GameObject findObject; //���߿� struct�� �� �پ��� ���� ���� ���� //target
    public string TagName;
    [SerializeField]
    private float shortDis;

    public Text conditionText;

    //characterInfo
    //level, ������ ����Ʈ,, ���
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
    }

    private void Start()
    {
        detectBug();
        conditionText.text = "Ž�� ��";
    }

    private void Update()
    {
        if (findObject != null) 
        {
            //Debug.Log(butterfly.name);
            
            if (isArrived)
            {
                attackBug();
            }
            else
            {

                Vector3 targetDir = findObject.transform.position - transform.position;
                float step = 1.0f * Time.deltaTime;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
                Debug.DrawRay(transform.position, newDir, Color.red);
                transform.rotation = Quaternion.LookRotation(newDir);


                float dist = Mathf.Sqrt(Mathf.Abs(findObject.transform.position.x-transform.position.x)+
                    Mathf.Abs(findObject.transform.position.y - transform.position.y)+
                    Mathf.Abs(findObject.transform.position.z - transform.position.z)
                    );
                speed = 0.001f * (dist);
                transform.position = Vector3.MoveTowards(transform.position, findObject.transform.position, speed);

            }
        }
        else
        {
            conditionText.text = "Ž�� ��";
            detectBug();
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("�浹 ����!");
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
        Debug.Log("�ڵ�����");
    }
    //�ӽ÷�... ���߿� �����ö��� ��ü ���� ����
    void attackBug()
    {
        //Debug.Log("�����̽��ٸ� ���� �����ϼ���~!");
        conditionText.text = "�����̽��ٸ� ���� ������ ��������!";

        if (Input.GetKeyDown(KeyCode.Space))
        {

            findObject.GetComponent<bugController>().bug.hp -= attackPower;
        }

        if (findObject.GetComponent<bugController>().bug.hp <= 0.0f)
        {
            isArrived = false;
            Destroy(findObject.gameObject);
        }
    }
}
