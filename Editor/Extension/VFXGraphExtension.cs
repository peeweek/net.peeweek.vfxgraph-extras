using UnityEditor.VFX.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.VFX;
using UnityEngine.VFX;

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
            button.text = "=";
            
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
                m.ShowAsContext();
            }

        }
    }

}
