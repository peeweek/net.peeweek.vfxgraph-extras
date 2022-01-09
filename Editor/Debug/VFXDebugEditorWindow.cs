using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.VFX.UI;
using UnityEngine.Profiling;

namespace UnityEngine.VFX.DebugTools
{
    public class VFXDebugEditorWindow : EditorWindow
    {
        public static VFXDebugEditorWindow instance 
        { 
            get 
            { 
                if (s_Instance == null) 
                    Open(); 
                return s_Instance; 
            } 
        }

        bool groupByScene { 
            get => EditorPrefs.GetBool("VFXDebug.groupByScene", false);
            set => EditorPrefs.SetBool("VFXDebug.groupByScene", value);
        }

        bool groupByAsset
        {
            get => EditorPrefs.GetBool("VFXDebug.groupByAsset", true);
            set => EditorPrefs.SetBool("VFXDebug.groupByAsset", value);
        }

        bool filterCulled
        {
            get => EditorPrefs.GetBool("VFXDebug.filterCulled", false);
            set => EditorPrefs.SetBool("VFXDebug.filterCulled", value);
        }
        bool filterInactive
        {
            get => EditorPrefs.GetBool("VFXDebug.filterInactive", false);
            set => EditorPrefs.SetBool("VFXDebug.filterInactive", value);
        }

        bool filterEmptyGroups
        {
            get => EditorPrefs.GetBool("VFXDebug.filterEmptyGroups", false);
            set => EditorPrefs.SetBool("VFXDebug.filterEmptyGroups", value);
        }

        bool filterPrefabAssets
        {
            get => EditorPrefs.GetBool("VFXDebug.filterPrefabAssets", false);
            set => EditorPrefs.SetBool("VFXDebug.filterPrefabAssets", value);
        }


        string filter = string.Empty;

        static VFXDebugEditorWindow s_Instance;

        [MenuItem("Window/Analysis/VFX Graph Debug")]
        static void Open()
        {
            s_Instance = GetWindow<VFXDebugEditorWindow>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("VFXGraph Debug");
            minSize = new Vector2(440, 280);
            autoRepaintOnSceneChange = true;
        }

        List<VFXDebug.DebugEntry> entries;


        Vector2 scroll;



        private void OnGUI()
        {
            if (entries == null)
                Reload();

            using(new GUILayout.HorizontalScope(Styles.header, GUILayout.Height(64)))
            {
                GUILayout.Box(Contents.VFXIcon, EditorStyles.label, GUILayout.Height(64));
                using(new GUILayout.VerticalScope())
                {
                    GUILayout.Label("VFXGraph Debug", Styles.bigLabel);
                }
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("FPS: XX.X", Styles.rightLabel);
                }
            }

            EditorGUI.DrawRect(new Rect(0, 82, position.width, 1), Color.black);

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Group By:", Styles.toolbarButtonBold);
                groupByScene = GUILayout.Toggle(groupByScene, "Scene", Styles.toolbarButton);
                groupByAsset = GUILayout.Toggle(groupByAsset, "Asset", Styles.toolbarButton);

                GUILayout.Space(32);

                GUILayout.Label("Hide:", Styles.toolbarButtonBold);
                filterCulled = GUILayout.Toggle(filterCulled, "Culled", Styles.toolbarButton);
                filterInactive = GUILayout.Toggle(filterInactive, "Inactive", Styles.toolbarButton);
                filterEmptyGroups = GUILayout.Toggle(filterEmptyGroups, "EmptyGroups", Styles.toolbarButton);
                filterPrefabAssets = GUILayout.Toggle(filterPrefabAssets, "Prefabs", Styles.toolbarButton);

                GUILayout.Space(32);

                filter = GUILayout.TextField(filter, EditorStyles.toolbarSearchField, GUILayout.Width(280));

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Deep Search", Styles.toolbarButton))
                    Reload(true);
                if (GUILayout.Button("...", EditorStyles.toolbarButton)) ;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            int i = 0;

            string currentScene = string.Empty;
            VisualEffectAsset currentAsset = null;

            foreach(var entry in entries)
            {
                // Skip Invalid
                if (!entry.valid)
                    continue;

                if (filterPrefabAssets && string.IsNullOrEmpty(entry.sceneName))
                    continue;


                // Display group header if new scene
                if(groupByScene && currentScene != entry.sceneName)
                {
                    currentScene = entry.sceneName;
                    GUI.backgroundColor = new Color(.6f, .6f, .6f, 1.0f);
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        if(string.IsNullOrEmpty(currentScene))
                            GUILayout.Label(new GUIContent($"-- IN PREFABS --", Contents.sceneIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));

                        else
                            GUILayout.Label(new GUIContent($"{currentScene}", Contents.sceneIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));
                    }
                }

                bool filterString = !string.IsNullOrEmpty(filter);

                // Display group header if new asset
                if (groupByAsset && currentAsset != entry.asset)
                {
                    currentAsset = entry.asset;
                    GUI.backgroundColor = new Color(.8f, .8f, .8f, 1.0f);

                    if(Selection.activeObject == currentAsset) GUI.backgroundColor *= new Color(1.5f,1.2f,0.8f, 1.0f);

                    var group = entries.Where(o => o.asset == currentAsset);

                    if (groupByScene)
                        group = group.Where(o => o.sceneName == currentScene);


                    int count = group.Count();
                    int actCount = group.Where(o => o.active).Count();
                    int cullCount = group.Where(o => o.culled).Count();

                    if (!(filterEmptyGroups 
                        && group.Where( o => (filterCulled ? o.culled == false : true))
                        .Where(o => (filterInactive ? o.active == true : true))
                        .Where(o => (filterString ? ContainsFilterString(o, filter) : true))
                        .Count() == 0))
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        if (groupByScene)
                            GUILayout.Space(24);

                        GUILayout.Label(new GUIContent($" {currentAsset.name} - {count} instances, {actCount} active, {cullCount} culled", Contents.vfxCompIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));
                    }
                }

