using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{

    public class EditorRecentTracker : GridContainer
    {
        private class RecentEditorData
        {
            public Action<string> OpenAction { get; }
            public string Name { get; }
            public string UniqueId { get; }
            public Color Color { get; }
            public Button Button { get; set; }

            public RecentEditorData(Action<string> OpenAction, string name, string uniqueId, Color color)
            {
                this.OpenAction = OpenAction;
                this.Name = name;
                this.UniqueId = uniqueId;
                this.Color = color;
            }

            public void Open()
            {
                OpenAction(UniqueId);
            }
        }
        public EditorMainScene Editor;

        private Dictionary<string, RecentEditorData> RecentData = new Dictionary<string, RecentEditorData>();
        private readonly Color PINNED_COLOR = Colors.Beige;

        private RecentEditorData PinnedItem = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

        }

        private void OnEditorOpened(Action<string> openAction, string name, string uniqueId, Color color)
        {
            if (RecentData.ContainsKey(uniqueId))
            {
                MoveToFront(RecentData[uniqueId]);
            }
            else
            {
                var data = new RecentEditorData(openAction, name, uniqueId, color);
                var btn = new Button();
                btn.Text = name.Length > 15 ? $"{name.Substr(0, 13)}..." : name;
                btn.SelfModulate = color;
                btn.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                btn.HintTooltip = name;

                var arry = new Godot.Collections.Array();
                arry.Add(uniqueId);
                btn.Connect("pressed", this, nameof(OpenEditor), arry);
                btn.Connect("gui_input", this, nameof(PinButton), arry);
                data.Button = btn;
                RecentData.Add(uniqueId, data);

                MoveToFront(data);
            }
        }

        private void PinButton(InputEvent evnt, string key)
        {
            if (evnt is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == (int)ButtonList.Right)
            {
                if (RecentData.ContainsKey(key))
                {
                    if (PinnedItem != null)
                    {
                        PinnedItem.Button.SelfModulate = PinnedItem.Color;
                    }
                    if (PinnedItem == RecentData[key])
                    {
                        PinnedItem = null;
                    }
                    else
                    {
                        PinnedItem = RecentData[key];
                        MoveChild(PinnedItem.Button, 0);
                        PinnedItem.Button.SelfModulate = PINNED_COLOR;
                        PinnedItem.Open();
                    }
                }
            }
        }

        private void OpenEditor(string key)
        {
            if (RecentData.ContainsKey(key))
            {
                RecentData[key].Open();
            }
        }

        private void MoveToFront(RecentEditorData data)
        {
            if (data.Button.GetParent() == null)
            {
                AddChild(data.Button);
            }

            if (data != PinnedItem)
            {
                MoveChild(data.Button, PinnedItem == null ? 0 : 1);
            }

            if (GetChildren().Count > 10)
            {
                RemoveButton(GetChildren().Count - 1);
            }
        }

        private void RemoveButton(int index)
        {
            var child = GetChild(index);
            if (child == null) { return; }

            child.QueueFree();
            string key = "";
            foreach (var value in RecentData.Values)
            {
                if (value.Button == child)
                {
                    key = value.UniqueId;
                    break;
                }
            }
            if (!key.IsNullOrEmpty())
            {
                RecentData.Remove(key);
            }
        }

        public void SetEditor(EditorMainScene editor)
        {
            this.Editor = editor;
            Editor.OnEditorOpened += OnEditorOpened;
        }
    }
}