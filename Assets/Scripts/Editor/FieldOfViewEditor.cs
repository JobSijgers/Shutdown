using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FieldOfView))]

public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fieldOfView = (FieldOfView)target;

        Handles.color = Color.red;
        Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, Vector3.forward, 360, fieldOfView.viewRadius); // draw the outer view circle
        Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, Vector3.forward, 360, fieldOfView.shootingDistance); // draw the inner shooting circle

        Vector3 line = fieldOfView.transform.position + (fieldOfView.transform.forward * fieldOfView.viewRadius); // get the position for the forwards line
        Vector3 endPos1 = Quaternion.Euler(0, -fieldOfView.viewAngle / 2, 0) * (line - fieldOfView.transform.position) + fieldOfView.transform.position; // get the left line pos
        Vector3 endPos2 = Quaternion.Euler(0, fieldOfView.viewAngle / 2, 0) * (line - fieldOfView.transform.position) + fieldOfView.transform.position; // get the right line pos

        //draw everything
        Handles.color = Color.cyan;
        Handles.DrawLine(fieldOfView.transform.position, line);
        Handles.color = Color.green;
        Handles.DrawLine(fieldOfView.transform.position, endPos1);
        Handles.DrawLine(fieldOfView.transform.position, endPos2);
    }
}
