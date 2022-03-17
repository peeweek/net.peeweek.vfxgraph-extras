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
        public string eventName = "Event";

        [SerializeReference]
        public List<EventAttributeSetup> eventAttributes;

        [SerializeReference]
        public EventTestUpdateBehavior updateBehavior;

        VFXEventAttribute attribute;

        private void Awake()
        {
            updateBehavior = new ConstantRateBehavior();
        }

        public void PerformEvent(VisualEffect vfx)
        {
            attribute = vfx.CreateVFXEventAttribute();

            foreach (var evtAttr in eventAttributes)
            {
                if (evtAttr == null || !evtAttr.enabled)
                    continue;

                evtAttr.ApplyEventAttribute(attribute);
            }

            vfx.SendEvent(eventName, attribute);
        }

        public void ResetTest(VisualEffect vfx)
        {
            if (updateBehavior != null)
                updateBehavior.Reset(this);
        }

        public void UpdateTest(VisualEffect vfx)
        {
            if (updateBehavior != null && updateBehavior.enableUpdate)
                updateBehavior.Update(this, vfx);
        }

    }

}