                if (filterCulled && entry.culled) continue;
                if (filterInactive && !entry.active) continue;
                if (filterString && !ContainsFilterString(entry, filter)) continue;

                float f = i % 2 == 0 ? 1.0f : 1.2f;
                GUI.backgroundColor = new Color(f, f, f, 1.0f);
                i++;

                if (Selection.activeObject == currentAsset) 
                    GUI.backgroundColor *= new Color(1.5f, 1.2f, 0.8f, 1.0f);
                else if (Selection.activeGameObject == entry.gameObject) 
                    GUI.backgroundColor *= new Color(0.8f, 1.2f, 1.8f, 1.0f);

                Profiler.BeginSample("VFXDebugWindow.DrawItem");

                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    if (groupByScene) GUILayout.Space(24);
                    if (groupByAsset) GUILayout.Space(24);


                    bool b = GUILayout.Toggle(entry.gameObject.activeSelf, "", EditorStyles.toggle);
                    if(entry.gameObject.activeSelf != b)
                        entry.gameObject.SetActive(b);

                    if (GUILayout.Button(new GUIContent(entry.name, Contents.gameObjectIcon), Styles.toolbarButton, GUILayout.Width(280-18)))
                    {
                        if (Selection.activeGameObject == entry.gameObject)
                            Selection.activeObject = null;
                        else
                            Selection.activeGameObject = entry.gameObject;
                    }

                    if (GUILayout.Button("F", Styles.toolbarButton, GUILayout.Width(18)))
                    {
                        SceneView.lastActiveSceneView.Frame(entry.renderer.bounds);
                    }
                    GUILayout.Space(25);
                    GUILayout.Label($"{(entry.active ? entry.aliveCount : 0)} p.", Styles.toolbarButtonRight, GUILayout.Width(80));
                    GUILayout.Label(entry.culled ? Contents.culled : Contents.none, Styles.toolbarButton, GUILayout.Width(56));
                    GUILayout.Label(entry.active ? Contents.active : Contents.none, Styles.toolbarButton, GUILayout.Width(56));
                    GUILayout.Label(entry.resetSeedOnPlay ? Contents.reseed : Contents.Seed(entry.seed), Styles.toolbarButton, GUILayout.Width(80));
                    GUILayout.Label(Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, entry.position).ToString("F2"), Styles.toolbarButtonRight, GUILayout.Width(80));

                    if(GUILayout.Button(entry.paused? Contents.pauseIcon : Contents.playIcon, EditorStyles.toolbarButton, GUILayout.Width(32)))
                        entry.TogglePause();

                    if (GUILayout.Button(Contents.restartIcon, EditorStyles.toolbarButton, GUILayout.Width(32)))
                        entry.Restart();

                    if (GUILayout.Button(Contents.stepIcon, EditorStyles.toolbarButton, GUILayout.Width(32)))
                        entry.Step();

                    if (GUILayout.Button(entry.rendered ? Contents.rendererOnIcon : Contents.rendererOffIcon, EditorStyles.toolbarButton, GUILayout.Width(32)))
                        entry.ToggleRendered();


                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent(entry.asset.name, Contents.vfxAssetIcon), Styles.toolbarButton, GUILayout.Width(180)))
                        Selection.activeObject = entry.asset;
                    if (GUILayout.Button("Open", Styles.toolbarButton))
                    {
                        if (VFXViewWindow.currentWindow == null)
                            GetWindow<VFXViewWindow>();
                            
                        VFXViewWindow.currentWindow.LoadAsset(entry.asset, entry.component);
                    }    
                }
                Profiler.EndSample();
            }

            GUILayout.Space(80);

            EditorGUILayout.EndScrollView();

            
            Reload();
        }

        void Reload(bool deepSearch = false)
        {
            Profiler.BeginSample("VFXDebug.UpdateAll");
            VFXDebug.UpdateAll(ref entries, deepSearch);
            Profiler.EndSample();

            Profiler.BeginSample("VFXDebug.SortByDistanceTo");
            VFXDebug.SortByDistanceTo(SceneView.lastActiveSceneView.camera.transform.position, ref entries);
            Profiler.EndSample();

            if (groupByAsset)
            {
                Profiler.BeginSample("VFXDebug.SortByAsset");
                VFXDebug.SortByAsset(ref entries);
                Profiler.EndSample();
            }


            if (groupByScene)
            {
                Profiler.BeginSample("VFXDebug.SortByScene");
                VFXDebug.SortByScene(ref entries);
                Profiler.EndSample();
            }
        }

        bool ContainsFilterString(VFXDebug.DebugEntry e, string filter)
        {
            var f = filter.ToLowerInvariant();
            return e.name.ToLowerInvariant().Contains(f) || e.asset.name.ToLowerInvariant().Contains(f);
        }

        class Contents
        {
            public static Texture vfxAssetIcon;
            public static Texture vfxCompIcon;
            public static Texture sceneIcon;
            public static Texture gameObjectIcon;
            public static Texture rendererIcon;

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


            public static GUIContent Seed(uint seed)
            {
                if (!seeds.ContainsKey(seed))
                    seeds.Add(seed, new GUIContent(seed.ToString()));

                return seeds[seed];
            }

            static Contents()
            {
                seeds = new Dictionary<uint, GUIContent>();

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
            public static GUIStyle rightLabel;

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

                rightLabel = new GUIStyle(EditorStyles.label);
                rightLabel.alignment = TextAnchor.MiddleRight;

            }
        }
    }
}
