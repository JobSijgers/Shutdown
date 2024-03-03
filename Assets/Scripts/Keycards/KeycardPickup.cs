using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    [SerializeField] private Keycard keycard;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory.Instance.AddKeycard(keycard);
            Destroy(transform.parent.gameObject);
        }
    }
}
