using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    public NavMeshAgent _agent;
    public bool trackingWaypoints = true;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float targetDistance;
    [SerializeField] private float rotationSpeed;

    private int _currentWaypointIndex;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(waypoints[0].transform.position);
    }

    void Update()
    {
        if (_agent.remainingDistance > targetDistance || !trackingWaypoints) return;

        _currentWaypointIndex++;
        if (_currentWaypointIndex >= waypoints.Count)
        { _currentWaypointIndex = 0; }

        SetNextTarget(waypoints[_currentWaypointIndex].transform.position);
        StartCoroutine(RotateToTarget(waypoints[_currentWaypointIndex].transform.position));
    }

    private IEnumerator RotateToTarget(Vector3 target)
    {
        _agent.isStopped = true;
        Vector3 direction = (target - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        _agent.isStopped = false;
    }

    public void SetNextTarget(Vector3 position)
    {
        _agent.SetDestination(position);
    }

    /// <summary>
    /// Go back to following waypoints
    /// </summary>
    public void ContinueRoutine()
    {
        SetNextTarget(waypoints[_currentWaypointIndex].transform.position);
    }

    public void StartTracking()
    {
        trackingWaypoints = true;
        _agent.isStopped = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints.Count == 0 || waypoints == null) return;
        Gizmos.color = Color.black;
        foreach (Transform waypoint in waypoints)
        {
            Gizmos.DrawSphere(waypoint.transform.position, 0.2f);
        }
    }

}
