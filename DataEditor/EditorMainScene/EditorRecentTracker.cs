using Godot;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Logging;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{

    public class EditorRecentTracker : GridContainer
    {
        private class RecentEditorData
        {
            public int TypeId { get; }
            public string Name { get; set; }
            public string UniqueId { get; }
            public Color Color { get; set; }
            public Button Button { get; set; }

            public RecentEditorData(int typeId, string name, string uniqueId, Color color)
            {
                this.TypeId = typeId;
                this.Name = name;
                this.UniqueId = uniqueId;
                this.Color = color;
            }
        }
        private const string PREF_KEY = "editor_recent_items";
        private const string SPLIT1 = "?¤_:!:";
        private const string SPLIT2 = ":_:&¤#";
        /// <summary>
        /// Request the opening of the editor
        /// First is unique id, second is type
        /// </summary>
        public event Action<string, int> OnOpenEditorRequest = delegate { };

        public EditorMainScene Editor;

        private const string IMAGE_PATH = "res://GodotCSharpToolkit/DataEditor/Assets/Icons/pin.png";

        private Dictionary<string, RecentEditorData> RecentData = new Dictionary<string, RecentEditorData>();
        private List<RecentEditorData> PinnedItems = new List<RecentEditorData>();

        private Texture PinTexture = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            PinTexture = ResourceLoader.Load(IMAGE_PATH) as Texture;
            PinnedItems.Clear();
            RecentData.Clear();
            CallDeferred(nameof(Load));
        }

        private void Load()
        {
            var settings = Editor.Preferences.GetValue(PREF_KEY, "");
            if (settings.IsNullOrEmpty()) { return; }

            var lines = new List<string>(settings.Split(SPLIT1));
            lines.Sort(SortLine);
            foreach (var line in lines)
            {
                try
                {
                    var lineSettings = line.Split(SPLIT2);
                    var data = AddData(int.Parse(lineSettings[3]), lineSettings[1], lineSettings[2], (Color)GD.Str2Var(lineSettings[4]));
                    if (bool.Parse(lineSettings[5]))
                    {
                        PinItem(data, false);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error parsing stored recent item", ex);
                }
            }
        }

        private int SortLine(string l1, string l2)
        {
            int index = int.Parse(l1.Substr(0, l1.IndexOf(SPLIT2)));
            int index2 = int.Parse(l2.Substr(0, l2.IndexOf(SPLIT2)));
            return index.CompareTo(index2);
        }

        public override void _ExitTree()
        {
            var saveKey = "";
            foreach (var value in RecentData.Values)
            {
                if (saveKey.Length > 0) { saveKey += SPLIT1; }
                bool pinned = PinnedItems.Contains(value);
                saveKey += $"{value.Button.GetIndex()}{SPLIT2}{value.Name}{SPLIT2}{value.UniqueId}{SPLIT2}{value.TypeId}{SPLIT2}{GD.Var2Str(value.Color)}{SPLIT2}{pinned}";
            }
            Editor.Preferences.SetValue(PREF_KEY, saveKey);
            Editor.Preferences.Save();
        }

        private void OnEditorOpened(int typeId, string name, string uniqueId, Color color)
        {
            AddData(typeId, name, uniqueId, color);
        }

        private RecentEditorData AddData(int typeId, string name, string uniqueId, Color color)
        {
            if (RecentData.ContainsKey(uniqueId))
            {
                RecentData[uniqueId].Name = name;
                RecentData[uniqueId].Color = color;
                SetButtonText(RecentData[uniqueId]);
                MoveToFront(RecentData[uniqueId]);
                return RecentData[uniqueId];
            }
            else
            {
                var data = new RecentEditorData(typeId, name, uniqueId, color);
                var btn = new Button();
                data.Button = btn;
                SetButtonText(data);
                btn.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                btn.HintTooltip = name;

                var arry = new Godot.Collections.Array();
                arry.Add(uniqueId);
                btn.Connect("pressed", this, nameof(OpenEditor), arry);
                btn.Connect("gui_input", this, nameof(PinButton), arry);
                btn.Connect("mouse_entered", this, nameof(OnMouseOver), arry);
                btn.Connect("mouse_exited", this, nameof(OnMouseExit), arry);
                RecentData.Add(uniqueId, data);

                MoveToFront(data);
                return data;
            }
        }

        private void OnMouseOver(string uniqueId)
        {
            if (RecentData.ContainsKey(uniqueId))
            {
                SetButtonText(RecentData[uniqueId], true);
            }
        }

        private void OnMouseExit(string uniqueId)
        {
            if (RecentData.ContainsKey(uniqueId))
            {
                SetButtonText(RecentData[uniqueId], false);
            }
        }

        private void SetButtonText(RecentEditorData data, bool full = false)
        {
            data.Button.Text = data.Name.Length > 15 && !full ? $"{data.Name.Substr(0, 13)}..." : data.Name;
            data.Button.SelfModulate = data.Color;
        }

        private void PinButton(InputEvent evnt, string key)
        {
            if (evnt is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == (int)ButtonList.Right)
            {
                if (RecentData.ContainsKey(key))
                {
                    var item = RecentData[key];
                    PinItem(item);
                }
            }
        }

        private void PinItem(RecentEditorData item, bool open = true)
        {
            if (PinnedItems.Contains(item))
            {
                PinnedItems.Remove(item);
                item.Button.Icon = null;
                if (item.Button.GetIndex() < PinnedItems.Count)
                {
                    MoveChild(item.Button, PinnedItems.Count);
                }
            }
            else
            {
                PinnedItems.Add(item);
                MoveChild(item.Button, PinnedItems.Count - 1);
                item.Button.Icon = PinTexture;
                if (open) { Open(item); }
            }
        }

        private void OpenEditor(string key)
        {
            if (RecentData.ContainsKey(key))
            {
                Open(RecentData[key]);
            }
        }

        private void Open(RecentEditorData data)
        {
            OnOpenEditorRequest(data.UniqueId, data.TypeId);
        }

        private void MoveToFront(RecentEditorData data)
        {
            if (data.Button.GetParent() == null)
            {
                AddChild(data.Button);
            }

            if (!PinnedItems.Contains(data))
            {
                MoveChild(data.Button, PinnedItems.Count);
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