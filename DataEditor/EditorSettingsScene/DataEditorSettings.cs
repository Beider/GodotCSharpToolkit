using Godot;
using System;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class DataEditorSettings : Control, IDataEditorContent
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
            var btn = FindChild("BtnSave") as Button;
            btn.Connect("pressed", new Callable(this, nameof(SavePressed)));

            LoadLocalData = FindChild("AutoLoad") as CheckBox;
            WebMode = FindChild("WebMode") as CheckBox;
            WebMode.Connect("pressed", new Callable(this, nameof(WebModePressed)));
            SavePath = FindChild("SavePath") as LineEdit;
            SaveLabel = FindChild("SaveLabel") as Label;
            Refresh();
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        private void WebModePressed()
        {
            SavePath.Editable = !WebMode.ButtonPressed;
        }

        public void Refresh()
        {
            LoadLocalData.ButtonPressed = Editor.Preferences.SettingIsLoadLocalData;
            SavePath.Text = Editor.Preferences.SettingLocalSavePath;
            if (OS.HasFeature("web"))
            {
                Editor.Preferences.SettingWebMode = true;
                WebMode.ButtonPressed = true;
                WebMode.Disabled = true;
                WebModePressed();
                Save();
            }
            else
            {
                WebMode.ButtonPressed = Editor.Preferences.SettingWebMode;
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
            Editor.Preferences.SettingIsLoadLocalData = LoadLocalData.ButtonPressed;
            Editor.Preferences.SettingWebMode = WebMode.ButtonPressed;
            SetupWebMode();
            var path = FileUtils.NormalizeDirectory(SavePath.Text);
            if (path != Editor.Preferences.SettingLocalSavePath)
            {
                Editor.Preferences.SettingLocalSavePath = path;
                SavePath.Text = path;
            }
        }

        private void SetupWebMode()
        {
            if (!WebMode.ButtonPressed) { return; };
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