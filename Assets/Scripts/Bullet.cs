using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float despawnTime = 10f;

    private void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    void Update()
    {
        transform.position += movementSpeed * Time.deltaTime * transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Hazard") && !other.gameObject.CompareTag("PlayerBullet") && !other.gameObject.CompareTag("Trigger"))
        {
            Destroy(gameObject);
        }
    }
}
