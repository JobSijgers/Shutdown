using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    [SerializeField] private int amount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().PickupArmor(amount);
            Destroy(gameObject);
        }
    }
}
