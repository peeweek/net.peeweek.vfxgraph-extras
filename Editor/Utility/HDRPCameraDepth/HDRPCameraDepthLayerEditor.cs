using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.VFX.Utils;

[CustomEditor(typeof(HDRPCameraDepthLayer))]
public class HDRPCameraDepthLayerEditor : Editor
{
    SerializedProperty m_SizeMode;
    SerializedProperty m_Width;
    SerializedProperty m_Height;


    void OnEnable()
    {
        m_SizeMode = serializedObject.FindProperty("m_SizeMode");
        m_Width = serializedObject.FindProperty("m_Width");
        m_Height = serializedObject.FindProperty("m_Height");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_SizeMode);
        if(m_SizeMode.intValue == (int)HDRPCameraDepthLayer.SizeMode.Specific)
        {
            EditorGUILayout.PropertyField(m_Width);
            EditorGUILayout.PropertyField(m_Height);
        }

        serializedObject.ApplyModifiedProperties();

        // Preview
        HDRPCameraDepthLayer layer = target as HDRPCameraDepthLayer;
        Texture depth = layer.DepthTexture;

        float r = (float)depth.width / depth.height;
        float w = EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth;
        float h = w / r;

        EditorGUILayout.Space();
        GUILayout.Label($"Prewiew ({depth.width}x{depth.height})", EditorStyles.boldLabel);
        Rect rect = GUILayoutUtility.GetRect(w, h);
        GUI.DrawTexture(rect, depth);

    }
}
