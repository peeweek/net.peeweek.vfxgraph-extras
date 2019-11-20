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

        SerializedProperty BlockName;
        SerializedProperty ContextType;
        SerializedProperty CompatibleData;
        SerializedProperty Attributes;
        SerializedProperty Properties;
        SerializedProperty UseTotalTime;
        SerializedProperty UseDeltaTime;
        SerializedProperty UseRandom;
        SerializedProperty SourceCode;

        ReorderableList attributeList;
        ReorderableList propertiesList;

        bool dirty;

        private void OnEnable()
        {
            BlockName = serializedObject.FindProperty("BlockName");
            ContextType = serializedObject.FindProperty("ContextType");
            CompatibleData = serializedObject.FindProperty("CompatibleData");
            Attributes = serializedObject.FindProperty("Attributes");
            Properties = serializedObject.FindProperty("Properties");
            UseTotalTime = serializedObject.FindProperty("UseTotalTime");
            UseDeltaTime = serializedObject.FindProperty("UseDeltaTime");
            UseRandom = serializedObject.FindProperty("UseRandom");
            SourceCode = serializedObject.FindProperty("SourceCode");
            source = SourceCode.stringValue;
            dirty = false;
            serializedObject.Update();

            if (attributeList == null)
            {
                attributeList = new ReorderableList(serializedObject, Attributes, true, true, true, true);
                attributeList.drawHeaderCallback = (r) => { GUI.Label(r, "Attributes"); };
                attributeList.onAddCallback = OnAddAttribute;
                attributeList.onRemoveCallback = OnRemoveAttribute;
                attributeList.drawElementCallback = OnDrawAttribute;
                attributeList.onReorderCallback = OnReorderAttribute;
            }

            if (propertiesList == null)
            {
                propertiesList = new ReorderableList(serializedObject, Properties, true, true, true, true);
                propertiesList.drawHeaderCallback = (r) => { GUI.Label(r, "Properties"); };
                propertiesList.onAddCallback = OnAddProperty;
                propertiesList.onRemoveCallback = OnRemoveProperty;
                propertiesList.drawElementCallback = OnDrawProperty;
                propertiesList.onReorderCallback = OnReorderProperty;
            }
        }

        void OnAddAttribute(ReorderableList list)
        {
            Attributes.InsertArrayElementAtIndex(0);
            var sp = Attributes.GetArrayElementAtIndex(0);
            sp.FindPropertyRelative("name").stringValue = "position";
            sp.FindPropertyRelative("mode").enumValueIndex = 3; // ReadWrite
            dirty = true;
            Apply();
        }

        void OnRemoveAttribute(ReorderableList list)
        {
            if (list.index != -1)
                Attributes.DeleteArrayElementAtIndex(list.index);
            dirty = true;
            Apply();
        }

        void OnReorderAttribute(ReorderableList list)
        {
            dirty = true;
            Apply();
        }

        void OnDrawAttribute(Rect rect, int index, bool isActive, bool isFocused)
        {
            var sp = Attributes.GetArrayElementAtIndex(index);
            rect.yMin += 2;
            var nameRect = rect;
            float split = rect.width / 2;

            nameRect.width = split - 40;
            string name = sp.FindPropertyRelative("name").stringValue;
            int attribvalue = EditorGUI.Popup(nameRect, Array.IndexOf(VFXAttribute.All, name), VFXAttribute.All);
            sp.FindPropertyRelative("name").stringValue = VFXAttribute.All[attribvalue];

            var modeRect = rect;
            modeRect.xMin = split;
            var mode = sp.FindPropertyRelative("mode");
            var value = EditorGUI.EnumPopup(modeRect, (AttributeMode)mode.intValue);
            mode.intValue = (int)System.Convert.ChangeType(value, typeof(AttributeMode));

            if(GUI.changed)
                Apply();
        }

        enum AttributeMode
        {
            Read = 1,
            Write = 2,
            ReadWrite = 3,
        }

        void OnAddProperty(ReorderableList list)
        {
            Properties.InsertArrayElementAtIndex(0);
            var sp = Properties.GetArrayElementAtIndex(0);
            sp.FindPropertyRelative("name").stringValue = "newProperty";
            sp.FindPropertyRelative("type").stringValue = "float";
            dirty = true;
            Apply();
        }

        void OnRemoveProperty(ReorderableList list)
        {

            if (list.index != -1)
                Properties.DeleteArrayElementAtIndex(list.index);
            dirty = true;
            Apply();
        }
        
        void OnReorderProperty(ReorderableList list)
        {
            dirty = true;
            Apply();
        }

        void OnDrawProperty(Rect rect, int index, bool isActive, bool isFocused)
        {
            var sp = Properties.GetArrayElementAtIndex(index);
            rect.yMin += 2;
            rect.height = 16;
            var nameRect = rect;
            float split = rect.width / 2 ;

            nameRect.width = split - 40;
            string name = sp.FindPropertyRelative("name").stringValue;
            sp.FindPropertyRelative("name").stringValue = EditorGUI.DelayedTextField(nameRect, name);

            var modeRect = rect;
            modeRect.xMin = split;

            var knownTypes = CustomBlock.knownTypes.Keys.ToArray();
            var type = sp.FindPropertyRelative("type");
            var value = EditorGUI.Popup(modeRect, Array.IndexOf(knownTypes, type.stringValue), knownTypes);
            type.stringValue = knownTypes[value];

            if(GUI.changed)
                Apply();
        }

        string source;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(BlockName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(ContextType);
            EditorGUILayout.PropertyField(CompatibleData);
            attributeList.DoLayoutList();
            propertiesList.DoLayoutList(); 

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(UseTotalTime);
            EditorGUILayout.PropertyField(UseDeltaTime);
            EditorGUILayout.PropertyField(UseRandom);

            if (EditorGUI.EndChangeCheck())
            {
                Apply();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Source Code", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            source = EditorGUILayout.TextArea(source, Styles.codeEditor, GUILayout.MinHeight(64));

            if (EditorGUI.EndChangeCheck())
                dirty = true;

            using (new EditorGUI.DisabledGroupScope(!dirty))
            {
                if (GUILayout.Button("Apply"))
                {
                    SourceCode.stringValue = source;
                    Apply();
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Syntax Cheat Sheet", EditorStyles.boldLabel);
            GUILayout.Label(
@"deltaTime : Delta Time
totalTime : Total Time
RAND : Random Float
RAND3 : Random Vector

SampleGradient(GradientProperty, t)
SampleCurve(CurveProperty, t)", Styles.helpBox);


        }

        void Apply()
        {
            serializedObject.ApplyModifiedProperties();
            dirty = false;   
            serializedObject.Update();
            Reload();
        }

        void Reload()
        {
            var block = (serializedObject.targetObject as VFXBlock);
            block.Invalidate(VFXModel.InvalidationCause.kSettingChanged);
            block.ResyncSlots(true);
        }

        static class Styles
        {
            public static GUIStyle codeEditor;
            public static GUIStyle helpBox;
            static Styles()
            {

                codeEditor = new GUIStyle(EditorStyles.textArea);
                Font font = AssetDatabase.LoadAssetAtPath<Font>("Packages/net.peeweek.vfxgraph-extras/Fonts/Inconsolata-Regular.otf");
                codeEditor.font = font;
                codeEditor.padding = new RectOffset(16,4,4,4);

                helpBox = new GUIStyle(EditorStyles.helpBox);
                helpBox.font = font;
                helpBox.richText = true;
                helpBox.fontSize = 12;
            }
        } 
    }
}

