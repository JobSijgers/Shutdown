using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    public IEnumerator Respawn(float duration, GameObject go)
    {
        go.SetActive(false);
        yield return new WaitForSeconds(duration);
        go.SetActive(true);
    }
}
