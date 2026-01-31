using UnityEngine;

[ExecuteAlways]
public class SortingByY : MonoBehaviour
{
    [Tooltip("Optional sorting layer name to apply to all child sprite renderers.")]
    public string sortingLayerName = "";

    [Tooltip("Order offset if you need to bump this object above/below its Y-based value.")]
    public int orderOffset = 0;

    [Tooltip("Multiplier for converting world Y to order. Higher = more separation between nearby objects.")]
    public float orderMultiplier = 100f;

    private SpriteRenderer[] renderers;

    void OnEnable()
    {
        CacheRenderers();
        UpdateOrder();
    }

    void OnValidate()
    {
        CacheRenderers();
        UpdateOrder();
    }

    void LateUpdate()
    {
        UpdateOrder();
    }

    void CacheRenderers()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void UpdateOrder()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        int baseOrder = Mathf.RoundToInt(-transform.position.y * orderMultiplier) + orderOffset;

        foreach (var sr in renderers)
        {
            if (sr == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                sr.sortingLayerName = sortingLayerName;
            }

            sr.sortingOrder = baseOrder;
        }
    }
}
