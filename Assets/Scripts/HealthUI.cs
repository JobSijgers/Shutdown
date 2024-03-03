using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image health;
    [SerializeField] private Image armor;

    public void UpdateUI(int maxHealth, int health, int maxArmor, int armor)
    {
        this.health.fillAmount = health / (float)maxHealth;
        this.armor.fillAmount = armor / (float)maxArmor;
    }
}
