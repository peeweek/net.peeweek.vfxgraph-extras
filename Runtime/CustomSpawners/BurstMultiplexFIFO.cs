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
            [Tooltip("A multiplier applied to the spawnCount when processing the queue")]
            public float CountScale = 1.0f;
        }

        static readonly int spawnCount = Shader.PropertyToID("spawnCount");
        static readonly int countScale = Shader.PropertyToID("CountScale");

        Queue<VFXEventAttribute> attributeQueue;
        Queue<VFXEventAttribute> available;

        public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (attributeQueue == null)
                attributeQueue = new Queue<VFXEventAttribute>();

            if(available == null)
                available = new Queue<VFXEventAttribute>();

            if(available.Count == 0)
                available.Enqueue(vfxComponent.CreateVFXEventAttribute());

            var attribute = available.Dequeue();
            attribute.CopyValuesFrom(state.vfxEventAttribute);
            attributeQueue.Enqueue(attribute);
        }

        public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            attributeQueue.Clear();
        }

        public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if(attributeQueue.Count > 0)
            {
                var attribute = attributeQueue.Dequeue();
                state.vfxEventAttribute.CopyValuesFrom(attribute);
                available.Enqueue(attribute);
                state.spawnCount = attribute.GetFloat(spawnCount) * vfxValues.GetFloat(countScale);
            }
        }
    }
}