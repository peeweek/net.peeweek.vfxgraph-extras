using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public class RateMultiplexFIFO : VFXSpawnerCallbacks
    {
        public class InputProperties
        {
            [Tooltip("The per-instance Spawn rate")]
            public float SpawnRate = 1.0f;
            [Tooltip("Lifetime of each source")]
            public Vector2 SourceLifeTime = new Vector2(1f,3f);
        }

        static readonly int spawnCount = Shader.PropertyToID("spawnCount");
        static readonly int sourceLifetime = Shader.PropertyToID("SourceLifeTime");
        static readonly int spawnRate = Shader.PropertyToID("SpawnRate");

        List<(float, float, float, VFXEventAttribute)> attributeQueue;
        Queue<(float, float, float, VFXEventAttribute)> available;

        int current = 0;

        public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (attributeQueue == null)
                attributeQueue = new List<(float, float, float, VFXEventAttribute)>();

            if (available == null)
                available = new Queue<(float, float, float, VFXEventAttribute)>();

            if (state.vfxEventAttribute.GetFloat(spawnCount) == 0f)
                return;

            if (available.Count == 0)
                available.Enqueue((0f,0f,0f,vfxComponent.CreateVFXEventAttribute()));

            var tuple = available.Dequeue();
            tuple.Item1 = 0f; // age
            tuple.Item2 = 0f; // spawnCount
            var lt = vfxValues.GetVector2(sourceLifetime); 
            tuple.Item3 = UnityEngine.Random.Range(lt.x, lt.y); // lifetime
            tuple.Item4.CopyValuesFrom(state.vfxEventAttribute);
            attributeQueue.Add(tuple);
        }

        public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            while (attributeQueue.Count > 0)
            {
                available.Enqueue(attributeQueue[0]);
                attributeQueue.RemoveAt(0);
            }
            current = 0;
        }


        public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            if (attributeQueue != null && attributeQueue.Count > 0)
            {
                var tuple = attributeQueue[current];

                float dt = state.deltaTime * attributeQueue.Count;
                tuple.Item1 += dt;

                float val = tuple.Item2 + (vfxValues.GetFloat(spawnRate) * dt);
                state.vfxEventAttribute.CopyValuesFrom(tuple.Item4);

                tuple.Item2 = val % 1f;
                state.spawnCount = val - tuple.Item2;


                if (tuple.Item1 > tuple.Item3)
                {
                    attributeQueue.RemoveAt(current);
                    available.Enqueue(tuple);
                }
                else // ... or replace values in the list
                {
                    attributeQueue[current] = tuple;
                }

                if (++current >= attributeQueue.Count)
                    current = 0;
            }
            else
            {
                state.playing = false;
                current = 0;
            }
        }
    }
}