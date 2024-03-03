using PathCreation;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraElevatorAnimation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 playerOffset;
    [SerializeField] private Vector3 pathDirection;
    [SerializeField] private ElevatorPathFollower pathFollower;
    [SerializeField] private Door elevatorToChangeState;
    [SerializeField] private PlayerCamera playerCamera;

    private bool giveStartPathCompletedCall;
    private bool giveEndPathCompletedCall;

    public void CreatePlayerToCamPath(Door newElevator, bool startPathCompleted, bool endPathCompletd, Quaternion startRotation, float delay)
    {
        giveStartPathCompletedCall = startPathCompleted;
        giveEndPathCompletedCall = endPathCompletd;
        elevatorToChangeState = newElevator;
        Vector3[] positions = new Vector3[2];
        //start point is player position + the playeroffset
        positions[0] = player.position + playerOffset;
        //end position is the camera position
        positions[1] = player.position + playerCamera.GetCameraOffset();

        //create a new path with anchor points on the start and end positions
        BezierPath path = new(positions, false, PathSpace.xyz);
        //set the control nodes to free
        path.ControlPointMode = BezierPath.ControlMode.Free;
        //set the first control point to the position of the first point plus the path direction
        path.SetPoint(1, positions[0] + (pathDirection * 2));
        //set the second control point to the position of the second point plus the path direction
        path.SetPoint(2, positions[1] + (pathDirection * 2));

        PathCreator pathCreator = gameObject.AddComponent<PathCreator>();

        //sets the path of the pathcreator to the created path
        pathCreator.bezierPath = path;

        //sets the path of the follower
        pathFollower.SetNewPath(pathCreator, playerCamera.GetCameraRotation());
        //starts the following
        pathFollower.StartFollow(this, true, Vector3.zero, player.transform, startRotation, false, delay);
    }
    public void CreateCamToPlayerPath(Door newElevator, bool startPathcompleted, bool endPathCompletd, Quaternion endRotation, Vector3 pathDirection, Transform lookAt)
    {
        giveStartPathCompletedCall = startPathcompleted;
        giveEndPathCompletedCall = endPathCompletd;
        elevatorToChangeState = newElevator;
        Vector3[] positions = new Vector3[2];
        //start position position is the camera position
        positions[0] = player.position + playerCamera.GetCameraOffset();
        //end point is player position + the playeroffset
        positions[1] = player.position + playerOffset;

        //create a new path with anchor points on the start and end positions
        BezierPath path = new(positions, false, PathSpace.xyz);
        //set the control nodes to free
        path.ControlPointMode = BezierPath.ControlMode.Free;
        //set the first control point to the position of the first point plus the path direction
        path.SetPoint(1, positions[0] + (pathDirection * 2));
        //set the second control point to the position of the second point plus the path direction
        path.SetPoint(2, positions[1] + (pathDirection * 2));


        PathCreator pathCreator = gameObject.AddComponent<PathCreator>();
        //sets the path of the pathcreator to the created path
        pathCreator.bezierPath = path;

        //sets the path of the follower
        pathFollower.SetNewPath(pathCreator, endRotation);
        //starts the following
        pathFollower.StartFollow(this, false, playerOffset, lookAt, true, 0);
    }

    public void CreatePositionToCamPath(Vector3 beginPosition, bool startPathcompleted, bool endPathCompletd, Quaternion endRotation, Vector3 pathDirection)
    {
        giveStartPathCompletedCall = startPathcompleted;
        giveEndPathCompletedCall = endPathCompletd;
        Vector3[] positions = new Vector3[2];
        positions[0] = beginPosition;
        positions[1] = player.position + playerCamera.GetCameraOffset();

        //create a new path with anchor points on the start and end positions
        BezierPath path = new(positions, false, PathSpace.xyz);
        //set the control nodes to free
        path.ControlPointMode = BezierPath.ControlMode.Free;
        //set the first control point to the position of the first point plus the path direction
        path.SetPoint(1, positions[0] + (pathDirection * 2));
        //set the second control point to the position of the second point plus the path direction
        path.SetPoint(2, positions[1] + (pathDirection * 2));


        PathCreator pathCreator = gameObject.AddComponent<PathCreator>();
        //sets the path of the pathcreator to the created path
        pathCreator.bezierPath = path;

        //sets the path of the follower
        pathFollower.SetNewPath(pathCreator, endRotation);
        //starts the following
        pathFollower.StartFollow(this, false, playerOffset, player, false, 0);
    }
    public float ChangeDoorState()
    {
        return elevatorToChangeState.ChangeDoorState();
    }
    public void PathCompleted()
    {
        if (giveStartPathCompletedCall)
        {
            GameManager.Instance.StartCompleted();
            giveStartPathCompletedCall = false;
        }
        if (giveEndPathCompletedCall)
        {
            GameManager.Instance.EndCompleted();
            giveEndPathCompletedCall = false;
        }
    }
}
