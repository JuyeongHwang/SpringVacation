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


    private float speed_move = 6.0f;
    private float speed_rota = 2.0f;

    void moveObjectFunc()
    {
        float keyH = Input.GetAxis("Horizontal");
        float keyV = Input.GetAxis("Vertical");
        keyH = keyH * speed_move * Time.deltaTime;
        keyV = keyV * speed_move * Time.deltaTime;
        transform.Translate(Vector3.right * keyH);
        transform.Translate(Vector3.forward * keyV);



        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.up * Time.deltaTime * 2.0f);
            //??
            //transform.Rotate(Vector3.right * speed_rota * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed_move = 12.0f;
        }
        else { speed_move = 4.5f; }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * Time.deltaTime * -2.0f);
        }

        if (Input.GetKey(KeyCode.R))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.Rotate(Vector3.up * speed_rota * mouseX);
            transform.Rotate(Vector3.left * speed_rota * mouseY);
        }
    }
}
