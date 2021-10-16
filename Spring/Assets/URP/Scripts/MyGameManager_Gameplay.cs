using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Gameplay : MonoBehaviour
{
    [Header ("게임플레이 설정")]
    public float gameplayDuration = 60f;
    public float gameplayDuration_remaining = 0;

    [Header ("캐릭터 설정")]
    public KidController kidController;

    protected IEnumerator igameplay;

    public static MyGameManager_Gameplay Inst = null;

    //protected KidController kidController;

    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MyGameManager_Gameplay> ();
        }
        else
        {
            Destroy (gameObject);
        }

        // kidController가 없으면 씬 내에서 찾는다
        if (kidController == null)
            kidController = FindObjectOfType <KidController> ();
    }

    void Start ()
    {
        Gameplay ();
    }

    public void Gameplay ()
    {
        // 시간을 측정하기 위한 코루틴 수행
        if (igameplay != null)
            StopCoroutine (igameplay);

        igameplay = IGameplay ();
        StartCoroutine (igameplay);

        // 캐릭터 애니메이션 수행
        if (kidController != null)
            kidController.SetAnimatorTrigger ("Run");
    }


    IEnumerator IGameplay ()
    {
        gameplayDuration_remaining = gameplayDuration;

        while (gameplayDuration_remaining > 0)
        {
            gameplayDuration_remaining -= Time.deltaTime;
            
            if (UIManager_Gameplay.Inst != null)
            {
                UIManager_Gameplay.Inst.SetTimeText (gameplayDuration_remaining);
            }
            
            yield return null;
        }

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }
}
