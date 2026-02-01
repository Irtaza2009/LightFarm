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
            Debug.Log($"Leaderboard returned {msg.Length} entries");
            int loopLength = Mathf.Min(msg.Length, playerNameTexts.Count);
            for (int i = 0; i < loopLength; ++i)
            {
                playerNameTexts[i].text = msg[i].Username;
                playerScoreTexts[i].text = msg[i].Score.ToString();
            }

            // Clear unused slots so old names don't linger when fewer results are returned.
            for (int i = loopLength; i < playerNameTexts.Count; ++i)
            {
                playerNameTexts[i].text = "-";
                playerScoreTexts[i].text = "-";
            }
        }));
    }

    public void SetLeaderboardEntry(string username, int score)
    {
        // To allow multiple submissions from the same device/session, reset the player ID before uploading.
        // This is needed when the leaderboard has unique usernames enabled, which otherwise overwrites the previous entry.
        LeaderboardCreator.ResetPlayer(() =>
        {
            LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, score, (msg) =>
            {
                Debug.Log("Leaderboard entry set: " + msg);
                GetLeaderboard();
            }, (error) =>
            {
                Debug.LogError("Upload error: " + error);
            });
        });
    }

}
