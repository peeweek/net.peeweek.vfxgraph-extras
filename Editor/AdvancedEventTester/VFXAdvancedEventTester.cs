using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using UnityEditorInternal;
using System.Collections.Generic;

namespace UnityEditor.VFX
{
    class VFXAdvancedEventTester : EditorWindow
    {
        [SerializeField]
        VisualEffect visualEffect;
        [SerializeField]
        bool lockSelection;

        [SerializeField]
        bool LockTool;

        [SerializeField]
        List<VFXEventTest> tests;
        ReorderableList testsRList;

        [MenuItem("Window/Visual Effects/VFX Advanced Event Tester")]
        static void OpenWindow()
        {
            GetWindow<VFXAdvancedEventTester>();
        }

        Texture m_Icon;
        GUIContent m_VFXIcon;

        private void OnEnable()
        {
            m_Icon = EditorGUIUtility.IconContent("UnityEditor.ProfilerWindow").image;
            m_VFXIcon = EditorGUIUtility.IconContent("VisualEffectAsset Icon");

            titleContent = new GUIContent("VFX Advanced Event Tester", m_Icon);

            if (tests == null)
                tests = new List<VFXEventTest>();

            if (testsRList == null)
            {
                testsRList = new ReorderableList(tests, typeof(VFXEventTest), true, true, true, true);
                testsRList.drawHeaderCallback = OnDrawHeader;
                testsRList.drawElementCallback = OnDrawElement;
                testsRList.onAddCallback = OnTestAdd;
                testsRList.onRemoveCallback = OnTestRemove;
                testsRList.onSelectCallback = OnTestSelect;
            }

            EditorApplication.update += TesterUpdate;
            SceneView.duringSceneGui += ToolUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TesterUpdate;
            SceneView.duringSceneGui -= ToolUpdate;
        }

        void ToolUpdate(SceneView sceneView)
        {
            if (!LockTool || Event.current.alt)
                return;

            Selection.activeGameObject = null;


        }

        void TesterUpdate()
        {
            if (visualEffect == null)
                return;

            foreach(var test in tests)
            {
                if (test == null)
                    continue;

                test.UpdateTest(visualEffect);

            }
        }

        void OnDrawHeader(Rect r)
        {
            GUI.Label(r, "VFX Event Tests", EditorStyles.boldLabel);
        }

        void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.yMin += 2;
            rect.height = 18;

            var b = rect;
            b.width = 32;

            if (tests[index] == null || tests[index].updateBehavior == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Toggle(b, false, string.Empty);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                tests[index].updateBehavior.enableUpdate = GUI.Toggle(b, tests[index].updateBehavior.enableUpdate, Contents.Refresh, EditorStyles.miniButton);
            }
            rect.xMin += 32;
            b = rect;
            b.width = 32;

            if (GUI.Button(b, "▶"))
                tests[index].PerformEvent(visualEffect);

            rect.xMin += 32;
            tests[index] = (VFXEventTest)EditorGUI.ObjectField(rect, tests[index], typeof(VFXEventTest), false);

        }

        void OnTestAdd(ReorderableList l) 
        {
            tests.Add(null); 
        }
        void OnTestRemove(ReorderableList l) 
        {
            int i = l.index;
            tests.RemoveAt(l.index);
            if (tests.Count > 0)
            {
                i = Mathf.Clamp(i - 1, 0, tests.Count);
                l.index = i;
            }
        }
        void OnTestSelect(ReorderableList l) 
        {
            if (tests[l.index] == null)
                return;

        }

        private void OnSelectionChange()
        {
            if( !lockSelection 
                && Selection.activeGameObject != null 
                && Selection.activeGameObject.TryGetComponent(out VisualEffect vfx))
            {
                visualEffect = vfx;
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (visualEffect == null)
                lockSelection = false;

            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope(GUILayout.Height(32)))
            {
                Rect r = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
                GUI.Label(r, m_VFXIcon);
                GUILayout.Label("VFX Advanced Event Tester", Styles.title);
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(lockSelection);
                visualEffect = (VisualEffect)EditorGUILayout.ObjectField("Target", visualEffect, typeof(VisualEffect), true);
                EditorGUI.EndDisabledGroup();
                lockSelection = GUILayout.Toggle(lockSelection, EditorGUIUtility.IconContent("InspectorLock"), EditorStyles.miniButton, GUILayout.Width(32));
            }
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            LockTool = GUILayout.Toggle(LockTool, new GUIContent("Tool"), EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
                Tools.current = LockTool ? Tool.None : Tool.Transform;
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(visualEffect == null);
            using(new GUILayout.HorizontalScope())
            {
                if(GUILayout.Button(visualEffect == null || visualEffect.pause ?"Play" : "Pause", GUILayout.Width(64)))
                {
                    visualEffect.pause = !visualEffect.pause;
                }

                if (GUILayout.Button("Step"))
                {
                    visualEffect.pause = true;
                    visualEffect.AdvanceOneFrame();
                }

                if (GUILayout.Button("Reinit"))
                {
                    visualEffect.Reinit();
                }

                if (GUILayout.Button("OnPlay"))
                {
                    visualEffect.Play();
                }

                if (GUILayout.Button("OnStop"))
                {
                    visualEffect.Play();
                }

                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = visualEffect.gameObject;
                }
                if (GUILayout.Button("Edit"))
                {
                    AssetDatabase.OpenAsset(visualEffect.visualEffectAsset);
                }

            }
            EditorGUILayout.Space();

            testsRList.DoLayoutList();

            if(testsRList.index != -1 && tests[testsRList.index] != null)
            {
                EditorGUILayout.Space();
                Editor.CreateCachedEditor(tests[testsRList.index], null, ref m_EvtEditor);
                m_EvtEditor.DrawHeader();
                EditorGUI.indentLevel ++;
                m_EvtEditor.OnInspectorGUI();
                EditorGUI.indentLevel --;
            }

            EditorGUI.EndDisabledGroup();
        }

        Editor m_EvtEditor;
        static class Contents
        {
            public static GUIContent Refresh;

            static Contents()
            {
                Refresh = EditorGUIUtility.IconContent("Refresh");
            }
        }
        static class Styles
        {
            public static GUIStyle title;
            static Styles()
            {
                title = new GUIStyle(EditorStyles.boldLabel);
                title.fontSize = 18;
            }
        }
    }
}


