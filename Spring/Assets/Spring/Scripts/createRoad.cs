using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createRoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Debug.Log("Drawing Gizmos!");
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Vector3.zero, 0.3f);
    }
}
