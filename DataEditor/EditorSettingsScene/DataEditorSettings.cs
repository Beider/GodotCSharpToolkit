using Godot;
using System;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorSettings : Control, IDataEditorContent
    {
        private CheckBox LoadLocalData;
        private LineEdit SavePath;
        private Label SaveLabel;

        private IDataEditor Editor;

        private bool Shown = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            var btn = FindNode("BtnSave") as Button;
            btn.Connect("pressed", this, nameof(SavePressed));

            LoadLocalData = FindNode("AutoLoad") as CheckBox;
            SavePath = FindNode("SavePath") as LineEdit;
            SaveLabel = FindNode("SaveLabel") as Label;
            Refresh();
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        public void Refresh()
        {
            LoadLocalData.Pressed = Editor.Preferences.SettingIsLoadLocalData;
            SavePath.Text = Editor.Preferences.SettingLocalSavePath;

            if (!Editor.Preferences.IsPathValid(SavePath.Text))
            {
                SaveLabel.SelfModulate = Colors.Red;
            }
            else
            {
                SaveLabel.SelfModulate = Colors.White;
            }
        }

        private void SavePressed()
        {
            Save();
            Editor.Refresh();
        }

        public void Save()
        {
            Editor.Preferences.SettingIsLoadLocalData = LoadLocalData.Pressed;
            var path = SavePath.Text.Replace("/", "\\");
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            Editor.Preferences.SettingLocalSavePath = path;
            SavePath.Text = path;
        }

        public void SetData(object data, object provider)
        {
            // Do nothing we are a special snowflake editor
        }

        public string GetUniqueId()
        {
            return "";
        }
    }
}