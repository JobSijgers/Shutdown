using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        Vector3 targetPos = transform.position - cam.forward; // get a position with a direction perpendicular to the camera
        targetPos.y = cam.position.y; // set the height to face to wherever the camera is
        Vector3 direction = (targetPos - transform.position).normalized; // get the direction to the targetPos

        transform.rotation = Quaternion.LookRotation(direction); // rotate to face that way
    }
}
