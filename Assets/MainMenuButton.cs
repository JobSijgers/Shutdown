using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string name;

    public void GoToScene()
    {
        SceneManager.LoadScene(name);
    }
    public void ApplicationQuit()
    {
        Application.Quit();
    }
}
