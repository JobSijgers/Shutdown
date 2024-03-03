using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TextPopUp : MonoBehaviour
{
    [SerializeField] private Slider leftSlider;
    [SerializeField] private Slider rightSlider;
    [SerializeField] private float horizontalDuration;
    [SerializeField] private float verticalDuration;
    [SerializeField] private float sliderClosedFillAmount;
    [Header("Sting")]
    [SerializeField] private string[] textToDisplay;
    [SerializeField] private TMP_Text textDisplay;
    [Range(0f, 0.3f)]
    [SerializeField] private float characterInterval;
    [SerializeField] private KeyCode keyToAdvance;
    private Coroutine addStringRoutine;
    private bool textWindowOpen = false;
    private int currentTextIndex;
    private Vector3 rightClosedPosition;
    private Vector3 leftClosedPosition;
    private Vector3 rightOpenPosition;
    private Vector3 leftOpenPosition;

    private void Start()
    {
        rightOpenPosition = rightSlider.transform.position;
        leftOpenPosition = leftSlider.transform.position;

        leftClosedPosition = new Vector3(leftSlider.transform.position.x,  Screen.height + leftSlider.fillRect.rect.height / 2);
        rightClosedPosition = new Vector3(rightSlider.transform.position.x, Screen.height + rightSlider.fillRect.rect.height / 2);

        leftSlider.transform.position = leftClosedPosition;
        rightSlider.transform.position = rightClosedPosition;
    }
    public void Update()
    {
        if (textWindowOpen)
        {
            if (addStringRoutine == null)
            {
                addStringRoutine = StartCoroutine(AddStingToTMP(textToDisplay[currentTextIndex], textDisplay, characterInterval));
            }
            if (Input.GetKeyDown(keyToAdvance))
            {
                if (addStringRoutine != null)
                {
                    StopCoroutine(addStringRoutine);
                    addStringRoutine = null;
                }
                if (currentTextIndex == textToDisplay.Length - 1)
                {
                    StartCoroutine(CloseTextMenu());
                    textDisplay.text = "";
                    textWindowOpen = false;
                }
                else
                {
                    currentTextIndex++;
                }
            }
        }
    }

    private IEnumerator AddStingToTMP(string text, TMP_Text TMPtext, float timePerCharacter)
    {
        string currentString = "";
        int currentStringIndex = 0;

        while (currentString.Length < text.Length)
        {
            currentString += text[currentStringIndex];
            TMPtext.text = currentString;
            currentStringIndex++;
            yield return new WaitForSeconds(timePerCharacter);
        }
    }
    public void SetNewStringArray(string[] newStringArray)
    {
        textToDisplay = newStringArray;
    }
    private IEnumerator LerpSlidersValue(float oldValue, float newValue, float duration)
    {
        float elapsed = 0;

        while (elapsed < 1)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed);
            float lerpedValue = Mathf.Lerp(oldValue, newValue, t);
            leftSlider.value = lerpedValue;
            rightSlider.value = lerpedValue;
            elapsed += Time.deltaTime / duration;
            yield return null;
        }
    }
    private IEnumerator LerpSliderPosition(Transform objectToLerp, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0;

        while (elapsed < 1)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed); 
            Vector3 lerpedValue = Vector3.Lerp(start, end, t);
            objectToLerp.position = lerpedValue;
            elapsed += Time.deltaTime / duration;
            yield return null;
        }
    }
    private IEnumerator OpenTextMenu()
    {
        StartCoroutine(LerpSliderPosition(rightSlider.transform, rightClosedPosition, rightOpenPosition, verticalDuration));
        yield return StartCoroutine(LerpSliderPosition(leftSlider.transform, leftClosedPosition, leftOpenPosition, verticalDuration));

        StartCoroutine(LerpSlidersValue(sliderClosedFillAmount, 1f, horizontalDuration));
        yield return StartCoroutine(LerpSlidersValue(sliderClosedFillAmount, 1f, horizontalDuration));

        textWindowOpen = true;
    }
    private IEnumerator CloseTextMenu()
    {
        StartCoroutine(LerpSlidersValue(1f, sliderClosedFillAmount, horizontalDuration));
        yield return StartCoroutine(LerpSlidersValue(1f, sliderClosedFillAmount, horizontalDuration));

        StartCoroutine(LerpSliderPosition(rightSlider.transform, rightOpenPosition, rightClosedPosition, verticalDuration));
        yield return StartCoroutine(LerpSliderPosition(leftSlider.transform, leftOpenPosition, leftClosedPosition, verticalDuration));

    }
    public void StartTextPopup()
    {
        StartCoroutine(OpenTextMenu()); 
    }
}
