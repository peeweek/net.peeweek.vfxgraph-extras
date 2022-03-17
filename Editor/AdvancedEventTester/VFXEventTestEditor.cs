using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.VFX
{
    [CustomEditor(typeof(VFXEventTest))]
    public class VFXEventTestEditor : Editor
    {
        SerializedProperty eventName;
        SerializedProperty eventAttributes;
        SerializedProperty updateBehavior;

        ReorderableList rlist;

        private void OnEnable()
        {
            eventName = serializedObject.FindProperty("eventName");
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
                b.width = 24;
                EditorGUI.BeginChangeCheck();
                var enabledProp = item.FindPropertyRelative("enabled");
                bool enabled = GUI.Toggle(b, enabledProp.boolValue, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                    enabledProp.boolValue = enabled;

                rect.xMin += 24;
                GUI.Label(rect, ObjectNames.NicifyVariableName(item.FindPropertyRelative("name").stringValue));
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
                s_AttributeTypes = FindConcreteSubclasses(typeof(EventAttributeSetup)).ToArray();

            foreach (var type in s_AttributeTypes)
            {
                string name = type.Name;
                m_AttributeMenu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(name)), false, AddAttribute, type);
            }
        }

        public void BuildBehaviorMenu()
        {
            m_BehaviorMenu = new GenericMenu();
            if (s_BehaviorTypes == null)
                s_BehaviorTypes = FindConcreteSubclasses(typeof(EventTestUpdateBehavior)).ToArray();

            foreach (var type in s_BehaviorTypes)
            {
                string name = type.Name;
                m_BehaviorMenu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(name)), false, SetUpdateBehavior, type);
            }
        }

        void AddAttribute(object o)
        {
            Type t = o as Type;
            Undo.RecordObject(serializedObject.targetObject, "Add Attribute");
            var attrib = Activator.CreateInstance(t) as EventAttributeSetup;
            attrib.name = t.Name;
            if(attrib != null)
                (serializedObject.targetObject as VFXEventTest).eventAttributes.Add(attrib);
        }

        void SetUpdateBehavior(object o)
        {
            Type t = o as Type;
            Undo.RecordObject(serializedObject.targetObject, "Set Update Behavior");
            var bh = Activator.CreateInstance(t) as EventTestUpdateBehavior;
            
            if (bh != null)
                (serializedObject.targetObject as VFXEventTest).updateBehavior = bh;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(eventName);

            var testTarget = serializedObject.targetObject as VFXEventTest;

            using (new GUILayout.HorizontalScope(Styles.header))
            {
                GUILayout.Label("Update Behavior", Styles.headerLabel);
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
                GUILayout.Label("Event Attributes", Styles.headerLabel);
                GUILayout.FlexibleSpace();
            }

            rlist.DoLayoutList();

            if(rlist.index != -1)
            {

                var prop = eventAttributes.GetArrayElementAtIndex(rlist.index);

                using (new GUILayout.VerticalScope(Styles.header, GUILayout.Height(24)))
                {
                    GUILayout.Label(prop.displayName, Styles.headerLabel);
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
            }
        }
    }
}

