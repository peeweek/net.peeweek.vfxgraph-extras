using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.VirtualImpacts
{
    [CreateAssetMenu(fileName = "New VFXVirtualImpact", menuName = "Visual Effects/VFX Virtual Impact")]
    public partial class VFXVirtualImpact : ScriptableObject
    {
        public VisualEffectAsset Asset;

        [Header("Instancing")]
        [Min(1)]
        public uint MaxInstanceCount = 64;

        [Header("Events")]
        public ExposedProperty DefaultStartEvent = "OnPlay";
        public ExposedProperty DefaultEndEvent = "OnStop";

        [Header("Bounds")]
        public ExposedProperty BoundsAABoxProperty = "Bounds";
        [SerializeField, HideInInspector]
        ExposedProperty _boundsPosition = "Bounds_center";
        [SerializeField, HideInInspector]
        ExposedProperty _boundsSize = "Bounds_size";

        public float BoundsPadding = 1.0f;

        // Private
        VisualEffect visualEffect; 
        Impact[] instances;
        Queue<uint> available;

        public class Impact
        {
            public int startEventID;
            public int endEventID;
            public Bounds Bounds;
            public float TTL;
            public VFXEventAttribute EventAttribute;

            public Impact(VisualEffect vfx, int startEventID, int endEventID)
            {
                this.startEventID = startEventID;
                this.endEventID = endEventID;
                this.Bounds = new Bounds(Vector3.zero, Vector3.one);
                this.TTL = -1f;
                this.EventAttribute = vfx.CreateVFXEventAttribute();
            }
        }

        public bool IsValid() => (Asset != null && MaxInstanceCount > 0);

        private void OnValidate()
        {
            _boundsPosition = BoundsAABoxProperty + "_center";
            _boundsSize = BoundsAABoxProperty + "_size";
        }

        private GameObject CreateVirtualImpactGameObject()
        {
            if (visualEffect == null)
            {
                var gameObject = new GameObject($"VFXVirtualImpact : {this.name}");
                visualEffect = gameObject.AddComponent<VisualEffect>();
                visualEffect.visualEffectAsset = Asset;

                // Create Instances
                instances = new Impact[MaxInstanceCount];
                available = new Queue<uint>();

                for (uint i = 0; i < MaxInstanceCount; i++)
                {
                    instances[i] = new Impact(visualEffect, DefaultStartEvent, DefaultEndEvent);
                    available.Enqueue(i);
                }
                UpdateBounds();
                return gameObject;
            }
            else
                return null;
        }

        private void DisposeVirtualImpactGameObject()
        {
            Destroy(visualEffect.gameObject);
            visualEffect = null;
            instances = null;
        }

        private bool TryGetImpact(float lifeTime, out Impact impact)
        {
            Debug.Assert(lifeTime > 0f);

            if(available.Count > 0 && lifeTime > 0f)
            {
                uint index = available.Dequeue();
                impact = instances[index];
                impact.TTL = lifeTime;
                return true;
            }
            else
            {
                impact = null;
                return false;
            }
        }

        private void SpawnImpact(Impact impact)
        {
            visualEffect.SendEvent(impact.startEventID, impact.EventAttribute);
            UpdateBounds();
        }

        private void UpdateVirtualImpact(float deltaTime)
        {
            for(uint i = 0; i < instances.Length; i++)
            {
                var instance = instances[i];
                if (instance.TTL < 0f)
                    continue;

                instance.TTL -= deltaTime;
                if(instance.TTL < 0.0f)
                {
                    visualEffect.SendEvent(instance.endEventID, instance.EventAttribute);
                    instance.TTL = -1f;
                    available.Enqueue(i);
                    UpdateBounds();
                }
            }
        }

        Bounds bounds = new Bounds();

        private void UpdateBounds()
        {
            if (visualEffect.HasVector3(_boundsPosition) && visualEffect.HasVector3(_boundsSize))
            {
                bounds.size = Vector3.zero;
                for (uint i = 0; i < instances.Length; i++)
                {
                    var instance = instances[i];

                    if (instance.TTL < 0f)
                        continue;

                    if (bounds.size.sqrMagnitude == 0f)
                        bounds = instance.Bounds;
                    else
                        bounds.Encapsulate(instances[i].Bounds);
                }

                if(bounds.size.sqrMagnitude > 0f)
                    bounds.Expand(BoundsPadding);

                visualEffect.SetVector3(_boundsPosition, bounds.center);
                visualEffect.SetVector3(_boundsSize, bounds.size);
            }
        }
    }
}
