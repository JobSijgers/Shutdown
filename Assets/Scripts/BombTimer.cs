using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BombTimer : MonoBehaviour
{
    [SerializeField] private float timer = 120;
    [SerializeField] private float blinkStartTime = 30;
    [SerializeField] private float blinkTime = 0.2f;
    [SerializeField] private float fadeTime = 1.5f;
    [SerializeField] private float fadeDelay = 1f;
    [SerializeField] private Transform explosionPos;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private TMP_Text display;
    [SerializeField] private GameObject detonationCam;
    [SerializeField] private Image fadeOut;
    private GameObject mainCam;
    private Coroutine blinkCoroutine;
    private Coroutine detonation;

    private void Start()
    {
        mainCam = Camera.main.gameObject;
        display.text = FormatTime(timer);
    }

    private void Update()
    {
        if (timer < 0) return;

        timer -= Time.deltaTime;
        display.text = FormatTime(timer);
        if (timer < blinkStartTime && blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(Blink());
        }

        if (timer <= 0)
        {
            timer = 0;
            display.text = FormatTime(timer);
            StopCoroutine(blinkCoroutine);
            display.alpha = 255;
            if (detonation == null)
            { detonation = StartCoroutine(Detonate()); }
        }

    }

    private IEnumerator Blink()
    {
        while (true)
        {
            display.alpha = display.alpha == 0 ? 255 : 0;
            yield return new WaitForSeconds(blinkTime);
        }
    }


    private string FormatTime(float time)
    {
        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float fraction = time * 100;
        fraction %= 100;
        string timeText = string.Format("{0:0}:{1:00}.{2:00}", minutes, seconds, fraction);
        return timeText;
    }

    private IEnumerator Detonate()
    {
        mainCam.SetActive(false);
        detonationCam.SetActive(true);
        Instantiate(explosionEffect, explosionPos);
        AudioManager.Instance.Play("Detonation");
        AudioManager.Instance.Stop("Level Music");

        yield return new WaitForSeconds(fadeDelay);

        Color fillColor = fadeOut.color;
        float progress = 0;
        while(progress < 1)
        {
            progress += Time.deltaTime / fadeTime;
            fillColor.a = progress;
            fadeOut.color = fillColor;
            yield return null;
        }

        mainCam.SetActive(true);
        detonationCam.SetActive(false);
        GameManager.Instance.PlayerDied();
    }

    public void StopTimer()
    {
        display.text = FormatTime(timer);
        timer = -1; // stops the timer from updating
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            display.alpha = 255;
        }
        GameManager.Instance.LevelCompleted();
    }

}
