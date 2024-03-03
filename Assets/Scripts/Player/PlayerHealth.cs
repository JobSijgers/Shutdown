using UnityEngine.Playables;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private int maxArmor;
    [SerializeField] private HealthUI healthUI;
    private int armor;

    public override void Start()
    {
        base.Start();
        healthUI.UpdateUI(maxHealth, currentHealth, maxArmor, armor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            ChangeHealth(-1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hazard"))
        {
            ChangeHealth(-1);
        }
    }

    protected override void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public override void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            int remainingDamage;
            if (armor > 0)
            {
                if (armor + amount >= 0)
                {
                    armor += amount;
                }
                else
                {
                    remainingDamage = amount + armor;
                    armor = 0;
                    currentHealth += remainingDamage;
                }
            }
            else
            {
                currentHealth += amount;
            }
            CheckHealth();
        }
        else if (currentHealth + amount >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
        }
        healthUI.UpdateUI(maxHealth, currentHealth, maxArmor, armor);

    }

    protected override void Die()
    {
        GameManager.Instance.PlayerDied();
    }
    
    public void PickupArmor(int amount)
    {
        if (amount + armor >= maxArmor)
        {
            armor = maxArmor;
        }
        else
        {
            armor += amount;
        }
        healthUI.UpdateUI(maxHealth, currentHealth, maxArmor, armor);
    }
}
