using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.VFX.Utils;

[VFXBinder("Terrain/Terrain")]
public class TerrainBinder : VFXBinderBase
{
    [VFXParameterBinding("UnityEditor.VFX.VFXTerrainType")]
    public ExposedParameter TerrainParameter;

    public Terrain Terrain;

    ExposedParameter Terrain_Bounds_center;
    ExposedParameter Terrain_Bounds_size;
    ExposedParameter Terrain_HeightMap;
    ExposedParameter Terrain_Height;

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateSubParameters();
    }

    private void OnValidate()
    {
        UpdateSubParameters();
    }

    void UpdateSubParameters()
    {
        Terrain_Bounds_center = TerrainParameter + "_Bounds_center";
        Terrain_Bounds_size = TerrainParameter + "_Bounds_size";
        Terrain_HeightMap = TerrainParameter + "_HeightMap";
        Terrain_Height = TerrainParameter + "_Height";


    }

    public override bool IsValid(VisualEffect component)
    {
        return Terrain != null &&
            component.HasVector3(Terrain_Bounds_center) &&
            component.HasVector3(Terrain_Bounds_size) &&
            component.HasTexture(Terrain_HeightMap) &&
            component.HasFloat(Terrain_Height);

    }

    public override void UpdateBinding(VisualEffect component)
    {
        Bounds b = Terrain.terrainData.bounds;

        component.SetVector3(Terrain_Bounds_center, b.center);
        component.SetVector3(Terrain_Bounds_size, b.size);
        component.SetTexture(Terrain_HeightMap, Terrain.terrainData.heightmapTexture);
        component.SetFloat(Terrain_Height, Terrain.terrainData.heightmapScale.y);
    }
}
