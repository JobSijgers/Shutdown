using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Medkit : MonoBehaviour
{
    [SerializeField] private int healAmount;
    [SerializeField] private float respawnDuration;
    [SerializeField] private GameObject medkitObject;
    [SerializeField] private GameObject respawnTimer;
    [SerializeField] private Image respawnClock;

    private IEnumerator Respawn()
    {
        float t = 1;
        medkitObject.SetActive(false);
        respawnTimer.SetActive(true);
        while (t > 0)
        {
            t -= Time.deltaTime / respawnDuration;
            respawnClock.fillAmount = t;
            yield return null;
        }
        medkitObject.SetActive(true);
        respawnTimer.SetActive(false);
    }
    public void MedkitCollected()
    {
        StartCoroutine(Respawn());
    }

    public int GetHealAmount()
    {
        return healAmount;
    }
}
