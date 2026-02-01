using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LandArea : MonoBehaviour
{
    public bool unlocked = false;

    [Header("References")]
    public Collider2D landCollider;
    public GameObject fogLight; // optional, can be null
    public GameObject buttonPanel; // optional, can be null

    void Awake()
    {
        ApplyState();
    }

    public bool Contains(Vector3 worldPos)
    {
        if (!unlocked || landCollider == null)
            return false;

        return landCollider.OverlapPoint(worldPos);
    }

    public void Unlock()
    {
        unlocked = true;
        ApplyState();
    }

    void ApplyState()
    {
        if (fogLight != null)
            fogLight.SetActive(!unlocked);

        if (buttonPanel != null)
            buttonPanel.SetActive(!unlocked);
    }
}
