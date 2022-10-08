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
            this.title = "Navigator";
            this.subTitle = string.Empty;
            this.scrollable = true;
        }

        public override void UpdatePresenterPosition()
        {
            var position = this.GetPosition();
            if (position.y < 32)
            {
                position.y = 32;
                SetPosition(position);
            }

            base.UpdatePresenterPosition();
        }

        void SelectButton(VFXNodeUI node, string label)
        {
            using(new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                if (GUILayout.Button(label, Styles.button))
                {
                    m_View.ClearSelection();
                    m_View.AddToSelection(node);
                    m_View.FrameSelection();
                }
            }


        }

        void EventButton(VFXContextUI context, VFXBasicEvent evt)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                if (GUILayout.Button(evt.eventName, Styles.button))
                {
                    m_View.ClearSelection();
                    m_View.AddToSelection(context);
                    m_View.FrameSelection();
                }
                if (GUILayout.Button(">", Styles.button, GUILayout.Width(16)))
                {
                    FireEvent(evt.eventName);
                }
            }
        }

        void FireEvent(string eventName)
        {
            if(m_View.attachedComponent != null)
            {
                m_View.attachedComponent.SendEvent(eventName);
            }
        }

        void OnGUI()
        {
            UpdateSearch();

            using(new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                searchString = EditorGUILayout.DelayedTextField(searchString, EditorStyles.toolbarSearchField);
                if(EditorGUI.EndChangeCheck())
                {
                    UpdateSearch();
                }
            }

            if(events.Count > 0)
            {
                GUILayout.Label("Events");
                foreach(var e in events)
                {
                    VFXBasicEvent evt = e.controller.model as VFXBasicEvent;
                    EventButton(e, evt);
                }
            }

            if(systems.Count > 0)
            {
                GUILayout.Label("Systems");
                foreach(var c in systems)
                {
                    var sysName = c.controller.model.GetParent().systemNames.GetUniqueSystemName(c.controller.model.GetData());
                    SelectButton(c, $"{sysName} ({c.controller.model.GetData().type})");
                }
            }

            if(contexts.Count > 0)
            {
                GUILayout.Label("Contexts", EditorStyles.boldLabel);
                foreach(var c in contexts)
                {
                    VFXModel context;
                    if(c.controller.model is VFXBasicSpawner)
                        context = c.controller.model;
                    else
                        context = c.controller.model.GetData();

                    string sysName = c.controller.model.GetParent().systemNames.GetUniqueSystemName(context);

                    var contextName = c.controller.model.name;
                    SelectButton(c, $"{sysName} > {contextName}");
                }
                GUILayout.Space(12);
            }

            if(blocks.Count > 0)
            {
                GUILayout.Label("Blocks", EditorStyles.boldLabel);
                foreach (var b in blocks)
                {
                    VFXModel context;
                    if(b.controller.model.GetParent() is VFXBasicSpawner)
                        context = b.controller.model.GetParent();
                    else
                        context = b.controller.model.GetParent().GetData();

                    var sysName = b.controller.model.GetParent().GetParent().systemNames.GetUniqueSystemName(context);

                    var contextName = b.controller.model.GetParent().name;
                    var blockName = b.title;
                    SelectButton(b, $"{sysName} > {contextName} > {blockName}");
                }
                GUILayout.Space(12);
            }

            if(nodes.Count > 0)
            {
                GUILayout.Label("Operators", EditorStyles.boldLabel);
                foreach (var n in nodes)
                {
                    var nodeName = n.title;
                    SelectButton(n, $"{nodeName}");
                }
                GUILayout.Space(12);
            }
        }

        List<VFXNodeUI> nodes;
        List<VFXContextUI> events;
        List<VFXContextUI> systems;
        List<VFXContextUI> contexts;
        List<VFXBlockUI> blocks;


        bool Filter(string name, string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;

            else
                return Contains(name.ToLower(), searchString);
        }

        void UpdateSearch()
        {
            if (nodes == null) nodes = new List<VFXNodeUI>(); else nodes.Clear();
            if (events == null) events = new List<VFXContextUI>(); else events.Clear();
            if (systems == null) systems = new List<VFXContextUI>(); else systems.Clear();
            if (contexts == null) contexts = new List<VFXContextUI>(); else contexts.Clear();
            if (blocks == null) blocks = new List<VFXBlockUI>(); else blocks.Clear();

            string s = searchString.ToLower();

            foreach (var n in m_View.GetAllNodes())
            {
                if(Contains(n.title,s))
                {
                    if(n is VFXOperatorUI)
                        nodes.Add(n);
                }
            }

            foreach(var c in m_View.GetAllContexts())
            {
                if(c.controller.model is VFXBasicEvent && Filter((c.controller.model as VFXBasicEvent).eventName,s))
                {
                    events.Add(c);
                    continue;
                }

                if (c.controller.model.contextType == VFXContextType.Init && Filter(c.controller.model.GetParent().systemNames.GetUniqueSystemName(c.controller.model.GetData()), s))
                {
                    systems.Add(c);
                }

                if(Filter(c.controller.model.name,s))
                {
                    contexts.Add(c);
                }

                foreach(var b in c.GetAllBlocks())
                {
                    if(Filter(b.title,s))
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

