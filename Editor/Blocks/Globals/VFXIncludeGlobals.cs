using System.Collections.Generic;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX.Globals
{
    [VFXInfo(category ="Globals")]
    internal class VFXIncludeGlobals : VFXBlock, IHLSLCodeHolder
    {
        public override string name => "Include Globals";

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public VFXGlobalsDefinition definition;

        public override VFXContextType compatibleContexts => VFXContextType.InitAndUpdateAndOutput;
        public override VFXDataType compatibleData => VFXDataType.Particle | VFXDataType.ParticleStrip;


        public IEnumerable<string> includes
        {
            get
            {
                if (definition != null && definition.hlslInclude != null)
                    yield return AssetDatabase.GetAssetPath(definition.hlslInclude);
            }
        }


        // Compatibility API : We do not use HasShaderFile() so it's not expected that we use this API
        // Following API is for compatibility Only, should break anytime if refactors are incoming
        // Use with caution !

        public ShaderInclude shaderFile => throw new System.NotImplementedException();

        public string sourceCode { get => string.Empty; set => throw new System.NotImplementedException(); }

        public string customCode => string.Empty;

        public bool HasShaderFile() => false;

        public bool Equals(IHLSLCodeHolder other) => true;

    }
}


