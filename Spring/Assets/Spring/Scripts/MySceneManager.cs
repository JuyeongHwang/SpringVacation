using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Inst = null;

    [Header ("다음 씬 이름")]
    public string nextSceneName = "";
    public string nextSceneName2 = "";

//    public bool canClick = false;
    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <MySceneManager> ();
        }
        else
        {
            Destroy (gameObject);
        }
    }

    void Start ()
    {
        if (SceneTransitionManager.Inst != null)
        {
            SceneTransitionManager.Inst.PlaySceneTransition_FadeIn ();
        }
    }

    public void GotoNextScene ()
    {
        Scene s = SceneManager.GetSceneByName (nextSceneName);
        if (s != null)
        {
            // 코루틴으로 구조 변경
            StartCoroutine (IGotoNextScene (nextSceneName));
        }
    }

    public void GotoNextScene2 ()
    {
        Scene s = SceneManager.GetSceneByName (nextSceneName2);
        if (s != null)
        {
            // 코루틴으로 구조 변경
            StartCoroutine (IGotoNextScene (nextSceneName2));
        }
    }

    IEnumerator IGotoNextScene (string sceneName)
    {
        if (SceneTransitionManager.Inst != null)
        {
            SceneTransitionManager.Inst.PlaySceneTransition_FadeOut ();
        }

        while (SceneTransitionManager.Inst.GetIsAnimationEnd () == false)
        {
            yield return null;
        }

        if(sceneName == "Scene_Result")
        {
            if (EnvManager.Inst != null)
            {
                EnvManager.Inst.gameObject.SetActive (false);
            }
        }
        else if(sceneName == "Gameplay")
        {
            if (EnvManager.Inst != null)
            {
                EnvManager.Inst.gameObject.SetActive (true);
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}
