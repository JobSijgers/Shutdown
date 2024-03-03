using PathCreation;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { private set; get; }

    [Header("Start")]
    [SerializeField] private float startDelay;
    [SerializeField] private bool startLevelWithFade;
    [SerializeField] private float startFadeDuration;
    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private bool doBeepBeep;
    [SerializeField] private GameObject beepLights;
    [SerializeField] private float blinkTime = 0.12f;
    [Space(5)]
    [SerializeField] private bool startLevelWithElevatorDoors;
    [SerializeField] private Door startDoor;
    [SerializeField] private Quaternion startRotation;
    [Space(5)]
    [SerializeField] private bool startWithPositionToCamoffsetPath;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Vector3 startDirection;
    [Space(5)]
    [SerializeField] private bool useFadeAndPath;
    [Space(5)]
    [Tooltip("The text of the textpopup should be put in the script itself")]
    [SerializeField] private bool openTextPopupAfterStart;

    [Header("End")]
    [SerializeField] private string nextLevel;
    [Space(5)]
    [SerializeField] private bool endLevelWithElevatorDoor;
    [SerializeField] private Door endDoor;
    [SerializeField] private Transform endLookAt;
    [SerializeField] private Quaternion endRotation;
    [SerializeField] private Vector3 endPathDirection;

    [Space(5)]
    [SerializeField] private bool endLevelWithWinscreen;
    [SerializeField] private Animator winScreen;
    [Space(5)]
    [SerializeField] private GameObject gameOverScreen;


    [Header("Other")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private CameraElevatorAnimation cameraAnimation;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private TextPopUp textPopUp;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Image interactionImage;
    private int amountOfInteractionTriggers;
    private bool gameOver = false;
    public delegate void PauseDuringCutscene(bool pause);
    public event PauseDuringCutscene PauseDuringCutsceneEvent;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(LateStart());
    }

    private IEnumerator FadeIn(float duration)
    {
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / duration;
            float t = fadeCurve.Evaluate(progress);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, t);
            yield return null;
        }
    }
    public void PlayerDied()
    {
        Time.timeScale = 0;
        gameOver = true;
        gameOverScreen.SetActive(true);
    }

    /// <returns>If the game is done returns true</returns>
    public bool GetGameStatus()
    {
        return gameOver;
    }
    public void StartCompleted()
    {
        if (doBeepBeep)
        {
            AudioManager.Instance.Play("Beep Beep");
            StartCoroutine(BlinkLights());
        }
        PauseDuringCutsceneEvent?.Invoke(true);
        if (openTextPopupAfterStart)
        {
            textPopUp.StartTextPopup();
        }
    }

    private IEnumerator BlinkLights()
    {
        beepLights.SetActive(true);
        yield return new WaitForSeconds(blinkTime);
        beepLights.SetActive(false);
        yield return new WaitForSeconds(blinkTime);
        beepLights.SetActive(true);
        yield return new WaitForSeconds(blinkTime);
        beepLights.SetActive(false);
    }

    public void EndCompleted()
    {
        LoadNextLevel();
    }
    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        if (useFadeAndPath)
        {
            if (startWithPositionToCamoffsetPath)
            {
                playerCamera.transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
            }
            PauseDuringCutsceneEvent?.Invoke(false);
            StartCoroutine(FadeIn(startFadeDuration));
            yield return new WaitForSeconds(startFadeDuration);
            if (startLevelWithElevatorDoors)
            {
                cameraAnimation.CreatePlayerToCamPath(startDoor, true, false, startRotation, startDelay);
            }
            if (startWithPositionToCamoffsetPath)
            {
                cameraAnimation.CreatePositionToCamPath(startPosition.position, true, false, playerCamera.GetCameraRotation(), startDirection);
            }
        }
        else
        {
            if (startLevelWithFade)
            {
                StartCoroutine(FadeIn(startFadeDuration));
                Invoke("StartCompleted", startFadeDuration);
                PauseDuringCutsceneEvent?.Invoke(false);
            }
            else if (startLevelWithElevatorDoors)
            {
                cameraAnimation.CreatePlayerToCamPath(startDoor, true, false, startRotation, startDelay);
                PauseDuringCutsceneEvent?.Invoke(false);

            }
            else if (startWithPositionToCamoffsetPath)
            {
                cameraAnimation.CreatePositionToCamPath(startPosition.position, true, false, playerCamera.GetCameraRotation(), startDirection);
                PauseDuringCutsceneEvent?.Invoke(false);
            }
            else
            {
                Debug.LogWarning("No level start set, make sure to assign one in the game manager");
            }
        }

        yield break;
    }

    public void LevelCompleted()
    {
        if (endLevelWithElevatorDoor)
        {
            cameraAnimation.CreateCamToPlayerPath(endDoor, false, true, endRotation, endPathDirection, endLookAt);
            PauseDuringCutsceneEvent?.Invoke(false);
        }
        else if (endLevelWithWinscreen)
        {
            PauseMovement(true);
            winScreen.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No level end set, make sure to assign one in the game manager");
        }
    }

    public void PauseMovement(bool disableMovement)
    {
        PauseDuringCutsceneEvent?.Invoke(!disableMovement);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    public void PlayerEnteredInteractionTrigger()
    {
        amountOfInteractionTriggers++;
        interactionImage.enabled = true;
    }
    public void PlayerLeftInteractionTrigger()
    {
        amountOfInteractionTriggers--;
        if (amountOfInteractionTriggers == 0)
        {
            interactionImage.enabled = false;
        }
    }
}
