using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int lightCount = 0;
    public TMPro.TextMeshProUGUI lightCountText;

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
            lightCountText.text = "Light Count: " + lightCount;
        }

    }
}
