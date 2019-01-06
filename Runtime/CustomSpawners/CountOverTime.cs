using System;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace UnityEditor.VFX
{
    public class CountOverTime : VFXSpawnerCallbacks
    {
        public class InputProperties
        {
            [Tooltip("Number of Particles to spawn")]
            public uint Count = 1000;
            [Tooltip("Duration of one loop, evaluated every loop")]
            public float Duration = 4.0f;
            [Tooltip("Time at ")]
            public float StartTime = 0.0f;
        }

        private float m_LastTime;
        private float m_StartTime;
        private float m_Rate;
        private float m_Remaining;
        private float m_Incoming;

        static private readonly int countPropertyID = Shader.PropertyToID("Count");
        static private readonly int startTimePropertyID = Shader.PropertyToID("StartTime");
        static private readonly int durationPropertyID = Shader.PropertyToID("Duration");

        public sealed override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            Reset(vfxValues.GetFloat(startTimePropertyID), vfxValues.GetFloat(durationPropertyID), vfxValues.GetUInt(countPropertyID));
        }

        void Reset(float startTime, float duration, uint count, float lastTime = 0.0f)
        {
            m_LastTime = lastTime;
            m_StartTime = startTime;
            m_Rate = count / duration ;
            m_Remaining = count;
            m_Incoming = 0.0f;
        }

        public sealed override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (state.totalTime < m_LastTime)
            {
                // We got reset shomehow (loop and delay?)
                Reset(vfxValues.GetFloat(startTimePropertyID), vfxValues.GetFloat(durationPropertyID), vfxValues.GetUInt(countPropertyID), state.totalTime);
            }

            if (state.deltaTime > 0.0f && m_Remaining >= 0 && state.totalTime > m_StartTime )
            {
                float frameRatio = Mathf.Clamp01( (state.totalTime - m_StartTime) / state.deltaTime); // How much of the entirety of a frame did we elapse
                float count = Mathf.Min(m_Remaining, frameRatio * m_Rate * state.deltaTime);

                m_Remaining -= count;
                m_Incoming += count;

                int toSpawn = (int)Mathf.Floor(m_Incoming);
                state.spawnCount += toSpawn;
                m_Incoming -= toSpawn;
            }

            m_LastTime = state.totalTime;
        }

        public sealed override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            m_Remaining = 0;
        }
    }
}
