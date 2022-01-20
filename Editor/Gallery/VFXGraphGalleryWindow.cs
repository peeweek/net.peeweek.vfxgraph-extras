using System.Collections.Generic;
using UnityEditor.VFX.UI;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public class VFXGraphGalleryWindow : EditorWindow
    {
        string outPath; 
        bool debug = false;
        public static void OpenWindowCreateAsset(string outPath)
        {
            var window = GetWindow<VFXGraphGalleryWindow>(true);
            window.outPath = outPath;
            window.SetTitle($"Create New VFX Asset");
        }

        private void OnEnable()
        {
            minSize = new Vector2(800, 480);
            maxSize = new Vector2(800, 480);

            UpdateTemplates();
        }

        void SetTitle(string title)
        {
            titleContent = new GUIContent(title);
            
        }

        Vector2 scroll;
        Vector2 scrollDesc;


        List<VFXGraphGalleryTemplate> categories;

        VFXGraphGalleryTemplate.Template selected;
        string selectedCategory;
        VisualEffectAsset selectedSource;

        bool createGameObject
        {
            get
            {
                return EditorPrefs.GetBool("VFXGraphGalleryWindow.createGameObject", false);
            }

            set
            {
                EditorPrefs.SetBool("VFXGraphGalleryWindow.createGameObject", value);
            }
        }


        void UpdateTemplates()
        {
            if (categories == null)
                categories = new List<VFXGraphGalleryTemplate>();

            categories.Clear();

            string[] assets = AssetDatabase.FindAssets("t:VFXGraphGalleryTemplate");
            foreach(var guid in assets)
            {
                var template = (VFXGraphGalleryTemplate)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(VFXGraphGalleryTemplate));
                
                if(template.templates != null && template.templates.Length > 0)
                    categories.Add(template);
            }

            categories.Sort((a, b) => a.categoryName.CompareTo(b.categoryName));

            selected = categories[0].templates[0];
            selectedCategory = categories[0].categoryName;
            selectedSource = selected.templateAsset;
        }

        private void OnGUI()
        { 
            using (new GUILayout.HorizontalScope(Styles.header, GUILayout.Height(80)))
            {
                GUILayout.Box(Contents.VFXIcon, EditorStyles.label, GUILayout.Height(64));
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(Contents.Cache("Select a Starting Point"), Styles.bigLabel);
                    GUILayout.Label(Contents.Cache("Pick a template from the gallery"), EditorStyles.label);
                }
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label($"{outPath}", Styles.rightLabel);
                    GUILayout.FlexibleSpace();
                    using(new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Reload", GUILayout.Width(80)))
                            UpdateTemplates();
                    }
                }
            }
            EditorGUI.DrawRect(new Rect(0, 80, 800, 1), Color.black);

            int width = 512;

            using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                EditorGUI.DrawRect(new Rect(0, 80, width, 368), new Color(0,0,0,0.2f));
                using (new GUILayout.VerticalScope(GUILayout.Width(width)))
                {
                    scroll = GUILayout.BeginScrollView(scroll);

                    foreach(var category in categories)
                    {
                        // Group
                        using(new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
                        {
                            GUILayout.Label(string.IsNullOrEmpty(category.categoryName) ? category.name : category.categoryName, Styles.midLabel);
                            GUILayout.FlexibleSpace();
                        }

                        int i = 0;
                        foreach (var t in category.templates)
                        {
                            if (i % 3 == 0)
                                GUILayout.BeginHorizontal();

                            if (selectedSource != null && selectedSource == t.templateAsset)
                                GUI.backgroundColor = new Color(0.6f, 1.2f, 1.6f, 1.0f);


                            using (new GUILayout.VerticalScope(Styles.galButton, GUILayout.Width((width/3) - 8), GUILayout.Height(128)))
                            {
                                GUILayout.Space(8);
                                Rect r = GUILayoutUtility.GetRect(120, 96);
                                GUI.DrawTexture(r, t.preview == null ? Contents.emptyPreview : t.preview);
                                GUILayout.Label(t.name, Styles.labelCenter);

                                r = new RectOffset(4,4,4,20).Add(r);
                                if(Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                                {
                                    selected = t;
                                    selectedCategory = !string.IsNullOrEmpty(category.categoryName)? category.categoryName : category.name;
                                    selectedSource = selected.templateAsset;
                                }
                            }

                            if (i % 3 == 2)
                                GUILayout.EndHorizontal();

                            i++;

                            GUI.backgroundColor = Color.white;
                        }

                        if ((i-1) % 3 < 2)
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }

                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndScrollView();
                }
                EditorGUI.DrawRect(new Rect(width, 80, 1, 368), Color.black);
                
                using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    GUILayout.Space(180);
                    Rect previewRect = new Rect(width + 1, 81, 799 - width, 180);
                    GUI.DrawTexture(previewRect, selected.preview == null ? Contents.emptyPreview : selected.preview); ;

                    GUILayout.Label($"{selected.name}", Styles.bigLabel);
                    GUILayout.Label($"Category : {selectedCategory}");
                    GUILayout.Space(4);
                    using(new GUILayout.VerticalScope(EditorStyles.label, GUILayout.Height(136)))
                    {
                        scrollDesc = GUILayout.BeginScrollView(scrollDesc);
                        GUILayout.Label(selected.description, Styles.labelWW);
                        GUILayout.EndScrollView();
                    }
                }
            }
            GUILayout.Space(4);
            EditorGUI.DrawRect(new Rect(0, 448, 800, 1), Color.black);
            using (new GUILayout.HorizontalScope(GUILayout.Height(24)))
            {
                GUILayout.Space(8);
                createGameObject = GUILayout.Toggle(createGameObject, "Create Game Object");

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(selectedSource == null);

                if (GUILayout.Button("Create", GUILayout.Width(80),GUILayout.Height(22)))
                {
                    string sourceAssetPath = AssetDatabase.GetAssetPath(selectedSource);
                    AssetDatabase.CopyAsset(sourceAssetPath, outPath);
                    var asset = AssetDatabase.LoadAssetAtPath(outPath, typeof(VisualEffectAsset));
                    ProjectWindowUtil.ShowCreatedAsset(asset);

                    VFXViewWindow window = EditorWindow.GetWindow<VFXViewWindow>();

                    if (createGameObject)
                    {
                        var go = new GameObject();
                        go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 4;
                        go.name = asset.name;
                        var vfx = go.AddComponent<VisualEffect>();
                        vfx.visualEffectAsset = asset as VisualEffectAsset;
                        Selection.activeGameObject = go;
                        window.LoadAsset(asset as VisualEffectAsset, vfx );
                    }
                    else
                    {
                        window.LoadAsset(asset as VisualEffectAsset, null);
                    }
                    Close();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(8);

            }
        }
        class Contents
        {
            public static Texture vfxAssetIcon;
            public static Texture vfxCompIcon;
            public static Texture sceneIcon;
            public static Texture gameObjectIcon;
            public static Texture rendererIcon;
            public static Texture emptyPreview;

            public static GUIContent playIcon;
            public static GUIContent pauseIcon;
            public static GUIContent restartIcon;
            public static GUIContent stepIcon;

            public static GUIContent rendererOnIcon;
            public static GUIContent rendererOffIcon;

            public static GUIContent VFXIcon;


            public static GUIContent none;
            public static GUIContent culled;
            public static GUIContent active;
            public static GUIContent reseed;

            static Dictionary<uint, GUIContent> seeds;
            static Dictionary<string, GUIContent> cached;

            public static GUIContent Seed(uint seed)
            {
                if (!seeds.ContainsKey(seed))
                    seeds.Add(seed, new GUIContent(seed.ToString()));

                return seeds[seed];
            }

            public static GUIContent Cache(string label, Texture t = null)
            {
                if (!cached.ContainsKey(label))
                    cached.Add(label, new GUIContent(label, t));

                return cached[label];
            }

            static Contents()
            {
                seeds = new Dictionary<uint, GUIContent>();
                cached = new Dictionary<string, GUIContent>();

                vfxAssetIcon = EditorGUIUtility.IconContent("VisualEffectAsset Icon").image;
                vfxCompIcon = EditorGUIUtility.IconContent("VisualEffect Icon").image;
                sceneIcon = EditorGUIUtility.IconContent("UnityLogo").image;
                gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
                rendererIcon = EditorGUIUtility.IconContent("SceneViewVisibility").image;


                playIcon = EditorGUIUtility.IconContent("PlayButton On");
                pauseIcon = EditorGUIUtility.IconContent("PauseButton On");
                restartIcon = EditorGUIUtility.IconContent("preAudioAutoPlayOff");
                stepIcon = EditorGUIUtility.IconContent("StepButton On");

                rendererOnIcon = EditorGUIUtility.IconContent("animationvisibilitytoggleon");
                rendererOffIcon = EditorGUIUtility.IconContent("animationvisibilitytoggleoff");

                VFXIcon = EditorGUIUtility.IconContent("VisualEffectAsset Icon");

                emptyPreview = (Texture2D)EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Gallery/EmptyPreview.png");

                none = new GUIContent("");
                culled = new GUIContent("Culled");
                active = new GUIContent("Active");
                reseed = new GUIContent("# RESEED #");
            }

        }

        class Styles
        {
            public static GUIStyle header;

            public static GUIStyle toolbarButton;
            public static GUIStyle toolbarButtonBold;
            public static GUIStyle toolbarButtonRight;

            public static GUIStyle bigLabel;
            public static GUIStyle midLabel;
            public static GUIStyle rightLabel;
            public static GUIStyle labelCenter;
            public static GUIStyle labelWW;

            public static GUIStyle galButton;

            static Styles()
            {
                header = new GUIStyle(EditorStyles.label);
                header.padding = new RectOffset(2, 8, 8, 8);

                toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButton.alignment = TextAnchor.MiddleLeft;

                toolbarButtonRight = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButtonRight.alignment = TextAnchor.MiddleRight;

                toolbarButtonBold = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButtonBold.alignment = TextAnchor.MiddleLeft;
                toolbarButtonBold.fontStyle = FontStyle.Bold;

                bigLabel = new GUIStyle(EditorStyles.boldLabel);
                bigLabel.fontSize = 18;

                midLabel = new GUIStyle(EditorStyles.boldLabel);
                midLabel.fontSize = 12;

                rightLabel = new GUIStyle(EditorStyles.label);
                rightLabel.alignment = TextAnchor.MiddleRight;

                labelCenter = new GUIStyle(EditorStyles.label);
                labelCenter.alignment = TextAnchor.MiddleCenter;

                labelWW = new GUIStyle(EditorStyles.label);
                labelWW.wordWrap = true;

                galButton = new GUIStyle(EditorStyles.miniButton);
                galButton.fixedHeight = 0;
            }
        }

    }
}

