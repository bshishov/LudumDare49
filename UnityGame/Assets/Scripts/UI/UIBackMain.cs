
using UnityEngine;
using UnityEngine.UI;

public class UIBackMain : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(TransitionToGame);
    }

    private void TransitionToGame()
    {
        GameManager.Instance.TransitionToGame();
    }
}
