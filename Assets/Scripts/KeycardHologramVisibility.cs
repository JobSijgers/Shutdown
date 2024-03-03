using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class KeycardHologramVisibility : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float minAlpha = 0;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float furthestDistance = 5;
    private Transform player;
    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < furthestDistance)
        {
            image.color = new Color(255, 255, 255, Mathf.Lerp(minAlpha, maxAlpha, 1 - (distance / furthestDistance)));
        }
    }
    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
    private void OnEnable()
    {
        image.enabled = true;
    }
}
