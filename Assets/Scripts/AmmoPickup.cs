using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int magazinesAmount;
    [SerializeField] private WeaponType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerShooting>().AddMagazines(magazinesAmount);
            Destroy(gameObject);
        }
    }

}
