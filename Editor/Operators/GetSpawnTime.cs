using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.VFX;
using UnityEngine.Experimental.VFX;

[VFXInfo]
class GetSpawnTime : VFXOperator
{
    public override string name { get { return "Spawn Time"; } }

    public class OutputProperties
    {
        public float Time;
    }

    protected override VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
    {
        return new VFXExpression[] {new VFXAttributeExpression(new VFXAttribute("spawnTime", VFXValueType.Float), VFXAttributeLocation.Source) };
    }
}
