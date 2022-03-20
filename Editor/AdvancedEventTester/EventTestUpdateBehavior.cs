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
        public virtual bool canUseTool => false;

        public bool enableUpdate = true;

        public abstract void Reset(VFXEventTest test);
        public abstract void Update(VFXEventTest test, VisualEffect vfx);

        public virtual bool OnBeforeSceneGUI(SceneView sceneView, Event evt) => false;
        public virtual bool OnDuringSceneGUI(SceneView sceneView, Event evt) => false;

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

    [Serializable]
    public class GatlingRaycastBehavior : EventTestUpdateBehavior
    {
        public override bool canUseTool => true;

        public float shootInterval = 1.0f;

        float m_TTL;
        double m_Time;

        public override void Reset(VFXEventTest test)
        {
            m_TTL = 0f;
            m_Time = EditorApplication.timeSinceStartup;
        }

        public override bool OnBeforeSceneGUI(SceneView sceneView, Event evt)
        {
            float deltaTime = (float)(EditorApplication.timeSinceStartup - m_Time);
            m_Time = EditorApplication.timeSinceStartup;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                m_TTL = 0f;
                shooting = true;
                ProcessShoot(sceneView);
            }
            else if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                shooting = false;
            }

            if(shooting)
            {
                m_TTL += deltaTime;
                if (m_TTL > shootInterval)
                {
                    ProcessShoot(sceneView);
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

        public void ProcessShoot(SceneView sv)
        {
            Vector2 pos = Event.current.mousePosition;
            pos.y = (SceneView.lastActiveSceneView.position.height - 22) - pos.y;
            
            Ray r = SceneView.lastActiveSceneView.camera.ScreenPointToRay(pos);

            if (Physics.Raycast(r, out RaycastHit hit))
            {
                impact = true;
                impactPosition = hit.point;
                impactNormal = hit.normal;
            }
            else
                impact = false;
        }

        public override void Update(VFXEventTest test, VisualEffect vfx)
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
