using Godot;
using System;
namespace GodotCSharpToolkit.Editor
{
    public class EditorToolbar : Panel
    {
        private IDataEditor Editor;
        private Button BtnSettings;
        private Button BtnSave;
        private Button BtnSort;
        private Button BtnDisplayName;
        private Button BtnLocalOnly;
        private Button BtnRefresh;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            BtnSettings = FindNode("BtnSettings") as Button;
            BtnSettings.Connect("pressed", this, nameof(OnSettingsPressed));

            BtnSave = FindNode("BtnSave") as Button;
            BtnSave.Connect("pressed", this, nameof(OnSavePressed));

            BtnSort = FindNode("BtnSort") as Button;
            BtnSort.Connect("pressed", this, nameof(OnSortPressed));

            BtnDisplayName = FindNode("BtnDisplayName") as Button;
            BtnDisplayName.Connect("pressed", this, nameof(OnToggleNamesPressed));

            BtnLocalOnly = FindNode("BtnLocalOnly") as Button;
            BtnLocalOnly.Connect("pressed", this, nameof(OnLocalOnlyPressed));

            BtnRefresh = FindNode("BtnRefresh") as Button;
            BtnRefresh.Connect("pressed", this, nameof(OnRefreshPressed));
        }

        public void Init(IDataEditor editor)
        {
            this.Editor = editor;
            BtnSort.Pressed = Editor.Preferences.PrefSortTree;
            BtnDisplayName.Pressed = Editor.Preferences.PrefUseDisplayNames;
            BtnLocalOnly.Pressed = Editor.Preferences.PrefIsLocalOnly;

            BtnSave.Disabled = !Editor.Preferences.SettingIsLoadLocalData;
            BtnRefresh.Disabled = !Editor.Preferences.SettingIsLoadLocalData;
        }

        private void OnRefreshPressed()
        {
            Editor.Refresh();
        }

        private void OnSortPressed()
        {
            Editor.Preferences.PrefSortTree = !Editor.Preferences.PrefSortTree;
        }

        private void OnToggleNamesPressed()
        {
            Editor.Preferences.PrefUseDisplayNames = !Editor.Preferences.PrefUseDisplayNames;
        }

        private void OnLocalOnlyPressed()
        {
            Editor.Preferences.PrefIsLocalOnly = !Editor.Preferences.PrefIsLocalOnly;
        }

        private void OnSavePressed()
        {
            Editor.Save();
        }

        private void OnSettingsPressed()
        {
            Control settings = DataEditorConstants.SCENE_SETTINGS.Instance() as Control;
            Editor.ShowEditor(settings);
        }
    }
}