using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VolumeMixerUtility.Editor
{
    [CustomPropertyDrawer(typeof(VolumeMixerPropertyAttribute))]
    public class VFXVolumeMixerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            VolumeMixerPropertyAttribute.PropertyType type = (attribute as VolumeMixerPropertyAttribute).type;

            string[] names;
            int[] values;
            int count;

            switch (type)
            {
                case VolumeMixerPropertyAttribute.PropertyType.Float:
                    count = VolumeMixerSettings.floatPropertyCount;
                    names = new string[count];
                    values = new int[count];
                    for (int i = 0; i < VolumeMixerSettings.floatPropertyCount; i++)
                    {
                        names[i] = VolumeMixerSettings.floatPropertyNames[i];
                        values[i] = i;
                    }
                    property.intValue = EditorGUI.IntPopup(position, ObjectNames.NicifyVariableName(property.name), property.intValue, names, values);
                    break;
                case VolumeMixerPropertyAttribute.PropertyType.Vector:
                    count = VolumeMixerSettings.vectorPropertyCount;
                    names = new string[count];
                    values = new int[count];
                    for (int i = 0; i < VolumeMixerSettings.vectorPropertyCount; i++)
                    {
                        names[i] = VolumeMixerSettings.vectorPropertyNames[i];
                        values[i] = i;
                    }
                    property.intValue = EditorGUI.IntPopup(position, ObjectNames.NicifyVariableName(property.name), property.intValue, names, values);
                    break;
                case VolumeMixerPropertyAttribute.PropertyType.Color:
                    count = VolumeMixerSettings.colorPropertyCount;
                    names = new string[count];
                    values = new int[count];
                    for (int i = 0; i < VolumeMixerSettings.colorPropertyCount; i++)
                    {
                        names[i] = VolumeMixerSettings.colorPropertyNames[i];
                        values[i] = i;
                    }
                    property.intValue = EditorGUI.IntPopup(position, ObjectNames.NicifyVariableName(property.name), property.intValue, names, values);
                    break;
            }
        }
    }

}
