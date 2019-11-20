using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeMixerUtility
{
    public class VolumeMixerPropertyAttribute : PropertyAttribute
    {
        public enum PropertyType
        {
            Float,
            Vector,
            Color
        }

        public PropertyType type;

        public VolumeMixerPropertyAttribute(PropertyType type)
        {
            this.type = type;
        }
    }
}

