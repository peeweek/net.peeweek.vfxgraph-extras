using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.VirtualImpacts;

namespace UnityEditor.VFX.DebugTools
{
    public class VFXVirtualImpactDebugWindow : EditorWindow
    {
        Dictionary<VFXVirtualImpact, bool> fold = new Dictionary<VFXVirtualImpact, bool>();
        Dictionary<VFXVirtualImpact, bool> handlesFor = new Dictionary<VFXVirtualImpact, bool>();
        Dictionary<VFXVirtualImpact, bool> instanceFold = new Dictionary<VFXVirtualImpact, bool>();
        Dictionary<VFXVirtualImpact, Color> handleColors = new Dictionary<VFXVirtualImpact, Color>();
        Vector2 scroll;

        [MenuItem("Window/Analysis/VFX Virtual Impact Debug")]
        static void OpenWindow()
        {
            GetWindow<VFXVirtualImpactDebugWindow>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("VFX Virtual Impact Debug");
            autoRepaintOnSceneChange = true;
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
        }



        private void SceneView_duringSceneGui(SceneView obj)
        {
            if (handlesFor == null || !Application.isPlaying)
                return;

            foreach (var virtualImpact in VFXVirtualImpact.virtualImpacts)
            {
                if (handlesFor.ContainsKey(virtualImpact) && handlesFor[virtualImpact])
                {
                    Handles.color = handleColors[virtualImpact];
                    var b = virtualImpact.activeBounds;
                    Handles.DrawWireCube(b.center, b.size);
                }
            }
        }

        private void OnGUI()
        {
            var virtualImpacts = VFXVirtualImpact.virtualImpacts;

            using (new GUILayout.HorizontalScope(Styles.header, GUILayout.Height(40)))
            {
                GUILayout.Box(Contents.VFXIcon, EditorStyles.label, GUILayout.Height(40), GUILayout.Width(40));
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(Contents.Cache("VFX Virtual Impact Debug"), Styles.bigLabel);
                    if(!Application.isPlaying)
                        GUILayout.Label(Contents.Cache($"USE PLAY TO DEBUG VIRTUAL IMPACTS"), EditorStyles.label);
                    else if (virtualImpacts == null || virtualImpacts.Count == 0)
                        GUILayout.Label(Contents.Cache($"NO VIRTUAL IMPACTS"), EditorStyles.label);
                    else
                        GUILayout.Label(Contents.Cache($"{virtualImpacts.Count} VIRTUAL IMPACT(S) ACTIVE"), EditorStyles.label);

                }
            }

            Rect r = GUILayoutUtility.GetRect(position.width, 1);
            EditorGUI.DrawRect(r, Color.black);

            scroll = GUILayout.BeginScrollView(scroll);

            foreach(var virtualImpact in virtualImpacts)
            {
                if (!fold.ContainsKey(virtualImpact))
                    fold.Add(virtualImpact, true);

                bool f = fold[virtualImpact];
                EditorGUI.BeginChangeCheck();
                f = EditorGUILayout.Foldout(f, Contents.Cache($"{virtualImpact.name} (Available : {virtualImpact.available.Count})", Contents.gameObjectIcon));
                if (EditorGUI.EndChangeCheck())
                    fold[virtualImpact] = f;

                if(f)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.BoundsField(Contents.Cache("Bounds"),virtualImpact.activeBounds);
                    EditorGUILayout.ObjectField(Contents.Cache("Impact"), virtualImpact, typeof(VFXVirtualImpact), false);
                    EditorGUILayout.ObjectField(Contents.Cache("VFXGraph Asset"), virtualImpact.Asset, typeof(VisualEffectAsset), false);
                    EditorGUILayout.ObjectField(Contents.Cache("Prefab Asset"), virtualImpact.Prefab, typeof(GameObject), false);

                    if (!handlesFor.ContainsKey(virtualImpact))
                        handlesFor.Add(virtualImpact, false);

                    if (!handleColors.ContainsKey(virtualImpact))
                        handleColors.Add(virtualImpact, RandomColor());

                    f = handlesFor[virtualImpact];
                    EditorGUI.BeginChangeCheck();
                    GUI.color = handleColors[virtualImpact];
                    f = EditorGUILayout.Toggle(Contents.Cache("Show Handles"), f);
                    GUI.color = Color.white;
                    if (EditorGUI.EndChangeCheck())
                        handlesFor[virtualImpact] = f;


                    GUILayout.Space(16);

                    if (!instanceFold.ContainsKey(virtualImpact))
                        instanceFold.Add(virtualImpact, false);

                    f = instanceFold[virtualImpact];
                    EditorGUI.BeginChangeCheck();
                    f = EditorGUILayout.Foldout(f, Contents.Cache($"Instances : ({virtualImpact.MaxInstanceCount - virtualImpact.available.Count}/{virtualImpact.MaxInstanceCount})"));
                    if (EditorGUI.EndChangeCheck())
                        instanceFold[virtualImpact] = f;

                    if(f)
                    {
                        EditorGUI.indentLevel ++;
                        for (int i = 0; i < virtualImpact.activeImpacts.Count; i++)
                        {
                            var impact = virtualImpact.activeImpacts[i];

                            if (impact.TTL < 0f)
                                continue;

                            EditorGUILayout.LabelField($"[#{impact.index}] (TTL: {impact.TTL.ToString("F2")}s) - Bounds : {impact.Bounds}");
                        }
                        EditorGUI.indentLevel --;
                    }

                    EditorGUI.indentLevel -= 2;
                    GUILayout.Space(16);
                }
            }
            GUILayout.EndScrollView();
        }

        static float hue;
        static Color RandomColor()
        {
            hue = (hue + 0.4751f) % 1.0f;
            return Color.HSVToRGB(hue, 1f, 1f);
        }

        class Contents
        {
            public static Texture gameObjectIcon;
            public static GUIContent VFXIcon;

            static Dictionary<string, GUIContent> cached;

            public static GUIContent Cache(string label, Texture t = null)
            {
                if (!cached.ContainsKey(label))
                    cached.Add(label, new GUIContent(label, t));

                return cached[label];
            }

            static Contents()
            {
                cached = new Dictionary<string, GUIContent>();
                var tex = EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-vfxvirtualimpact.png") as Texture2D;
                VFXIcon = new GUIContent(string.Empty, tex);
            }

        }

        class Styles
        {
            public static GUIStyle header;
            public static GUIStyle bigLabel;

            static Styles()
            {
                header = new GUIStyle(EditorStyles.label);
                header.padding = new RectOffset(2, 8, 8, 8);

                bigLabel = new GUIStyle(EditorStyles.boldLabel);
                bigLabel.fontSize = 18;
            }
        }
    }
}


