using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.Extras
{

    [Serializable]
    public abstract class EventTestUpdateBehavior
    {
        public virtual bool canUseTool => false;

        public bool enableUpdate = true;

        public abstract void Reset(VFXEventTest test, float currentTime);
        public abstract void Update(VFXEventTest test, VisualEffect vfx, float currentTime);

        public virtual bool OnSceneGUIUpdate(Camera camera, Vector2 mousePosition, bool mouseLeft, bool mouseRight, float currentTime) => false;

    }

    [Serializable]
    public class ConstantRateBehavior : EventTestUpdateBehavior
    {
        [Min(0.0f)]
        public float periodicity = 1.0f;

        double m_Time = -1.0;

        public override void Reset(VFXEventTest test, float currentTime)
        {
            m_Time = currentTime;
        }

        public override void Update(VFXEventTest test, VisualEffect vfx, float currentTime)
        {
            if (periodicity == 0.0f)
                return;

            if ((m_Time == -1)|| ((currentTime - m_Time) > periodicity))
            {
                Reset(test, currentTime);
                test.PerformEvent(vfx);
            }
        }
    }

    [Serializable]
    public class GatlingRaycastBehavior : EventTestUpdateBehavior
    {
        public override bool canUseTool => true;

        public float shootInterval = 1.0f;

        float m_TTL;
        double m_Time;

        public override void Reset(VFXEventTest test, float currentTime)
        {
            m_TTL = 0f;
            m_Time = currentTime;
        }

        public override bool OnSceneGUIUpdate(Camera camera, Vector2 mousePosition, bool mouseLeft, bool mouseRight, float currentTime)
        {
            float deltaTime = (float)(currentTime - m_Time);
            m_Time = currentTime;

            if (mouseLeft)
            {
                m_TTL = 0f;
                shooting = true;
                ProcessShoot(mousePosition, camera);
            }
            else
            {
                shooting = false;
            }

            if(shooting)
            {
                m_TTL += deltaTime;
                if (m_TTL > shootInterval)
                {
                    ProcessShoot(mousePosition, camera);
                    m_TTL = 0f;
                }
                return true;
            }
            else
                return false;

        }

        bool shooting = false;
        bool impact = false;
        Vector3 impactPosition;
        Vector3 impactNormal;

        public void ProcessShoot(Vector2 mousePosition, Camera camera)
        {
            Ray r = camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(r, out RaycastHit hit))
            {
                impact = true;
                impactPosition = hit.point;
                impactNormal = hit.normal;
            }
            else
                impact = false;
        }

        public override void Update(VFXEventTest test, VisualEffect vfx, float currentTime)
        {
            if(shooting && impact)
            {
                VFXEventAttribute attr = vfx.CreateVFXEventAttribute();
                attr.SetVector3("position", impactPosition);
                attr.SetVector3("direction", impactNormal);
                test.PerformEvent(vfx, attr);
                impact = false;
            }
        }
    }
}
