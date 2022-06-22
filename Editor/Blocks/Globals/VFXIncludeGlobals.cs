using System.Collections.Generic;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX.Globals
{
    [VFXInfo(category ="Globals")]
    internal class VFXIncludeGlobals : VFXBlock
    {
        public override string name => "Include Globals";

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public VFXGlobalsDefinition definition;

        public override VFXContextType compatibleContexts => VFXContextType.InitAndUpdateAndOutput;
        public override VFXDataType compatibleData => VFXDataType.Particle | VFXDataType.ParticleStrip;


        public override IEnumerable<string> includes
        {
            get
            {
                if (definition != null && definition.hlslInclude != null)
                    yield return AssetDatabase.GetAssetPath(definition.hlslInclude);
            }
        }
    }
}


