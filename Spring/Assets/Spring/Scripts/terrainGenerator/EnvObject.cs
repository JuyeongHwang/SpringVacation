using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvObjectType
{
    TREE, FLOWER, ROCK
}

public class EnvObject : MonoBehaviour
{
    [Header ("오브젝트 설정")]
    public EnvObjectType envObjectType;
    public GameObject[] envOjects;

    [Header ("랜덤 트랜스폼을 위한 설정")]
    public bool randomRot = true;
    public float scaleMin = 1f;
    public float scaleMax = 1.25f;

    [Header ("랜덤 인스턴스 배치를 위한 설정 (randNum = 0인 경우 랜덤 배치 수행하지 않음)")]
    public int randInstNum = 0;
    public float randRange = 0f;    // 범위
    
    protected void Start ()
    {
        if (EnvManager.Inst != null)
        {
            EnvManager.Inst.AddEnvObject (this);
        }

        // 랜덤배치 x
        if (randInstNum == 0)
        {
            foreach (GameObject g in envOjects)
            {
                if (randomRot)
                {
                    g.transform.localRotation = Quaternion.Euler (Vector3.up * Random.Range (0, 360));
                }

                g.transform.localScale = Vector3.one * Random.Range (scaleMin, scaleMax);
            }
        }
        // 랜덤 배치
        else
        {
            foreach (GameObject g in envOjects)
            {
                g.SetActive (false);
            }

            for (int i = 0; i < randInstNum && envOjects.Length > 0; i++)
            {
                Vector3 offsetPos = Vector3.forward * Random.Range (-randRange, randRange);
                offsetPos += Vector3.right * Random.Range (-randRange, randRange);

                GameObject g = Instantiate (envOjects [Random.Range (0, envOjects.Length)], gameObject.transform.position, Quaternion.identity, gameObject.transform);
                g.transform.localPosition = offsetPos;
                g.SetActive (true);

                if (randomRot)
                {
                    g.transform.localRotation = Quaternion.Euler (Vector3.up * Random.Range (0, 360));
                }

                g.transform.localScale = new Vector3 (g.transform.localScale.x * Random.Range (scaleMin, scaleMax)
                , g.transform.localScale.y * Random.Range (scaleMin, scaleMax)
                , g.transform.localScale.z * Random.Range (scaleMin, scaleMax));
            }
        }
    }

    public EnvObjectType GetEnvObjectType ()
    {
        return envObjectType;
    }
}

