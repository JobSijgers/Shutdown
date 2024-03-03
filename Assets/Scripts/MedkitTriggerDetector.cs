using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitTriggerDetector : MonoBehaviour
{
    [SerializeField] private Medkit medkit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            medkit.MedkitCollected();
            other.GetComponent<PlayerHealth>().ChangeHealth(medkit.GetHealAmount());
        }
    }
}
