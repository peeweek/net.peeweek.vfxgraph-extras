using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Terrain")]
    class PositionTerrain : VFXBlock
    {
        public override string name { get { return "Position on Terrain"; } }

        public override VFXContextType compatibleContexts { get { return VFXContextType.kInitAndUpdateAndOutput;  } }

        public override VFXDataType compatibleData { get { return VFXDataType.kParticle; } }

        public class InputProperties
        {
            public VFXTerrainType Terrain;
        }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.Write);
                yield return new VFXAttributeInfo(VFXAttribute.Seed, VFXAttributeMode.ReadWrite);
            }
        }


        public override string source
        {
            get
            {
                return @"
float3 minPos = Terrain_Bounds_center - Terrain_Bounds_size * 0.5;
float3 maxPos = Terrain_Bounds_center + Terrain_Bounds_size * 0.5;
float2 nPos = RAND2;
float h = SampleTexture(Terrain_HeightMap, nPos);
position = float3(lerp(minPos.x,maxPos.x,nPos.x), minPos.y + h * Terrain_Height * 2 , lerp(minPos.z,maxPos.z,nPos.y));
";
            }
        }
    }

}

