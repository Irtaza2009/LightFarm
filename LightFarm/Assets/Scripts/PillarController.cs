using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    [Header("Capacity")]
    public int maxFireflies = 5;

    [Header("UI")]
    public TMP_Text countText;

    private readonly HashSet<FireflyController> fireflies = new HashSet<FireflyController>();

    public int FireflyCount => fireflies.Count;

    public bool TryAddFirefly(FireflyController firefly)
    {
        if (firefly == null)
        {
            return false;
        }

        if (fireflies.Contains(firefly))
        {
            return true;
        }

        if (fireflies.Count >= maxFireflies)
        {
            return false;
        }

        fireflies.Add(firefly);
        UpdateCountText();
        return true;
    }

    public void RemoveFirefly(FireflyController firefly)
    {
        if (firefly == null)
        {
            return;
        }

        if (fireflies.Remove(firefly))
        {
            UpdateCountText();
        }
    }

    void OnDisable()
    {
        fireflies.Clear();
        UpdateCountText();
    }

    void OnValidate()
    {
        UpdateCountText();
    }

    void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = fireflies.Count + "/" + maxFireflies;
        }
    }
}
