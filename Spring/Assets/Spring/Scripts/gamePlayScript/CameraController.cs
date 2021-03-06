using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header ("카메라 설정")]

    public GameObject cameraFocusObject;
    public GameObject cameraObject;
    public float cameraFocusSmooth = 1;
    public float cameraFocusSmooth_rot = 1;

    private bool startFrame = true;
    

    void LateUpdate ()
    {
        if (cameraFocusObject == null)
            return;

        if (startFrame)
        {
            startFrame = false;
            gameObject.transform.position = cameraFocusObject.transform.position;
        }

        // 이동
        Vector3 currentPos = gameObject.transform.position;
        Vector3 focusPos = cameraFocusObject.transform.position;

        gameObject.transform.position += (focusPos - currentPos) * cameraFocusSmooth * Time.deltaTime;

        // 회전
        Vector3 currentDir = gameObject.transform.forward;
        Vector3 focusDir = cameraFocusObject.transform.forward;

        Vector3 resultDir = currentDir + focusDir * Time.deltaTime * cameraFocusSmooth_rot;
        resultDir = resultDir.normalized;

        gameObject.transform.forward = resultDir;
    }

    public GameObject GetCameraObject ()
    {
        return cameraObject;
    }
}
