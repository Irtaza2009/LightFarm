using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputName;

    public UnityEvent<string, int> submitScoreEvent;

    public void SubmitScore()
    {
        submitScoreEvent.Invoke(inputName.text, GameManager.Instance.GetCoinCount());
    }
}
