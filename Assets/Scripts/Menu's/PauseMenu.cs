using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private string mainMenuSceneName;
    private GameManager gameManager;
    private CustomInput input;
    private InputAction changeUiState;

    private void Awake()
    {
        input = new CustomInput();
        input.UI.ChangePauseMenuState.performed += ChangeMenuState;
    }
    private void Start()
    {
        gameManager = GameManager.Instance;    
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void ChangeMenuState(InputAction.CallbackContext ctx)
    {
        ChangeMenuState();
    }
    public void ChangeMenuState()
    {
        if (gameManager.GetGameStatus() == true)
            return;

        pauseMenu.SetActive(!pauseMenu.activeInHierarchy);

        if (pauseMenu.activeInHierarchy)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            optionsMenu.SetActive(false);
            controlsMenu.SetActive(false);
        }
    }
}

