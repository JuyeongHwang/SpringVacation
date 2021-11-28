using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvObjectCheckUnderwater : MonoBehaviour
{
    bool isUnderwater = false;

    [Header ("오브젝트 설정")]
    public GameObject[] groundObjects;      // 땅 오브젝트
    public GameObject[] underwaterObjects;  // 물 오브젝트
    public GameObject[] floatingObjects;    // 물 떠다니는 오브젝트 (underwaterObjects에 속해있어야 함)

    void Start()
    {
        CheckIsUnderwater ();
    }

    void CheckIsUnderwater ()
    {
        if (EnvManager.Inst == null)
            return;

        float groundY = 0f;
        float waterY = -1f;

        RaycastHit hit;
        float offsetY = 5f;
        float dist = 10f;
        float waterYAdd = 0.05f;

        if (Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermask_Ground ()))
        {
            groundY = hit.point.y;
        }

        if (Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermask_Water ()))
        {
            waterY = hit.point.y;
        }

        if (groundY > waterY)
        {
            isUnderwater = false;
        }
        else
        {
            isUnderwater = true;
        }

        if (!isUnderwater)
        {
            foreach (GameObject gg in groundObjects)
            {
                gg.gameObject.SetActive (true);
            }

            foreach (GameObject gg in underwaterObjects)
            {
                gg.gameObject.SetActive (false);
            }
        }
        else
        {
            foreach (GameObject gg in groundObjects)
            {
                gg.gameObject.SetActive (false);
            }

            foreach (GameObject gg in underwaterObjects)
            {
                gg.gameObject.SetActive (true);
            }

            // 물 표면 위에 위치하도록 설정 + 회전 원위치
            foreach (GameObject gg in floatingObjects)
            {
                Vector3 pos = gg.gameObject.transform.position;
                pos.y = waterY + waterYAdd;
                gg.gameObject.transform.position = pos;

                gg.gameObject.transform.rotation = Quaternion.Euler (Vector3.up * Random.Range (0, 360));
            }
        }
    }
}
