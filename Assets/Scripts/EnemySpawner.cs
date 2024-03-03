using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] EnemiesToActivate;
    [SerializeField] private float delayBetweenSpawns;
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && activated == false        )
        {
            activated = true;

            StartCoroutine(SpawnEnemiesWithDelay());
        }
    }

    private IEnumerator SpawnEnemiesWithDelay()
    {
        foreach (GameObject enemy in EnemiesToActivate)
        {
            yield return new WaitForSeconds(delayBetweenSpawns);
            enemy.SetActive(true);
        }
    }
}
