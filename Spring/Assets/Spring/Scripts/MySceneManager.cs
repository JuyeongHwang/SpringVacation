using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Inst = null;

    [Header ("다음 씬 이름")]
    public string nextSceneName = "";

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

    public void GotoNextScene ()
    {
        Scene s = SceneManager.GetSceneByName (nextSceneName);
        if (s != null)
        {
            SceneManager.LoadScene(nextSceneName);
            if(nextSceneName == "Scene_Result")
            {
                MyGameManager_Gameplay.Inst.isLoadGameScene = false;
                GameObject.Find("Env.").SetActive(false);
            }
            else if(nextSceneName == "Gameplay")
            {
                MyGameManager_Gameplay.Inst.isLoadGameScene = true;
                //GameObject.Find("Env.").SetActive(true);
            }
            
        }
            
    }
}
