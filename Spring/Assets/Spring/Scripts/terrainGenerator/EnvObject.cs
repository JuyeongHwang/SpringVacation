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

    public float checkY = 0f;

    protected void Start ()
    {
        // 기울기 판단 후 적절한 위치인지 판단
        RaycastHit hit;
        float offset = 1f;
        float offsetY = 2f;
        float dist = 5f;
        float placeDotCutoff = 0.75f;
        int maxHitCount = 10;

        bool place = true;

        // 바로 아래 물이라면
        if (place == true)
        {
            float wy = -1f;
            float gy = 0f;
            if (Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Water ()))
            {
                wy = hit.point.y;
            }
            if (Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
            {
                gy = hit.point.y;
            }

            if (gy < wy)
            {
                place = false;
            }
        }

        // f
        if (place == true
        && Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY + Vector3.forward * offset, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
        {
            // 방향 구하기
            Vector3 dir = hit.point - gameObject.transform.position;
            dir = dir.normalized;

            // 내적
            float dt = Vector3.Dot (dir, Vector3.down);
            dt = Mathf.Abs (dt);

            if (dt > placeDotCutoff)
            {
                place = false;
            }
        }

        // d
        if (place == true
        && Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY + Vector3.forward * -offset, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
        {
            // 방향 구하기
            Vector3 dir = hit.point - gameObject.transform.position;
            dir = dir.normalized;

            // 내적
            float dt = Vector3.Dot (dir, Vector3.down);
            dt = Mathf.Abs (dt);

            if (dt > placeDotCutoff)
            {
                place = false;
            }
        }

        // l
        if (place == true
        && Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY + Vector3.right * -offset, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
        {
            // 방향 구하기
            Vector3 dir = hit.point - gameObject.transform.position;
            dir = dir.normalized;

            // 내적
            float dt = Vector3.Dot (dir, Vector3.down);
            dt = Mathf.Abs (dt);

            if (dt > placeDotCutoff)
            {
                place = false;
            }
        }

        // r
        if (place == true
        && Physics.Raycast (gameObject.transform.position + Vector3.up * offsetY + Vector3.right * offset, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
        {
            // 방향 구하기
            Vector3 dir = hit.point - gameObject.transform.position;
            dir = dir.normalized;

            // 내적
            float dt = Vector3.Dot (dir, Vector3.down);
            dt = Mathf.Abs (dt);

            if (dt > placeDotCutoff)
            {
                place = false;
            }
        }

        // 만약 배치 불가라면
        if (place == false)
        {
            Destroy (gameObject);
        }
        else
        {
            // Env 매니저에게 등록
            if (EnvManager.Inst != null)
            {
                EnvManager.Inst.AddEnvObject (this);
            }

            // 아래의 런덤 배치는 로컬상에서의 의미
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

                    // 레이를 쏴서 위치 및 방향 재조절 + 나무인 경우 스킵
                    if (Physics.Raycast (g.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ()))
                    {
                        g.transform.position = hit.point;

                        if (envObjectType != EnvObjectType.TREE)
                        {
                            g.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
                        }
                    }
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
                    GameObject g = Instantiate (envOjects [Random.Range (0, envOjects.Length)], gameObject.transform.position, Quaternion.identity, gameObject.transform);
                    
                    g.SetActive (true);

                    if (randomRot)
                    {
                        g.transform.localRotation = Quaternion.Euler (Vector3.up * Random.Range (0, 360));
                    }

                    g.transform.localScale = new Vector3 (g.transform.localScale.x * Random.Range (scaleMin, scaleMax)
                    , g.transform.localScale.y * Random.Range (scaleMin, scaleMax)
                    , g.transform.localScale.z * Random.Range (scaleMin, scaleMax));

                    int hitCount = 0;
                    do
                    {
                        // 배치 후
                        Vector3 offsetPos = Vector3.forward * Random.Range (-randRange, randRange);
                        offsetPos += Vector3.right * Random.Range (-randRange, randRange);

                        g.transform.localPosition = offsetPos;

                        // 레이 검사
                        Physics.Raycast (g.transform.position + Vector3.up * offsetY, Vector3.down, out hit, dist, EnvManager.Inst.GetLayermaskValue_Ground ());

                        hitCount += 1;

                    } while (hitCount < maxHitCount
                    && Mathf.Abs (Vector3.Dot (Vector3.up, hit.normal)) < placeDotCutoff);

                    // 배치를 못하면
                    if (hitCount == maxHitCount)
                    {
                        Destroy (g.gameObject);
                    }
                    // 배치를 잘하면 적절히 방향 조절 + 나무인 경우 스킵
                    else
                    {
                        g.transform.position = hit.point;
                        
                        if (envObjectType != EnvObjectType.TREE)
                        {
                            g.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
                        }
                    }
                }
            }
        }
    }

    void OnDestroy ()
    {
        if (EnvManager.Inst != null)
        {
            EnvManager.Inst.DeleteEnvObject (this);
        }
    }

    public EnvObjectType GetEnvObjectType ()
    {
        return envObjectType;
    }
}

