using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class videoCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        moveObjectFunc();
    }


    private float speed_move = 3.0f;
    private float speed_rota = 2.0f;

    void moveObjectFunc()
    {
        float keyH = Input.GetAxis("Horizontal");
        float keyV = Input.GetAxis("Vertical");
        keyH = keyH * speed_move * Time.deltaTime;
        keyV = keyV * speed_move * Time.deltaTime;
        transform.Translate(Vector3.right * keyH);
        transform.Translate(Vector3.forward * keyV);

        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");
        //transform.Rotate(Vector3.up * speed_rota * mouseX);
        //transform.Rotate(Vector3.left * speed_rota * mouseY);

        if (Input.GetKey(KeyCode.Space))
        {
            //??
            transform.Rotate(Vector3.right * speed_rota * Time.deltaTime);
        }
    }
}
