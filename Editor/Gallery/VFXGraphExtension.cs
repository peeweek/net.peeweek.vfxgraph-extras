using System.Collections;
using System.Collections.Generic;
using UnityEditor.VFX.UI;
using UnityEditor;
using UnityEditor.VFX.UI;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.VFX;

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
            VFXGraphGalleryWindow.OpenWindowCreateAsset("");
        }
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
            button.text = "...";
            
            Add(button);
        }

        void OnClick()
        {
            GenericMenu m = new GenericMenu();
            m.AddItem(new GUIContent("Add from Template"), false, () => { });
            m.ShowAsContext();
        }
    }

}
