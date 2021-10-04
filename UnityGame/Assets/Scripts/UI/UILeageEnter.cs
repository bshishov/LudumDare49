using UnityEngine;
using UnityEngine.UI;

public class UILeageEnter : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OpenLeage);
    }

    private void OpenLeage()
    {
        GameManager.Instance.TransitionToLeagueDivision();
    }
}
