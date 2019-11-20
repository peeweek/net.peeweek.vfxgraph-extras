using UnityEngine.Serialization;
using VolumeMixerUtility;

namespace UnityEngine.VFX.Utility
{
    [VFXBinder("VFX Volume Mixer/Color Property Binder")]
    public class VFXVolumeMixerColorPropertyBinder : VFXVolumeMixerPropertyBinderBase
    {
        [VolumeMixerProperty(VolumeMixerPropertyAttribute.PropertyType.Color)]
        public int ColorMixerProperty = 0;
        [VFXPropertyBinding("UnityEngine.Color"), FormerlySerializedAs("ColorParameter")]
        public ExposedProperty ColorProperty = "ColorProperty";

        public override bool IsValid(VisualEffect component)
        {
            return base.IsValid(component) && ColorMixerProperty < 8 && ColorMixerProperty >= 0 && computedTransform != null && component.HasVector4(ColorProperty);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetVector4(ColorProperty, VolumeMixer.GetColorValueAt(ColorMixerProperty, computedTransform, Layer)); 
        }

        public override string ToString()
        {
            return "VolumeMixer Color #" + ColorMixerProperty + " : " + ColorProperty.ToString() + " " + base.ToString();
        }
    }
}
