using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VFX.UI
{
    internal class VFXNavigator : Blackboard
    {
        VFXView m_View;

        string searchString = string.Empty;

        internal VFXNavigator(VFXViewWindow window)
        {
            m_View = window.graphView;

            IMGUIContainer contents = new IMGUIContainer(OnGUI);
            Add(contents);
            
            this.AddManipulator(new Dragger { clampToParentEdges = true });
            this.title = "Navigator";
            this.subTitle = string.Empty;
            this.scrollable = true;
        }

        void SelectButton(VFXNodeUI node, string label)
        {
            if(GUILayout.Button(label, Styles.button))
            {
                m_View.ClearSelection();
                m_View.AddToSelection(node);
                m_View.FrameSelection();
            }
        }

        void OnGUI()
        {
            using(new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                searchString = EditorGUILayout.DelayedTextField(searchString, EditorStyles.toolbarSearchField);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateSearch();
                }
            }

            if(string.IsNullOrEmpty(searchString))
            {
                EditorGUILayout.HelpBox("Please enter a search string", MessageType.Info);
            }
            else
            {
                if(contexts.Count > 0)
                {
                    GUILayout.Label("Contexts", EditorStyles.boldLabel);
                    foreach(var c in contexts)
                    {
                        var sysName = c.controller.model.GetData().title;
                        var contextName = c.controller.model.name;
                        SelectButton(c, $"{sysName}/{contextName}");
                    }
                    GUILayout.Space(12);
                }

                if(blocks.Count > 0)
                {
                    GUILayout.Label("Blocks", EditorStyles.boldLabel);
                    foreach (var b in blocks)
                    {
                        var contextName = b.controller.model.GetParent().name;
                        var blockName = b.title;
                        SelectButton(b, $"{contextName}/{blockName}");
                    }
                    GUILayout.Space(12);
                }

                if(nodes.Count > 0)
                {
                    GUILayout.Label("Nodes", EditorStyles.boldLabel);
                    foreach (var n in nodes)
                    {
                        var nodeName = n.title;
                        SelectButton(n, $"{nodeName}");
                    }
                    GUILayout.Space(12);
                }
            }
        }

        List<VFXNodeUI> nodes;
        List<VFXContextUI> contexts;
        List<VFXBlockUI> blocks;

        void UpdateSearch()
        {
            if (string.IsNullOrEmpty(searchString))
                return;

            if (nodes == null) nodes = new List<VFXNodeUI>(); else nodes.Clear();
            if (contexts == null) contexts = new List<VFXContextUI>(); else contexts.Clear();
            if (blocks == null) blocks = new List<VFXBlockUI>(); else blocks.Clear();

            string s = searchString.ToLower();

            foreach (var n in m_View.GetAllNodes())
            {
                if(Contains(n.title,s))
                {
                    nodes.Add(n);
                }
            }

            foreach(var c in m_View.GetAllContexts())
            {
                if(Contains(c.controller.model.name.ToLower(),s))
                {
                    contexts.Add(c);
                }

                foreach(var b in c.GetAllBlocks())
                {
                    if(Contains(b.title,s))
                    {
                        blocks.Add(b);
                    }
                }
            }
        }

        bool Contains(string baseString, string filter)
        {
            baseString = baseString.ToLower();

            var words = filter.Split(' ');
            foreach(var word in words)
            {
                if (baseString.Contains(word))
                    return true;
            }
            return false;
        }

        public void OnMoved()
        {

        }

        public void OnResized()
        {

        }

        public void OnStartResize()
        {

        }

        void Rebuild()
        {

        }

        static class Styles
        {
            public static GUIStyle button;

            static Styles()
            {
                button = new GUIStyle(EditorStyles.miniButton);
                button.alignment = TextAnchor.MiddleLeft;
            }
        }
    }
}

