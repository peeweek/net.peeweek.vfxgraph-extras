using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public class BurstMultiplexFIFO : VFXSpawnerCallbacks
    {
        public class InputProperties
        {
            [Tooltip("A multiplier applied to the spawnCount attribute when processing the queue")]
            public float CountScale = 1.0f;
            [Tooltip("Whether to multiply the spawn rate by the spawnCount attribute")]
            public bool ProcessSpawnCount = false;
        }

        static readonly int spawnCount = Shader.PropertyToID("spawnCount");
        static readonly int countScale = Shader.PropertyToID("CountScale");
        static readonly int processSpawnCount = Shader.PropertyToID("ProcessSpawnCount");

        Queue<VFXEventAttribute> attributeQueue = new Queue<VFXEventAttribute>();
        Queue<VFXEventAttribute> available = new Queue<VFXEventAttribute>();

        public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if(available.Count == 0)
                available.Enqueue(vfxComponent.CreateVFXEventAttribute());

            var attribute = available.Dequeue();
            attribute.CopyValuesFrom(state.vfxEventAttribute);
            attributeQueue.Enqueue(attribute);
        }

        public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            while (attributeQueue.Count > 0)
            {
                available.Enqueue(attributeQueue.Dequeue());
            }
        }

        public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if(attributeQueue.Count > 0)
            {
                var attribute = attributeQueue.Dequeue();
                state.vfxEventAttribute.CopyValuesFrom(attribute);
                available.Enqueue(attribute);

                float count = 1f;
                if (vfxValues.GetBool(processSpawnCount))
                    count = attribute.GetFloat(spawnCount);

                state.spawnCount = count * vfxValues.GetFloat(countScale);
            }
        }
    }
}