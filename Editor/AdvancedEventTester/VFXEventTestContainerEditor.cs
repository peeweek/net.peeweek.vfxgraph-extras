using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.VFX.EventTesting;

namespace UnityEditor.VFX.EventTesting
{
    [CustomEditor(typeof(VFXEventTestContainer))]
    public class VFXEventTestContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is used for holding VFXGraph Event Test data in editor only", MessageType.Info);

            if(GUILayout.Button("Open VFX Event Tester", GUILayout.Height(32)))
            {
                VFXAdvancedEventTester.OpenSceneTest(target as VFXEventTestContainer);
            }
        }
    }
}

