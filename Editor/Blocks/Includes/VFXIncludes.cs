using System.Collections.Generic;
using UnityEngine.VFX.Includes;

namespace UnityEditor.VFX.Includes
{
    [VFXInfo(category ="Includes")]
    internal class VFXIncludes : VFXBlock
    {
        public override string name => "Include";

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public VFXIncludeDefinition include;

        public override VFXContextType compatibleContexts => VFXContextType.InitAndUpdateAndOutput;
        public override VFXDataType compatibleData => VFXDataType.Particle | VFXDataType.ParticleStrip;


        public override IEnumerable<string> includes
        {
            get
            {
                if (include != null && include.hlslInclude != null)
                    yield return AssetDatabase.GetAssetPath(include.hlslInclude);
            }
        }
    }
}


