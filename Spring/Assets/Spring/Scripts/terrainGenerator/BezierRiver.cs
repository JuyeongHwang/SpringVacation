using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierRiver : MonoBehaviour
{

    public GameObject GameObject;
    
    [Range(0,1)]
    public float TestValue;

    public Vector3 P1;
    public Vector3 P2;
    public Vector3 P3;
    public Vector3 P4;

    private void Update()
    {
        GameObject.transform.position = BezierTest(P1,P2,P3,P4,TestValue);
    }


    public Vector3 BezierTest(
        Vector3 _P1,
        Vector3 _P2,
        Vector3 _P3,
        Vector3 _P4,
        float Value
        )
    {

        Vector3 A = Vector3.Lerp(_P1, _P2, Value);
        Vector3 B = Vector3.Lerp(_P2, _P3, Value);
        Vector3 C = Vector3.Lerp(_P3, _P4, Value);

        Vector3 D = Vector3.Lerp(A, B, Value);
        Vector3 E = Vector3.Lerp(B,C, Value);

        Vector3 F = Vector3.Lerp(D, E, Value);

        return F;
    }

    //반지름 5정도..
    //이걸..myDel terrain의 generate안에 하면 되지 않을..까..?
}
