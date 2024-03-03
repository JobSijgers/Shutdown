using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] protected int maxHealth;
    protected int currentHealth;

    public virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void ChangeHealth(int amount)
    {
        currentHealth += amount;
        CheckHealth();
    }
    protected virtual void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    protected virtual void Die()
    {
    }
}
