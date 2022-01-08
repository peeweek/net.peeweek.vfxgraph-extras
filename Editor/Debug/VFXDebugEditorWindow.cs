using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.VFX.UI;

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


        static VFXDebugEditorWindow s_Instance;

        [MenuItem("Window/Analysis/VFX Graph Debug")]
        static void Open()
        {
            s_Instance = GetWindow<VFXDebugEditorWindow>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("VFXGraph Debug");
            minSize = new Vector2(440, 180);
            autoRepaintOnSceneChange = true;
        }

        List<VFXDebug.DebugEntry> entries;


        Vector2 scroll;



        private void OnGUI()
        {
            if (entries == null)
                Reload();

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Group By:", EditorStyles.toolbarButton);
                groupByScene = GUILayout.Toggle(groupByScene, "Scene", Styles.toolbarButton);
                groupByAsset = GUILayout.Toggle(groupByAsset, "Asset", Styles.toolbarButton);

                GUILayout.Space(64);

                GUILayout.Label("Hide:", EditorStyles.toolbarButton);
                filterCulled = GUILayout.Toggle(filterCulled, "Culled", Styles.toolbarButton);
                filterInactive = GUILayout.Toggle(filterInactive, "Inactive", Styles.toolbarButton);
                filterEmptyGroups = GUILayout.Toggle(filterEmptyGroups, "EmptyGroups", Styles.toolbarButton);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("...", EditorStyles.toolbarButton)) ;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            int i = 0;

            string currentScene = string.Empty;
            VisualEffectAsset currentAsset = null;

            foreach(var entry in entries)
            {

                if (!entry.valid)
                {
                    continue;
                }

                if(groupByScene && currentScene != entry.sceneName)
                {
                    currentScene = entry.sceneName;
                    GUI.backgroundColor = new Color(.5f, .5f, .5f, 1.0f);
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        GUILayout.Label(new GUIContent($"{currentScene}",Styles.scene), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));
                    }
                }

                if (groupByAsset && currentAsset != entry.asset)
                {
                    currentAsset = entry.asset;
                    GUI.backgroundColor = new Color(1.2f, 1.2f, 1.2f, 1.0f);

                    var group = entries.Where(o => o.asset == currentAsset);

                    if (groupByScene)
                        group = group.Where(o => o.sceneName == currentScene);


                    int count = group.Count();
                    int actCount = group.Where(o => o.active).Count();
                    int cullCount = group.Where(o => o.culled).Count();

                    if(!(filterEmptyGroups && group.Where( o => (filterCulled ? o.culled == false : true)).Where(o => (filterInactive ? o.active == true : true)).Count() == 0))
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        if (groupByScene)
                            GUILayout.Space(24);

                        GUILayout.Label(new GUIContent($" {currentAsset.name} - {count} instances, {actCount} active, {cullCount} culled", Styles.vfxComp), Styles.toolbarButtonBold, GUILayout.ExpandWidth(true));

                    }
                }

                if (filterCulled && entry.culled) continue;
                if (filterInactive && !entry.active) continue;

                float f = i % 2 == 0 ? 0.8f : 0.6f;
                GUI.backgroundColor = new Color(f, f, f, 1.0f);
                i++;

                using (new GUILayout.HorizontalScope(Styles.toolbarButton))
                {
                    if (groupByScene) GUILayout.Space(24);
                    if (groupByAsset) GUILayout.Space(24);


                    bool b = GUILayout.Toggle(entry.gameObject.activeSelf, "", EditorStyles.toggle);
                    if(entry.gameObject.activeSelf != b)
                        entry.gameObject.SetActive(b);

                    if (GUILayout.Button(new GUIContent(entry.name, Styles.gameObject), Styles.toolbarButton, GUILayout.Width(280-18)))
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
                    GUILayout.Label(entry.culled ? "Culled" : "", Styles.toolbarButton, GUILayout.Width(56));
                    GUILayout.Label(entry.active ? "Active" : "", Styles.toolbarButton, GUILayout.Width(56));
                    GUILayout.Label(entry.resetSeedOnPlay ? "#RESEED#" : entry.seed.ToString(), Styles.toolbarButton, GUILayout.Width(80));
                    GUILayout.Label(Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, entry.position).ToString("F2"), Styles.toolbarButtonRight, GUILayout.Width(80));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent(entry.asset.name,Styles.vfxAsset), Styles.toolbarButton, GUILayout.Width(180)))
                        Selection.activeObject = entry.asset;
                    if (GUILayout.Button("Open", Styles.toolbarButton))
                    {
                        if (VFXViewWindow.currentWindow == null)
                            GetWindow<VFXViewWindow>();
                            
                        VFXViewWindow.currentWindow.LoadAsset(entry.asset, entry.component);
                    }    
                } 
            }

            GUILayout.Space(80);

            EditorGUILayout.EndScrollView();

            Reload();
        }

        void Reload()
        {
            VFXDebug.UpdateAll(ref entries);

            VFXDebug.SortByDistanceTo(SceneView.lastActiveSceneView.camera.transform.position, ref entries);

            if (groupByAsset)
                VFXDebug.SortByAsset(ref entries);

            if (groupByScene)
                VFXDebug.SortByScene(ref entries);
        }

        class Styles
        {
            public static GUIStyle toolbarButton;
            public static GUIStyle toolbarButtonBold;
            public static GUIStyle toolbarButtonRight;

            public static Texture vfxAsset;
            public static Texture vfxComp;
            public static Texture scene;
            public static Texture gameObject;
            public static Texture renderer;

            static Styles()
            {
                toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButton.alignment = TextAnchor.MiddleLeft;

                toolbarButtonRight = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButtonRight.alignment = TextAnchor.MiddleRight;

                toolbarButtonBold = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButtonBold.alignment = TextAnchor.MiddleLeft;
                toolbarButtonBold.fontStyle = FontStyle.Bold;


                vfxAsset = EditorGUIUtility.IconContent("VisualEffectAsset Icon").image;
                vfxComp = EditorGUIUtility.IconContent("VisualEffect Icon").image;
                scene = EditorGUIUtility.IconContent("UnityLogo").image;
                gameObject = EditorGUIUtility.IconContent("GameObject Icon").image;
                renderer = EditorGUIUtility.IconContent("SceneViewVisibility").image;
            }
        }
    }
}
