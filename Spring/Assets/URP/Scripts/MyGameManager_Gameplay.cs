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
        if (igameplay != null)
            StopCoroutine (igameplay);

        igameplay = IGameplay ();
        StartCoroutine (igameplay);
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
