using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createPlane : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    public GameObject plane;

    Mesh mesh;
    Vector3[] vertices;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, -Vector3.up, out hit, 100.0f))
        {
            if (hit.collider.gameObject != null)
            {
                print("Found an object - distance: " + hit.distance);
            }
            else
            {
                Instantiate(plane, new Vector3(cam.transform.position.x ,6, cam.transform.position.z),Quaternion.identity);
            }
        }
            
    }
}
