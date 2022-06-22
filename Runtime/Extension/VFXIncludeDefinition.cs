using System;
using UnityEngine;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.Includes
{
    [CreateAssetMenu(fileName = "New VFXInclude", menuName = "Visual Effects/VFX Include Definition")]
    public class VFXIncludeDefinition : ScriptableObject
    {
        public TextAsset hlslInclude;

        public enum IncludeMemberType
        {
            Float,
            Uint,
            Int,
            Bool,
            Vector2,
            Vector3,
            Vector4,
            Color,
            Texture2D,
            Texture3D,
            TextureCube
        }

        [Serializable]
        public struct IncludeDefinition
        {
            public ExposedProperty name;
            public IncludeMemberType type;

            public Type realType
            {
                get
                {
                    switch (type)
                    {
                        case IncludeMemberType.Float:
                            return typeof(float);
                        case IncludeMemberType.Uint:
                            return typeof(uint);
                        case IncludeMemberType.Int:
                            return typeof(int);
                        case IncludeMemberType.Bool:
                            return typeof(bool);
                        case IncludeMemberType.Vector2:
                            return typeof(Vector2);
                        case IncludeMemberType.Vector3:
                            return typeof(Vector3);
                        case IncludeMemberType.Vector4:
                            return typeof(Vector4);
                        case IncludeMemberType.Color:
                            return typeof(Color);
                        case IncludeMemberType.Texture2D:
                            return typeof(Texture2D);
                        case IncludeMemberType.Texture3D:
                            return typeof(Texture3D);
                        case IncludeMemberType.TextureCube:
                            return typeof(Cubemap);
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        [field: SerializeField]
        public IncludeDefinition[] includes { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Create or Update HLSL Include")]
        void WriteTextAsset()
        {
            string fileName;
            if (hlslInclude == null)
            {
                fileName = UnityEditor.AssetDatabase.GetAssetPath(this);
                fileName = Application.dataPath + fileName.Substring(6);
                fileName = fileName.Remove(fileName.Length - 6, 6) + "_include.hlsl";
            }
            else
            {
                fileName = UnityEditor.AssetDatabase.GetAssetPath(hlslInclude);   
            }
            var tw = new System.IO.StreamWriter(fileName);
            foreach(var def in includes)
            {
                tw.WriteLine($"{TypeString(def.type)} {(string)def.name};");
            }
            tw.Close();
            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
        }

        string TypeString(IncludeMemberType type)
        {
            switch (type)
            {
                case IncludeMemberType.Float:
                    return "float";
                case IncludeMemberType.Uint:
                    return "uint";
                case IncludeMemberType.Int:
                    return "int";
                case IncludeMemberType.Bool:
                    return "bool";
                case IncludeMemberType.Vector2:
                    return "float2";
                case IncludeMemberType.Vector3:
                    return "float3";
                case IncludeMemberType.Vector4:
                    return "float4";
                case IncludeMemberType.Color:
                    return "float4";
                case IncludeMemberType.Texture2D:
                    return "sampler2d";
                case IncludeMemberType.Texture3D:
                    return "sampler3d";
                case IncludeMemberType.TextureCube:
                    return "samplerCube";
                default:
                    throw new System.NotImplementedException();
            }
        }
#endif

    }
}


