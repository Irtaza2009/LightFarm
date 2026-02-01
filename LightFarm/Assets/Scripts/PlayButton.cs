using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void PressPlayButton()
    {
        AudioManager.Instance.PlayClick();
        SceneManager.LoadScene("Main");
    }
}
