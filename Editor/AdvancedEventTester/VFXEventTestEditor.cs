using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.VFX.EventTesting;
using UnityEngine.VFX.Extras;

namespace UnityEditor.VFX.EventTesting
{
    [CustomEditor(typeof(VFXEventTest))]
    public class VFXEventTestEditor : Editor
    {
        SerializedProperty enableUpdate;
        SerializedProperty singleEventName;
        SerializedProperty enableStartEvent;
        SerializedProperty startEventName;
        SerializedProperty enableStopEvent;
        SerializedProperty stopEventName;
        SerializedProperty eventAttributes;
        SerializedProperty updateBehavior;

        ReorderableList rlist;

        private void OnEnable()
        {
            enableUpdate = serializedObject.FindProperty("enableUpdate");
            singleEventName = serializedObject.FindProperty("singleEventName");
            enableStartEvent = serializedObject.FindProperty("enableStartEvent");
            startEventName = serializedObject.FindProperty("startEventName");
            enableStopEvent = serializedObject.FindProperty("enableStopEvent");
            stopEventName = serializedObject.FindProperty("stopEventName");
            updateBehavior = serializedObject.FindProperty("updateBehavior");

            eventAttributes = serializedObject.FindProperty("eventAttributes");

            rlist = new ReorderableList(serializedObject, eventAttributes, true, false, true, true);
            rlist.drawHeaderCallback += RList_Header;
            rlist.onAddCallback += RList_AddItem;
            rlist.drawElementCallback += RList_DrawElement;

            BuildAttributeMenu();
            BuildBehaviorMenu();
        }

        void RList_Header(Rect rect)
        {
            GUI.Label(rect, "Event Attributes", EditorStyles.boldLabel);
        }

        void RList_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = rlist.serializedProperty.GetArrayElementAtIndex(index);
            if (item != null)
            {
                Rect b = rect;
                b.width = 20;
                EditorGUI.BeginChangeCheck();
                var enabledProp = item.FindPropertyRelative("enabled");
                bool enabled = GUI.Toggle(b, enabledProp.boolValue, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    enabledProp.boolValue = enabled;
                }

                rect.xMin += 24;
                string typeName = item.type.Remove(0, 16);
                GUI.Label(rect, $"{item.FindPropertyRelative("attributeName").stringValue} : {typeName}");
            }
            else
                GUI.Label(rect, "NULL");

        }

        void RList_AddItem(ReorderableList l)
        {
            m_AttributeMenu.ShowAsContext();
        }


        GenericMenu m_AttributeMenu;
        GenericMenu m_BehaviorMenu;
        static Type[] s_AttributeTypes;
        static Type[] s_BehaviorTypes;

