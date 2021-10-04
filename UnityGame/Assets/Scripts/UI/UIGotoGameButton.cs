using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UIGotoGameButton : MonoBehaviour
    {    
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(GameManager.Instance.TransitionToGame);
        }
    }
}
