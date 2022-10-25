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

        [Header("Attributes")]
        public bool ForwardLifetime = true;
        public ExposedProperty LifetimeAttribute = "lifetime";
        public bool ForwardBoundsCenter = true;
        public ExposedProperty BoundsCenterAttribute = "position";
        public bool ForwardBoundsSize = true;
        public ExposedProperty BoundsSizeAttribute = "size";

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
            VFXVirtualImpact virtualImpact;
            public int startEventID;
            public int endEventID;
            public Bounds Bounds;
            public float TTL;
            public VFXEventAttribute EventAttribute;

            public Impact(VFXVirtualImpact virtualImpact)
            {
                this.virtualImpact = virtualImpact;
                this.startEventID = virtualImpact.DefaultStartEvent;
                this.endEventID = virtualImpact.DefaultEndEvent;
                this.Bounds = new Bounds(Vector3.zero, Vector3.one);
                this.TTL = -1f;
                this.EventAttribute = virtualImpact.visualEffect.CreateVFXEventAttribute();
            }

            public void Spawn()
            {
                virtualImpact.Spawn(this);
            }

            public void SetUint(int eventNameID, uint value) => EventAttribute?.SetUint(eventNameID, value);
            public void SetInt(int eventNameID, int value) => EventAttribute?.SetInt(eventNameID, value);
            public void SetBool(int eventNameID, bool value) => EventAttribute?.SetBool(eventNameID, value);
            public void SetFloat(int eventNameID, float value) => EventAttribute?.SetFloat(eventNameID, value);
            public void SetVector2(int eventNameID, Vector2 value) => EventAttribute?.SetVector2(eventNameID, value);
            public void SetVector3(int eventNameID, Vector3 value) => EventAttribute?.SetVector3(eventNameID, value);
            public void SetVector4(int eventNameID, Vector4 value) => EventAttribute?.SetVector4(eventNameID, value);
            public void SetMatrix4x4(int eventNameID, Matrix4x4 value) => EventAttribute?.SetMatrix4x4(eventNameID, value);
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
                var gameObject = new GameObject($"{this.name} ({this.Asset.name})");
                visualEffect = gameObject.AddComponent<VisualEffect>();
                visualEffect.visualEffectAsset = Asset;

                // Create Instances
                instances = new Impact[MaxInstanceCount];
                available = new Queue<uint>();

                for (uint i = 0; i < MaxInstanceCount; i++)
                {
                    instances[i] = new Impact(this);
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
            // Dispose if not already disposed (i.e Game Shutdown)
            if(visualEffect.gameObject != null)
            {
                Destroy(visualEffect.gameObject);
                visualEffect = null;
                instances = null;
            }
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

        private void Spawn(Impact impact)
        {
            if (ForwardLifetime)
                impact.EventAttribute.SetFloat(LifetimeAttribute, impact.TTL);

            if (ForwardBoundsCenter)
                impact.EventAttribute.SetVector3(BoundsCenterAttribute, impact.Bounds.center);

            if (ForwardBoundsSize)
                impact.EventAttribute.SetVector3(BoundsSizeAttribute, impact.Bounds.size);

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