        public static IEnumerable<Type> FindConcreteSubclasses(Type objectType = null)
        {
            List<Type> types = new List<Type>();
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = null;
                try
                {
                    assemblyTypes = domainAssembly.GetTypes();
                }
                catch (Exception)
                {
                    assemblyTypes = null;
                }
                if (assemblyTypes != null)
                    foreach (var assemblyType in assemblyTypes)
                        if ((objectType == null || assemblyType.IsSubclassOf(objectType)) && !assemblyType.IsAbstract)
                            types.Add(assemblyType);
            }
            return types;
        }

        public void BuildAttributeMenu()
        {
            m_AttributeMenu = new GenericMenu();
            if (s_AttributeTypes == null)
                s_AttributeTypes = FindConcreteSubclasses(typeof(VFXEventAttributeSetup)).ToArray();

            foreach (var type in s_AttributeTypes)
            {
                string name = ObjectNames.NicifyVariableName(type.Name);
                string category = "";

                var attr = type.GetCustomAttributes(typeof(VFXEventAttributeSetupAttribute), false).FirstOrDefault() as VFXEventAttributeSetupAttribute;
                if(attr != null)
                {
                    if (!string.IsNullOrEmpty(attr.name))
                        name = attr.name;
                    if (!string.IsNullOrEmpty(attr.category))
                        category = attr.category;
                }

                m_AttributeMenu.AddItem(new GUIContent(string.IsNullOrEmpty(category)? name : $"{category}/{name}"), false, AddAttribute, type);
            }
        }

        public void BuildBehaviorMenu()
        {
            m_BehaviorMenu = new GenericMenu();
            if (s_BehaviorTypes == null)
                s_BehaviorTypes = FindConcreteSubclasses(typeof(VFXEventSendUpdateBehavior)).ToArray();

            foreach (var type in s_BehaviorTypes)
            {
                if (type.IsAbstract)
                    continue;

                string name = type.Name;
                m_BehaviorMenu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(name)), false, SetUpdateBehavior, type);
            }
        }

        void AddAttribute(object o)
        {
            Type t = o as Type;
            Undo.RecordObject(serializedObject.targetObject, "Add Attribute");
            var attrib = Activator.CreateInstance(t) as VFXEventAttributeSetup;
            if(attrib != null)
                (serializedObject.targetObject as VFXEventTest).eventAttributes.Add(attrib);
        }

        void SetUpdateBehavior(object o)
        {
            Type t = o as Type;
            Undo.RegisterCreatedObjectUndo(serializedObject.targetObject, "Set Update Behavior");
            var bh = Activator.CreateInstance(t) as VFXEventSendUpdateBehavior;
            
            if (bh != null)
            {
                var eventSetup = (serializedObject.targetObject as VFXEventTest);
                eventSetup.updateBehavior = bh;
                EditorUtility.SetDirty(eventSetup);
                AssetDatabase.SaveAssets();
            }
        }

        protected override void OnHeaderGUI()
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Height(8));
            r.height = 1;
            EditorGUI.DrawRect(r, Color.black);
            using (new GUILayout.HorizontalScope(Styles.header))
            {
                GUILayout.Label($"{serializedObject.targetObject.name}", Styles.headerLabel);
            }
            GUILayout.Space(8);

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(serializedObject.targetObject)));
            EditorGUI.BeginChangeCheck();
            string n = EditorGUILayout.DelayedTextField("Name", serializedObject.targetObject.name);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(serializedObject.targetObject, "Set Object Name");
                serializedObject.targetObject.name = n;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(enableUpdate);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(singleEventName);
            EditorGUILayout.Space();

            using(new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                EditorGUI.BeginChangeCheck();
                bool value = GUILayout.Toggle(enableStartEvent.boolValue, GUIContent.none, GUILayout.Width(16));
                if(EditorGUI.EndChangeCheck())
                {
                    enableStartEvent.boolValue = value;
                }
                EditorGUI.BeginDisabledGroup(!enableStartEvent.boolValue);
                EditorGUILayout.PropertyField(startEventName);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                EditorGUI.BeginChangeCheck();
                bool value = GUILayout.Toggle(enableStopEvent.boolValue, GUIContent.none, GUILayout.Width(16));
                if (EditorGUI.EndChangeCheck())
                {
                    enableStopEvent.boolValue = value;
                }
                EditorGUI.BeginDisabledGroup(!enableStopEvent.boolValue);
                EditorGUILayout.PropertyField(stopEventName);
                EditorGUI.EndDisabledGroup();
            }


            var testTarget = serializedObject.targetObject as VFXEventTest;

            using (new GUILayout.HorizontalScope(Styles.header))
            {
                string typeName = testTarget.updateBehavior == null ? "null" : testTarget.updateBehavior.GetType().Name;

                GUILayout.Label($"Update Behavior ({typeName})", Styles.headerSubLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(24), GUILayout.Height(22)))
                {
                    m_BehaviorMenu.ShowAsContext();
                }
            }

            using (new GUILayout.VerticalScope())
            {
                EditorGUI.BeginDisabledGroup(testTarget.updateBehavior == null);
                updateBehavior = serializedObject.FindProperty("updateBehavior");
                DrawItemPropertyWithoutFoldout(updateBehavior);
                EditorGUI.EndDisabledGroup();

            }

            using (new GUILayout.HorizontalScope(Styles.header))
            {
                GUILayout.Label("Event Attributes", Styles.headerSubLabel);
                GUILayout.FlexibleSpace();
            }

            rlist.DoLayoutList();

            if(rlist.index != -1)
            {

                var prop = eventAttributes.GetArrayElementAtIndex(rlist.index);

                using (new GUILayout.VerticalScope(Styles.header, GUILayout.Height(24)))
                {
                    string display = $"{prop.FindPropertyRelative("attributeName").stringValue} : {prop.type.Remove(0, 16)}";
                    GUILayout.Label(display, Styles.headerLabel);
                }

                DrawItemPropertyWithoutFoldout(prop);

            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawItemPropertyWithoutFoldout(SerializedProperty prop)
        {
            var path = prop.propertyPath;

            if(prop.hasChildren)
            {
                prop.NextVisible(true);
            }

            EditorGUILayout.PropertyField(prop, true);

            while (prop.NextVisible(false) && prop.propertyPath.Contains(path))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        static class Styles
        {
            public static GUIStyle header;
            public static GUIStyle headerLabel;
            public static GUIStyle headerSubLabel;

            static Styles()
            {
                header = new GUIStyle("ShurikenModuleTitle");
                header.fontSize = 13;
                header.fontStyle = FontStyle.Bold;
                header.border = new RectOffset(15, 7, 4, 4);
                header.margin = new RectOffset(0, 0, 16, 0);
                header.fixedHeight = 24;
                header.contentOffset = new Vector2(32f, -2f);

                headerLabel = new GUIStyle(EditorStyles.boldLabel);
                headerLabel.fontSize = 13;
                headerLabel.alignment = TextAnchor.MiddleLeft;

                headerSubLabel = new GUIStyle(headerLabel);
                headerSubLabel.fontSize = 11;

            }
        }
    }
}

