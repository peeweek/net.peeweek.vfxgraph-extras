using UnityEngine;
using UnityEngine.VFX;

public class VFXSpawnMultiplexerImplicitBlock : VFXSpawnerCallbacks
{

    public class InputProperties
    {
        public float SpawnCount = 10;
        [Range(1,60)]
        public uint MaxCount = 8;
        public int mode = 0;

    }

    public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {
        Debug.Log("OnPlay");
    }

    public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {
        Debug.Log("OnStop");
    }

    public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {
        Debug.Log($"Update {vfxValues.GetInt("SpawnCount")}");
    }
}
