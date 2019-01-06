using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.VFX.Utils;

[VFXBinder("Transform/Position (Previous)")]
public class VFXPreviousPosiionBinder : VFXBinderBase
{
    [VFXParameterBinding("UnityEngine.Vector3")]
    public ExposedParameter PreviousPositionParameter = "PreviousPosition";
    Vector3 oldPosition;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        oldPosition = gameObject.transform.position;
    }

    public override bool IsValid(VisualEffect component)
    {
        return component.HasVector3(PreviousPositionParameter);
    }

    public override void UpdateBinding(VisualEffect component)
    {
        component.SetVector3(PreviousPositionParameter, oldPosition);
        oldPosition = gameObject.transform.position;
    }

    public override string ToString()
    {
        return "Position (Previous) : " + PreviousPositionParameter.ToString();
    }


}
