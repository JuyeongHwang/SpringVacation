using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder_Ending : MonoBehaviour
{
    // 첫 위치: 꼬마 가까이
    Vector3 startPoint = Vector3.zero;

    // 마지막 위치: 시작할 때의 위치
    Vector3 endPoint = Vector3.zero;

    void Start ()
    {
        if (SceneTransitionManager.Inst != null)
        {
            SceneTransitionManager.Inst.PlaySceneTransition_FadeIn ();
        }

        StartCoroutine (IMove ());
    }

    IEnumerator IMove ()
    {
        yield return null;

        print ("A");

        endPoint = gameObject.transform.position;

        KidController_Ending kid = FindObjectOfType <KidController_Ending> ();
        Vector3 kidPos = kid.gameObject.transform.position;
        float kidDist = 5f;

        startPoint = kidPos + (endPoint - kidPos).normalized * kidDist;

        float cameraDuration = 10f;

        float p = 0;
        while (p < 1)
        {
            print ("B");

            float trueP = Mathf.Sin ((p*2 - 1)*Mathf.PI*0.5f) * 0.5f + 0.5f; 
            gameObject.transform.position = Vector3.Lerp (startPoint, endPoint, trueP);

            p += Time.deltaTime / cameraDuration;
            yield return null;
        }

        gameObject.transform.position = endPoint;
    }
}
