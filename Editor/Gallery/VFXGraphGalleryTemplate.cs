using System;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    [CreateAssetMenu(fileName ="New VFX Template.asset", menuName = "Visual Effects/Visual Effect Graph Template", order = 310)]
    public class VFXGraphGalleryTemplate : ScriptableObject
    {
        public string categoryName;
        public Template[] templates;

        [Serializable]
        public struct Template
        {
            public VisualEffectAsset templateAsset;
            [Tooltip("Preview Image (286x180)")]
            public Texture2D preview;
            public string name;
            [Multiline]
            public string description;
        }

    }
}

