using System.Collections.Generic;
using UnityEngine.VFX.Globals;

namespace UnityEditor.VFX.Globals
{
    [VFXInfo(category = "Globals", name= "Get VFX Globals", synonyms = new string[] { "Global", "Includes", "Gameplay" } )]
    class VFXGetFromInclude : VFXOperator
    {
        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public VFXGlobalsDefinition definition;

        public override string name => $"Get VFX Globals {(definition == null? "":"("+(ObjectNames.NicifyVariableName(definition.name))+")")}";

        protected override IEnumerable<VFXPropertyWithValue> outputProperties
        {
            get
            {
                if(definition != null)
                {
                    foreach (var id in definition.includes)
                        yield return new VFXPropertyWithValue(new VFXProperty(id.realType, (string)id.name));
                }

            }
        }
        protected override VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            List<VFXExpression> expressions = new List<VFXExpression>();
            foreach (var id in definition.includes)
            {
                expressions.Add(new VFXExpressionGlobal(id));
            }
            return expressions.ToArray();
        }
    }
}