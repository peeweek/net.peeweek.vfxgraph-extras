using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.VFX.EventTesting
{
    public class VFXEventTestContainer : MonoBehaviour
    {
#if UNITY_EDITOR
        [FormerlySerializedAs("events")]
        public List<VFXEventTest> eventTests = new List<VFXEventTest>();
#endif
    }
} 