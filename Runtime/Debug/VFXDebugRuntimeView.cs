using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.VFX.DebugTools
{
    public class VFXDebugRuntimeView : MonoBehaviour
    {
        public static VFXDebugRuntimeView instance => s_Instance;
        static VFXDebugRuntimeView s_Instance;

        public event VFXDebugDelegate onDebugVisibilityChange;
        public delegate void VFXDebugDelegate(bool visible);

        [SerializeField]
        GUISkin guiSkin;

        [SerializeField]
        bool handleCursorVisibility;

        bool visible = false;

        void Awake()
        {
            entries = new List<VFXDebug.DebugEntry>();
            s_Instance = this;
        }

        private void OnDestroy()
        {
            s_Instance = null;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if(InputSystem.Keyboard.current.f7Key.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.F7))
#endif
                visible = !visible;

            if(handleCursorVisibility)
            {
                Cursor.visible = visible;
                Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            }

            onDebugVisibilityChange?.Invoke(visible);
        }


        bool groupByScene
        {
            get => PlayerPrefs.GetInt("VFXDebug.groupByScene", 0) == 1;
            set => PlayerPrefs.SetInt("VFXDebug.groupByScene", value ? 1 : 0);
        }

        bool groupByAsset
        {
            get => PlayerPrefs.GetInt("VFXDebug.groupByAsset", 0) == 1;
            set => PlayerPrefs.SetInt("VFXDebug.groupByAsset", value ? 1 : 0);
        }

        bool filterCulled
        {
            get => PlayerPrefs.GetInt("VFXDebug.filterCulled", 0) == 1;
            set => PlayerPrefs.SetInt("VFXDebug.filterCulled", value ? 1 : 0);
        }
        bool filterInactive
        {
            get => PlayerPrefs.GetInt("VFXDebug.filterInactive", 0) == 1;
            set => PlayerPrefs.SetInt("VFXDebug.filterInactive", value ? 1 : 0);
        }

        bool filterEmptyGroups
        {
            get => PlayerPrefs.GetInt("VFXDebug.filterEmptyGroups", 0) == 1;
            set => PlayerPrefs.SetInt("VFXDebug.filterEmptyGroups", value ? 1 : 0);
        }

        string filterText;

        List<VFXDebug.DebugEntry> entries;
        Vector2 scroll;

        private void OnGUI()
        {
            if (!visible)
                return;

            Rect r = new Rect(32, 32, Mathf.Min(Camera.main.pixelWidth,1280) - 64, Camera.main.pixelHeight - 64);
            GUI.Box(r, "", guiSkin.box);
            r = new RectOffset(12, 12, 12, 12).Remove(r);
            using (new GUILayout.AreaScope(r))
            {
                GUILayout.Label("VFX Scene Debug", guiSkin.GetStyle("title"));

                using(new GUILayout.VerticalScope(guiSkin.box, GUILayout.ExpandWidth(true)))
                {
                    using(new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.HorizontalScope(guiSkin.box))
                        {
                            GUILayout.Label("  Group By : ", guiSkin.GetStyle("bold"));
                            groupByAsset = GUILayout.Toggle(groupByAsset, "Asset  ", guiSkin.toggle);
                            groupByScene = GUILayout.Toggle(groupByScene, "Scene  ", guiSkin.toggle);

                        }

                        using (new GUILayout.HorizontalScope(guiSkin.box))
                        {
                            GUILayout.Label("  Filter Out : ", guiSkin.GetStyle("bold"));
                            filterCulled = GUILayout.Toggle(filterCulled, "Culled  ", guiSkin.toggle);
                            filterInactive = GUILayout.Toggle(filterInactive, "Inactive  ", guiSkin.toggle);
                            filterEmptyGroups = GUILayout.Toggle(filterEmptyGroups, "Empty Groups  ", guiSkin.toggle);
                        }
                        GUILayout.FlexibleSpace();
                    }

                    using (new GUILayout.HorizontalScope(guiSkin.box))
                    {
                        GUILayout.Label("Find : ", guiSkin.GetStyle("bold"), GUILayout.Width(80));
                        filterText = GUILayout.TextField(filterText, GUILayout.ExpandWidth(true));
                    }
                }

                scroll = GUILayout.BeginScrollView(scroll);

                int i = 0;
                string currentScene = string.Empty;
                VisualEffectAsset currentAsset = null;

                foreach (var entry in entries)
                {
                    // Skip Invalid
                    if (!entry.valid)
                        continue;

                    GUI.backgroundColor = Color.white;


                    // Display group header if new scene
                    if (groupByScene && currentScene != entry.sceneName)
                    {
                        currentScene = entry.sceneName;
                        using (new GUILayout.HorizontalScope(guiSkin.box))
                        {
                            GUILayout.Label($"Scene: {currentScene}", guiSkin.GetStyle("bold"), GUILayout.ExpandWidth(true));
                        }
                    }

                    bool hasStringFilter = !string.IsNullOrEmpty(filterText);

                    // Display group header if new asset
                    if (groupByAsset && currentAsset != entry.asset)
                    {
                        currentAsset = entry.asset;
                        GUI.backgroundColor = new Color(.8f, .8f, .8f, 1.0f);
                        
                        var group = entries.Where(o => o.asset == currentAsset);

                        if (groupByScene)
                            group = group.Where(o => o.sceneName == currentScene);

                        int count = group.Count();
                        int actCount = group.Where(o => o.active).Count();
                        int cullCount = group.Where(o => o.culled).Count();

                        if (!(filterEmptyGroups
                            && group.Where(o => (filterCulled ? o.culled == false : true))
                            .Where(o => (filterInactive ? o.active == true : true))
                            .Where(o => (hasStringFilter ? ContainsFilterString(o, filterText) : true))
                            .Count() == 0))
                        {
                            using (new GUILayout.HorizontalScope(guiSkin.box))
                            {
                                if (groupByScene)
                                    GUILayout.Space(24);

                                GUILayout.Label($" {currentAsset.name} - {count} instances, {actCount} active, {cullCount} culled", guiSkin.GetStyle("bold"), GUILayout.ExpandWidth(true));
                            }
                        }
                    }

                    if (filterCulled && entry.culled) continue;
                    if (filterInactive && !entry.active) continue;

                    if (hasStringFilter && !ContainsFilterString(entry, filterText)) continue;

                    float f = i % 2 == 0 ? 1.0f : 1.2f;
                    GUI.backgroundColor = new Color(f, f, f, 1.0f);
                    i++;


                    Rect line = GUILayoutUtility.GetRect(GUIContent.none, guiSkin.box, GUILayout.ExpandWidth(true));
                    {
                        GUI.Box(line, "");

                        if (groupByScene) line.xMin += 24;
                        if (groupByAsset) line.xMin += 24;

                        r = line;
                        r.width = 24;

                        bool b = GUI.Toggle(r, entry.gameObject.activeSelf, "", guiSkin.toggle);
                        if (entry.gameObject.activeSelf != b)
                            entry.gameObject.SetActive(b);

                        r.xMin = r.xMax;
                        r.width = 280 - 18;

                        GUI.Label(r, entry.name);


                        r.xMin = r.xMax + 25; r.width = 80;
                        GUI.Label(r, $"{(entry.active ? entry.aliveCount : 0)} p.");
                        r.xMin = r.xMax; r.width = 56;
                        GUI.Label(r, entry.culled ? "Culled" : "");
                        r.xMin = r.xMax; r.width = 56;
                        GUI.Label(r, entry.active ? "Active" : "");
                        r.xMin = r.xMax; r.width = 80;
                        GUI.Label(r, entry.resetSeedOnPlay ? "#RESEED#" : entry.seed.ToString());
                        r.xMin = r.xMax; r.width = 80;
                        GUI.Label(r, Vector3.Distance(Camera.main != null ? Camera.main.transform.position : Vector3.zero, entry.position).ToString("F2"));

                        r.xMin = r.xMax; r.width = 32;
                        if (GUI.Button(r, entry.paused ? "׀׀" : "►"))
                            entry.TogglePause();

                        r.xMin = r.xMax; r.width = 32;
                        if (GUI.Button(r, "«"))
                            entry.Restart();

                        r.xMin = r.xMax; r.width = 32;
                        if (GUI.Button(r, "►׀"))
                            entry.Step();

                        r.xMin = r.xMax; r.width = 80;
                        if (GUI.Button(r, entry.rendered?"Render On" : "Render Off"))
                            entry.ToggleRendered();

                        r.xMin = r.xMax; r.width = 180;
                        GUI.Label(r, entry.asset.name);
                    }
                }

                GUILayout.EndScrollView();
                
            }
            Reload();
        }

        void Reload(bool deepSearch = false)
        {
            VFXDebug.UpdateAll(ref entries, deepSearch);

            if (Camera.main != null)
                VFXDebug.SortByDistanceTo(Camera.main.transform.position, ref entries);
            else
                VFXDebug.SortByDistanceTo(Vector3.zero, ref entries);

            if (groupByAsset)
            {
                VFXDebug.SortByAsset(ref entries);
            }

            if (groupByScene)
            {
                VFXDebug.SortByScene(ref entries);
            }
        }

        bool ContainsFilterString(VFXDebug.DebugEntry e, string filter)
        {
            var f = filter.ToLowerInvariant();
            return e.name.ToLowerInvariant().Contains(f) || e.asset.name.ToLowerInvariant().Contains(f);
        }


    }
}
