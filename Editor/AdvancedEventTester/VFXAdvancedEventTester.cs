using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Extras;
using UnityEditorInternal;
using UnityEngine.VFX.EventTesting;

namespace UnityEditor.VFX.EventTesting
{
    class VFXAdvancedEventTester : EditorWindow
    {
        [SerializeField]
        VisualEffect visualEffect = null;
        [SerializeField]
        bool lockSelection = false;

        VFXEventTestContainer sceneEventTestComponent;

        List<VFXEventTest> tests
        {
            get
            {
                if (visualEffect == null)
                    return null;

                if(visualEffect.TryGetComponent(out VFXEventTestContainer sceneTest))
                {
                    return sceneTest.eventTests;
                }

                return null;
            }
        }

        ReorderableList testsRList;

        [MenuItem("Window/Visual Effects/VFX Advanced Event Tester")]
        static void OpenWindow()
        {
            GetWindow<VFXAdvancedEventTester>();
        }

        public static void OpenSceneTest(VFXEventTestContainer container)
        {
            var instance = GetWindow<VFXAdvancedEventTester>();

            if(instance != null && container != null)
                instance.visualEffect = container.GetComponent<VisualEffect>();
        }

        Texture m_Icon;
        GUIContent m_VFXIcon;

        private void OnEnable()
        {
            m_Icon = EditorGUIUtility.IconContent("UnityEditor.ProfilerWindow").image;
            m_VFXIcon = EditorGUIUtility.IconContent("VisualEffectAsset Icon");

            titleContent = new GUIContent("VFX Advanced Event Tester", m_Icon);

            EditorApplication.update += TesterUpdate;
            SceneView.beforeSceneGui += TesterSceneUpdate;
        }

        void GetSceneTestComponentFor(VisualEffect sceneComponent, bool createIfNotPresent)
        {   
            if (!sceneComponent.TryGetComponent(out sceneEventTestComponent) && createIfNotPresent)
            {
                sceneEventTestComponent = sceneComponent.gameObject.AddComponent<VFXEventTestContainer>();
            }

            if(sceneEventTestComponent != null)
            {
                if (testsRList == null || (testsRList.list != sceneEventTestComponent.eventTests))
                {
                    //Debug.Log("Recreate RList");
                    testsRList = new ReorderableList(sceneEventTestComponent.eventTests, typeof(VFXEventTest), true, true, true, true);
                    testsRList.drawHeaderCallback = OnDrawHeader;
                    testsRList.drawElementCallback = OnDrawElement;
                    testsRList.onAddCallback = OnTestAdd;
                    testsRList.onRemoveCallback = OnTestRemove;
                    testsRList.onSelectCallback = OnTestSelect;
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= TesterUpdate;
            SceneView.beforeSceneGui -= TesterSceneUpdate;
        }

        bool mouseLeft = false;
        bool mouseRight = false;

        void TesterSceneUpdate(SceneView sceneView)
        {
            if (tests == null || Event.current.type == EventType.Layout)
                return;

            if (Event.current.alt)
            {
                mouseLeft = false;
                mouseRight = false;
                return;
            }

            bool used = false;
            foreach(var test in tests)
            {
                if (test == null || test.updateBehavior == null)
                    continue;

                if (!test.updateBehavior.canUseTool)
                    continue;

                if (test.enableUpdate)
                {
                    Vector2 mousePosition = Event.current.mousePosition;
                    mousePosition.y = (SceneView.lastActiveSceneView.position.height - 22) - mousePosition.y;
                    Event e = Event.current;

                    if (e.type == EventType.MouseDown)
                    {
                        if (e.button == 0)
                            mouseLeft = true;
                        else if (e.button == 1)
                            mouseRight = true;

                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        if (e.button == 0)
                            mouseLeft = false;
                        else if (e.button == 1)
                            mouseRight = false;
                    }
                    
                    used = used || test.updateBehavior.OnSceneGUIUpdate(sceneView.camera, mousePosition, mouseLeft, mouseRight, (float)EditorApplication.timeSinceStartup);
                }
            }
            
            if(used && Event.current.type != EventType.Repaint)
            {
                Selection.activeGameObject = null;
                Event.current.Use();
            }
        }

        void TesterUpdate()
        {
            if (visualEffect == null || tests == null)
                return;

            foreach(var test in tests)
            {
                if (test == null)
                    continue;

                test.UpdateTest(visualEffect, (float)EditorApplication.timeSinceStartup);
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
            b.width = 28;

            if (tests[index] == null || tests[index].updateBehavior == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Toggle(b, false, GUIContent.none, EditorStyles.miniButton);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                tests[index].enableUpdate = GUI.Toggle(b, tests[index].enableUpdate, Contents.Refresh, EditorStyles.miniButton);
                if(EditorGUI.EndChangeCheck() )
                {
                    if(tests[index].enableUpdate)
                    {
                        tests[index].StartTest(visualEffect, (float)EditorApplication.timeSinceStartup);
                    }
                    else
                    {
                        tests[index].StopTest(visualEffect, (float)EditorApplication.timeSinceStartup);
                    }
                }
            }

            rect.xMin += 32;
            b = rect;
            b.width = 28;

            EditorGUI.BeginDisabledGroup(!tests[index].updateBehavior.canSendSingleEvent);
            if (GUI.Button(b, "▶"))
                tests[index]?.PerformSingleEvent(visualEffect);
            EditorGUI.EndDisabledGroup();

            rect.xMin += 32;

            if (tests[index] == null)
                rect.width -= 32;

            EditorGUI.BeginDisabledGroup(true);
            tests[index] = (VFXEventTest)EditorGUI.ObjectField(rect, tests[index], typeof(VFXEventTest), false);
            EditorGUI.EndDisabledGroup();

            if(tests[index] == null)
            {
                rect.xMin = rect.xMax;
                rect.width = 32;
                if (GUI.Button(rect, "+"))
                {
                    var instance = CreateInstance<VFXEventTest>();
                    Undo.RegisterCreatedObjectUndo(instance, "Create new VFXEventTest Object");

                    Undo.RecordObject(sceneEventTestComponent, "Assign VFXEventTest to SceneEventTest Component");
                    sceneEventTestComponent.eventTests[index] = instance;
                }
            }

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
            Rect r;

            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope(GUILayout.Height(32)))
            {
                r = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
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

            r = GUILayoutUtility.GetLastRect();
            r.yMin += 4;
            r.height = 1;
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.5f));
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

            if(visualEffect != null)
            {
                GetSceneTestComponentFor(visualEffect, false);

                if (tests != null)
                {
                    if (testsRList.count == 0)
                        testsRList.index = -1;

                    testsRList.DoLayoutList();

                    if (testsRList.index != -1 && tests[testsRList.index] != null)
                    {
                        EditorGUILayout.Space();
                        Editor.CreateCachedEditor(tests[testsRList.index], null, ref m_EvtEditor);
                        m_EvtEditor.DrawHeader();
                        EditorGUI.indentLevel++;
                        m_EvtEditor.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (GUILayout.Button("Create Test Component on Game Object", GUILayout.Height(24)))
                    {
                        GetSceneTestComponentFor(visualEffect, true);
                    }
                }
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


