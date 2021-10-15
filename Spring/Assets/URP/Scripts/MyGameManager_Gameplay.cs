using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager_Gameplay : MyGameManager
{
    [Header ("게임플레이 설정")]
    public float gameplayDuration = 60f;
    public float gameplayDuration_remaining = 0;

    protected IEnumerator igameplay;

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
            yield return null;
        }

        if (MySceneManager.Inst != null)
        {
            MySceneManager.Inst.GotoNextScene ();
        }
    }
}
