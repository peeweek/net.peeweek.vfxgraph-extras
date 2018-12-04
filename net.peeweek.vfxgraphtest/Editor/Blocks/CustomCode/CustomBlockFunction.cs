using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnityEditor.VFX
{
    class CustomBlockFunction : ScriptableObject
    {
        [Serializable]
        public struct AttributeDeclarationInfo
        {
            public string name;
            public VFXAttributeMode mode;
        }

        [Serializable]
        public struct PropertyDeclarationInfo
        {
            public string name;
            public string type;
        }
        
        public string BlockName = "NewCustomBlock";

        public VFXContextType ContextType = VFXContextType.kAll;
        public VFXDataType CompatibleData = VFXDataType.kParticle;

        public List<AttributeDeclarationInfo> Attributes = new List<AttributeDeclarationInfo>();
        public List<PropertyDeclarationInfo> Properties = new List<PropertyDeclarationInfo>();

        public bool UseTotalTime = false;
        public bool UseDeltaTime = false;
        public bool UseRandom = false;

        [Multiline]
        public string SourceCode = "";

        public override int GetHashCode()
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this)).GetHashCode();
        }
    }
}


