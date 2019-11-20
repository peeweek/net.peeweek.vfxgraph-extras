using UnityEngine.Serialization;
using VolumeMixerUtility;

namespace UnityEngine.VFX.Utility
{
    [VFXBinder("VFX Volume Mixer/Float Property Binder")]
    public class VFXVolumeMixerFloatPropertyBinder : VFXVolumeMixerPropertyBinderBase
    {
        [VolumeMixerProperty(VolumeMixerPropertyAttribute.PropertyType.Float)]
        public int FloatMixerProperty = 0;
        [VFXPropertyBinding("System.Single"), FormerlySerializedAs("FloatParameter")]
        public ExposedProperty FloatProperty = "FloatProperty";

        public override bool IsValid(VisualEffect component)
        {
            return base.IsValid(component) && FloatMixerProperty < 8 && FloatMixerProperty >= 0 && computedTransform != null && component.HasFloat(FloatProperty);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetFloat(FloatProperty, VolumeMixer.GetFloatValueAt(FloatMixerProperty, computedTransform, Layer));
        }

        public override string ToString()
        {
            return "VolumeMixer Float #" + FloatMixerProperty + " : " + FloatProperty.ToString() + " " + base.ToString();
        }
    }
}

