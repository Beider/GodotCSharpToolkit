using Godot;
using System;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

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
        private Button BtnAddMod;
        private Button BtnClose;

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

            BtnAddMod = FindNode("BtnAddMod") as Button;
            BtnAddMod.Connect("pressed", this, nameof(OnNewModPressed));

            BtnClose = FindNode("BtnClose") as Button;
            BtnClose.Connect("pressed", this, nameof(OnClosePressed));
        }

        public void Init(IDataEditor editor)
        {
            this.Editor = editor;
            BtnSort.Pressed = Editor.Preferences.PrefSortTree;
            BtnDisplayName.Text = Editor.Preferences.PrefDisplayNameDelegateName;
            BtnLocalOnly.Pressed = Editor.Preferences.PrefIsLocalOnly;

            BtnSave.Disabled = !Editor.Preferences.SettingIsLoadLocalData;
            BtnRefresh.Disabled = !Editor.Preferences.SettingIsLoadLocalData;

            BtnDisplayName.Visible = Editor.Tree.DisplayNameDelegates.Count > 1;

            if (!Editor.Preferences.IsPathValid(Editor.Preferences.SettingLocalSavePath))
            {
                OnSettingsPressed();
            }
        }

        private void OnClosePressed()
        {
            Editor.Close();
        }

        public void OnNewModPressed()
        {
            Func<string, bool> nameValidator = (name) =>
                {
                    if (name.IsNullOrEmpty()) { return false; }
                    foreach (var mod in Editor.Tree.ModFolders.Keys)
                    {
                        if (mod.ToLower().Equals(name.ToLower()))
                        {
                            return false;
                        }
                    }
                    return true;
                };
            Editor.ShowTextEntryDialog("Please enter the mod name", "Name", AddNewModule, nameValidator);
        }

        private void AddNewModule(string name, string listValue)
        {
            FileUtils.CreateDirectory(Editor.Preferences.SettingLocalSavePath, name);
            Editor.Tree.RefreshTree(false);
        }

        public void OnRefreshPressed()
        {
            Editor.Refresh();
        }

        public void OnSortPressed()
        {
            Editor.Preferences.PrefSortTree = !Editor.Preferences.PrefSortTree;
            BtnSort.Pressed = Editor.Preferences.PrefSortTree;
        }

        public void OnToggleNamesPressed()
        {
            Editor.Tree.NextDisplayName();
            BtnDisplayName.Text = Editor.Preferences.PrefDisplayNameDelegateName;
        }

        public void OnLocalOnlyPressed()
        {
            Editor.Preferences.PrefIsLocalOnly = !Editor.Preferences.PrefIsLocalOnly;
            BtnLocalOnly.Pressed = Editor.Preferences.PrefIsLocalOnly;
        }

        private void OnSavePressed()
        {
            Editor.Save();
        }

        public void OnSettingsPressed()
        {
            Control settings = DataEditorConstants.SCENE_SETTINGS.Instance() as Control;
            Editor.ShowEditor(settings);
        }
    }
}