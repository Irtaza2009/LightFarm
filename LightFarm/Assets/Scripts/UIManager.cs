using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject settingsPanel;
    private bool settingsEnabled = false;

    public GameObject leaderboardPanel;
    private bool leaderboardEnabled = false;

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

    public void ToggleLeaderboard()
    {
        leaderboardEnabled = !leaderboardEnabled;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(leaderboardEnabled);
        }
    }

    public void CloseLeaderboard()
    {
        leaderboardEnabled = false;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
}