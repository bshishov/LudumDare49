using UnityEngine;

namespace UI.Scripts
{
    [CreateAssetMenu(fileName = "Frame", menuName = "UIF/Frame", order = 0)]
    public class FrameData : ScriptableObject
    {
        public FrameElementData[] Elements;
    }
}
