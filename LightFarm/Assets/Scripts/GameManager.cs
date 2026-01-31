using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int lightCount = 0;
    private int coinCount = 0;
    public TMPro.TextMeshProUGUI lightCountText;
    public TMPro.TextMeshProUGUI coinCountText;

    [Header("UI Costs")]
    public TextMeshProUGUI coreCostText;
    public TextMeshProUGUI pillarCostText;
    public TextMeshProUGUI fireflyCostText;
    public TextMeshProUGUI sellButtonText;

    [Header("Costs")]
    public int coreCost = 40;
    public int pillarCost = 40;
    public int fireflyCost = 40;
    public int coreCostStep = 20;
    public int pillarCostStep = 20;
    public int fireflyCostStep = 20;

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

        UpdateAllTexts();
    }

    public void IncrementLightCount()
    {
        lightCount++;
        Debug.Log("Light: " + lightCount);
        UpdateLightAndSellText();
    }

    public void SellLight()
    {
        if (lightCount > 0)
        {
            AudioManager.Instance.PlayClick();
            coinCount += lightCount;
            lightCount = 0;
            UpdateAllTexts();
        }
    }

    public void BuyCore()
    {
        AttemptPurchase(ref coreCost, coreCostStep, coreCostText, () => PlacementManager.Instance.StartPlacement(corePrefab));
    }

    public void BuyPillar()
    {
        AttemptPurchase(ref pillarCost, pillarCostStep, pillarCostText, () => PlacementManager.Instance.StartPlacement(pillarPrefab));
    }

    public void BuyFirefly()
    {
        AttemptPurchase(ref fireflyCost, fireflyCostStep, fireflyCostText, () =>
        {
            // instantiate firefly at random position near center
            Instantiate(fireflyPrefab, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f), Quaternion.identity);
        });
    }

    void AttemptPurchase(ref int cost, int step, TextMeshProUGUI costText, System.Action onSuccess)
    {
        AudioManager.Instance.PlayClick();

        if (coinCount < cost)
        {
            return;
        }

        coinCount -= cost;
        cost += step;
        onSuccess?.Invoke();
        UpdateCostText(costText, cost);
        UpdateCoinText();
    }

    void UpdateAllTexts()
    {
        UpdateLightAndSellText();
        UpdateCoinText();
        UpdateCostText(coreCostText, coreCost);
        UpdateCostText(pillarCostText, pillarCost);
        UpdateCostText(fireflyCostText, fireflyCost);
    }

    void UpdateLightAndSellText()
    {
        if (lightCountText != null)
        {
            lightCountText.text = "Light: " + lightCount;
        }

        if (sellButtonText != null)
        {
            sellButtonText.text = "Sell (" + lightCount + ")";
        }
    }

    void UpdateCoinText()
    {
        if (coinCountText != null)
        {
            coinCountText.text = "Coins: " + coinCount;
        }
    }

    void UpdateCostText(TextMeshProUGUI text, int cost)
    {
        if (text != null)
        {
            text.text = "Cost: " + cost;
        }
    }

}
