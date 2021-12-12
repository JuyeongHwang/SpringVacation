using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Gameplay : MonoBehaviour
{
    [Header ("게임플레이 설정")]
    //public float gameplayDuration = 60f;
    public float gameplayDuration_remaining = 0;

    [Header ("캐릭터 설정")]
    public KidController kidController;
    public CameraController cameraController;

    //[Header("지형 생성")]
    //public GameObject forterrain;

    protected IEnumerator igameplay;

    public static MyGameManager_Gameplay Inst = null;
    public bool isLoadGameScene;

    [Header ("이펙트 설정")]
    public GameObject[] effects;
    // 0: 청크 클릭
    // 1: 곤충 채집

    //protected KidController kidController;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MyGameManager_Gameplay> ();
            isLoadGameScene = true;
        }
        else
        {
            Destroy (gameObject);
        }

        // kidController가 없으면 씬 내에서 찾는다
        if (kidController == null)
            kidController = FindObjectOfType <KidController> ();

        if (cameraController == null)
            cameraController = FindObjectOfType <CameraController> ();
    }

    void Start ()
    {
        //gameplayDuration += DataManager.Inst.level*5;

        Gameplay ();
        //Instantiate(forterrain,new Vector3(-50,0,-50),Quaternion.identity);
    }

    public void Gameplay ()
    {
        // 시간을 측정하기 위한 코루틴 수행
        if (igameplay != null)
            StopCoroutine (igameplay);

        igameplay = IGameplay ();
        StartCoroutine (igameplay);

        // 캐릭터 애니메이션 수행
        //if (kidController != null)
        //    kidController.SetAnimatorTrigger ("Run");
    }


    IEnumerator IGameplay ()
    {
        //gameplayDuration_remaining = gameplayDuration;
        if (DataManager.Inst != null)
        {
            gameplayDuration_remaining = 60f;
                //DataManager.Inst.GetDataPreset ().DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].DAYDURATION;
        }
        else
        {
            // 기본시간 지정
            gameplayDuration_remaining = 60f;
        }

        while (gameplayDuration_remaining > 0)
        {
            gameplayDuration_remaining -= Time.deltaTime;
            
            if (UIManager_Gameplay.Inst != null)
            {
                UIManager_Gameplay.Inst.SetTimeText (gameplayDuration_remaining);
            }
            
            yield return null;
        }

        // 게임시간 경과 후
        if (MySceneManager.Inst != null && EnvManager.Inst != null)
        {
            // 진행율이 일정 수치 이상인 경우 엔딩
            // 2일 경우 엔딩 씬으로 설정, 아니면 결과 씬으로 설정
            if (EnvManager.Inst.GetIsEnding ())
            {
                // 아니면 결과화면
                MySceneManager.Inst.GotoNextScene2 ();
            }
            else
            {
                // 아니면 결과화면
                MySceneManager.Inst.GotoNextScene ();
            }
        }
    }

    // 클릭했을 때 꼬마로 다이렉트로 꽂는 것 보단 MyGameManager이라는 정거장을 통해 호출을 전달
    public void ClickFromBug (bugController bc)
    {
        if (kidController == null)
            return;

        kidController.ClickFromBug (bc);
    }

    public void ClickFromTerrain (Vector3 Pos)
    {
        if (kidController == null)
            return;

        kidController.ClickFromTerrain (Pos);
    }

    public GameObject GetCameraObject ()
    {
        if (cameraController == null)
            return null;
        
        return cameraController.GetCameraObject ();
    }

    public Vector3 GetKidPosition ()
    {
        if (kidController == null)
            return Vector3.zero;

        return kidController.transform.position;
    }

    // ================================  이펙트 =======================================

    /*public GameObject GetEffectByIndex (int index)
    {
        if (effects.Length <= index)
            return null;

        return effects [index];
    }*/

    public void InstantiateEffectByIndex (int index, Vector3 pos, Quaternion rot)
    {
        if (effects.Length <= index)
            return;

        GameObject effectObject = Instantiate (effects [index], pos, rot);
        Destroy (effectObject, 5f);
    }
}
