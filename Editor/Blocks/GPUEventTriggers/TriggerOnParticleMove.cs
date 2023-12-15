using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "GPUEvent", experimental = true)]
    class TriggerOnParticleMove : VFXBlock
    {
        public enum Mode
        {
            OverTime,
            OverDistance
        }

#if UNITY_2023_2_OR_NEWER
        [SerializeField, VFXSetting(VFXSettingAttribute.VisibleFlags.Default), Tooltip("True to allow one event max per frame")]
#else
        [SerializeField, VFXSetting(VFXSettingAttribute.VisibleFlags.All), Tooltip("True to allow one event max per frame")]
#endif
        protected bool clampToOne = true;

        public override string name { get { return string.Format("Trigger On Particle Move"); } }
        public override VFXContextType compatibleContexts { get { return VFXContextType.Update; } }
        public override VFXDataType compatibleData { get { return VFXDataType.Particle; } }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(new VFXAttribute(GetRateCountAttribute(), VFXValueType.Float), VFXAttributeMode.ReadWrite);
                yield return new VFXAttributeInfo(VFXAttribute.EventCount, VFXAttributeMode.Write);

                yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.OldPosition, VFXAttributeMode.Read);
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
            [Tooltip("Sets the rate of spawning particles per unit moved.")]
            public float RatePerUnit = 10.0f;

            [Tooltip("Determines the maximum distance a particle can move before being considered teleported")]
            public float MaxFrameMoveDistance = 3f;
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
                string outSource = "";
                string rateCount = GetRateCountAttribute();

                outSource += $@"
float delta = length(oldPosition - position);
bool teleported = delta > MaxFrameMoveDistance;

{rateCount} = teleported ? 0.0 : ({rateCount} + delta * RatePerUnit);
uint count = floor({rateCount});
{rateCount} = frac({rateCount});
eventCount = count;";

                if (clampToOne)
                    outSource += @"
eventCount = min(eventCount,1);
";

                return outSource;
            }
        }
    }
}
