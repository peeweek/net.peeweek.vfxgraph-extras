using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "GPUEvent", experimental = true)]
    class TriggerOnParticleAge : VFXBlock
    {
        public enum Mode
        {
            Absolute,
            Relative
        }

#if UNITY_2023_2_OR_NEWER
        [SerializeField, VFXSetting(VFXSettingAttribute.VisibleFlags.Default), Tooltip("Whether to check absolute age, or relative (age/lifetime)")]
#else
        [SerializeField, VFXSetting(VFXSettingAttribute.VisibleFlags.All), Tooltip("Whether to check absolute age, or relative (age/lifetime)")]
#endif
        protected Mode ageMode = Mode.Absolute;

        public override string name { get { return string.Format("Trigger On Particle Age"); } }
        public override VFXContextType compatibleContexts { get { return VFXContextType.Update; } }
        public override VFXDataType compatibleData { get { return VFXDataType.Particle; } }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.EventCount, VFXAttributeMode.Write);
                yield return new VFXAttributeInfo(VFXAttribute.Age, VFXAttributeMode.Read);

                if(ageMode == Mode.Relative)
                    yield return new VFXAttributeInfo(VFXAttribute.Lifetime, VFXAttributeMode.Read);
            }
        }

        public override IEnumerable<VFXNamedExpression> parameters
        {
            get
            {
                foreach (var parameter in base.parameters)
                    yield return parameter;

                yield return new VFXNamedExpression(VFXBuiltInExpression.DeltaTime, "deltaTime");
            }
        }

        public class InputProperties
        {
            [Tooltip("The age at which particles will be spawned.")]
            public float Age = 0f;
            [Tooltip("How many particles to spawn")]
            public uint Count = 10;

        }

        public class OutputProperties
        {
            [Tooltip("Outputs a GPU event which can connect to another system via a GPUEvent context. Attributes from the current system can be inherited in the new system.")]
            public GPUEvent evt = new GPUEvent();
        }

        private string GetRateCountAttribute()
        {
            return "rateCount_" + VFXCodeGeneratorHelper.GeneratePrefix((uint)GetParent().GetIndex(this));
        }

        public override string source
        {
            get
            {
                string condition = "";
                switch (ageMode)
                {
                    default:
                        throw new System.NotImplementedException();
                    case Mode.Absolute:
                        condition = "float a = age;";
                        break;
                    case Mode.Relative:
                        condition = "float a = age/lifetime; deltaTime /= lifetime;";
                        break;

                }
                string outSource = $@"
{condition}

eventCount = ((deltaTime > 0.0) && (a >= Age && a < (Age+deltaTime)))? Count : 0;
";
                
                return outSource;
            }
        }
    }
}
