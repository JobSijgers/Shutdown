using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour
{
    [SerializeField] private float changeTime = 2f;
    [SerializeField] private Gradient gradient;
    [SerializeField] private List<Light> lights = new();
    [SerializeField] private AnimationCurve transitionCurve;
    float progress = 0;
    bool isReturning;

    void Update()
    {
        if (progress < 1)
        {
            progress += Time.deltaTime / changeTime;

            foreach(Light light in lights)
            {
                float point = isReturning ? 1 - progress : progress;
                light.color = gradient.Evaluate(transitionCurve.Evaluate(point));
            }
        }
        else
        {
            isReturning = !isReturning;
            progress = 0;
        }


    }
}
