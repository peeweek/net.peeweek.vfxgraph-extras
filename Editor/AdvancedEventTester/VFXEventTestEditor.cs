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
        SerializedProperty enabled;
        SerializedProperty eventName;
        SerializedProperty periodicity;
        SerializedProperty eventAttributes;

        ReorderableList rlist;

        private void OnEnable()
        {
            enabled = serializedObject.FindProperty("enabled");
            eventName = serializedObject.FindProperty("eventName");
            periodicity = serializedObject.FindProperty("periodicity");
            eventAttributes = serializedObject.FindProperty("eventAttributes");

            rlist = new ReorderableList(serializedObject, eventAttributes);
            rlist.drawHeaderCallback += RList_Header;
            rlist.onAddCallback += RList_AddItem;
            rlist.drawElementCallback += RList_DrawElement;
            BuildMenu();
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
                bool enabled = GUI.Toggle(b, enabledProp.boolValue, string.Empty);
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
            m_Menu.ShowAsContext();
        }

        GenericMenu m_Menu;
        static Type[] s_Types;

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

        public void BuildMenu()
        {
            m_Menu = new GenericMenu();
            if (s_Types == null)
                s_Types = FindConcreteSubclasses(typeof(VFXEventTest.EventAttributeSetup)).ToArray();

            foreach (var type in s_Types)
            {
                string name = type.Name;
                m_Menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(name)), false, AddAttribute, type);
            }
        }

        void AddAttribute(object o)
        {
            Type t = o as Type;
            Undo.RecordObject(serializedObject.targetObject, "Add Attribute");
            var attrib = Activator.CreateInstance(t) as VFXEventTest.EventAttributeSetup;
            attrib.name = t.Name;
            if(attrib != null)
                (serializedObject.targetObject as VFXEventTest).eventAttributes.Add(attrib);
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(enabled);
            EditorGUILayout.PropertyField(eventName);
            EditorGUILayout.PropertyField(periodicity);

            rlist.DoLayoutList();

            if(rlist.index != -1)
            {
                var prop = eventAttributes.GetArrayElementAtIndex(rlist.index);
                
                prop.NextVisible(true);
                while(prop.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

