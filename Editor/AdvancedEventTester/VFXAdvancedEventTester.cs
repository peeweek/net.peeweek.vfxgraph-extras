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
        List<VFXEventTest> tests;
        ReorderableList testsRList;

        [MenuItem("Window/Visual Effects/VFX Advanced Event Tester")]
        static void OpenWindow()
        {
            GetWindow<VFXAdvancedEventTester>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("VFX Advanced Event Tester");
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
        }

        private void OnDisable()
        {
            EditorApplication.update -= TesterUpdate;
        }

        void TesterUpdate()
        {
            if (visualEffect == null)
                return;

            foreach(var test in tests)
            {
                if (!test.enabled)
                    continue;

                test.UpdateTest(visualEffect);

            }
        }

        void OnDrawHeader(Rect r)
        {
            GUI.Label(r, "Events");
        }

        void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.yMin += 2;
            rect.height = 18;

            var b = rect;
            b.width = 24;
            tests[index].enabled = GUI.Toggle(b, tests[index].enabled, string.Empty);

            rect.xMin += 24;
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

            Selection.activeObject = tests[l.index];
        }

        private void OnSelectionChange()
        {
            if(!lockSelection && Selection.activeGameObject.TryGetComponent(out VisualEffect vfx))
            {
                visualEffect = vfx;
                Repaint();
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Visual Effect", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(lockSelection);
                visualEffect = (VisualEffect)EditorGUILayout.ObjectField("Target", visualEffect, typeof(VisualEffect), true);
                EditorGUI.EndDisabledGroup();
                lockSelection = GUILayout.Toggle(lockSelection, EditorGUIUtility.IconContent("InspectorLock"), EditorStyles.miniButton, GUILayout.Width(32));
            }
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
            GUILayout.Space(16);

            GUILayout.Label("Event Setup", EditorStyles.boldLabel);

            testsRList.DoLayoutList();

            EditorGUI.EndDisabledGroup();
        }
    }
}


