using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine.VFX.Includes;

namespace UnityEditor.VFX.Includes
{
    class VFXExpressionInclude : VFXExpression
    {
        VFXIncludeDefinition.IncludeDefinition m_Definition;
        public VFXExpressionInclude(VFXIncludeDefinition.IncludeDefinition definition) : base(VFXExpression.Flags.PerElement | VFXExpression.Flags.InvalidOnCPU) 
        {
            m_Definition = definition;
        }

        public override VFXValueType valueType
        {
            get
            {
                switch (m_Definition.type)
                {
                    case VFXIncludeDefinition.IncludeMemberType.Float:
                        return VFXValueType.Float;
                    case VFXIncludeDefinition.IncludeMemberType.Uint:
                        return VFXValueType.Uint32;
                    case VFXIncludeDefinition.IncludeMemberType.Int:
                        return VFXValueType.Int32;
                    case VFXIncludeDefinition.IncludeMemberType.Bool:
                        return VFXValueType.Boolean;
                    case VFXIncludeDefinition.IncludeMemberType.Vector2:
                        return VFXValueType.Float2;
                    case VFXIncludeDefinition.IncludeMemberType.Vector3:
                        return VFXValueType.Float3;
                    case VFXIncludeDefinition.IncludeMemberType.Vector4:
                        return VFXValueType.Float4;
                    case VFXIncludeDefinition.IncludeMemberType.Color:
                        return VFXValueType.Float4;
                    case VFXIncludeDefinition.IncludeMemberType.Texture2D:
                        return VFXValueType.Texture2D;
                    case VFXIncludeDefinition.IncludeMemberType.Texture3D:
                        return VFXValueType.Texture3D;
                    case VFXIncludeDefinition.IncludeMemberType.TextureCube:
                        return VFXValueType.TextureCube;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        } 
        public override VFXExpressionOperation operation => VFXExpressionOperation.None;

        public override string GetCodeString(string[] parents)
        {
            return (string)m_Definition.name;
        }
    }
}
