using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Inst = null;

    [Header ("다음 씬 이름")]
    public string nextSceneName = "";

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

    public void GotoNextScene ()
    {
        Scene s = SceneManager.GetSceneByName (nextSceneName);
        if (s != null)
            SceneManager.LoadScene (nextSceneName);
    }
}
