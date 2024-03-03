using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Net.Sockets;

public class KeycardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image holder;
    [SerializeField] private float moveTime = 1f;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] GameObject[] activeKeycards;
    private bool openConditionsMet;
    private bool secondaryOpenConditionsMet;
    private Vector3 closedLocation;
    private Vector3 openLocation;
    private float elapsed;
    private CustomInput input;

    private void Awake()
    {
        input = new CustomInput();
    }
    void Start()
    {
        closedLocation = holder.transform.position;
        openLocation = new Vector3(holder.transform.position.x, Screen.height - holder.rectTransform.rect.height / 2);
    }

    private void Update()
    {
        openConditionsMet = input.UI.OpenKeycardUI.ReadValue<float>() > 0.1f;
        if (elapsed > 0 || elapsed < 1)
        {
            float t = curve.Evaluate(elapsed);
            holder.transform.position = Vector3.Lerp(closedLocation, openLocation, t);
        }
        if (openConditionsMet && elapsed < 1 || secondaryOpenConditionsMet && elapsed < 1)
        {
            elapsed += Time.deltaTime / moveTime;
        }
     
        if (!openConditionsMet && elapsed > 0 && !secondaryOpenConditionsMet)
        {
            elapsed -= Time.deltaTime / moveTime;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        secondaryOpenConditionsMet = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        secondaryOpenConditionsMet = false;
    }

    public void ChangeKeycardUIState(KeycardType type)
    {
        //if the object is not currently active activate it else deactivate it
        if (activeKeycards[(int)type].activeInHierarchy == false)
        {
            activeKeycards[(int)type].SetActive(true);
        }
        else
        {
            activeKeycards[(int)type].SetActive(false);
        }
    }

}
