using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject settingsPanel;
    private bool settingsEnabled = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleSettings()
    {
        settingsEnabled = !settingsEnabled;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(settingsEnabled);
        }
    }

    public void CloseSettings()
    {
        settingsEnabled = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
