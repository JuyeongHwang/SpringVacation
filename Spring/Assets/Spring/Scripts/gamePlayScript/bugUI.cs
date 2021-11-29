using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bugUI : MonoBehaviour
{
    public GameObject mc;

    public Slider bugHPBar;
    public Text bugName;

    const float bugUIDuration = 5f;
    IEnumerator ibugUI;

    // 처음에는 UI가 안보이다가 채집하면 보이도록 동작함

    void OnEnable ()
    {
        if (ibugUI != null)
            StopCoroutine (ibugUI);
    }

    void Awake ()
    {

    }

    void Start ()
    {
        if (MyGameManager_Gameplay.Inst != null)
        {   
            mc = MyGameManager_Gameplay.Inst.GetCameraObject ();
        }
       
        SetActiveBugUI_Off ();
    }

    // 방향 설정
    void LateUpdate ()
    {
        if (mc == null)
            return;

        gameObject.transform.LookAt (mc.gameObject.transform.position, Vector3.up);
    }

    public void SetBugUIInfo (string name, BugGrade grade)
    {
        if (bugHPBar == null)
            return;

        bugHPBar.value = 1f;

        if (bugName == null)
            return;

        if (DataManager.Inst == null || DataManager.Inst.GetDataPreset () == null)
            return;

        bugName.text = name + " <" + grade.ToString () + ">";
        bugName.color = DataManager.Inst.GetDataPreset ().DATABUGGRADES [(int)grade].BUGGRADECOLOR;
    }

    public void SetBugHPValue (float v)
    {
        if (bugHPBar == null)
            return;

        bugHPBar.value = v;
    }

    void SetActiveBugUI_Off ()
    {
        if (bugHPBar != null)
        {
            bugHPBar.gameObject.SetActive (false);
        }

        if (bugName != null)
        {
            bugName.gameObject.SetActive (false);
        }
    }

    public void SetActiveBugUI_On ()
    {
        if (bugHPBar != null)
        {
            bugHPBar.gameObject.SetActive (true);
        }

        if (bugName != null)
        {
            bugName.gameObject.SetActive (true);
        }

        if (ibugUI != null)
            StopCoroutine (ibugUI);

        ibugUI = IBugUI ();
        StartCoroutine (ibugUI);
    }

    IEnumerator IBugUI ()
    {
        yield return new WaitForSeconds (bugUIDuration);

        SetActiveBugUI_Off ();
    }
}
