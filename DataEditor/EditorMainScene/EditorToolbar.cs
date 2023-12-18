using Godot;
using System;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorToolbar : Panel
    {
        private EditorMainScene Editor;
        private Button BtnSettings;
        private Button BtnSave;
        private Button BtnSort;
        private Button BtnDisplayName;
        private Button BtnLocalOnly;
        private Button BtnFolderManager;
        private Button BtnRefresh;
        private Button BtnAddMod;
        private Button BtnClose;
        private Button BtnBrowse;
        private Button BtnSearch;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            BtnSettings = FindChild("BtnSettings") as Button;
            BtnSettings.Connect("pressed", new Callable(this, nameof(OnSettingsPressed)));

            BtnSave = FindChild("BtnSave") as Button;
            BtnSave.Connect("pressed", new Callable(this, nameof(OnSavePressed)));

            BtnSort = FindChild("BtnSort") as Button;
            BtnSort.Connect("pressed", new Callable(this, nameof(OnSortPressed)));

            BtnDisplayName = FindChild("BtnDisplayName") as Button;
            BtnDisplayName.Connect("pressed", new Callable(this, nameof(OnToggleNamesPressed)));

            BtnLocalOnly = FindChild("BtnLocalOnly") as Button;
            BtnLocalOnly.Connect("pressed", new Callable(this, nameof(OnLocalOnlyPressed)));

            BtnRefresh = FindChild("BtnRefresh") as Button;
            BtnRefresh.Connect("pressed", new Callable(this, nameof(OnRefreshPressed)));

            BtnAddMod = FindChild("BtnAddMod") as Button;
            BtnAddMod.Connect("pressed", new Callable(this, nameof(OnNewModPressed)));

            BtnClose = FindChild("BtnClose") as Button;
            BtnClose.Connect("pressed", new Callable(this, nameof(OnClosePressed)));

            BtnBrowse = FindChild("BtnBrowse") as Button;
            BtnBrowse.Connect("pressed", new Callable(this, nameof(OnBrowsePressed)));

            BtnSearch = FindChild("BtnSearch") as Button;
            BtnSearch.Connect("pressed", new Callable(this, nameof(OnSearchPressed)));

            BtnFolderManager = FindChild("BtnFolderManager") as Button;
            BtnFolderManager.Connect("pressed", new Callable(this, nameof(OnBtnFolderManagerPressed)));
        }

        public void Init(EditorMainScene editor)
        {
            this.Editor = editor;
            BtnSort.ButtonPressed = Editor.Preferences.PrefSortTree;
            BtnDisplayName.Text = Editor.Preferences.PrefDisplayNameDelegateName;
            BtnLocalOnly.ButtonPressed = Editor.Preferences.PrefIsLocalOnly;

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

        public void OnSearchPressed()
        {
            Editor.NotifyOpenSearch();
        }

        private void OnBrowsePressed()
        {
            Editor.NotifyOpenBrowseDialog();
        }

        private void OnBtnFolderManagerPressed()
        {
            Editor.OpenFolderManager();
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
            Editor.Refresh(true);
        }

        public void OnSortPressed()
        {
            Editor.Preferences.PrefSortTree = !Editor.Preferences.PrefSortTree;
            BtnSort.ButtonPressed = Editor.Preferences.PrefSortTree;
        }

        public void OnToggleNamesPressed()
        {
            Editor.Tree.NextDisplayName();
            BtnDisplayName.Text = Editor.Preferences.PrefDisplayNameDelegateName;
        }

        public void OnLocalOnlyPressed()
        {
            Editor.Preferences.PrefIsLocalOnly = !Editor.Preferences.PrefIsLocalOnly;
            BtnLocalOnly.ButtonPressed = Editor.Preferences.PrefIsLocalOnly;
        }

        private void OnSavePressed()
        {
            Editor.Save();
        }

        public void OnSettingsPressed()
        {
            Control settings = DataEditorConstants.SCENE_SETTINGS.Instantiate() as Control;
            Editor.ShowEditor(settings);
        }
    }
}