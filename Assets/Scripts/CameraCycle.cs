using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCycle : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform[] cameraLocations;
    [SerializeField] private float interval;
    private int currentCameraIndex;
    void Start()
    {
        StartCoroutine(CycleCamera());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Cycle();
        }
    }

    private IEnumerator CycleCamera()
    {
        while (true)
        {
            Cycle();
            yield return new WaitForSeconds(interval);
        }
    }
    private void Cycle()
    {
        mainCamera.SetPositionAndRotation(cameraLocations[currentCameraIndex].position, cameraLocations[currentCameraIndex].rotation);
        currentCameraIndex++;
        if (currentCameraIndex >= cameraLocations.Length)
            currentCameraIndex = 0;
    }
}
