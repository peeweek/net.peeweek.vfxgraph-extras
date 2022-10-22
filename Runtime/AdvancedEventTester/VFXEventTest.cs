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
        public bool enableUpdate = true;

        public string singleEventName = "Event";
        public bool enableStartEvent = true;
        public string startEventName = "OnPlay";
        public bool enableStopEvent = true;
        public string stopEventName = "OnStop";

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

        public void PerformSingleEvent(VisualEffect vfx, VFXEventAttribute attribute = null)
        {
            if (attribute == null)
                attribute = vfx.CreateVFXEventAttribute();

            vfx.SendEvent(singleEventName, ApplyEventAttribute(attribute));
        }

        VFXEventAttribute ApplyEventAttribute(VFXEventAttribute attribute)
        {
            foreach (var evtAttr in eventAttributes)
            {
                if (evtAttr == null || !evtAttr.enabled)
                    continue;

                evtAttr.ApplyEventAttribute(attribute);
            }
            return attribute;
        }

        public void StartTest(VisualEffect vfx, float currentTime)
        {
            if (updateBehavior != null)
            {
                updateBehavior.OnStart(this, vfx, currentTime);
                vfx.SendEvent(startEventName, ApplyEventAttribute(vfx.CreateVFXEventAttribute()));
            }
        }

        public void StopTest(VisualEffect vfx, float currentTime)
        {
            if (updateBehavior != null)
            {
                updateBehavior.OnStop(this, vfx, currentTime);
                vfx.SendEvent(stopEventName, ApplyEventAttribute(vfx.CreateVFXEventAttribute()));
            }
        }

        public void UpdateTest(VisualEffect vfx, float currentTime)
        {
            if (updateBehavior != null && enableUpdate)
                updateBehavior.Update(this, vfx, currentTime);
        }

    }

}
