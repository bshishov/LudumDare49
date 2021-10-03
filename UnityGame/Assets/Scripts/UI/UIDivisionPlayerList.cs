using Network.Game;
using UnityEngine;

namespace UI
{
    public class UIDivisionPlayerList : MonoBehaviour
    {
        public GameObject DivisionPlayerPrefab;
        public Transform Container;

        public void Add(DivisionPlayer divisionPlayer)
        {
            var go = GameObject.Instantiate(DivisionPlayerPrefab, parent: Container);
            var uiDivisionPlayer = go.GetComponent<UIDivisionPlayer>();
            uiDivisionPlayer.Setup(divisionPlayer);
        }

        public void Clear()
        {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
        }
    }
}