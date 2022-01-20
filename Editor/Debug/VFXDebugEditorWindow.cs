using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.VFX.UI;
using UnityEngine.Profiling;
using UnityEditor.SceneManagement;

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

            if (entries == null)
                entries = new List<VFXDebug.DebugEntry>();
            
            Reload(true);
            EditorSceneManager.sceneClosed += EditorSceneManager_sceneClosed;
            EditorSceneManager.sceneLoaded += EditorSceneManager_sceneLoaded;
        }

        private void EditorSceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            entries.Clear();
        }

        private void EditorSceneManager_sceneClosed(Scene scene)
        {
            entries.Clear();
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneClosed -= EditorSceneManager_sceneClosed;
            EditorSceneManager.sceneLoaded -= EditorSceneManager_sceneLoaded;
        }

        List<VFXDebug.DebugEntry> entries;

        Vector2 scroll;

        private void OnGUI()
        {
            Profiler.BeginSample("VFXDebugWindow.Reload");
            Reload();
            Profiler.EndSample();

            float ms = 16.6f;

            using (new GUILayout.HorizontalScope(Styles.header, GUILayout.Height(64)))
            {
                GUILayout.Box(Contents.VFXIcon, EditorStyles.label, GUILayout.Height(64));
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(Contents.Cache("VFXGraph Scene Debug"), Styles.bigLabel);
                    GUILayout.Label(Contents.Cache("Profiling : Editor"), EditorStyles.label);
                }
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label($"", Styles.rightLabel);
                    GUILayout.FlexibleSpace();
                }
            }

            EditorGUI.DrawRect(new Rect(0, 82, position.width, 1), Color.black);

            Profiler.BeginSample("VFXDebugWindow.DrawToolbar");
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(Contents.Cache("Group By:"), Styles.toolbarButtonBold);
                groupByScene = GUILayout.Toggle(groupByScene, Contents.Cache("Scene"), Styles.toolbarButton);
                groupByAsset = GUILayout.Toggle(groupByAsset, Contents.Cache("Asset"), Styles.toolbarButton);

                GUILayout.Space(32);

                GUILayout.Label("Hide:", Styles.toolbarButtonBold);
                filterCulled = GUILayout.Toggle(filterCulled, Contents.Cache("Culled"), Styles.toolbarButton);
                filterInactive = GUILayout.Toggle(filterInactive, Contents.Cache("Inactive"), Styles.toolbarButton);
                filterEmptyGroups = GUILayout.Toggle(filterEmptyGroups, Contents.Cache("EmptyGroups"), Styles.toolbarButton);
                filterPrefabAssets = GUILayout.Toggle(filterPrefabAssets, Contents.Cache("Prefabs"), Styles.toolbarButton);

                GUILayout.Space(32);

                filter = GUILayout.TextField(filter, EditorStyles.toolbarSearchField, GUILayout.Width(280));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Contents.restartIcon, Styles.toolbarButton))
                {
                    entries.Clear();
                    Reload(true);
                }
            }
            Profiler.EndSample();


            scroll = EditorGUILayout.BeginScrollView(scroll);

            int i = 0;

            string currentScene = string.Empty;
            VisualEffectAsset currentAsset = null;

            foreach (var entry in entries)
            {
                // Skip Invalid
                if (!entry.valid)
                    continue;

                if (filterPrefabAssets && string.IsNullOrEmpty(entry.sceneName))
                    continue;

                GUI.backgroundColor = Color.white;

                Profiler.BeginSample("VFXDebugWindow.DrawSceneHeader");
                // Display group header if new scene
                if (groupByScene && currentScene != entry.sceneName)
                {
                    currentScene = entry.sceneName;
                    GUI.backgroundColor = new Color(.6f, .6f, .6f, 1.0f);
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        if (string.IsNullOrEmpty(currentScene))
                            GUILayout.Label(Contents.Cache($"-- IN PREFABS --", Contents.sceneIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));

                        else
                            GUILayout.Label(Contents.Cache($"{currentScene}", Contents.sceneIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));
                    }
                }
                Profiler.EndSample();

                bool filterString = !string.IsNullOrEmpty(filter);

                Profiler.BeginSample("VFXDebugWindow.DrawAssetHeader");
                // Display group header if new asset
                if (groupByAsset && currentAsset != entry.asset)
                {
                    currentAsset = entry.asset;
                    GUI.backgroundColor = new Color(.8f, .8f, .8f, 1.0f);

                    if (Selection.activeObject == currentAsset) GUI.backgroundColor *= new Color(1.5f, 1.2f, 0.8f, 1.0f);

                    var group = entries.Where(o => o.asset == currentAsset);

                    if (groupByScene)
                        group = group.Where(o => o.sceneName == currentScene);

                    int count = group.Count();
                    int actCount = group.Where(o => o.active).Count();
                    int cullCount = group.Where(o => o.culled).Count();

                    if (!(filterEmptyGroups
                        && group.Where(o => (filterCulled ? o.culled == false : true))
                        .Where(o => (filterInactive ? o.active == true : true))
                        .Where(o => (filterString ? ContainsFilterString(o, filter) : true))
                        .Count() == 0))
                    {
                        using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                        {
                            if (groupByScene)
                                GUILayout.Space(24);

                            GUILayout.Label(Contents.Cache($" {currentAsset.name} - {count} instances, {actCount} active, {cullCount} culled", Contents.vfxCompIcon), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));
                        }
                    }
                }
                Profiler.EndSample();


                if (filterCulled && entry.culled) continue;
                if (filterInactive && !entry.active) continue;

                Profiler.BeginSample("VFXDebugWindow.FilterString");
                if (filterString && !ContainsFilterString(entry, filter)) continue;
                Profiler.EndSample();

                float f = i % 2 == 0 ? 1.0f : 1.2f;
                GUI.backgroundColor = new Color(f, f, f, 1.0f);
                i++;

                if (groupByAsset && Selection.activeObject == currentAsset)
                    GUI.backgroundColor *= new Color(1.5f, 1.2f, 0.8f, 1.0f);
                else if (Selection.activeGameObject == entry.gameObject)
                    GUI.backgroundColor = new Color(0.6f, 1.4f, 2.0f, 1.0f);

                Profiler.BeginSample("VFXDebugWindow.DrawItem");

                Rect line = GUILayoutUtility.GetRect(Contents.none, EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                {
                    GUI.Box(line, Contents.none, EditorStyles.toolbarButton);

                    if (groupByScene) line.xMin += 24;
                    if (groupByAsset) line.xMin += 24;

                    Rect r = line;
                    r.width = 24;

                    bool b = GUI.Toggle(r, entry.gameObject.activeSelf, "", EditorStyles.toggle);
                    if (entry.gameObject.activeSelf != b)
                        entry.gameObject.SetActive(b);

                    r.xMin = r.xMax;
                    r.width = 280 - 18;

                    if (GUI.Button(r, new GUIContent(entry.name, Contents.gameObjectIcon), Styles.toolbarButton))
                    {
                        if (Selection.activeGameObject == entry.gameObject)
                            Selection.activeObject = null;
                        else
                            Selection.activeGameObject = entry.gameObject;
                    }

                    r.xMin = r.xMax; r.width = 18;

                    if (GUI.Button(r, "F", Styles.toolbarButton))
                    {
                        SceneView.lastActiveSceneView.Frame(entry.renderer.bounds);
                    }

                    r.xMin = r.xMax + 25; r.width = 80;
                    GUI.Label(r, $"{(entry.active ? entry.aliveCount : 0)} p.", Styles.toolbarButtonRight);
                    r.xMin = r.xMax; r.width = 56;
                    GUI.Label(r, entry.culled ? Contents.culled : Contents.none, Styles.toolbarButton);
                    r.xMin = r.xMax; r.width = 56;
                    GUI.Label(r, entry.active ? Contents.active : Contents.none, Styles.toolbarButton);
                    r.xMin = r.xMax; r.width = 80;
                    GUI.Label(r, entry.resetSeedOnPlay ? Contents.reseed : Contents.Seed(entry.seed), Styles.toolbarButton);
                    r.xMin = r.xMax; r.width = 80;
                    GUI.Label(r, Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, entry.position).ToString("F2"), Styles.toolbarButtonRight);

                    r.xMin = r.xMax; r.width = 32;
                    if (GUI.Button(r, entry.paused ? Contents.pauseIcon : Contents.playIcon, EditorStyles.toolbarButton))
                        entry.TogglePause();

                    r.xMin = r.xMax; r.width = 32;
                    if (GUI.Button(r, Contents.restartIcon, EditorStyles.toolbarButton))
                        entry.Restart();

                    r.xMin = r.xMax; r.width = 32;
                    if (GUI.Button(r, Contents.stepIcon, EditorStyles.toolbarButton))
                        entry.Step();

                    r.xMin = r.xMax; r.width = 32;
                    if (GUI.Button(r, entry.rendered ? Contents.rendererOnIcon : Contents.rendererOffIcon, EditorStyles.toolbarButton))
                        entry.ToggleRendered();


                    float m = r.xMax;

                    if(m < line.width - 180)
                    {
                        r = line;
                        r.xMin = r.xMax - 220;
                        r.width = 180;

                        if (GUI.Button(r, new GUIContent(entry.asset.name, Contents.vfxAssetIcon), Styles.toolbarButton))
                            Selection.activeObject = entry.asset;

                        r.xMin = r.xMax; r.width = 40;
                        if (GUI.Button(r, "Edit", Styles.toolbarButton))
                        {
                            if (VFXViewWindow.currentWindow == null)
                                GetWindow<VFXViewWindow>();

                            VFXViewWindow.currentWindow.LoadAsset(entry.asset, entry.component);
                        }
                    }

                }
                Profiler.EndSample();
            }

            GUILayout.Space(80);

            EditorGUILayout.EndScrollView();


        }

        void Reload(bool deepSearch = false)
        {
            Profiler.BeginSample("VFXDebug.UpdateAll");
            VFXDebug.UpdateAll(ref entries, deepSearch);
            Profiler.EndSample();

            Profiler.BeginSample("VFXDebug.SortByDistanceTo");
            if(SceneView.lastActiveSceneView != null)
                VFXDebug.SortByDistanceTo(SceneView.lastActiveSceneView.camera.transform.position, ref entries);
            else
                VFXDebug.SortByDistanceTo(Vector3.zero, ref entries);
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
