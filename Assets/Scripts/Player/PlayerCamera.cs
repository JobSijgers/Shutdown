using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private Quaternion cameraRotation;
    [Header("Minimum offset to trigger the fast alignment")]
    [SerializeField] private float minOffset = 1f;
    [Header("Approach speed for finer centering")]
    [SerializeField, Range(0f, 1f)] private float centeringSpeed;
    [SerializeField] private float maxMouseOffset = 5;

    private Vector3 focusPoint;

    private void OnValidate()
    {
        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void Start()
    {
        GameManager.Instance.PauseDuringCutsceneEvent += OnPause; 
        focusPoint = player.transform.position;
    }

    void Update()
    {
        if (Time.timeScale != 0)
        {
            UpdateFocusPoint();
            focusPoint += GetMousePos() * maxMouseOffset;
            transform.position = focusPoint + cameraOffset;
        }
    }

    private Vector3 GetMousePos()
    {
        Vector3 mousePos = new();
        mousePos.x = Mathf.Clamp(Input.mousePosition.x, 0, Screen.width) / Screen.width;
        mousePos.z = Mathf.Clamp(Input.mousePosition.y, 0, Screen.height) / Screen.height;
        return new Vector3(-0.5f, 0, -0.5f) + mousePos;
    }

    private void UpdateFocusPoint()
    {
        Vector3 targetPos = player.transform.position;
        float distance = Vector3.Distance(targetPos, focusPoint);
        float t = 1f;
        if (distance > 0.01f && centeringSpeed > 0f)
        {
            t = Mathf.Pow(1f - centeringSpeed, Time.unscaledDeltaTime);
        }
        if (distance > minOffset)
        {
            t = Mathf.Min(t, minOffset / distance);
        }
        focusPoint = Vector3.Lerp(targetPos, focusPoint, t);
    }
    public Quaternion GetCameraRotation()
    {
        return cameraRotation;
    }
    public void UpdateCameraPosition()
    {
        transform.position = player.position + cameraOffset;
    }
    public void UpdateCameraRotation()
    {
        transform.rotation = cameraRotation;
    }
    public Vector3 GetCameraOffset()
    {
        return cameraOffset;
    }
    private void OnPause(bool pause)
    {
        enabled = pause;
    }
    private void OnDestroy()
    {
        GameManager.Instance.PauseDuringCutsceneEvent -= OnPause;
    }
}

