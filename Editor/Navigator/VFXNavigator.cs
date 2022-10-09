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

        VFXNavigatorTreeView m_TreeView;
        TreeViewState m_State;

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

        bool needReloadNavigator = false;
        private void M_Graph_onInvalidateDelegate(VFXModel model, VFXModel.InvalidationCause cause)
        {
            needReloadNavigator = true;
        }

        void OnGUI()
        {
            if(window.graphView.controller == null) // No Asset
            {
                EditorGUILayout.HelpBox("Please Load an asset first", MessageType.Info);
                return;
            }

            if (needReloadNavigator || m_TreeView == null)
                ReloadNavigator();

            if (window.graphView.controller != null && m_Graph != window.graphView.controller.graph)
            {
                m_Graph = window.graphView.controller.graph;
                m_Graph.onInvalidateDelegate += M_Graph_onInvalidateDelegate;

                needReloadNavigator = true;
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

        void ReloadNavigator()
        {
            if(m_TreeView == null)
            {
                m_State = new TreeViewState();
                m_TreeView = new VFXNavigatorTreeView(window, m_State);
            }

            m_TreeView.Reload();
            needReloadNavigator = false;
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
                var eventRoot = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.systemEventTypeIcon , "Events");
                var spawnSystemsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.systemEventTypeIcon, "Spawn Systems");
                var systemsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.systemParticleTypeIcon, "Systems");
                var operatorsRoot = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.operatorTypeIcon, "Operators");
                var propertiesRoot = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.operatorTypeIcon, "Properties");
                var orphans = new VFXNavigatorTreeViewItem(index++, 0, null, Styles.contextTypeIcon, "Orphans");


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
                    eventRoot.AddChild(new VFXNavigatorTreeViewItem(index++, 1, e, Styles.contextEventTypeIcon, (e.controller.model as VFXBasicEvent).eventName));
                }

                // Find Spawn Contexts/Blocks
                foreach(var context in allContexts.Where(c => c.controller.model is VFXBasicSpawner))
                {
                    var contextItem = new VFXNavigatorTreeViewItem(index++, 1, context, Styles.contextSpawnTypeIcon, GetSpawnSystemName(context.controller.model as VFXBasicSpawner));

                    var blocks = context.GetAllBlocks();
                    foreach(var block in blocks)
                    {
                        contextItem.AddChild(new VFXNavigatorTreeViewItem(index++, 2, block, Styles.blockTypeIcon));
                    }

                    spawnSystemsRoot.AddChild(contextItem);

                }

                // Find Systems/Contexts/Blocks
                Dictionary<string, Dictionary<VFXContextUI, List<VFXBlockUI>>> systemsContextsBlocks = new Dictionary<string, Dictionary<VFXContextUI, List<VFXBlockUI>>>();
                foreach(var context in allContexts) // Create All Systems
                {
                    var data = context.controller.model.GetData();
                    if (data != null && data is VFXDataParticle)
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
                        // Process Context if orphan
                        if (context.controller.model.GetData().GetAttributes().Count() == 0)
                        {
                            var orphan = new VFXNavigatorTreeViewItem(index++, 1, context, Styles.contextTypeIcon);
                            foreach (var block in context.GetAllBlocks())
                            {
                                var blockItem = new VFXNavigatorTreeViewItem(index++, 3, block, Styles.blockTypeIcon);
                                orphan.AddChild(blockItem);
                            }
                            orphans.AddChild(orphan);
                        }

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

                    if (contexts.Count == 0)
                        continue;

                    var systemItem = new VFXNavigatorTreeViewItem(index++, 1, null, Styles.systemParticleTypeIcon, systemName);
                    foreach(var context in contexts.Keys.OrderBy(o => GetContextPriority(o)))
                    {

                        var icon = Styles.contextOutputTypeIcon;
                        if (context.controller.model.taskType == VFXTaskType.Initialize)
                            icon = Styles.contextInitializeTypeIcon;
                        else if (context.controller.model.taskType == VFXTaskType.Update)
                            icon = Styles.contextUpdateTypeIcon;

                        var contextItem = new VFXNavigatorTreeViewItem(index++, 2, context, icon);
                        foreach (var block in context.GetAllBlocks())
                        {
                            var blockItem = new VFXNavigatorTreeViewItem(index++, 3, block, Styles.blockTypeIcon);
                            contextItem.AddChild(blockItem);
                        }
                        systemItem.AddChild(contextItem);
                    }

                    systemsRoot.AddChild(systemItem);
                }


                // Find Operators / properties
                List<VFXNodeUI> operators = new List<VFXNodeUI>();
                List<VFXNodeUI> properties = new List<VFXNodeUI>();
                foreach (var n in allNodes)
                {
                    if (n is VFXOperatorUI)
                    {
                        operators.Add(n);
                    }
                    else if(n is VFXParameterUI)
                    {
                        properties.Add(n);
                    }
                }
                operators.Sort((n,m) => string.Compare(n.title, m.title));
                foreach(var n in operators)
                {
                    operatorsRoot.AddChild(new VFXNavigatorTreeViewItem(index++, 1, n, Styles.operatorTypeIcon));
                }

                properties.Sort((n, m) => string.Compare(n.title, m.title));
                foreach (var p in properties)
                {
                    propertiesRoot.AddChild(new VFXNavigatorTreeViewItem(index++, 1, p, Styles.operatorTypeIcon));
                }


                if (eventRoot.hasChildren)
                    m_Root.AddChild(eventRoot);

                if(spawnSystemsRoot.hasChildren)
                    m_Root.AddChild(spawnSystemsRoot);

                if(systemsRoot.hasChildren)
                    m_Root.AddChild(systemsRoot);

                if(operatorsRoot.hasChildren)
                    m_Root.AddChild(operatorsRoot);

                if (propertiesRoot.hasChildren)
                    m_Root.AddChild(propertiesRoot);

                if (orphans.hasChildren)
                    m_Root.AddChild(orphans);

                return m_Root;
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;

            protected override void SingleClickedItem(int id)
            {
                base.SingleClickedItem(id);
            }

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                base.SelectionChanged(selectedIds);
                if(selectedIds.Count > 0)
                {
                    Select(selectedIds[0]);
                }
            }

            void Select(int id)
            {
                var item = FindItem(id, m_Root) as VFXNavigatorTreeViewItem;
                if (item != null && item.nodeui != null)
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

            public VFXNavigatorTreeViewItem(int id, int depth, VFXNodeUI nodeui, Texture2D icon, string fallbackName = "") : base(id, depth, string.IsNullOrEmpty(fallbackName) ? nodeui.title : fallbackName)
            {
                this.nodeui = nodeui;
                this.icon = icon;
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
            public static Texture2D systemParticleTypeIcon;
            public static Texture2D systemEventTypeIcon;
            public static Texture2D contextEventTypeIcon;
            public static Texture2D contextSpawnTypeIcon;
            public static Texture2D contextInitializeTypeIcon;
            public static Texture2D contextUpdateTypeIcon;
            public static Texture2D contextOutputTypeIcon;
            public static Texture2D contextTypeIcon;
            public static Texture2D blockTypeIcon;
            public static Texture2D operatorTypeIcon;

            static Styles()
            {
                button = new GUIStyle(EditorStyles.miniButton);
                button.alignment = TextAnchor.MiddleLeft;

                systemParticleTypeIcon =    EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-system-particle.png") as Texture2D;
                systemEventTypeIcon =       EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-system-spawnevent.png") as Texture2D;

                contextEventTypeIcon =      EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-context-event.png") as Texture2D;
                contextSpawnTypeIcon =      EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-context-spawn.png") as Texture2D;
                contextInitializeTypeIcon = EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-context-initialize.png") as Texture2D;
                contextUpdateTypeIcon =     EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-context-update.png") as Texture2D;
                contextOutputTypeIcon =     EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-context-output.png") as Texture2D;

                contextTypeIcon =     EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-type-context.png") as Texture2D;
                blockTypeIcon =     EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-type-block.png") as Texture2D;
                operatorTypeIcon =     EditorGUIUtility.Load("Packages/net.peeweek.vfxgraph-extras/Editor/Icons/icons-type-operator.png") as Texture2D;
            }
        }
    }
}

