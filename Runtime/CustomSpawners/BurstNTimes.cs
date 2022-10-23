using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public class BurstNTimes : VFXSpawnerCallbacks
    {
        public class InputProperties
        {
            [Tooltip("How many burst iterations to add, this count is added to the queue every frame")]
            [Min(1)]
            public uint Count = 10;
        }

        static readonly int count = Shader.PropertyToID("Count");
        List<(VFXEventAttribute, uint)> attributeQueue;
        Queue<(VFXEventAttribute, uint)> available;

        public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (attributeQueue == null)
                attributeQueue = new List<(VFXEventAttribute, uint)>();

            if (available == null)
                available = new Queue<(VFXEventAttribute, uint)>();

            if (available.Count == 0)
                available.Enqueue((vfxComponent.CreateVFXEventAttribute(),0));

            var tuple = available.Dequeue();
            tuple.Item1.CopyValuesFrom(state.vfxEventAttribute);
            tuple.Item2 = Math.Max(1, vfxValues.GetUInt(count));
            attributeQueue.Add(tuple);
        }

        public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            while (attributeQueue.Count > 0)
            {
                available.Enqueue(attributeQueue[0]);
                attributeQueue.RemoveAt(0);
            }
        }

        public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (attributeQueue.Count > 0)
            {
                var tuple = attributeQueue[0];

                state.vfxEventAttribute.CopyValuesFrom(tuple.Item1);
                tuple.Item2--;
                state.spawnCount = 1;

                if (tuple.Item2 == 0)
                {
                    available.Enqueue(tuple);
                    attributeQueue.RemoveAt(0);
                }
                else
                {
                    attributeQueue[0] = tuple;
                }
            }
            else
                state.playing = false;
        }
    }
}