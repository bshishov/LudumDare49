using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIP
{
    public class UIRollButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Animator Animator;
        private static readonly int HoldParam = Animator.StringToHash("Hold");
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Animator.SetBool(HoldParam, true);
            Debug.Log("Down");
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {

            Debug.Log("Up");
            Animator.SetBool(HoldParam, false);
        }

    }
}
