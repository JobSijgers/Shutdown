using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(Door))]
public class DoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Door door = (Door)target;

        base.OnInspectorGUI();

        if (door.doorType == EDoorType.DoubleSlidingDoor)
        {
            //gets all the propeties used for the double sliding doors
            SerializedProperty leftDoor = serializedObject.FindProperty("leftDoor");
            SerializedProperty leftDirection = serializedObject.FindProperty("leftDirection");
            SerializedProperty leftDoorOpenAmount = serializedObject.FindProperty("leftDoorOpenAmount");
            SerializedProperty rightDoor = serializedObject.FindProperty("rightDoor");
            SerializedProperty rightDirection = serializedObject.FindProperty("rightDirection");
            SerializedProperty rightDoorOpenAmount = serializedObject.FindProperty("rightDoorOpenAmount");

            EditorGUILayout.Space(10);

            //displays propeties
            EditorGUILayout.PropertyField(leftDoor);
            EditorGUILayout.PropertyField(leftDirection);
            EditorGUILayout.PropertyField(leftDoorOpenAmount);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(rightDoor);
            EditorGUILayout.PropertyField(rightDirection);
            EditorGUILayout.PropertyField(rightDoorOpenAmount);
        }
        if (door.doorType == EDoorType.DoubleRotatingDoor)
        {
            //gets all propeties used for the rotating doors
            SerializedProperty leftDoor = serializedObject.FindProperty("leftDoor");
            SerializedProperty leftEndRotation = serializedObject.FindProperty("leftEndRotation");
            SerializedProperty rightDoor = serializedObject.FindProperty("rightDoor");
            SerializedProperty rightEndRotation = serializedObject.FindProperty("rightEndRotation");

            EditorGUILayout.Space(10);
            //displays the propeties
            EditorGUILayout.PropertyField(leftDoor);
            EditorGUILayout.PropertyField(leftEndRotation);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(rightDoor);
            EditorGUILayout.PropertyField(rightEndRotation);
        }

        EditorGUILayout.Space(10);
        //keycards
        EditorGUILayout.TextArea("Keycard");
        SerializedProperty doorKeycardReq = serializedObject.FindProperty("doorRequiresKeycard");
        EditorGUILayout.PropertyField(doorKeycardReq);
        if (door.doorRequiresKeycard)
        {
            SerializedProperty keycardType = serializedObject.FindProperty("doorKeyType");
            SerializedProperty keycardVisibility = serializedObject.FindProperty("keycardVisibility");
            SerializedProperty keycardSprite = serializedObject.FindProperty("keycardSprite");

            EditorGUILayout.PropertyField(keycardType);
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Fetch Scripts"))
            {
                door.FetchHolograms();
            }
            EditorGUILayout.PropertyField(keycardVisibility);
            EditorGUILayout.PropertyField(keycardSprite);
        }
        //timer
        EditorGUILayout.Space(10);
        EditorGUILayout.TextArea("Close door after open");
        SerializedProperty closeDoorAfterOpen = serializedObject.FindProperty("closeDoorAfterOpen");
        EditorGUILayout.PropertyField(closeDoorAfterOpen);
        if (door.closeDoorAfterOpen)
        {
            SerializedProperty closeDoorAfterDuration = serializedObject.FindProperty("closeDoorAfterDuration");
            EditorGUILayout.PropertyField(closeDoorAfterDuration);
        }
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(door);
            EditorSceneManager.MarkSceneDirty(door.gameObject.scene);
        }
    }
}
