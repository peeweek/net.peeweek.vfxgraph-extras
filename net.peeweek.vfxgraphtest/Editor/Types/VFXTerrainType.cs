using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX
{
    [VFXType]
    struct VFXTerrainType
    {
        public AABox Bounds;
        public Texture2D HeightMap;
        public float Height;
    }
}

