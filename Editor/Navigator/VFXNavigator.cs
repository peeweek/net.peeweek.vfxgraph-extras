using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VFX.UI
{
    internal class VFXNavigator : Blackboard
    {
        VFXViewWindow window;

        VFXGraph m_Graph;

        string searchString = string.Empty;

        internal VFXNavigator(VFXViewWindow window)
        {
            this.window = window;
            IMGUIContainer contents = new IMGUIContainer(OnGUI);
            Add(contents);
            this.title = "Navigator";
            this.subTitle = "Navigate graph tree and select nodes";
            this.scrollable = true;
            this.SetPosition(new Rect(0, 0, 240, 180));
        }

        public override void UpdatePresenterPosition()
        {
            var position = this.GetPosition();
            bool dirty = false;

            if (position.y < 32)
            {
                position.y = 32;
                dirty = true;
            }

            if (position.width < 240)
            {
                position.width = 240;
                dirty = true;
            }

            if (position.height < 180)
            {
                position.height = 180;
                dirty = true;
            }

            if (dirty)
                SetPosition(position);

            base.UpdatePresenterPosition();
        }

        void OnGUI()
        {
            if(window.graphView.controller == null) // No Asset
            {
                EditorGUILayout.HelpBox("Please Load an asset first", MessageType.Info);
                return;
            }    

            if (window.graphView.controller != null && m_Graph != window.graphView.controller.graph)
            {
                if (m_Graph != null)
                    window.graphView.controller.UnRegisterNotification(m_Graph, OnGraphChange);

                m_Graph = window.graphView.controller.graph;
                window.graphView.controller.RegisterNotification(m_Graph, OnGraphChange);
                OnGraphChange();
            }

            var position = this.GetPosition();

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                searchString = EditorGUILayout.DelayedTextField(searchString, EditorStyles.toolbarSearchField);
                if(EditorGUI.EndChangeCheck())
                {
                    m_TreeView.searchString = searchString;
                }
            }

            m_TreeView.OnGUI(GUILayoutUtility.GetRect(position.width-20, position.height -82));
        }

        #region OLD, TODELETE
        void OnGUIOld()
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

        void SelectButton(VFXNodeUI node, string label)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                if (GUILayout.Button(label, Styles.button))
                {
                    window.graphView.ClearSelection();
                    window.graphView.AddToSelection(node);
                    window.graphView.FrameSelection();
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
                    window.graphView.ClearSelection();
                    window.graphView.AddToSelection(context);
                    window.graphView.FrameSelection();
                }
                if (GUILayout.Button(">", Styles.button, GUILayout.Width(16)))
                {
                    FireEvent(evt.eventName);
                }
            }
        }

        void FireEvent(string eventName)
        {
            if (window.graphView.attachedComponent != null)
            {
                window.graphView.attachedComponent.SendEvent(eventName);
            }
        }


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
            
            foreach (var n in window.graphView.GetAllNodes())
            {
                if(Contains(n.title,s))
                {
                    if(n is VFXOperatorUI)
                        nodes.Add(n);
                }
            }

            foreach(var c in window.graphView.GetAllContexts())
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

        #endregion


        VFXNavigatorTreeView m_TreeView;
        TreeViewState m_State;

        void OnGraphChange()
        {
            if(m_TreeView == null)
            {
                m_State = new TreeViewState();
                m_TreeView = new VFXNavigatorTreeView(window, m_State);
            }

            m_TreeView.Reload();
        }

        class VFXNavigatorTreeView : TreeView
        {
            VFXViewWindow m_Window;
            VFXView m_VFXView;
            TreeViewItem m_Root;

            public VFXNavigatorTreeView(VFXViewWindow window, TreeViewState state) : base (state)
            {
                this.m_Window = window;
                this.m_VFXView = window.graphView;
            }

            string GetSpawnSystemName(VFXBasicSpawner context)
            {
                return context.GetParent().systemNames.GetUniqueSystemName(context);
            }

            string GetParticleSystemName(VFXContext context)
            {
                return context.GetParent().systemNames.GetUniqueSystemName(context.GetData());
            }


            protected override TreeViewItem BuildRoot()
            {
                int index = 0;

                // hidden root item
                m_Root = new TreeViewItem(index++, -1);

                // Root Categories
                var eventRoot = new VFXNavigatorTreeViewItem(index++, 0, null, "Events");
                var spawnSystemsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, "Spawn Systems");
                var systemsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, "Systems");
                var operatorsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, "Operators");


                var allNodes = m_VFXView.GetAllNodes();
                var allContexts = m_VFXView.GetAllContexts();

                // Find Events
                List<VFXContextUI> events = new List<VFXContextUI>();
                foreach (var context in allContexts)
                {
                    if (context.controller.model is VFXBasicEvent)
                        events.Add(context);
                }
                events.Sort((n, m) => string.Compare((n.controller.model as VFXBasicEvent).eventName, (m.controller.model as VFXBasicEvent).eventName));
                foreach (var e in events)
                {
                    eventRoot.AddChild(new VFXNavigatorTreeViewItem(index++, 1, e, (e.controller.model as VFXBasicEvent).eventName));
                }

                // Find Spawn Contexts/Blocks
                foreach(var context in allContexts.Where(c => c.controller.model is VFXBasicSpawner))
                {
                    var contextItem = new VFXNavigatorTreeViewItem(index++, 1, context, GetSpawnSystemName(context.controller.model as VFXBasicSpawner));

                    var blocks = context.GetAllBlocks();
                    foreach(var block in blocks)
                    {
                        contextItem.AddChild(new VFXNavigatorTreeViewItem(index++, 2, block));
                    }

                    spawnSystemsRoot.AddChild(contextItem);

                }

                // Find Systems/Contexts/Blocks
                Dictionary<string, Dictionary<VFXContextUI, List<VFXBlockUI>>> systemsContextsBlocks = new Dictionary<string, Dictionary<VFXContextUI, List<VFXBlockUI>>>();
                foreach(var context in allContexts) // Create All Systems
                {
                    if(context.controller.model is VFXBasicInitialize)
                    {
                        string systemName = GetParticleSystemName(context.controller.model);
                        if(!systemsContextsBlocks.ContainsKey(systemName))
                        {
                            systemsContextsBlocks.Add(systemName, new Dictionary<VFXContextUI, List<VFXBlockUI>>());
                        }
                    }
                }
                foreach (var context in allContexts) // Create All Contexts/Blocks
                {
                    var inputType = context.controller.model.inputType;
                    var outputType = context.controller.model.outputType;
                    if (
                        (outputType == VFXDataType.Particle) || 
                        (outputType == VFXDataType.ParticleStrip) || 
                        ((inputType == VFXDataType.Particle || inputType == VFXDataType.ParticleStrip) && outputType == VFXDataType.None)
                        )
                    {
                        string systemName = GetParticleSystemName(context.controller.model);
                        systemsContextsBlocks[systemName].Add(context, new List<VFXBlockUI>());
                        foreach(var block in context.GetAllBlocks())
                        {
                            systemsContextsBlocks[systemName][context].Add(block);
                        }
                    }
                }

                int GetContextPriority(VFXContextUI context)
                {
                    switch (context.controller.model.taskType)
                    {
                        default: 
                            return 0;
                        case VFXTaskType.Spawner: 
                            return 0;
                        case VFXTaskType.Initialize: 
                            return 1;
                        case VFXTaskType.Update: 
                            return 2;

                        case VFXTaskType.ParticlePointOutput:
                        case VFXTaskType.ParticleLineOutput:
                        case VFXTaskType.ParticleQuadOutput:
                        case VFXTaskType.ParticleHexahedronOutput:
                        case VFXTaskType.ParticleMeshOutput:
                        case VFXTaskType.ParticleTriangleOutput:
                        case VFXTaskType.ParticleOctagonOutput:
                         return 3;
                    }
                }

                foreach(var systemName in systemsContextsBlocks.Keys.OrderBy(s=>s))
                {
                    var contexts = systemsContextsBlocks[systemName];
                    var systemItem = new VFXNavigatorTreeViewItem(index++, 1, null, systemName);
                    foreach(var context in contexts.Keys.OrderBy(o => GetContextPriority(o)))
                    {
                        var contextItem = new VFXNavigatorTreeViewItem(index++, 2, context);
                        foreach (var block in context.GetAllBlocks())
                        {
                            var blockItem = new VFXNavigatorTreeViewItem(index++, 3, block);
                            contextItem.AddChild(blockItem);
                        }
                        systemItem.AddChild(contextItem);
                    }

                    systemsRoot.AddChild(systemItem);
                }


                // Find Operators
                List<VFXNodeUI> operators = new List<VFXNodeUI>();
                foreach (var n in allNodes)
                {
                    if (n is VFXOperatorUI)
                        operators.Add(n);
                }
                operators.Sort((n,m) => string.Compare(n.title, m.title));
                foreach(var n in operators)
                {
                    operatorsRoot.AddChild(new VFXNavigatorTreeViewItem(index++, 1, n));
                }

                m_Root.AddChild(eventRoot);
                m_Root.AddChild(spawnSystemsRoot);
                m_Root.AddChild(systemsRoot);
                m_Root.AddChild(operatorsRoot);
                return m_Root;
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;

            protected override void SingleClickedItem(int id)
            {
                base.SingleClickedItem(id);
                var item = FindItem(id, m_Root) as VFXNavigatorTreeViewItem;
                if(item != null && item.nodeui != null)
                {
                    m_Window.graphView.ClearSelection();
                    m_Window.graphView.AddToSelection(item.nodeui);
                    m_Window.graphView.FrameSelection();
                }
            }

        }

        class VFXNavigatorTreeViewItem : TreeViewItem
        {
            public VFXNodeUI nodeui { get; private set; }

            public VFXNavigatorTreeViewItem(int id, int depth, VFXNodeUI nodeui, string fallbackName = "") : base(id, depth, string.IsNullOrEmpty(fallbackName) ? nodeui.title : fallbackName)
            {
                this.nodeui = nodeui;
            }

            public void SetParent(VFXNavigatorTreeViewItem item)
            {
                this.parent = item;
                this.depth = item.depth + 1;
            }
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

