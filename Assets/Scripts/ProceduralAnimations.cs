using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ProceduralAnimations : MonoBehaviour
{
    [SerializeField] private bool playSoundOnStep = false;
    [SerializeField] private Transform[] legs;
    [SerializeField] private Transform[] targets;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float stepSpeed = .1f;
    [SerializeField] private float stepHeight = .25f;
    [SerializeField] private int minLegsGroundedForMove = 3;
    [SerializeField] private float stepMultiplier;

    [Header("Idle")]
    [SerializeField] private float idleReturnThreshold = 0.01f;
    [SerializeField] private float idleReturnDistance = 0.1f;
    [SerializeField] private float idleReturnTime = .1f;

    private Vector3[] currentLegLocations;
    private Vector3[] previousTargetPositions;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private bool[] legsGrounded;
    private float resetTimer;


    private void Start()
    {
        previousTargetPositions = new Vector3[legs.Length];
        currentLegLocations = new Vector3[legs.Length];
        legsGrounded = new bool[legs.Length];

        for (int i = 0; i < legs.Length; i++)
        {
            if (Physics.Raycast(legs[i].position, Vector3.down, out RaycastHit hit, 100, groundMask))
            {
                currentLegLocations[i] = hit.point;
                targets[i].position = hit.point;
                legsGrounded[i] = true;
            }
        }
    }
    private void Update()
    {

        Vector3 movement = transform.position - previousPosition;
        Vector3 rotation = transform.rotation.eulerAngles - previousRotation.eulerAngles;

        previousRotation = transform.rotation;
        previousPosition = transform.position;

        if (Vector3.Distance(movement, Vector3.zero) < idleReturnThreshold &&
            Vector3.Distance(rotation, Vector3.zero) < idleReturnThreshold)
        {
            resetTimer += Time.deltaTime;
        }
        else
        {
            resetTimer = 0;
        }

        if (resetTimer > idleReturnTime)
        {
            ReturnToIdle();
        }
    }
    private void FixedUpdate()
    {

        for (int i = 0; i < legs.Length; i++)
        {
            if (!legsGrounded[i])
                continue;
            legs[i].position = currentLegLocations[i];
            if (Vector3.Distance(legs[i].position, targets[i].position) > stepDistance)
            {
                if (CountGroundedLegs() < minLegsGroundedForMove)
                {
                    continue;
                }
                Vector3 movement = targets[i].position - previousTargetPositions[i];
                Vector3 targetPoint = targets[i].position + Vector3.ClampMagnitude(movement * stepMultiplier, stepDistance);
                StartCoroutine(MoveLeg(i, targetPoint));
            }
            previousTargetPositions[i] = targets[i].position;
        }

    }
    private int CountGroundedLegs()
    {
        int groundedLegs = 0;
        for (int i = 0; i < legs.Length; i++)
        {

            if (legsGrounded[i])
            {
                groundedLegs++;
            }
        }
        return groundedLegs;
    }
    private IEnumerator MoveLeg(int index, Vector3 targetPoint)
    {
        legsGrounded[index] = false;

        Vector3 startPos = currentLegLocations[index];
        float t = 0f;

        while (t < 1f)
        {
            // Calculate the vertical offset based on time and step height
            float yOffset = Mathf.Sin(t * Mathf.PI) * stepHeight;

            // Interpolate horizontally towards the target position
            t += Time.deltaTime / stepSpeed;
            legs[index].position = Vector3.Lerp(startPos, targetPoint, t);

            // Apply the vertical offset
            legs[index].position += Vector3.up * yOffset;

            yield return null;
        }
        if (playSoundOnStep)
        {
            AudioManager.Instance.Play("Step");
        }
        legs[index].position = targetPoint;
        currentLegLocations[index] = legs[index].position;
        legsGrounded[index] = true;
    }
    private void ReturnToIdle()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (!legsGrounded[i])
                continue;

            if (CountGroundedLegs() < minLegsGroundedForMove)
                continue;

            if (Vector3.Distance(legs[i].position, targets[i].position) < idleReturnDistance)
                continue;


            StartCoroutine(MoveLeg(i, targets[i].position));
        }
    }
    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < targets.Length; i++)
    //    {
    //        Handles.color = Color.red;
    //        Handles.DrawLine(legs[i].position, targets[i].position, 5);
    //    }
    //}
}
