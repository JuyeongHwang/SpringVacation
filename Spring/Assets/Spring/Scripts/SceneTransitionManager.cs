using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header ("씬 전환 설정")]
    public Animator sceneAnimator;

    public static SceneTransitionManager Inst = null;

    IEnumerator iplay;
    void Awake ()
    {
        // 싱글톤
        if (Inst == null)
        {
            Inst = gameObject.GetComponent <SceneTransitionManager> ();
        }
        else
        {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (this.gameObject);
    }
    bool animationEnd = true;

    public void PlaySceneTransition_FadeIn ()
    {
        if (iplay != null)
            StopCoroutine (iplay);

        iplay = IPlaySceneTransition_FadeIn ();
        StartCoroutine (iplay);
    }

    public void PlaySceneTransition_FadeOut ()
    {
        if (iplay != null)
            StopCoroutine (iplay);

        iplay = IPlaySceneTransition_FadeOut ();
        StartCoroutine (iplay);
    }

    IEnumerator IPlaySceneTransition_FadeIn ()
    {
        animationEnd = false;

        sceneAnimator.SetTrigger ("FadeIn");

        yield return new WaitForSeconds (sceneAnimator.GetCurrentAnimatorStateInfo (0).length);

        animationEnd = true;
    }

    IEnumerator IPlaySceneTransition_FadeOut ()
    {
        animationEnd = false;

        sceneAnimator.SetTrigger ("FadeOut");

        yield return new WaitForSeconds (sceneAnimator.GetCurrentAnimatorStateInfo (0).length);

        animationEnd = true;
    }

    public bool GetIsAnimationEnd ()
    {
        return animationEnd;
    }
}
