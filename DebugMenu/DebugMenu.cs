using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace GodotCSharpToolkit.DebugMenu
{
    public partial class DebugMenu : Node2D
    {
        private class NodeDebugInfo
        {
            public List<MemberInfo> MembersWithAnnotation = new List<MemberInfo>();
            public List<MethodInfo> MethodsWithAnnotation = new List<MethodInfo>();
        }
        public static DebugMenu Instance;
        private static int SequenceNumber = 0;

        [Export]
        public Font OnScreenDebugFont;

        [Export]
        public Theme DebugMenuTheme;

        private OnScreenDebugInterface OnScreenDebugControl;
        private DebugButtonMenu DebugButtonMenu;

        private List<Node> CachedNodes = new List<Node>();

        /// <summary>
        /// All types that have the DebugIncludeClass attribute will be in this list.
        /// Used as a filter so we don't waste time on non-debug menu types.
        /// 
        /// Holds additional info about which members and methods have annotations
        /// so we don't need to check all of them every time.
        /// </summary>
        private Dictionary<Type, NodeDebugInfo> IncludeList = new Dictionary<Type, NodeDebugInfo>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Do not show unless we are in debug mode
            if (!OS.IsDebugBuild())
            {
                QueueFree();
                return;
            }
            Instance = this;
            OnScreenDebugControl = FindNode("OnScreenDebug") as OnScreenDebugInterface;
            DebugButtonMenu = FindNode("DebugButtonMenu") as DebugButtonMenu;
            DebugButtonMenu.Visible = false;
            OnScreenDebugControl.Visible = false;
            if (GetTheme() != null)
            {
                DebugButtonMenu.Theme = GetTheme();
            }

            // Add basic stuff to OnScreenDebug (FPS)
            OnScreenDebugManager.Initialize();
            InitTools();
        }

        public override void _Process(float delta)
        {
            if (CacheBuildDone)
            {
                ProcessCachedNodes();
                SetProcess(false);
            }
        }

        public override void _EnterTree()
        {
            CacheThread = new Thread();
            CacheThread.Start(this, nameof(BuildReflectionCache));
            // This is done in _EnterTree so that we get the signal of other autoloads being added to the tree
            Instance = this;
            OnScreenDebug.Init();
            GetTree().Connect("node_added", this, nameof(OnNodeAdded));
            GetTree().Connect("node_removed", this, nameof(OnNodeRemoved));
        }

        public static Font GetFont()
        {
            return Instance.OnScreenDebugFont;
        }

        public static Theme GetTheme()
        {
            return Instance.DebugMenuTheme;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey)
            {
                InputEventKey eventKey = (InputEventKey)@event;
                bool just_pressed = @event.IsPressed() && !@event.IsEcho();
                if (!just_pressed)
                {
                    return;
                }
                if (eventKey.Scancode == (int)KeyList.F11)
                {
                    OnScreenDebugControl.Visible = !OnScreenDebugControl.Visible;
                }
                else if (eventKey.Scancode == (int)KeyList.F12)
                {
                    DebugButtonMenu.Visible = !DebugButtonMenu.Visible;
                    if (DebugButtonMenu.Visible)
                    {
                        DebugButtonMenu.RefreshAllCallbackButtons();
                    }
                }
            }
        }

        private void ProcessCachedNodes()
        {
            Logging.Logger.Info($"DebugMenu processing a cache of {CachedNodes.Count} nodes");
            CachedNodes.ForEach(n => OnNodeAdded(n));
            CachedNodes.Clear();
        }


        private void OnNodeAdded(Node node)
        {
            if (!CacheBuildDone)
            {
                CachedNodes.Add(node);
                return;
            }

            Type type = node.GetType();
            if (!IncludeList.ContainsKey(type))
            {
                return;
            }

            ProcessMembers(node, type, true);
            ProcessMethods(node, type, true);
        }

        private void OnNodeRemoved(Node node)
        {
            Type type = node.GetType();
            if (CachedNodes.Contains(node))
            {
                CachedNodes.Remove(node);
            }
            if (!IncludeList.ContainsKey(type))
            {
                return;
            }

            ProcessMembers(node, type, false);
            ProcessMethods(node, type, false);
        }

        private string ReplaceName(string name, Node node)
        {
            string newName = name;
            newName = newName.Replace("%node_name%", node.Name);
            if (newName.Contains("%seq%"))
            {
                newName = newName.Replace("%seq%", GetNextSequenceNumber());
            }
            return newName;
        }

        private string GetNextSequenceNumber()
        {
            SequenceNumber++;
            return SequenceNumber.ToString();
        }

        protected void AddMemberInfoToNode(Type type, MemberInfo info)
        {
            if (!IncludeList.ContainsKey(type))
            {
                IncludeList.Add(type, new NodeDebugInfo());
            }

            if (!IncludeList[type].MembersWithAnnotation.Contains(info))
            {
                IncludeList[type].MembersWithAnnotation.Add(info);
            }
        }

        protected void AddMethodInfoToNode(Type type, MethodInfo info)
        {
            if (!IncludeList.ContainsKey(type))
            {
                IncludeList.Add(type, new NodeDebugInfo());
            }

            if (!IncludeList[type].MethodsWithAnnotation.Contains(info))
            {
                IncludeList[type].MethodsWithAnnotation.Add(info);
            }
        }

    }
}
