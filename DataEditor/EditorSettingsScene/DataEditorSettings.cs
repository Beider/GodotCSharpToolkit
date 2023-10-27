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
        private CheckBox WebMode;
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
            WebMode = FindNode("WebMode") as CheckBox;
            WebMode.Connect("pressed", this, nameof(WebModePressed));
            SavePath = FindNode("SavePath") as LineEdit;
            SaveLabel = FindNode("SaveLabel") as Label;
            Refresh();
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        private void WebModePressed()
        {
            SavePath.Editable = !WebMode.Pressed;
        }

        public void Refresh()
        {
            LoadLocalData.Pressed = Editor.Preferences.SettingIsLoadLocalData;
            SavePath.Text = Editor.Preferences.SettingLocalSavePath;
            if (OS.HasFeature("web"))
            {
                Editor.Preferences.SettingWebMode = true;
                WebMode.Pressed = true;
                WebMode.Disabled = true;
                WebModePressed();
                Save();
            }
            else
            {
                WebMode.Pressed = Editor.Preferences.SettingWebMode;
                WebModePressed();
            }

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
            Editor.Preferences.SettingWebMode = WebMode.Pressed;
            SetupWebMode();
            var path = FileUtils.NormalizeDirectory(SavePath.Text);
            Editor.Preferences.SettingLocalSavePath = path;
            SavePath.Text = path;
        }

        private void SetupWebMode()
        {
            if (!WebMode.Pressed) { return; };
            FileUtils.CreateDirectory("user://", "mods");
            SavePath.Text = "user://mods/";
        }

        public void SetData(object data, object provider)
        {
            // Do nothing we are a special snowflake editor
        }

        public string GetUniqueId()
        {
            return "";
        }

        public int GetTypeId()
        {
            return 0;
        }

        public Color GetColor()
        {
            return Colors.Red;
        }

        public string GetContentName()
        {
            return "Settings";
        }

        public string GetContentID()
        {
            return "Settings";
        }
    }
}