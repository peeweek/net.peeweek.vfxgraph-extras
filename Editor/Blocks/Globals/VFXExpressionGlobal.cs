using UnityEngine.VFX;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX.Globals
{
    class VFXExpressionGlobal : VFXExpression
    {
        VFXGlobalsDefinition.Definition m_Definition;
        public VFXExpressionGlobal(VFXGlobalsDefinition.Definition definition) : base(VFXExpression.Flags.PerElement | VFXExpression.Flags.InvalidOnCPU) 
        {
            m_Definition = definition;
        }

        public override VFXValueType valueType
        {
            get
            {
                switch (m_Definition.type)
                {
                    case VFXGlobalsDefinition.MemberType.Float:
                        return VFXValueType.Float;
                    case VFXGlobalsDefinition.MemberType.Uint:
                        return VFXValueType.Uint32;
                    case VFXGlobalsDefinition.MemberType.Int:
                        return VFXValueType.Int32;
                    case VFXGlobalsDefinition.MemberType.Bool:
                        return VFXValueType.Boolean;
                    case VFXGlobalsDefinition.MemberType.Vector2:
                        return VFXValueType.Float2;
                    case VFXGlobalsDefinition.MemberType.Vector3:
                        return VFXValueType.Float3;
                    case VFXGlobalsDefinition.MemberType.Vector4:
                        return VFXValueType.Float4;
                    case VFXGlobalsDefinition.MemberType.Color:
                        return VFXValueType.Float4;
                    case VFXGlobalsDefinition.MemberType.Texture2D:
                        return VFXValueType.Texture2D;
                    case VFXGlobalsDefinition.MemberType.Texture3D:
                        return VFXValueType.Texture3D;
                    case VFXGlobalsDefinition.MemberType.TextureCube:
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
