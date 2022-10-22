using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Extras;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.EventTesting
{
    public class VFXEventTest : ScriptableObject
    {
        public string eventName = "Event";

        [SerializeReference]
        public List<VFXEventAttributeSetup> eventAttributes = new List<VFXEventAttributeSetup>();

        [SerializeReference]
        public VFXEventSendUpdateBehavior updateBehavior;

        VFXEventAttribute attribute;

        private void Awake()
        {
            if(updateBehavior == null)
                updateBehavior = new ConstantRateBehavior();
        }

        public void PerformEvent(VisualEffect vfx, VFXEventAttribute attribute = null)
        {
            if(attribute == null)
                attribute = vfx.CreateVFXEventAttribute();

            foreach (var evtAttr in eventAttributes)
            {
                if (evtAttr == null || !evtAttr.enabled)
                    continue;

                evtAttr.ApplyEventAttribute(attribute);
            }

            vfx.SendEvent(eventName, attribute);
        }

        public void ResetTest(VisualEffect vfx, float currentTime)
        {
            if (updateBehavior != null)
                updateBehavior.Reset(this, currentTime);
        }

        public void UpdateTest(VisualEffect vfx, float currentTime)
        {
            if (updateBehavior != null && updateBehavior.enableUpdate)
                updateBehavior.Update(this, vfx, currentTime);
        }

    }

}
