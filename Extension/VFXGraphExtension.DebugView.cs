using UnityEditor.VFX.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.VFX;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;
using System.Text;

static partial class VFXGraphExtension //.DebugView
{
    static bool GetDebugInfoVisible(VisualEffectAsset asset)
    {
        return EditorPrefs.GetBool($"VFXGraphExtension.debugInfoVisible.{asset.name}", true);
    }

    static void SetDebugInfoVisible(VisualEffectAsset asset, bool visible)
    {
        EditorPrefs.SetBool($"VFXGraphExtension.debugInfoVisible.{asset.name}", visible);
    }

    static List<SpawnerDebugInfo> spawnersToDelete = new List<SpawnerDebugInfo>();
    static List<SystemDebugInfo> systemsToDelete = new List<SystemDebugInfo>();

    struct SpawnerDebugInfo
    {
        public VFXContextUI ui;
        public string name;
        public VisualElement debugPanel;
        public Label playLabel;
        public Label bigLabel;
        public Label timeLabel;
        public Label timeLabel2;
        public VisualElement progress;
        public Label label;
        public Label evtAttribute;

        public void ResyncName()
        {
            if (ui != null)
            {
                name = ui.Q<Label>("user-label").text;
            }
        }
    }

    struct SystemDebugInfo
    {
        public VisualElement panel;
        public VFXContextUI ui;
        public VFXContext model;
        public Label countLabel;
        public Label infoLabel;
        public VisualElement progress;
        public Label attribLabel;
        public Label memoryLabel;
    }

    static Dictionary<VFXViewWindow, List<SpawnerDebugInfo>> spawnerDebugInfos;
    static Dictionary<VFXViewWindow, List<SystemDebugInfo>> systemDebugInfos;
    static Dictionary<VFXViewWindow, VisualEffectAsset> currentVisualEffectAsset;

    static Dictionary<VisualEffectAsset, List<string>> systemNames = new Dictionary<VisualEffectAsset, List<string>>();
    static Dictionary<VisualEffectAsset, List<string>> spawnerNames = new Dictionary<VisualEffectAsset, List<string>>();

    static void ToggleSpawnerStats(object wnd)
    {
        VFXViewWindow window = (VFXViewWindow)wnd;
        var asset = window.displayedResource.asset;

        if(asset != null)
        {
            bool visible = !GetDebugInfoVisible(asset);
            SetDebugInfoVisible(asset, visible);
            UpdateStatsUIElements(window);
        }
    }

