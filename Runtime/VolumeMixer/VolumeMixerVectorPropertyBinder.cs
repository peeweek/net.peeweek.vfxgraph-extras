using UnityEngine.Serialization;
using VolumeMixerUtility;

namespace UnityEngine.VFX.Utility
{
    [VFXBinder("VFX Volume Mixer/Vector Property Binder")]
    public class VFXVolumeMixerVectorPropertyBinder : VFXVolumeMixerPropertyBinderBase
    {
        [VolumeMixerProperty( VolumeMixerPropertyAttribute.PropertyType.Vector)]
        public int VectorMixerProperty = 0;
        [VFXPropertyBinding("UnityEngine.Vector3"), FormerlySerializedAs("VectorParameter")]
        public ExposedProperty VectorProperty = "VectorProperty";
        
        public override bool IsValid(VisualEffect component)
        {
            return base.IsValid(component) && VectorMixerProperty < 8 && VectorMixerProperty >= 0 && computedTransform != null && component.HasVector3(VectorProperty);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetVector3(VectorProperty, VolumeMixer.GetVectorValueAt(VectorMixerProperty, computedTransform, Layer));
        }

        public override string ToString()
        {
            return "VolumeMixer Vector3 #"+ VectorMixerProperty+ " : " + VectorProperty.ToString()+" "+ base.ToString();
        }
    }
}

