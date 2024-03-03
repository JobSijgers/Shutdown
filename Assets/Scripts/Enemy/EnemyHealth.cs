using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : Health
{
    [SerializeField] private Slider slider;
    [SerializeField] private float movementTime = 0.3f;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private GameObject deathParticles;
    private FieldOfView targetting;
    private Coroutine coroutine;

    public override void Start()
    {
        base.Start();
        targetting = GetComponent<FieldOfView>();
        slider.gameObject.SetActive(false);
    }

    protected override void Die()
    {
        Instantiate(deathParticles, transform.position, Quaternion.Euler(transform.up));
        Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            slider.gameObject.SetActive(true);

            targetting.StartChasing();
            ChangeHealth(-1);
            if (coroutine != null)
            { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(MoveSlider());
            Destroy(other.gameObject);
        }
    }

    private IEnumerator MoveSlider()
    {
        float startFill = slider.value;
        float endFill = (float)currentHealth / maxHealth;
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / movementTime;
            slider.value = ClamplessLerp(startFill, endFill, curve.Evaluate(progress));
            yield return null;
        }
        coroutine = null;
    }

    private float ClamplessLerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
