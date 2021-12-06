using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownDeploy : MonoBehaviour
{
    public List <EnvObject> nearEnvObjects;
    public List <GameObject> townGameObjects;
    public int townRelaxNum = 10;
    public float townRange = 30f;

    void Start ()
    {
        RelaxEnvObjects (townRelaxNum);
    }

    void LateUpdate ()
    {

    }

    void OnTriggerEnter (Collider other)
    {
        EnvObject eo = other.GetComponent <EnvObject> ();

        if (eo != null && nearEnvObjects.Contains (eo) == false)
        {
            nearEnvObjects.Add (eo);
        }
    }

    void RelaxEnvObjects (int relax)
    {
        if (EnvManager.Inst == null)
            return;

        nearEnvObjects = EnvManager.Inst.GetEnvObjectsByPointAndRange (gameObject.transform.position, townRange);

        float closeDist = 8f;
        float moveDist = 2f;

        bool check = true;

        for (int i = 0; i < relax && check == true; i++)
        {
            check = false;

            foreach (EnvObject eo in nearEnvObjects)
            {
                foreach (GameObject tgo in townGameObjects)
                {
                    if (Vector3.Distance (eo.gameObject.transform.position, tgo.gameObject.transform.position) < closeDist)
                    {
                        Vector3 dir = eo.gameObject.transform.position - tgo.gameObject.transform.position; 
                        dir.y = 0f;
                        dir = dir.normalized;

                        eo.gameObject.transform.position += dir * moveDist;

                        check = true;
                    }
                }
            }
        }
    }
}
