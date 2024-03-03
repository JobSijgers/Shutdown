using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCamera))]

public class PlayerCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerCamera pc = (PlayerCamera)target; 
        base.OnInspectorGUI();
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Update Camera Position"))
        {
            pc.UpdateCameraRotation();
            pc.UpdateCameraPosition();
        }
        EditorGUILayout.EndHorizontal();
    }
}
