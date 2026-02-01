using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dan.Main;

public class Leaderboard : MonoBehaviour
{
    [SerializeField]
    private List<TextMeshProUGUI> playerNameTexts;
    [SerializeField]
    private List<TextMeshProUGUI> playerScoreTexts;

    private string publicLeaderboardKey = "b60fe267dcac0272749930b332742984f523c55c73ad2976f8ec5f48e5f43092";

    private void Start()
    {
        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, ((msg) =>
        {
            int loopLength = Mathf.Min(msg.Length, playerNameTexts.Count);
            for (int i = 0; i < loopLength; ++i)
            {
                playerNameTexts[i].text = msg[i].Username;
                playerScoreTexts[i].text = msg[i].Score.ToString();
            }
        }));
    }

    public void SetLeaderboardEntry(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, score, (msg) =>
        {
            Debug.Log("Leaderboard entry set: " + msg);
            GetLeaderboard();
        });
    }

}
