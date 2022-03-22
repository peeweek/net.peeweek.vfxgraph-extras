using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "GPUEvent", experimental = true)]
    class TriggerOnCollisionXT : VFXBlock
    {
        public static readonly VFXAttribute collisionChannel = new VFXAttribute("collisionChannel", VFXValue.Constant(-1));
        public static readonly VFXAttribute collisionPosition = new VFXAttribute("collisionPosition", VFXValueType.Float3);
        public static readonly VFXAttribute collisionDirection = new VFXAttribute("collisionDirection", VFXValueType.Float3);

        public override string name { get { return "Trigger On Collision (XT)"; } }
        public override VFXContextType compatibleContexts { get { return VFXContextType.Update; } }
        public override VFXDataType compatibleData { get { return VFXDataType.Particle; } }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(collisionChannel, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.Alive, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.EventCount, VFXAttributeMode.Write);
            }
        }

        public class InputProperties
        {
            [Tooltip("Sets the number of particles spawned via a GPU event when this block is triggered.")]
            public uint count = 1u;
        }

        public class OutputProperties
        {
            [Tooltip("Outputs a GPU event which can connect to another system via a GPUEvent context. Attributes from the current system can be inherited in the new system.")]
            public GPUEvent evt = new GPUEvent();
        }

        public override string source
        {
            get
            {
                return @"
if(alive)
    eventCount = collisionChannel >= 0 ? count : 0;
";
            }
        }
    }
}
