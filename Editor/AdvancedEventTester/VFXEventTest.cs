using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace UnityEditor.VFX
{
    [CreateAssetMenu(fileName = "New VFX Event Test", menuName = "Visual Effects/VFX Event Test")]
    public class VFXEventTest : ScriptableObject
    {
        public bool enabled;
        public string eventName = "Event";
        [Min(0.0f)]
        public float periodicity = 1.0f;

        [SerializeReference]
        public List<EventAttributeSetup> eventAttributes;

        [Serializable]
        public abstract class EventAttributeSetup 
        {
            public bool enabled;

            public abstract void ApplyEventAttribute(VFXEventAttribute attrib);
        }

        [Serializable]
        public class ConstantFloat : EventAttributeSetup
        {
            public string attributeName;
            public float value;

            public override void ApplyEventAttribute(VFXEventAttribute attrib)
            {
                if (attrib.HasFloat(attributeName))
                    attrib.SetFloat(attributeName, value);
            }
        }

        [Serializable]
        public class RandomFloat : EventAttributeSetup
        {
            public string attributeName;
            public float min;
            public float max;

            public override void ApplyEventAttribute(VFXEventAttribute attrib)
            {
                if (attrib.HasFloat(attributeName))
                    attrib.SetFloat(attributeName, UnityEngine.Random.Range(min,max));
            }
        }

    }

}
