using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string level1Scene;
    [SerializeField] private Image fadeImage;
    public void QuitApplication()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        StartCoroutine(StartLevel());
    }
    private IEnumerator StartLevel()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime;
            float r = fadeImage.color.r;
            float g = fadeImage.color.g;
            float b = fadeImage.color.b;
            fadeImage.color = new Color(r, g, b ,t);
            yield return null;
        }
        SceneManager.LoadScene(level1Scene);
    }
}
