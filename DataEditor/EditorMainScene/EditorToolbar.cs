using Godot;
using System;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorToolbar : Panel
    {
        private const string ICON_PATH = "res://GodotCSharpToolkit/DataEditor/Assets/Icons/";
        private const string TEXT_NEW_MOD = "New Mod";
        private const string TEXT_MANAGER = "Manager";
        private const string TEXT_BROWSE = "Browse";
        private const string TEXT_REFRESH = "Refresh";
        private const string TEXT_SAVE_ALL = "Save All";
        private const string TEXT_SETTINGS = "Settings";
        public event Action OnToggleHelpVisible = delegate { };
        private EditorMainScene Editor;
        private Button BtnSort;
        private Button BtnLocalOnly;
        private Button BtnClose;
        private Button BtnHelp;
        private MenuButton BtnMenu;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            BtnSort = FindChild("BtnSort") as Button;
            BtnSort.Connect("pressed", new Callable(this, nameof(OnSortPressed)));

            BtnLocalOnly = FindChild("BtnLocalOnly") as Button;
            BtnLocalOnly.Connect("pressed", new Callable(this, nameof(OnLocalOnlyPressed)));

            BtnClose = FindChild("BtnClose") as Button;
            BtnClose.Connect("pressed", new Callable(this, nameof(OnClosePressed)));

            BtnHelp = FindChild("BtnHelp") as Button;
            BtnHelp.Connect("pressed", new Callable(this, nameof(OnHelpPressed)));

            BtnMenu = FindChild("BtnMenu") as MenuButton;
            FillButtonMenu();
        }

        private void FillButtonMenu()
        {
            var popup = BtnMenu.GetPopup();
            popup.Connect("id_pressed", new Callable(this, nameof(OnPopupMenuPressed)));
            var icon = ResourceLoader.Load($"{ICON_PATH}module.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_NEW_MOD, -1);

            icon = ResourceLoader.Load($"{ICON_PATH}open-book.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_BROWSE, -1);

            icon = ResourceLoader.Load($"{ICON_PATH}manager.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_MANAGER, -1);

            icon = ResourceLoader.Load($"{ICON_PATH}refresh.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_REFRESH, -1);

            icon = ResourceLoader.Load($"{ICON_PATH}save.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_SAVE_ALL, -1);

            icon = ResourceLoader.Load($"{ICON_PATH}settings.png") as Texture2D;
            popup.AddIconItem(icon, TEXT_SETTINGS, -1);
        }

        private void OnPopupMenuPressed(int index)
        {
            string text = BtnMenu.GetPopup().GetItemText(index);
            if (TEXT_NEW_MOD.Equals(text)) { OnNewModPressed(); }
            else if (TEXT_MANAGER.Equals(text)) { OnBtnFolderManagerPressed(); }
            else if (TEXT_BROWSE.Equals(text)) { OnBrowsePressed(); }
            else if (TEXT_REFRESH.Equals(text)) { OnRefreshPressed(); }
            else if (TEXT_SAVE_ALL.Equals(text)) { OnSavePressed(); }
            else if (TEXT_SETTINGS.Equals(text)) { OnSettingsPressed(); }
        }

        public void Init(EditorMainScene editor)
        {
            this.Editor = editor;
            BtnSort.ButtonPressed = Editor.Preferences.PrefSortTree;
            BtnLocalOnly.ButtonPressed = Editor.Preferences.PrefIsLocalOnly;

            if (!Editor.Preferences.IsPathValid(Editor.Preferences.SettingLocalSavePath))
            {
                OnSettingsPressed();
            }
        }

        private void OnClosePressed()
        {
            Editor.Close();
        }

        public void OnBrowsePressed()
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

        public void OnHelpPressed()
        {
            OnToggleHelpVisible();
        }

        public void OnSortPressed()
        {
            Editor.Preferences.PrefSortTree = !Editor.Preferences.PrefSortTree;
            BtnSort.ButtonPressed = Editor.Preferences.PrefSortTree;
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