    static void UpdateStatsUIElements(VFXViewWindow window)
    {
        if (window == null)
            return;

        
        if (!window.HasLoadedAsset())
            return;

        if (spawnerDebugInfos == null)
            spawnerDebugInfos = new Dictionary<VFXViewWindow, List<SpawnerDebugInfo>>();

        if (systemDebugInfos == null)
            systemDebugInfos = new Dictionary<VFXViewWindow, List<SystemDebugInfo>>();

        if (currentVisualEffectAsset == null)
            currentVisualEffectAsset = new Dictionary<VFXViewWindow, VisualEffectAsset>();

        // Check if asset changed
        VisualEffectAsset currentAsset = window.GetLoadedAsset();

        // If window was not registered, register it
        if(!currentVisualEffectAsset.ContainsKey(window))
        {
            currentVisualEffectAsset.Add(window, currentAsset);
            spawnerDebugInfos.Add(window, new List<SpawnerDebugInfo>());
            systemDebugInfos.Add(window, new List<SystemDebugInfo>());
        }

        if (currentAsset != currentVisualEffectAsset[window])
        {
            spawnerDebugInfos[window].Clear();
            systemDebugInfos[window].Clear();
            currentVisualEffectAsset[window] = currentAsset;
        }

        // We use a GraphView Query here as it can be made during compilation
        // However at this stage we can't enumerate VFXContextUI from GetAllContexts() (yet)

        window.graphView.Query<VFXContextUI>().Build().ForEach((context) =>
        {
            if (context.IsModel<VFXBasicSpawner>())
            {
                string name = context.Q<Label>("user-label").text;

                if (spawnerDebugInfos[window].Any(o => o.ui == context))
                    return;

                SpawnerDebugInfo info = new SpawnerDebugInfo();
                info.name = name;

                var panel = context.Q("Spawner-Debug");
                if (panel == null)
                {
                    panel = new VisualElement()
                    {
                        name = "Spawner-Debug"
                    };
                    panel.style.position = UnityEngine.UIElements.Position.Absolute;
                    panel.style.top = 22;
                    panel.style.borderBottomLeftRadius = 8;
                    panel.style.borderBottomRightRadius = 8;
                    panel.style.borderTopLeftRadius = 8;
                    panel.style.borderTopRightRadius = 8;
                    panel.style.paddingBottom = 8;
                    panel.style.paddingLeft = 8;
                    panel.style.paddingRight = 8;
                    panel.style.paddingTop = 8;
                    panel.style.left = 432;
                    panel.style.height = 180;
                    panel.style.width = 260;
                    panel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 1.0f));

                    var playLabel = new Label("?");
                    playLabel.style.fontSize = 26;
                    playLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    playLabel.style.left = 8;
                    playLabel.style.width = 32;
                    playLabel.style.top = 8;
                    playLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    playLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

                    var bigLabel = new Label("");
                    bigLabel.style.fontSize = 28;
                    bigLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    bigLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                    bigLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    bigLabel.style.left = 48;
                    bigLabel.style.top = 8;
                    bigLabel.style.width = 200;

                    var timeLabel = new Label("10.0");
                    timeLabel.style.fontSize = 28;
                    timeLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
                    timeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    timeLabel.style.left = 48;
                    timeLabel.style.top = 8;
                    timeLabel.style.width = 90;

                    var timeLabel2 = new Label("/10.0");
                    timeLabel2.style.fontSize = 28;
                    timeLabel2.style.position = UnityEngine.UIElements.Position.Absolute;
                    timeLabel2.style.unityTextAlign = TextAnchor.MiddleLeft;
                    timeLabel2.style.unityFontStyleAndWeight = FontStyle.Bold;
                    timeLabel2.style.left = 138;
                    timeLabel2.style.top = 8;
                    timeLabel2.style.width = 110;

                    var label = new Label($"Debug Spawner Info: {info.name}");
                    label.style.fontSize = 12;
                    label.style.position = UnityEngine.UIElements.Position.Absolute;
                    label.style.top = 64;
                    label.style.left = 8;

                    var progressBG = new VisualElement();
                    progressBG.style.position = UnityEngine.UIElements.Position.Absolute;
                    progressBG.style.top = 48;
                    progressBG.style.left = 8;
                    progressBG.style.width = 240;
                    progressBG.style.height = 8;
                    progressBG.style.backgroundColor = new StyleColor(new Color(.3f, .3f, .3f, 1f));
                    progressBG.style.borderBottomLeftRadius = 3;
                    progressBG.style.borderBottomRightRadius = 3;
                    progressBG.style.borderTopLeftRadius = 3;
                    progressBG.style.borderTopRightRadius = 3;

                    var progress = new VisualElement();
                    progress.style.position = UnityEngine.UIElements.Position.Absolute;
                    progress.style.top = 48;
                    progress.style.left = 8;
                    progress.style.width = 240;
                    progress.style.height = 8;
                    progress.style.backgroundColor = new StyleColor(new Color(.3f, 1f, .3f, 1f));
                    progress.style.borderBottomLeftRadius = 3;
                    progress.style.borderBottomRightRadius = 3;
                    progress.style.borderTopLeftRadius = 3;
                    progress.style.borderTopRightRadius = 3;

                    var evtAttributeHeader = new Label("Event Attributes");
                    evtAttributeHeader.style.top = 90;
                    evtAttributeHeader.style.left = 3;
                    evtAttributeHeader.style.fontSize = 12;
                    evtAttributeHeader.style.unityFontStyleAndWeight = FontStyle.Bold;

                    var evtAttribute = new Label("(No Attributes)");
                    evtAttribute.style.top = 93;
                    evtAttribute.style.left = 3;
                    evtAttribute.style.fontSize = 12;

                    panel.Add(playLabel);
                    panel.Add(bigLabel);
                    panel.Add(timeLabel);
                    panel.Add(timeLabel2);
                    panel.Add(progressBG);
                    panel.Add(progress);
                    panel.Add(label);
                    panel.Add(evtAttributeHeader);
                    panel.Add(evtAttribute);
                    context.Add(panel);

                    info.ui = context;
                    info.label = label;
                    info.bigLabel = bigLabel;
                    info.timeLabel = timeLabel;
                    info.timeLabel2 = timeLabel2;
                    info.playLabel = playLabel;
                    info.progress = progress;
                    info.debugPanel = panel;
                    info.evtAttribute = evtAttribute;

                    spawnerDebugInfos[window].Add(info);
                }

                info.debugPanel = panel;
            }
            else if (context.ClassListContains("init"))
            {
                if (systemDebugInfos[window].Any(o => o.ui == context))
                    return;

                var model = context.GetModel();
                SystemDebugInfo info = new SystemDebugInfo();

                var panel = context.Q("System-Debug");
                if (panel == null)
                {
                    panel = new VisualElement()
                    {
                        name = "System-Debug"
                    };
                    panel.style.position = UnityEngine.UIElements.Position.Absolute;
                    panel.style.top = 22;
                    panel.style.borderBottomLeftRadius = 8;
                    panel.style.borderBottomRightRadius = 8;
                    panel.style.borderTopLeftRadius = 8;
                    panel.style.borderTopRightRadius = 8;
                    panel.style.paddingBottom = 8;
                    panel.style.paddingLeft = 8;
                    panel.style.paddingRight = 8;
                    panel.style.paddingTop = 8;
                    panel.style.left = 432;
                    panel.style.height = 180;
                    panel.style.width = 260;
                    panel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 1.0f));

                    var label = new Label($"System Info");
                    label.style.fontSize = 22;
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    label.style.position = UnityEngine.UIElements.Position.Absolute;
                    label.style.top = 4;
                    label.style.left = 8;

                    var countLabel = new Label($"Count : 0");
                    countLabel.style.fontSize = 12;
                    countLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    countLabel.style.width = 240;
                    countLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    countLabel.style.top = 41;
                    countLabel.style.left = 8;
                    countLabel.style.color = Color.white;

                    var progressBG = new VisualElement();
                    progressBG.style.position = UnityEngine.UIElements.Position.Absolute;
                    progressBG.style.top = 40;
                    progressBG.style.left = 8;
                    progressBG.style.width = 240;
                    progressBG.style.height = 18;
                    progressBG.style.backgroundColor = new StyleColor(new Color(.3f, .3f, .3f, 1f));
                    progressBG.style.borderBottomLeftRadius = 3;
                    progressBG.style.borderBottomRightRadius = 3;
                    progressBG.style.borderTopLeftRadius = 3;
                    progressBG.style.borderTopRightRadius = 3;

                    var progress = new VisualElement();
                    progress.style.position = UnityEngine.UIElements.Position.Absolute;
                    progress.style.top = 40;
                    progress.style.left = 8;
                    progress.style.width = 240;
                    progress.style.height = 18;
                    progress.style.backgroundColor = new StyleColor(Styles.sysInfoGreen);
                    progress.style.borderBottomLeftRadius = 3;
                    progress.style.borderBottomRightRadius = 3;
                    progress.style.borderTopLeftRadius = 3;
                    progress.style.borderTopRightRadius = 3;

                    var infoLabel = new Label($"INFO");
                    infoLabel.style.fontSize = 12;
                    infoLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    infoLabel.style.top = 64;
                    infoLabel.style.left = 8;

                    var memLabel = new Label("GPU Memory : 0b");
                    memLabel.style.fontSize = 12;
                    memLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    memLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    memLabel.style.top = 90;
                    memLabel.style.left = 8;

                    var attrHeaderLabel = new Label("Stored Attributes");
                    attrHeaderLabel.style.fontSize = 12;
                    attrHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    attrHeaderLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    attrHeaderLabel.style.top = 116;
                    attrHeaderLabel.style.left = 8;

                    var attrLabel = new Label("---");
                    attrLabel.style.fontSize = 12;
                    attrLabel.style.position = UnityEngine.UIElements.Position.Absolute;
                    attrLabel.style.top = 132;
                    attrLabel.style.left = 8;

                    panel.Add(label);
                    panel.Add(progressBG);
                    panel.Add(progress);
                    panel.Add(countLabel);
                    panel.Add(infoLabel);
                    panel.Add(memLabel);
                    panel.Add(attrHeaderLabel);
                    panel.Add(attrLabel);
                    context.Add(panel);

                    info.ui = context;
                    info.infoLabel = infoLabel;
                    info.memoryLabel = memLabel;
                    info.attribLabel = attrLabel;
                    info.panel = panel;
                    info.model = model;
                    info.countLabel = countLabel;
                    info.progress = progress;

                    systemDebugInfos[window].Add(info);
                }
            }
        });
    }


    static void UpdateDebugInfo(VFXViewWindow window)
    {
        if (spawnerDebugInfos == null || systemDebugInfos == null)
            return;
 
        VisualEffect vfx = window.graphView.attachedComponent;
        VisualEffectAsset asset = window.displayedResource.asset;

        if (!systemNames.ContainsKey(asset))
            systemNames.Add(asset, new List<string>());
        if (!spawnerNames.ContainsKey(asset))
            spawnerNames.Add(asset, new List<string>());

        bool debugInfoVisible = GetDebugInfoVisible(asset);

        foreach (var debugInfo in spawnerDebugInfos[window])
        {
            debugInfo.debugPanel.visible = debugInfoVisible;
            if (!debugInfoVisible)
                continue;

            if (vfx == null)
            {
                debugInfo.label.text = "Please Attach Graph to instance in scene";
                debugInfo.playLabel.text = "?";
                debugInfo.bigLabel.text = "Unknown";
                debugInfo.timeLabel.text = "";
                debugInfo.timeLabel2.text = "";
                debugInfo.progress.style.width = 0;
                debugInfo.debugPanel.style.height = 140;
            }
            else
            {
                if (debugInfo.ui == null)
                {
                    spawnersToDelete.Add(debugInfo);
                    continue;
                }

                debugInfo.ResyncName();

                vfx.GetSpawnSystemNames(spawnerNames[asset]);
                if (!spawnerNames[asset].Contains(debugInfo.name))
                    continue;

                var spawnerInfo = vfx.GetSpawnSystemInfo(debugInfo.name);
                if (spawnerInfo == null)
                    continue;

                string text = "";
                if (spawnerInfo.loopState == VFXSpawnerLoopState.Finished)
                {
                    text = @"Spawner has stopped either by being hit 
on the stop input, or exhausting its loops";
                    debugInfo.playLabel.text = "■";
                    debugInfo.bigLabel.text = "Stopped";
                    debugInfo.timeLabel.text = "";
                    debugInfo.timeLabel2.text = "";
                    debugInfo.progress.style.width = 240;
                    debugInfo.progress.style.backgroundColor = new StyleColor(new Color(.8f, .1f, .3f, 1f));


                }
                else if (spawnerInfo.loopState == VFXSpawnerLoopState.Looping)
                {
                    text = "Playing";
                    if (spawnerInfo.loopCount > 0)
                    {
                        text += $" {spawnerInfo.loopIndex}/{spawnerInfo.loopCount} loops";
                    }
                    debugInfo.playLabel.text = "►";
                    debugInfo.bigLabel.text = "";
                    debugInfo.timeLabel.text = spawnerInfo.totalTime.ToString("F1");
                    float d = spawnerInfo.loopDuration;
                    debugInfo.timeLabel2.text = $"/ {(d < 0 ? "∞ " : d.ToString("F1"))}";
                    debugInfo.progress.style.width = 240 * (d < 0 ? 1.0f : (spawnerInfo.totalTime / spawnerInfo.loopDuration));
                    debugInfo.progress.style.backgroundColor = new StyleColor(new Color(.2f, .8f, .1f, 1f));

                }
                else
                {
                    text = $"Delayed ({(spawnerInfo.loopState == VFXSpawnerLoopState.DelayingAfterLoop ? "After Loop" : "Before Loop")})";
                    if (spawnerInfo.loopCount > 0)
                    {
                        text += $" {spawnerInfo.loopIndex + 1}/{spawnerInfo.loopCount} loops";
                    }

                    debugInfo.playLabel.text = "…";
                    debugInfo.bigLabel.text = "";
                    debugInfo.timeLabel.text = spawnerInfo.totalTime.ToString("F1");
                    float d = spawnerInfo.loopState == VFXSpawnerLoopState.DelayingAfterLoop ? spawnerInfo.delayAfterLoop : spawnerInfo.delayBeforeLoop;
                    debugInfo.timeLabel2.text = $"/ {d.ToString("F1")}";
                    debugInfo.progress.style.width = 240 * (spawnerInfo.totalTime / d);
                    debugInfo.progress.style.backgroundColor = new StyleColor(new Color(.8f, .6f, .1f, 1f));


                }
                debugInfo.label.text = text;

                VFXEventAttribute eva = spawnerInfo.vfxEventAttribute;
                StringBuilder sb = new StringBuilder();
                int i = 0;

                if (eva.HasFloat("spawnCount"))
                {
                    ++i;
                    sb.AppendLine($"spawnCount : {(int)eva.GetFloat("spawnCount")}");
                }

                if (eva.HasVector3("position"))
                {
                    ++i;
                    sb.AppendLine($"position: {eva.GetVector3("position")}");
                }

                if (eva.HasVector3("targetPosition"))
                {
                    ++i;
                    sb.AppendLine($"targetPosition: {eva.GetVector3("targetPosition")}");
                }

                if (eva.HasVector3("oldPosition"))
                {
                    ++i;
                    sb.AppendLine($"oldPosition: {eva.GetVector3("oldPosition")}");
                }

                if (eva.HasVector3("velocity"))
                {
                    ++i;
                    sb.AppendLine($"velocity: {eva.GetVector3("velocity")}");
                }

                if (eva.HasFloat("size"))
                {
                    ++i;
                    sb.AppendLine($"size: {eva.GetFloat("size")}");
                }

                if (eva.HasVector3("scale"))
                {
                    ++i;
                    sb.AppendLine($"scale: {eva.GetVector3("scale")}");
                }

                if (eva.HasVector3("angle"))
                {
                    ++i;
                    sb.AppendLine($"angle: {eva.GetVector3("angle")}");
                }

                if (eva.HasVector3("color"))
                {
                    ++i;
                    sb.AppendLine($"color: {eva.GetVector3("color")}");
                }

                if (eva.HasFloat("age"))
                {
                    ++i;
                    sb.AppendLine($"age: {eva.GetFloat("age")}");
                }

                if (eva.HasFloat("lifetime"))
                {
                    ++i;
                    sb.AppendLine($"lifetime: {eva.GetFloat("lifetime")}");
                }

                if (i == -1)
                {
                    sb.AppendLine("(No VFX Event Attributes)");
                    i++;
                }


                debugInfo.evtAttribute.text = sb.ToString();
                debugInfo.debugPanel.style.height = 140 + (i * 12);
            }
        }

        foreach (var info in spawnersToDelete)
            spawnerDebugInfos[window].Remove(info);

        spawnersToDelete.Clear();

        foreach (var systemInfo in systemDebugInfos[window])
        {
            systemInfo.panel.visible = debugInfoVisible;
            if (!debugInfoVisible)
                continue;

            if (systemInfo.model == null)
            {
                systemsToDelete.Add(systemInfo);
                continue;
            }

            var data = systemInfo.model.GetData() as VFXDataParticle;
            if (data == null || !systemInfo.model.CanBeCompiled())
            {
                systemInfo.countLabel.text = "CONTEXT NOT CONNECTED";
                systemInfo.infoLabel.text = "Please connect to either Update or Output";
                systemInfo.progress.style.width = 240;
                systemInfo.progress.style.backgroundColor = Styles.sysInfoUnknown;
                systemInfo.memoryLabel.text = "Unknown GPU Memory Usage";
                systemInfo.attribLabel.text = "(unknown)";
                systemInfo.panel.style.height = 156;
                continue;
            }

            var systemModel = systemInfo.model.GetParent();
            string systemName = systemModel.systemNames.GetUniqueSystemName(data);
            var capacity = (uint)data.GetSetting("capacity").value;
            var layout = data.GetCurrentAttributeLayout();

            if (vfx != null) // attached
            {
                vfx.GetSystemNames(systemNames[asset]);
                if (!systemNames[asset].Contains(systemName))
                    continue;

                var vfxSystemInfo = vfx.GetParticleSystemInfo(systemName);
                uint aliveCount = vfxSystemInfo.aliveCount;
                float t = (float)Mathf.Min(aliveCount,capacity)/ capacity;
                systemInfo.countLabel.text = $"{vfxSystemInfo.aliveCount.ToString("N0")} / {capacity.ToString("N0")}";
                systemInfo.progress.style.width = Mathf.Clamp01(t) * 240;

                if (t > 0.75f)
                    systemInfo.progress.style.backgroundColor = Styles.sysInfoGreen;
                else if (t > 0.33f)
                    systemInfo.progress.style.backgroundColor = Styles.sysInfoOrange;
                else
                    systemInfo.progress.style.backgroundColor = Styles.sysInfoRed;

                systemInfo.infoLabel.text = $"{(vfx.enabled ? "[ENABLED]" : "[DISABLED]")} {(vfxSystemInfo.sleeping ? "[SLEEPING]" : "[AWAKE]")} {(vfx.culled ? "[CULLED]" : "[VISIBLE]")} {(vfx.pause ? "[PAUSED]" : "")}";


            }
            else // not attached
            {
                systemInfo.countLabel.text = $"??? / {capacity.ToString("N0")}";
                systemInfo.infoLabel.text = "Please attach graph to scene instance";
                systemInfo.progress.style.width = 240;
                systemInfo.progress.style.backgroundColor = Styles.sysInfoUnknown;
            }

            StringBuilder sb = new StringBuilder();
            uint stride = 0;
            foreach (var b in layout)
            {
                foreach (var attr in b.attributes)
                {
                    if (!string.IsNullOrEmpty(attr.name))
                        sb.AppendLine($" - {attr.name} : (4 Bytes)");
                    else
                        sb.AppendLine($" - (unused) : (4 Bytes)");

                    stride += 1;
                }
            }
            systemInfo.attribLabel.text = sb.ToString();

            ulong totalSize = stride * 4 * capacity;

            if (totalSize > 1000000000)
                systemInfo.memoryLabel.text = $"GPU Memory : {(totalSize / 1000000000f).ToString("F2")} GB";
            if (totalSize > 1000000)
                systemInfo.memoryLabel.text = $"GPU Memory : {(totalSize / 1000000f).ToString("F2")} MB";
            else if (totalSize > 1000)
                systemInfo.memoryLabel.text = $"GPU Memory : {(totalSize / 1000f).ToString("F2")} KB";
            else
                systemInfo.memoryLabel.text = $"GPU Memory : {((float)totalSize).ToString("F2")} B";

            systemInfo.panel.style.height = 140 + (stride * 15);
        }

        foreach (var info in systemsToDelete)
            systemDebugInfos[window].Remove(info);

        systemsToDelete.Clear();

    }
}
