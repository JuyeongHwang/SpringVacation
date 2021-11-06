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
                //MyGameManager_Gameplay.Inst.isLoadGameScene = false;
                DataManager.Inst.Env.gameObject.SetActive(false);
                //DataManager.Inst.Kid.gameObject.SetActive(false);

                //GameObject.Find("Env.").SetActive(false);
            }
            else if(nextSceneName == "Gameplay")
            {
                //MyGameManager_Gameplay.Inst.isLoadGameScene = true;
                DataManager.Inst.Env.gameObject.SetActive(true);
                //DataManager.Inst.Kid.gameObject.SetActive(true);
                //reset transform.
                //DataManager.Inst.Kid.gameObject.transform.position = new Vector3(33, 0, 33);
                //DataManager.Inst.Kid.gameObject.transform.rotation = Quaternion.identity;

                //GameObject.Find("Env.").SetActive(true);
            }

        }
            
    }
}
