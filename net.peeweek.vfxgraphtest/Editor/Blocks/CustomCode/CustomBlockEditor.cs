using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System;

namespace UnityEditor.VFX.Block
{
    [CustomEditor(typeof(CustomBlock))]
    public class CustomBlockEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Reload"))
            {
                Reload();
            }
        }

        void Reload()
        {
            (serializedObject.targetObject as VFXBlock).Invalidate(VFXModel.InvalidationCause.kSettingChanged);
        }

    }

}

