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
        private CheckBox DisableContext;
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
            DisableContext = FindChild("DisableContext") as CheckBox;
            SavePath = FindChild("SavePath") as LineEdit;
            SaveLabel = FindChild("SaveLabel") as Label;
            Refresh();
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        public void Refresh()
        {
            LoadLocalData.ButtonPressed = Editor.Preferences.SettingIsLoadLocalData;
            SavePath.Text = Editor.Preferences.SettingLocalSavePath;
            DisableContext.ButtonPressed = Editor.Preferences.DisableContextScriptEditor;

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
            Editor.Preferences.DisableContextScriptEditor = DisableContext.ButtonPressed;
            var path = FileUtils.NormalizeDirectory(SavePath.Text);
            if (path != Editor.Preferences.SettingLocalSavePath)
            {
                Editor.Preferences.SettingLocalSavePath = path;
                SavePath.Text = path;
            }
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