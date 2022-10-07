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

static partial class VFXGraphExtension
{
    [InitializeOnLoadMethod]
    static void InitializeExtension()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        foreach(var vfxViewWindow in VFXViewWindow.GetAllWindows())
        {
            UpdateOrCreateExtensionForWindow(vfxViewWindow);
        }
    }

    static void UpdateOrCreateExtensionForWindow(VFXViewWindow window)
    { 
        VisualElement extension = window.graphView.Q("extension");

        if (extension == null)
        {
            var content = window.graphView.Q("contentViewContainer");
            var root = content.parent;
            window.graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
            extension = new VFXExtensionBoard(window);
            extension.name = "extension";
            root.Add(extension);
        }

        Profiler.BeginSample("VFXGraphExtension.UpdateStatsUI");
        UpdateStatsUIElements(window);
        Profiler.EndSample();
        if(window.graphView != null && window.graphView.controller != null && window.graphView.controller.model.asset != null)
        {
            Profiler.BeginSample("VFXGraphExtension.UpdateDebugInfo");
            UpdateDebugInfo(window);
            Profiler.EndSample();
        }
    }

    static void OnKeyDown(KeyDownEvent e)
    {
        var wnd = VFXViewWindow.focusedWindow as VFXViewWindow;

        if (wnd != null && e.keyCode == KeyCode.T)
        {
            Vector2 pos = e.originalMousePosition;
            pos = wnd.graphView.ChangeCoordinatesTo(wnd.graphView.contentViewContainer, pos);
            OpenAddCreateWindow(pos, wnd);
        }
    }

    static void OpenAddCreateWindowScreenCenter(object window)
    {
        var wnd = window as VFXViewWindow;

        if (wnd == null)
            return;

        Vector2 pos = wnd.graphView.ScreenToViewPosition(wnd.position.center);
        pos = wnd.graphView.ChangeCoordinatesTo(wnd.graphView.contentViewContainer, pos);
        OpenAddCreateWindow(pos, wnd);
    }

    static void OpenAddCreateWindow(Vector2 pos, VFXViewWindow window)
    {
        VFXGraphGalleryWindow.OpenWindowAddTemplate(pos, window);
    }


    static void CreateGameObjectAndAttach(object window)
    {
        var wnd = window as VFXViewWindow;
        var resource = wnd.graphView.controller.model.visualEffectObject.GetResource();
        if (resource == null)
            return;
        var asset = resource.asset;
        var go = new GameObject();
        go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 4;
        go.name = asset.name;
        var vfx = go.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = asset;
        Selection.activeGameObject = go;
        wnd.LoadAsset(asset, vfx);
    }

    static class Styles
    {
        public static Color sysInfoGreen = new Color(.1f, .5f, .1f, 1f);
        public static Color sysInfoOrange = new Color(.5f, .3f, .1f, 1f);
        public static Color sysInfoRed = new Color(.5f, .1f, .1f, 1f);
        public static Color sysInfoUnknown = new Color(.5f, .1f, .7f, 1f); 
    }

    class VFXExtensionBoard : GraphElement
    {
        VFXViewWindow window;
        public VFXExtensionBoard(VFXViewWindow window) : base()
        {
            this.window = window;
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
            if(this.window.graphView.controller == null) // No Asset
            {
                // WIP : No Asset Menu
                m.AddItem(new GUIContent("No Asset"), false, null);
                m.ShowAsContext();
            }
            else
            {
                m.AddItem(new GUIContent("Add System from Template... (T)"), false, OpenAddCreateWindowScreenCenter, this.window);
                m.AddSeparator("");
                m.AddItem(new GUIContent("Create Game Object and Attach"), false, CreateGameObjectAndAttach, this.window);
                m.AddItem(new GUIContent("Show Debug Stats"), debugInfoVisible, ToggleSpawnerStats, this.window);
                m.ShowAsContext();
            }

        }
    }

}
