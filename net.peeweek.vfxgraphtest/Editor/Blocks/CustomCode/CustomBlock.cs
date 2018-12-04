using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Custom")]
    class CustomBlock : VFXBlock
    {
        [SerializeField, VFXSetting]
        protected CustomBlockFunction CustomBlockFunction;

        public override string name { get { return CustomBlockFunction == null ? "Empty Custom Node" : CustomBlockFunction.BlockName + " (Custom)"; } }

        public override VFXContextType compatibleContexts { get { return CustomBlockFunction == null ? VFXContextType.kAll : CustomBlockFunction.ContextType; } }

        public override VFXDataType compatibleData { get { return VFXDataType.kParticle; } }

        public override void OnEnable()
        {
            base.OnEnable();

            if ((compatibleContexts & (m_Parent as VFXContext).contextType) == 0)
                Debug.LogWarning(string.Format("Custom Block {0} present in invalid Context ({1})", name, (m_Parent as VFXContext).contextType));
        }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                if (CustomBlockFunction != null)
                {
                    foreach (var info in CustomBlockFunction.Attributes)
                        yield return new VFXAttributeInfo(VFXAttribute.Find(info.name), info.mode);

                    if (CustomBlockFunction.UseRandom)
                        yield return new VFXAttributeInfo(VFXAttribute.Seed, VFXAttributeMode.ReadWrite);
                }
            }
        }

        protected override IEnumerable<VFXPropertyWithValue> inputProperties
        {
            get
            {
                if (CustomBlockFunction != null)
                {
                    foreach (var info in CustomBlockFunction.Properties)
                        yield return new VFXPropertyWithValue(new VFXProperty(knownTypes[info.type], info.name));
                }

            }
        }

        public override IEnumerable<VFXNamedExpression> parameters
        {
            get
            {
                foreach (var param in base.parameters)
                    yield return param;

                if(CustomBlockFunction != null)
                {
                    if (CustomBlockFunction.UseDeltaTime)
                        yield return new VFXNamedExpression(VFXBuiltInExpression.DeltaTime, "deltaTime");

                    if (CustomBlockFunction.UseTotalTime)
                        yield return new VFXNamedExpression(VFXBuiltInExpression.TotalTime, "totalTime");
                }
            }
        }

        public override string source
        {
            get
            {
                if (CustomBlockFunction == null)
                {
                    return "";
                }
                else
                {
                    return CustomBlockFunction.SourceCode;
                }

            }
        }

        public override int GetHashCode()
        {
            if (CustomBlockFunction == null)
                return 12345;
            else 
                return CustomBlockFunction.GetHashCode();
        }

        public static Dictionary<string, Type> knownTypes = new Dictionary<string, Type>()
        {
            { "float", typeof(float) },
            { "Vector2", typeof(Vector2) },
            { "Vector3", typeof(Vector3) },
            { "Vector4", typeof(Vector4) },
            { "AnimationCurve", typeof(AnimationCurve) },
            { "Gradient", typeof(Gradient) },
            { "Texture2D", typeof(Texture2D) },
            { "Texture3D", typeof(Texture3D) },
            { "bool", typeof(bool) },
            { "uint", typeof(uint) },
            { "int", typeof(int) },
        };

    }
}
