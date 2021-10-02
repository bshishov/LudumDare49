using TMPro;
using UnityEngine;

public class UITotalPower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PowerText;

    public void SetTotalPower(int power)
    {
        PowerText.text = power.ToString();
    }
}
