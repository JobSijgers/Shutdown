using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class Door : MonoBehaviour
{
    public EDoorType doorType;

    public float openingTime;

    public AnimationCurve curve;


    [HideInInspector] public GameObject leftDoor;
    [HideInInspector] public GameObject rightDoor;

    [HideInInspector] public Vector3 leftDirection;
    [HideInInspector] public Vector3 rightDirection;

    [HideInInspector] public float leftDoorOpenAmount;
    [HideInInspector] public float rightDoorOpenAmount;

    [HideInInspector] public Quaternion leftEndRotation;
    [HideInInspector] public Quaternion rightEndRotation;

    [HideInInspector] public bool doorRequiresKeycard;
    [HideInInspector] public KeycardType doorKeyType;
    [HideInInspector] public KeycardHologramVisibility[] keycardVisibility;
    [HideInInspector] public Sprite     keycardSprite;

    [HideInInspector] public bool closeDoorAfterOpen;
    [HideInInspector] public float closeDoorAfterDuration;
    [Header("Sound")]
    [SerializeField] private string soundName;

    private DoorStatus doorStatus = DoorStatus.Closed;
    private CustomInput input;
    private bool inDoorRange;

    private Quaternion leftDoorStartRotation;
    private Quaternion rightDoorStartRotation;

    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorOpenPosition;

    private Vector3 leftDoorClosedPosition;
    private Vector3 rightDoorClosedPosition;
    private void Awake()
    {
        input = new CustomInput();
        input.Intercation.Intercation.performed += OpenDoor;
    }
    private void Start()
    {
        foreach(KeycardHologramVisibility visibility in keycardVisibility)
        {
            visibility.enabled = true;
            visibility.SetSprite(keycardSprite);
        }
        //sets all the positions used for opening and closing
        if (doorType == EDoorType.DoubleSlidingDoor)
        {
            leftDoorClosedPosition = leftDoor.transform.position;
            rightDoorClosedPosition = rightDoor.transform.position;

            leftDoorOpenPosition = leftDoorClosedPosition + leftDoorOpenAmount * leftDirection;
            rightDoorOpenPosition = rightDoorClosedPosition + rightDoorOpenAmount * rightDirection;
        }
        if (doorType == EDoorType.DoubleRotatingDoor)
        {
            leftDoorStartRotation = leftDoor.transform.rotation;
            rightDoorStartRotation = rightDoor.transform.rotation;  
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inDoorRange = true;
            GameManager.Instance.PlayerEnteredInteractionTrigger();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inDoorRange = false;
            GameManager.Instance.PlayerLeftInteractionTrigger();
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    private void OpenDoor(InputAction.CallbackContext context)
    {
        if (!inDoorRange)
            return;
        if (doorStatus != DoorStatus.Closed)
            return;
        //if the door needs a keycard and the player has the correct keycard open the door.
        if (doorRequiresKeycard)
            if (Inventory.Instance.CheckForKeycard(doorKeyType) == true)
            {
                Inventory.Instance.RemoveKeycard(doorKeyType);
            }
            else
                return;

        StartCoroutine(OpenDoorRoutine());
    }
    private IEnumerator OpenDoorRoutine()
    {
        AudioManager.Instance.Play(soundName);
        //double doors
        if (doorType == EDoorType.DoubleSlidingDoor)
        {
            doorStatus = DoorStatus.Changing;
            StartCoroutine(LerpVector3(leftDoor.transform, leftDoorClosedPosition, leftDoorOpenPosition, openingTime, curve));
            StartCoroutine(LerpVector3(rightDoor.transform, rightDoorClosedPosition, rightDoorOpenPosition, openingTime, curve));
            yield return new WaitForSeconds(openingTime);
            doorStatus = DoorStatus.Open;
        }
        //rotating doors
        if (doorType == EDoorType.DoubleRotatingDoor)
        {
            doorStatus = DoorStatus.Changing;
            StartCoroutine(LerpQuaternion(rightDoor.transform, rightDoor.transform.rotation, rightEndRotation, openingTime, curve));
            StartCoroutine(LerpQuaternion(leftDoor.transform, leftDoor.transform.rotation, leftEndRotation, openingTime, curve));
            yield return new WaitForSeconds(openingTime);
            doorStatus = DoorStatus.Open;
        }

        if (closeDoorAfterOpen)
        {
            yield return new WaitForSeconds(closeDoorAfterDuration);
            StartCoroutine(CloseDoorRoutine());
        }
    }
    private IEnumerator CloseDoorRoutine()
    {
        AudioManager.Instance.Play(soundName);
        if (doorType == EDoorType.DoubleSlidingDoor)
        {
            doorStatus = DoorStatus.Changing;
            StartCoroutine(LerpVector3(leftDoor.transform, leftDoorOpenPosition, leftDoorClosedPosition, openingTime, curve));
            StartCoroutine(LerpVector3(rightDoor.transform, rightDoorOpenPosition, rightDoorClosedPosition, openingTime, curve));
            yield return new WaitForSeconds(openingTime);
            doorStatus = DoorStatus.Closed;
        }
        if (doorType == EDoorType.DoubleRotatingDoor)
        {
            doorStatus = DoorStatus.Changing;
            StartCoroutine(LerpQuaternion(rightDoor.transform, rightEndRotation, rightDoorStartRotation, openingTime, curve));
            StartCoroutine(LerpQuaternion(leftDoor.transform, leftEndRotation, leftDoorStartRotation, openingTime, curve));
            yield return new WaitForSeconds(openingTime);
            doorStatus = DoorStatus.Closed;
        }
    }

    /// <summary>
    /// Lerps the rotation of a object to a target rotation with a duration and curve
    /// </summary>
    private IEnumerator LerpQuaternion(Transform objectToLerp, Quaternion startValue, Quaternion endValue, float duration, AnimationCurve curve)
    {
        float progress = 0;

        while (progress < 1)
        {
            progress += Time.deltaTime / duration;
            float t = curve.Evaluate(progress);
            objectToLerp.rotation = Quaternion.Lerp(startValue, endValue, t);
            yield return null;
        }
    }
    /// <summary>
    /// Lerps the position of a object to a target rotation with a duration and curve
    /// </summary>
    private IEnumerator LerpVector3(Transform objectToLerp, Vector3 startValue, Vector3 endValue, float duration, AnimationCurve curve)
    {
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / duration;
            float t = curve.Evaluate(progress);
            objectToLerp.position = Vector3.Lerp(startValue, endValue, t);
            yield return null;
        }
    }
    public float ChangeDoorState()
    {
        if (doorStatus == DoorStatus.Open)
        {
            StartCoroutine(CloseDoorRoutine());
        }
        if (doorStatus == DoorStatus.Closed)
        {
            StartCoroutine(OpenDoorRoutine());
        }
        return openingTime;
    }
    public void FetchHolograms()
    {
        keycardVisibility = GetComponentsInChildren<KeycardHologramVisibility>();
    }
}
