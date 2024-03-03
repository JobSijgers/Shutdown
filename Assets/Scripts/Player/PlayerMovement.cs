using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Vector3 movementRotationOffset;
    [SerializeField] private Vector3 rotationOffset;

    [Header("Rotation")]
    [Range(0f, 1f)][SerializeField] private float controllerDeadzone = 0.05f;
    [SerializeField] private float rotationSmoothing = 500f;
    [SerializeField] private Camera _camera;

    [Header("Movement")]
    public float movementSpeed;
    [SerializeField] private PlayerShooting playerShooting;

    [HideInInspector] public Vector3 lastMoveDirecton; // last direction moved in, used for the enemies' prediction

    private Vector2 aim;
    private Vector2 moveVector;
    private Vector3 moveDirection;

    private CustomInput input;
    private Rigidbody rigidBody;
    private Transform activeSpawnpoint;

    private bool isGamepad;


    private void Awake()
    {
        input = new CustomInput();
        rigidBody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        GameManager.Instance.PauseDuringCutsceneEvent += OnPause;
    }
    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        HandleInput();
        HandleRotation();
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    private void MovePlayer()
    {
        moveDirection = Quaternion.Euler(movementRotationOffset) * (Vector3.forward * moveVector.y + Vector3.right * moveVector.x);
        rigidBody.AddForce(100f * movementSpeed * Time.deltaTime * moveDirection.normalized, ForceMode.Force);
    }

    private void HandleInput()
    {
        moveVector = input.Controls.Movement.ReadValue<Vector2>();
        aim = input.Controls.Aim.ReadValue<Vector2>();
    }
    private void HandleRotation()
    {
        activeSpawnpoint = playerShooting.GetActiveRotationHandle();
        if (isGamepad)
        {
            if (Mathf.Abs(aim.x) > controllerDeadzone || Mathf.Abs(aim.y) > controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * aim.x + Vector3.forward * aim.y;
                if (playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up) * Quaternion.Euler(rotationOffset);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation * Quaternion.Euler(movementRotationOffset), rotationSmoothing * Time.deltaTime);
                }
            }
        }
        else
        {
            Ray ray = _camera.ScreenPointToRay(aim);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (!groundPlane.Raycast(ray, out float rayDistance)) return;

            Vector3  point = ray.GetPoint(rayDistance);
            
            LookAt(point);
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        Vector3 direction = (heightCorrectedPoint - transform.position).normalized;
        Quaternion newRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, rotationSmoothing * Time.deltaTime);
        heightCorrectedPoint.y  = activeSpawnpoint.position.y;
        if (Vector3.Distance(activeSpawnpoint.position, heightCorrectedPoint) < 1) return;
        activeSpawnpoint.LookAt(heightCorrectedPoint);
        activeSpawnpoint.transform.rotation *= Quaternion.Euler(rotationOffset);

    }

    

    public void OnDeviceChange(PlayerInput playerInput)
    {
        isGamepad = playerInput.currentControlScheme.Equals("Gamepad") ? true : false;
        if (isGamepad)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void SetPlayerPosition(Vector3 newPlayerPosition)
    {
        transform.position = new Vector3(newPlayerPosition.x, transform.position.y, newPlayerPosition.z);
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
