using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// Simple callback used on button press
    /// </summary>
    /// <param name="btn">The button</param>
    public delegate void DebugMenuAction(Button btn, object[] parameters);

    /// <summary>
    /// Used to validate a dialog
    /// </summary>
    /// <param name="dialogId">The ID of the dialog</param>
    /// <param name="values">The list of values, keys are the fieldNames</param>
    /// <returns>An empty string or null if all is ok, some text if there is an error</returns>
    public delegate string DebugDialogValidatorCallback(int dialogId, Dictionary<string, object> values);
    public delegate bool DebugDialogInitialValueBoolCallback(int dialogId, string fieldName);
    public delegate string DebugDialogInitialValueStringCallback(int dialogId, string fieldName);
    public delegate List<String> DebugDialogListValuesCallback(int dialogId, string fieldName);

    public partial class DebugButtonMenu : Control
    {
        private class ButtonInfo
        {
            public Button Button { get; }
            public Node Node { get; }
            public DebugMenuEntry Attribute { get; }
            public DebugMenuAction Action { get; }
            public DebugMenuAction RefreshAction { get; }
            public List<DebugMenuEntry> DialogEntries { get; }
            public object[] Parameters { get; }

            public ButtonInfo(Button btn, Node node, DebugMenuEntry entry, DebugMenuAction action, DebugMenuAction refreshAction, object[] parameters)
            {
                this.Button = btn;
                this.Node = node;
                this.Attribute = entry;
                this.Action = action;
                this.RefreshAction = refreshAction;
                this.DialogEntries = new List<DebugMenuEntry>();
                this.Parameters = parameters;
            }
        }
        private static DebugButtonMenu Instance;

        private Dictionary<Node, List<ButtonInfo>> ButtonRegistry = new Dictionary<Node, List<ButtonInfo>>();

        private Dictionary<int, GridContainer> Columns = new Dictionary<int, GridContainer>();

        private Dictionary<string, GridContainer> Groups = new Dictionary<string, GridContainer>();


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

        }

        public override void _EnterTree()
        {
            Instance = this;

            for (int i = 1; i < 5; i++)
            {
                GridContainer node = FindChild($"Column{i}") as GridContainer;
                Columns.Add(i, node);
                node.ClearNodeChildren();
            }
        }

        public static void RefreshAllCallbackButtons()
        {
            foreach (Node node in Instance.ButtonRegistry.Keys)
            {
                foreach (ButtonInfo btn in Instance.ButtonRegistry[node])
                {
                    if (btn.RefreshAction != null)
                    {
                        btn.RefreshAction.Invoke(btn.Button, btn.Parameters);
                    }
                }
            }

        }

        private static Button FindButtonInCategoryByName(string name, string category)
        {
            GridContainer catRoot = Instance.GetCategoryRoot(category);
            foreach (Node child in catRoot.GetChildren())
            {
                if (child is Button && ((Button)child).Text.Equals(name))
                {
                    return child as Button;
                }
            }
            return null;
        }

        public static void AddSimpleButton(string text, DebugMenuEntrySimple atr, Node owningNode, DebugMenuAction action)
        {
            Button btn = AddSimpleButton(owningNode, atr.Category, atr.DialogId, text, atr.ButtonColor, atr.CloseOnClick, action);
            Instance.RegisterActionButton(owningNode, btn, atr, action, atr.Parameters);
        }

        public static void AddToggleButton(string text, DebugMenuEntryToggle atr, Node owningNode, DebugMenuAction action)
        {
            Button btn = AddSimpleButton(owningNode, atr.Category, 0, text, Colors.White, false, action);
            Instance.RegisterActionButton(owningNode, btn, atr, action, new object[] { });
            btn.Modulate = atr.CallGetValueMethod(owningNode) ? Colors.Green : Colors.Red;
        }

        public static void AddCallbackButton(DebugMenuEntryCallback atr, Node owningNode, DebugMenuAction action, DebugMenuAction refreshAction)
        {
            Button btn = AddSimpleButton(owningNode, atr.Category, atr.DialogId, "to be filled", Colors.White, false, action);
            Instance.RegisterActionButton(owningNode, btn, atr, action, atr.Parameters, refreshAction);
            refreshAction.Invoke(btn, atr.Parameters);
        }

        public static void SetCategoryColumn(string category, int column)
        {
            GridContainer gc = Instance.GetCategoryRoot(category);
            if (gc.GetParent().Name.Equals($"Column{column}"))
            {
                return;
            }
            gc.GetParent().RemoveChild(gc);
            Instance.Columns[column].AddChild(gc);
        }

        public static void RemoveButtonsForNode(Node node)
        {
            Instance.ClearAllActionButtons(node);
        }

        private static Button AddSimpleButton(Node node, string category, int dialogId, string text, Color color, bool closeOnClick, DebugMenuAction action, int column = 1)
        {
            Button btn = new()
            {
                Text = text,
                SizeFlagsHorizontal = SizeFlags.Expand & SizeFlags.Fill,
                Modulate = color
            };
            if (dialogId > 0)
            {
                btn.Connect("pressed", Callable.From(() => Instance.DialogButtonPressed(node, btn, closeOnClick, dialogId)));
            }
            else
            {
                btn.Connect("pressed", Callable.From(() => Instance.SimpleButtonPressed(node, btn, closeOnClick)));
            }

            Instance.GetCategoryRoot(category, column).AddChild(btn);
            return btn;
        }

        private GridContainer GetCategoryRoot(string category, int defaultColumn = 1)
        {
            if (Instance.Groups.ContainsKey(category))
            {
                return Instance.Groups[category];
            }

            // Create new
            GridContainer gc = new GridContainer();
            GetColumn(defaultColumn).AddChild(gc);

            Label title = new Label();
            title.CustomMinimumSize = new Vector2(200, 30);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.VerticalAlignment = VerticalAlignment.Center;
            title.Text = category;
            title.Name = "Title";
            gc.AddChild(title);

            Instance.Groups.Add(category, gc);

            return gc;
        }

        /// <summary>
        /// Adds data used by our dialog system for later usage
        /// </summary>
        /// <param name="entry">The entry to add, should be a dialog style entry</param>
        public static void AddDialogEntry(Node node, DebugMenuEntry entry)
        {
            ButtonInfo info = Instance.GetButtonInfo(node, entry.DialogId);
            if (info == null)
            {
                GD.PushError($"Dialog entry came before button entry in node '{node.Name}' for entry '{entry.ButtonText}'");
                return;
            }

            info.DialogEntries.Add(entry);
        }

        /// <summary>
        /// Get all dialog entries for the given menu of the given node
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="dialogId">The entries</param>
        public static List<DebugMenuEntry> GetDialogEntries(Node node, int dialogId)
        {
            ButtonInfo info = Instance.GetButtonInfo(node, dialogId);
            if (info != null)
            {
                return info.DialogEntries;
            }

            return new List<DebugMenuEntry>();
        }

        private GridContainer GetColumn(int column)
        {
            if (Columns.ContainsKey(column))
            {
                return Columns[column];
            }
            return Columns[1];
        }

        private void RegisterActionButton(Node node, Button button, DebugMenuEntry entry,
                                          DebugMenuAction invokeAction, object[] parameters, DebugMenuAction refreshAction = null)
        {
            if (!ButtonRegistry.ContainsKey(node))
            {
                ButtonRegistry.Add(node, new List<ButtonInfo>());
            }

            ButtonInfo info = new ButtonInfo(button, node, entry, invokeAction, refreshAction, parameters);
            ButtonRegistry[node].Add(info);
        }

        private void ClearAllActionButtons(Node node)
        {
            if (ButtonRegistry.ContainsKey(node))
            {
                foreach (var info in ButtonRegistry[node])
                {
                    info.Button.QueueFree();
                }
                ButtonRegistry.Remove(node);
            }
        }

        private void DialogButtonPressed(Node node, Button btn, bool closeOnClick, int dialogId)
        {
            ButtonInfo btnInfo = GetButtonInfo(node, btn);
            if (btnInfo != null)
            {
                DebugMenuDialog.ShowDialog(btn.Text, btn.Modulate, btnInfo.Node, btnInfo.Attribute.DialogId, btnInfo.DialogEntries);
            }
        }

        public static bool DialogOkPressed(Node node, int dialogId, object[] parameters)
        {
            ButtonInfo info = Instance.GetButtonInfo(node, dialogId);
            if (info != null)
            {
                try
                {
                    info.Action.Invoke(info.Button, info.Parameters.Concat(parameters).ToArray());
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed on dialog ok pressed", ex);
                    return false;
                }
            }
            return false;
        }

        private void SimpleButtonPressed(Node node, Button btn, bool closeOnClick)
        {
            ButtonInfo btnInfo = GetButtonInfo(node, btn);
            if (btnInfo != null)
            {
                try
                {
                    btnInfo.Action.Invoke(btn, btnInfo.Parameters);
                }
                catch (Exception ex)
                {
                    Logger.Error("Button press failed", ex);
                    return;
                }

                if (closeOnClick)
                {
                    Visible = false;
                }
                else
                {
                    RefreshAllCallbackButtons();
                }
            }
        }

        private ButtonInfo GetButtonInfo(Node node, Button btn)
        {
            ButtonInfo btnInfo = null;
            if (Instance.ButtonRegistry.ContainsKey(node))
            {
                foreach (ButtonInfo info in Instance.ButtonRegistry[node])
                {
                    if (info.Button == btn)
                    {
                        btnInfo = info;
                        break;
                    }
                }
            }
            if (btnInfo == null)
            {
                Logger.Info($"Could not find ButtonInfo for node '{node.Name}' and button '{btn.Text}'");
            }

            return btnInfo;
        }

        private ButtonInfo GetButtonInfo(Node node, int dialogId)
        {
            ButtonInfo btnInfo = null;
            if (Instance.ButtonRegistry.ContainsKey(node))
            {
                foreach (ButtonInfo info in Instance.ButtonRegistry[node])
                {
                    if (info.Attribute != null && info.Attribute.DialogId == dialogId)
                    {
                        btnInfo = info;
                        break;
                    }
                }
            }
            if (btnInfo == null)
            {
                Logger.Info($"Could not find ButtonInfo for node '{node.Name}' and dialog '{dialogId}'");
            }

            return btnInfo;

        }

    }
}
