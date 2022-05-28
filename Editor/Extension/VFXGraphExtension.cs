﻿using UnityEditor.VFX.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.VFX;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;

static class VFXGraphExtension
{
    [InitializeOnLoadMethod]
    static void InitializeExtension()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (VFXViewWindow.currentWindow == null)
            return;

        VFXViewWindow window = VFXViewWindow.currentWindow;
        VisualElement extension = window.graphView.Q("extension");
        if (extension == null)
        {
            var content = window.graphView.Q("contentViewContainer");
            var root = content.parent;
            window.graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
            extension = new VFXExtensionBoard();
            extension.name = "extension";
            root.Add(extension);
        }
        Profiler.BeginSample("VFXGraphExtension.UpdateStatsUI");
        UpdateStatsUI();
        Profiler.EndSample();
        Profiler.BeginSample("VFXGraphExtension.UpdateDebugInfo");
        UpdateDebugInfo();
        Profiler.EndSample();
    }


    static bool spawnerDebugInfoVisible
    {
        get { return EditorPrefs.GetBool("VFXGraphExtension.spawnerDebugInfoVisible", true); }
        set { EditorPrefs.SetBool("VFXGraphExtension.spawnerDebugInfoVisible", value); }
    }

    static List<string> names = new List<string>();
    static List<SpawnerDebugInfo> todelete = new List<SpawnerDebugInfo>();

    static void UpdateDebugInfo()
    {
        if (spawnerDebugInfos == null)
            return;

        VisualEffect vfx = VFXViewWindow.currentWindow.graphView.attachedComponent;

        foreach (var debugInfo in spawnerDebugInfos)
        {
            debugInfo.debugPanel.visible = spawnerDebugInfoVisible;
            if (!spawnerDebugInfoVisible)
                return;
            
            if(vfx == null)
            {
                debugInfo.label.text = "Please Attach Graph to instance in scene";
                debugInfo.playLabel.text = "?";
                debugInfo.bigLabel.text = "Unknown";
                debugInfo.timeLabel.text = "";
                debugInfo.timeLabel2.text = "";
                debugInfo.progress.style.width = 0;
            }
            else
            {
                if(debugInfo.ui == null)
                {
                    todelete.Add(debugInfo);
                    continue;
                }

                debugInfo.ResyncName();
                    

                vfx.GetSpawnSystemNames(names);
                if(!names.Contains(debugInfo.name))
                {
                    // should not happen
                    continue;
                }

                
                var spawnerInfo = vfx.GetSpawnSystemInfo(debugInfo.name);
                if (spawnerInfo == null)
                    continue;

                string text = "";
                if(spawnerInfo.loopState == VFXSpawnerLoopState.Finished)
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
                    if(spawnerInfo.loopCount > 0)
                    {
                        text += $" {spawnerInfo.loopIndex}/{spawnerInfo.loopCount} loops";
                    }
                    debugInfo.playLabel.text = "►";
                    debugInfo.bigLabel.text = "";
                    debugInfo.timeLabel.text = spawnerInfo.totalTime.ToString("F1");
                    float d = spawnerInfo.loopDuration;
                    debugInfo.timeLabel2.text = $"/ {(d < 0 ? "∞ " : d.ToString("F1"))}";
                    debugInfo.progress.style.width = 240 * (d < 0? 1.0f :(spawnerInfo.totalTime/spawnerInfo.loopDuration));
                    debugInfo.progress.style.backgroundColor = new StyleColor(new Color(.2f, .8f, .1f, 1f));

                }
                else
                {
                    text = $"Delayed ({(spawnerInfo.loopState == VFXSpawnerLoopState.DelayingAfterLoop? "After Loop" : "Before Loop")})";
                    if (spawnerInfo.loopCount > 0)
                    {
                        text += $" {spawnerInfo.loopIndex+1}/{spawnerInfo.loopCount} loops";
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
            }
        }

        foreach (var info in todelete)
            spawnerDebugInfos.Remove(info);

        todelete.Clear();
    }

    static void OnKeyDown(KeyDownEvent e)
    {
        if(e.keyCode == KeyCode.T)
        {
            var wnd = VFXViewWindow.currentWindow;
            Vector2 pos = e.originalMousePosition;
            pos = wnd.graphView.ChangeCoordinatesTo(wnd.graphView.contentViewContainer, pos);
            OpenAddCreateWindow(pos);
        }
    }

    static void OpenAddCreateWindowScreenCenter()
    {
        var wnd = VFXViewWindow.currentWindow;
        Vector2 pos = wnd.graphView.ScreenToViewPosition(wnd.position.center);
        pos = wnd.graphView.ChangeCoordinatesTo(wnd.graphView.contentViewContainer, pos);
        OpenAddCreateWindow(pos);
    }

    static void OpenAddCreateWindow(Vector2 pos)
    {
        VFXGraphGalleryWindow.OpenWindowAddTemplate(pos);
    }


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

        public void ResyncName()
        {
            if(ui != null)
            {
                name = ui.Q<Label>("user-label").text;
            }
        }
    }
    static List<SpawnerDebugInfo> spawnerDebugInfos;

    static void ToggleSpawnerStats()
    {
        spawnerDebugInfoVisible = !spawnerDebugInfoVisible;
        UpdateStatsUI();
    }

    static void UpdateStatsUI()
    {
        var wnd = VFXViewWindow.currentWindow;
        if (wnd == null)
            return;

        var gv = wnd.graphView;
        if (gv == null)
            return;


        if (spawnerDebugInfos == null)
            spawnerDebugInfos = new List<SpawnerDebugInfo>();

        
        gv.Query<VFXContextUI>().Build().ForEach((context) =>
        {
            if (!context.ClassListContains("spawner"))
                return;

            string name = context.Q<Label>("user-label").text;

            if (spawnerDebugInfos.Any(o => o.ui == context))
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
                panel.style.height = 112;
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

                panel.Add(playLabel);
                panel.Add(bigLabel);
                panel.Add(timeLabel);
                panel.Add(timeLabel2);
                panel.Add(progressBG);
                panel.Add(progress);
                panel.Add(label);
                context.Add(panel);

                info.ui = context;
                info.label = label;
                info.bigLabel = bigLabel;
                info.timeLabel = timeLabel;
                info.timeLabel2 = timeLabel2;
                info.playLabel = playLabel;
                info.progress = progress;
                info.debugPanel = panel;
                
                spawnerDebugInfos.Add(info);
            }

            info.debugPanel = panel;



        });
    }

    static void CreateGameObjectAndAttach()
    {
        var resource = VFXViewWindow.currentWindow.graphView.controller.model.visualEffectObject.GetResource();
        if (resource == null)
            return;
        var asset = resource.asset;
        var go = new GameObject();
        go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 4;
        go.name = asset.name;
        var vfx = go.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = asset;
        Selection.activeGameObject = go;
        VFXViewWindow.currentWindow.LoadAsset(asset, vfx);
    }

    class VFXExtensionBoard : GraphElement
    {
        public VFXExtensionBoard() : base()
        {
            style.width = 148;
            style.height = 24;
            
            style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1.0f));
            style.borderBottomLeftRadius = 4;
            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomRightRadius = 4;

            style.flexDirection = FlexDirection.Row;

            var label = new Label("VFX Extension");
            label.style.fontSize = 15;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);

            var button = new Button(OnClick);
            button.text = "▼";
            button.style.fontSize = 10;
            button.style.width = 24;
            button.style.height = 16;
            Add(button);
        }

        void OnClick()
        {
            GenericMenu m = new GenericMenu();
            if(VFXViewWindow.currentWindow.graphView.controller == null) // No Asset
            {
                // WIP : No Asset Menu
                m.AddItem(new GUIContent("No Asset"), false, null);
                m.ShowAsContext();
            }
            else
            {
                m.AddItem(new GUIContent("Add System from Template... (T)"), false, OpenAddCreateWindowScreenCenter);
                m.AddSeparator("");
                m.AddItem(new GUIContent("Create Game Object and Attach"), false, CreateGameObjectAndAttach);
                m.AddItem(new GUIContent("Show Spawner Stats"), spawnerDebugInfoVisible, ToggleSpawnerStats);
                m.ShowAsContext();
            }

        }
    }

}
