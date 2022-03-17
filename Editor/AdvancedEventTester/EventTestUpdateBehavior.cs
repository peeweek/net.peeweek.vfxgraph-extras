using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace UnityEditor.VFX
{

    [Serializable]
    public abstract class EventTestUpdateBehavior
    {
        public bool enableUpdate = true;

        public abstract void Reset(VFXEventTest test);

        public abstract void Update(VFXEventTest test, VisualEffect vfx);
    }

    [Serializable]
    public class ConstantRateBehavior : EventTestUpdateBehavior
    {
        [Min(0.0f)]
        public float periodicity = 1.0f;

        double m_Time = -1.0;

        public override void Reset(VFXEventTest test)
        {
            m_Time = EditorApplication.timeSinceStartup;
        }

        public override void Update(VFXEventTest test, VisualEffect vfx)
        {
            if (periodicity == 0.0f)
                return;

            if ((m_Time == -1)|| ((EditorApplication.timeSinceStartup - m_Time) > periodicity))
            {
                Reset(test);
                test.PerformEvent(vfx);
            }
        }
    }
}
