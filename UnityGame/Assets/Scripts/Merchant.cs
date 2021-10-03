using UnityEngine;

public class Merchant : MonoBehaviour
{
    public Animator Animator;
    private static readonly int ChargeParam = Animator.StringToHash("Charge");

    [ContextMenu("BeginCharge")]
    public void BeginCharge()
    {
        Animator.SetBool(ChargeParam, true);
    }
    
    [ContextMenu("BeginCharge")]
    public void EndCharge()
    {
        Animator.SetBool(ChargeParam, false);
    }
}