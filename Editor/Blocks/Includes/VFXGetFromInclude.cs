using System.Collections.Generic;
using UnityEngine.VFX.Includes;

namespace UnityEditor.VFX.Includes
{
    [VFXInfo(category ="Includes")]
    class VFXGetFromInclude : VFXOperator
    {
        [VFXSetting(VFXSettingAttribute.VisibleFlags.InGraph)]
        public VFXIncludeDefinition include;

        public override string name => "Get Values from Include";

        protected override IEnumerable<VFXPropertyWithValue> outputProperties
        {
            get
            {
                if(include != null)
                {
                    foreach (var id in include.includes)
                        yield return new VFXPropertyWithValue(new VFXProperty(id.realType, (string)id.name));
                }

            }
        }
        protected override VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            List<VFXExpression> expressions = new List<VFXExpression>();
            foreach (var id in include.includes)
            {
                expressions.Add(new VFXExpressionInclude(id));
            }
            return expressions.ToArray();
        }
    }
}