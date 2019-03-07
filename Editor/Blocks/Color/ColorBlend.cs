using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Color")]
    class ColorBlend : VFXBlock
    {

        public enum BlendMode
        {
            Darken,
            Multiply,
            ColorBurn,
            LinearBurn,
            Lighten,
            Screen,
            ColorDodge,
            LinearDodge,
            Overlay,
            SoftLight,
            HardLight,
            VividLight,
            LinearLight,
            PinLight,
            Difference,
            Exclusion
        }
        
        [SerializeField, VFXSetting, Tooltip("The blend mode to use")]
        private BlendMode blendMode = BlendMode.Screen;

        public override string name {get{return "Color Blend";}}
        public override VFXContextType compatibleContexts {get{return VFXContextType.kInitAndUpdateAndOutput;}}
        public override VFXDataType compatibleData {get{return VFXDataType.kParticle;}}
        
        public class InputProperties
        {
            public Color blendColor;
        }

        
        
        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.Color, VFXAttributeMode.ReadWrite);
            }
        }


        public override string source
        {
            get
            {
                switch (blendMode)
                {
                    case BlendMode.Darken:
                        return @"color = min(blendColor,color);";
                    case BlendMode.Multiply:
                        return @"color = blendColor * color;";
                    case BlendMode.ColorBurn:
                        return @"color = 1.0 - (1.0-blendColor) / color;";
                    case BlendMode.LinearBurn:
                        return @"color = blendColor + color - 1.0;";
                    case BlendMode.Lighten:
                        return @"color = max(blendColor,color);";
                    case BlendMode.Screen:
                        return @"color = 1.0 - (1.0-blendColor) * (1.0-color);";
                    case BlendMode.ColorDodge:
                        return @"color = blendColor / (1.0-color);";
                    case BlendMode.LinearDodge:
                        return @"color = blendColor + color;";
                    case BlendMode.Overlay:
                        return @"color = (blendColor > 0.5) * (1.0 - (1.0-2.0*(blendColor-0.5)) * (1.0-color)) + (blendColor <= 0.5) * ((2.0*blendColor) * color);";
                    case BlendMode.SoftLight:
                        return @"color = (color > 0.5) * (1.0 - (1.0-blendColor) * (1.0-(color-0.5))) + (color <= 0.5) * (blendColor * (color+0.5));";
                    case BlendMode.HardLight:
                        return @"color = (color > 0.5) * (1.0 - (1.0-blendColor) * (1.0-2.0*(color-0.5))) +  (color <= 0.5) * (blendColor * (2.0*color));";
                    case BlendMode.VividLight:
                        return @"color = (color > 0.5) * (1.0 - (1.0-blendColor) / (2.0*(color-0.5))) + (color <= 0.5) * (blendColor / (1.0-2.0*color));";
                    case BlendMode.LinearLight:
                        return @"color = (color > 0.5) * (blendColor + 2.0*(color-0.5)) +  (color <= 0.5) * (blendColor + 2.0*color - 1.0);";
                    case BlendMode.PinLight:
                        return @"color = (color > 0.5) * (max(blendColor,2.0*(color-0.5))) +  (color <= 0.5) * (min(blendColor,2.0*color));";
                    case BlendMode.Difference:
                        return @"color = abs(blendColor - color);";
                    case BlendMode.Exclusion:
                        return @"color = 0.5 - 2.0*(blendColor-0.5)*(color-0.5);";
                    default:
                        return "";
                }
            }
        }
    }

}

