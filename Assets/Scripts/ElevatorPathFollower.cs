using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEngine.GraphicsBuffer;

public class ElevatorPathFollower : MonoBehaviour
{
    [SerializeField] private float stopLookAt = 0.2f;
    [Header("Speed")]
    [SerializeField] private float maxMovementSpeed = 5;
    [SerializeField] private float rotationSpeed = 1;
    [Tooltip("Speed in seconds")]
    [SerializeField] private float rampUpSpeed = 1;

    private float startDelay;
    private CameraElevatorAnimation cameraElevatorAnimation;
    private Transform lookAt;
    private Quaternion endRotation;
    private PathCreator path;
    private float movementSpeed;
    private float distanceTravelled;
    private bool openDoorsAfterComplete;
    private float delay;
    private float timer;
    private Vector3 lookAtOffset;
    private float rampingSpeed;
    private bool openDoorFirst;

    private void Start()
    {
        rampingSpeed = maxMovementSpeed / rampUpSpeed; // calculate speed
    }

    private IEnumerator FollowPath()
    {
        while (true)
        {
            if (timer < delay)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            else
            {
                if (Vector3.Distance(path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop), path.path.GetPointAtDistance(path.path.length, EndOfPathInstruction.Stop)) > stopLookAt &&
                    path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop) != path.path.GetPointAtDistance(path.path.length, EndOfPathInstruction.Stop))
                {
                    distanceTravelled += movementSpeed * Time.unscaledDeltaTime;
                    transform.position = path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);

                    Vector3 target = lookAt.position + lookAtOffset - transform.position;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target), rotationSpeed * Time.unscaledDeltaTime);
                    RampMovementSpeed();
                }
                else if (path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop) != path.path.GetPointAtDistance(path.path.length, EndOfPathInstruction.Stop))
                {
                    distanceTravelled += movementSpeed * Time.unscaledDeltaTime;
                    transform.position = path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, endRotation, rotationSpeed * Time.unscaledDeltaTime);
                }
                else
                {
                    if (transform.rotation != endRotation)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, endRotation, rotationSpeed * Time.deltaTime);
                        yield return null;
                    }
                    else
                    {
                        if (openDoorsAfterComplete)
                        {
                            yield return new WaitForSeconds(cameraElevatorAnimation.ChangeDoorState());
                        }
                        cameraElevatorAnimation.PathCompleted();
                        yield break;
                    }
                }
                yield return null;
            }
        }

    }

    private void RampMovementSpeed()
    {
        float timeElapsed = Time.unscaledDeltaTime;
        float distanceToTargetSpeed = maxMovementSpeed - movementSpeed;
        float distanceThisFrame = rampingSpeed * timeElapsed;

        if (distanceThisFrame  > distanceToTargetSpeed)
        {
            movementSpeed = maxMovementSpeed;
        }
        else
        {
            movementSpeed += distanceThisFrame;
        }
    }

    public void SetNewPath(PathCreator newPath, Quaternion newEndRotation)
    {
        path = newPath;
        endRotation = newEndRotation;
    }

    public void StartFollow(CameraElevatorAnimation cameraElevatorAnimation, bool openDoorFirst, Vector3 lookAtOffset, Transform lookAt, bool openDoorAfterComplete, float delay)
    {
        distanceTravelled = 0;

        this.lookAt = lookAt;
        this.lookAtOffset = lookAtOffset;
        this.cameraElevatorAnimation = cameraElevatorAnimation;
        openDoorsAfterComplete = openDoorAfterComplete;
        transform.position = path.path.GetPointAtDistance(0, EndOfPathInstruction.Stop);
        timer = 0;
        startDelay = delay;
        this.openDoorFirst = openDoorFirst;

        Invoke("FollowBegin", delay);
    }
    public void StartFollow(CameraElevatorAnimation cameraElevatorAnimation, bool openDoorFirst, Vector3 lookAtOffset, Transform lookAt, Quaternion startRotation, bool openDoorAfterComplete, float delay)
    {
        StartFollow(cameraElevatorAnimation, openDoorFirst, lookAtOffset, lookAt, openDoorAfterComplete, delay);
        transform.rotation = startRotation;
    }
    private void FollowBegin()
    {
        if (openDoorFirst)
        {
            delay = cameraElevatorAnimation.ChangeDoorState();
        }
        else
        {
            delay = -1;
        }
        StartCoroutine(FollowPath());
    }
}

