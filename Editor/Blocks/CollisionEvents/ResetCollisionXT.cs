using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Collision", experimental = true)]
    class ResetCollisionXT : VFXBlock
    {
        public override string name { get { return "Reset Collision (XT)"; } }
        public override VFXContextType compatibleContexts { get { return VFXContextType.Update; } }
        public override VFXDataType compatibleData { get { return VFXDataType.Particle; } }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(TriggerOnCollisionXT.collisionChannel, VFXAttributeMode.Write);
            }
        }

        public override string source
        {
            get
            {
                return @"collisionChannel = -1;";
            }
        }
    }
}
