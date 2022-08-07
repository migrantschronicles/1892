using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 dragOrigin;


    // Update is called once per frame
    void Update()
    {
        PanCamera();        
    }

    private void PanCamera() 
    {
        // Save position of mouse in world space when drag starts (First time clicked)
        if (Input.GetMouseButtonDown(0))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);

        // Calculate distance between drag origin and new position if it is still held down
        if (Input.GetMouseButton(0)) 
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

            Debug.Log("Origin " + dragOrigin + " newPosition " + cam.ScreenToWorldPoint(Input.mousePosition) + "=difference " + difference);

            // Move the camera by that distance
            cam.transform.position += difference;
        }


    }
}
