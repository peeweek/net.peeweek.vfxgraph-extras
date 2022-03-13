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
        public bool enabled = true;
        public string eventName = "Event";
        [Min(0.0f)]
        public float periodicity = 1.0f;

        [SerializeReference]
        public List<EventAttributeSetup> eventAttributes;

        double m_Time = -1.0;

        VisualEffectAsset asset;
        VFXEventAttribute attribute;

        public void PerformEvent(VisualEffect vfx)
        {
            m_Time = EditorApplication.timeSinceStartup;
            
            if(vfx.visualEffectAsset != asset)
            {
                asset = vfx.visualEffectAsset;
                attribute = vfx.CreateVFXEventAttribute();
            }

            if (attribute == null)
                attribute = vfx.CreateVFXEventAttribute();

            foreach(var evtAttr in eventAttributes)
            {
                if (evtAttr == null)
                    continue;

                evtAttr.ApplyEventAttribute(attribute); 
            }

            vfx.SendEvent(eventName, attribute);
        }

        public void UpdateTest(VisualEffect vfx)
        {
            if (periodicity == 0.0f)
                return;

            if ((m_Time == -1) || ((EditorApplication.timeSinceStartup - m_Time) > periodicity))
                PerformEvent(vfx);
        }

        [Serializable]
        public abstract class EventAttributeSetup 
        {
            public string name;
            public bool enabled = true;

            public abstract void ApplyEventAttribute(VFXEventAttribute attrib);
        }

        [Serializable]
        public class ConstantFloat : EventAttributeSetup
        {
            public string attributeName = "Size";
            public float value = 1.0f;

            public override void ApplyEventAttribute(VFXEventAttribute attrib)
            {
                if (attrib.HasFloat(attributeName))
                    attrib.SetFloat(attributeName, value);
            }
        }

        [Serializable]
        public class RandomFloat : EventAttributeSetup
        {
            public string attributeName = "Size";
            public float min = 0.0f;
            public float max = 1.0f;

            public override void ApplyEventAttribute(VFXEventAttribute attrib)
            {
                if (attrib.HasFloat(attributeName))
                    attrib.SetFloat(attributeName, UnityEngine.Random.Range(min,max));
            }
        }
        public enum RandomMode
        {
            Uniform,
            PerComponent
        }

        [Serializable]
        public class RandomVector3 : EventAttributeSetup
        {
            public string attributeName = "Position";
            public Vector3 min = Vector3.zero;
            public Vector3 max = Vector3.one;
            public RandomMode randomMode = RandomMode.PerComponent;
            public bool normalize;

            public override void ApplyEventAttribute(VFXEventAttribute attrib)
            {
                if (!attrib.HasVector3(attributeName))
                    return;

                Vector3 value;
                if (randomMode == RandomMode.PerComponent)
                    value = new Vector3(
                        Mathf.Lerp(min.x, max.x, UnityEngine.Random.Range(0f, 1f)),
                        Mathf.Lerp(min.y, max.y, UnityEngine.Random.Range(0f, 1f)),
                        Mathf.Lerp(min.z, max.z, UnityEngine.Random.Range(0f, 1f))
                        );
                else
                {
                    var r = UnityEngine.Random.Range(0f, 1f);
                    value = Vector3.Lerp(min, max, r);
                }

                if (normalize)
                    value.Normalize();

                attrib.SetVector3(attributeName, value);
            }
        }

    }

}
