using System;
using System.Collections.Generic;
using UIF.Scripts.Transitions;
using UnityEngine;

namespace UI.Scripts
{
    [Serializable]
    public class FrameElementData
    {
        public GameObject Prefab;
        public List<BaseTransition> OverrideTransition;
    }
}