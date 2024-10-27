using System;
using UnityEngine;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX.Globals
{
    [CreateAssetMenu(fileName = "VFX Globals", menuName = "Visual Effects/VFX Globals Definition", order = 376)]
    public class VFXGlobalsDefinition : ScriptableObject
    {
        public TextAsset hlslInclude;

        public enum MemberType
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
        public struct Definition
        {
            public ExposedProperty name;
            public MemberType type;

            public Type realType
            {
                get
                {
                    switch (type)
                    {
                        case MemberType.Float:
                            return typeof(float);
                        case MemberType.Uint:
                            return typeof(uint);
                        case MemberType.Int:
                            return typeof(int);
                        case MemberType.Bool:
                            return typeof(bool);
                        case MemberType.Vector2:
                            return typeof(Vector2);
                        case MemberType.Vector3:
                            return typeof(Vector3);
                        case MemberType.Vector4:
                            return typeof(Vector4);
                        case MemberType.Color:
                            return typeof(Color);
                        case MemberType.Texture2D:
                            return typeof(Texture2D);
                        case MemberType.Texture3D:
                            return typeof(Texture3D);
                        case MemberType.TextureCube:
                            return typeof(Cubemap);
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        [field: SerializeField]
        public Definition[] includes { get; private set; }

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

        string TypeString(MemberType type)
        {
            switch (type)
            {
                case MemberType.Float:
                    return "float";
                case MemberType.Uint:
                    return "uint";
                case MemberType.Int:
                    return "int";
                case MemberType.Bool:
                    return "bool";
                case MemberType.Vector2:
                    return "float2";
                case MemberType.Vector3:
                    return "float3";
                case MemberType.Vector4:
                    return "float4";
                case MemberType.Color:
                    return "float4";
                case MemberType.Texture2D:
                    return "sampler2d";
                case MemberType.Texture3D:
                    return "sampler3d";
                case MemberType.TextureCube:
                    return "samplerCube";
                default:
                    throw new System.NotImplementedException();
            }
        }
#endif

    }
}


