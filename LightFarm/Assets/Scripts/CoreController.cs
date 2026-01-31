using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
[RequireComponent(typeof(Collider2D))]
public class CoreController : MonoBehaviour
{
    [Header("Base Light")]
    public float baseRadius = 1.6f;
    public float baseIntensity = 1f;

    [Header("Per Firefly Gain")]
    public float radiusPerFirefly = 0.5f;
    public float intensityPerFirefly = 0.2f;

    private Light2D lightComponent;
    private readonly HashSet<FireflyController> fireflies = new HashSet<FireflyController>();

    void Awake()
    {
        lightComponent = GetComponent<Light2D>();
        if (lightComponent == null)
        {
            Debug.LogError("CoreController: Missing Light2D component!");
        }

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        UpdateLight();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        FireflyController firefly = other.GetComponent<FireflyController>();
        if (firefly == null) return;

        if (fireflies.Add(firefly))
        {
            UpdateLight();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        FireflyController firefly = other.GetComponent<FireflyController>();
        if (firefly == null) return;

        if (fireflies.Remove(firefly))
        {
            UpdateLight();
        }
    }

    void OnDisable()
    {
        fireflies.Clear();
        UpdateLight();
    }

    void UpdateLight()
    {
        if (lightComponent == null) return;

        int count = fireflies.Count;

        lightComponent.pointLightOuterRadius =
            baseRadius + count * radiusPerFirefly;

        lightComponent.intensity =
            baseIntensity + count * intensityPerFirefly;
    }
}
