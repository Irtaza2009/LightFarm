using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int lightCount = 0;
    public TMPro.TextMeshProUGUI lightCountText;

    [Header("Prefabs")]
    public GameObject corePrefab;
    public GameObject pillarPrefab;
    public GameObject fireflyPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementLightCount()
    {
        lightCount++;
        Debug.Log("Light: " + lightCount);
        // update light count text
        if (lightCountText != null)
        {
            lightCountText.text = "Light: " + lightCount;
        }

    }

    public void BuyCore()
    {
        PlacementManager.Instance.StartPlacement(corePrefab);
    }

    public void BuyPillar()
    {
        PlacementManager.Instance.StartPlacement(pillarPrefab);
    }

    public void BuyFirefly()
    {
        PlacementManager.Instance.StartPlacement(fireflyPrefab);
    }

}
