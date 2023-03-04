using Godot;
using System;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// An extension for accessability methods
    /// </summary>
    public class EditorPrefsExtended : EditorPrefsStorage
    {
        public event Action OnPrefsChanged = delegate { };

        private const string KEY_COLOR_DEFAULT = "default_tree_item_color";
        private const string KEY_COLOR_BG_DEFAULT = "default_bg_tree_item_color";
        private const string KEY_COLOR_ISLOCAL = "islocal_tree_item_color";
        private const string KEY_COLOR_DELETE = "delete_tree_item_color";
        private const string KEY_COLOR_NEW = "new_tree_item_color";
        private const string KEY_COLOR_MODIFIED = "modified_tree_item_color";
        private const string KEY_COLOR_ERROR = "error_tree_item_color";
        private const string PREF_EDITOR_SORT = "sort_tree_items";
        private const string PREF_EDITOR_SHOW_DISPLAY_NAMES = "show_display_names";
        private const string PREF_EDITOR_IS_LOCAL_ONLY = "is_local_only";
        private const string PREFIX_TREE_COLLAPSED_STATE_PREF = "tree_col_state_";
        private const string PREFIX_TREE_COLOR_PREF = "tree_color_";
        private const string SETTING_AUTOLOAD = "editor_pref_autoload";
        private const string SETTING_SAVE_PATH = "editor_pref_save_path";
        public EditorPrefsExtended(string path = null, bool autoSave = true) : base(path, autoSave)
        {

        }

        public void SetTreeItemColor(string name, Color value)
        {
            SetColorValue($"{PREFIX_TREE_COLOR_PREF}{name}", value);
        }

        public Color GetTreeItemColor(string name, Color defaultValue)
        {
            return GetColorValue($"{PREFIX_TREE_COLOR_PREF}{name}", defaultValue);
        }

        public void SetTreeItemCollapsedState(string name, bool state)
        {
            SetBoolValue($"{PREFIX_TREE_COLLAPSED_STATE_PREF}{name}", state);
        }

        public bool GetTreeItemCollapsedState(string name, bool defaultValue)
        {
            return GetBoolValue($"{PREFIX_TREE_COLLAPSED_STATE_PREF}{name}", defaultValue);
        }

        public bool IsPathValid(string path)
        {
            if (path.IsNullOrEmpty()) { return false; }
            if (!System.IO.Directory.Exists(path)) { return false; }

            return true;
        }

        #region Settings

        public bool SettingIsLoadLocalData
        {
            get { return GetBoolValue(SETTING_AUTOLOAD, true); }
            set { SetBoolValue(SETTING_AUTOLOAD, value); }
        }

        public string SettingLocalSavePath
        {
            get { return GetValue(SETTING_SAVE_PATH, ""); }
            set { SetValue(SETTING_SAVE_PATH, value); }
        }

        public bool PrefSortTree
        {
            get { return GetBoolValue(PREF_EDITOR_SORT, true); }
            set { SetBoolValue(PREF_EDITOR_SORT, value); OnPrefsChanged(); }
        }

        public bool PrefUseDisplayNames
        {
            get { return GetBoolValue(PREF_EDITOR_SHOW_DISPLAY_NAMES, true); }
            set { SetBoolValue(PREF_EDITOR_SHOW_DISPLAY_NAMES, value); OnPrefsChanged(); }
        }

        public bool PrefIsLocalOnly
        {
            get { return GetBoolValue(PREF_EDITOR_IS_LOCAL_ONLY, false); }
            set { SetBoolValue(PREF_EDITOR_IS_LOCAL_ONLY, value); OnPrefsChanged(); }
        }

        /// <summary>
        /// Checks that load local data is set and that the path is valid
        /// </summary>
        public bool ShouldUseLocalPath()
        {
            return SettingIsLoadLocalData && IsPathValid(SettingLocalSavePath);
        }

        #endregion

        #region Colors

        public Color GetDefaultColor()
        {
            return GetColorValue(KEY_COLOR_DEFAULT, DataEditorConstants.COLOR_DEFAULT);
        }

        public Color GetDefaultBgColor()
        {
            return GetColorValue(KEY_COLOR_BG_DEFAULT, DataEditorConstants.COLOR_BG_DEFAULT);
        }

        public Color GetIsLocalColor()
        {
            return GetColorValue(KEY_COLOR_ISLOCAL, DataEditorConstants.COLOR_ISLOCAL);
        }

        public Color GetDeleteColor()
        {
            return GetColorValue(KEY_COLOR_DELETE, DataEditorConstants.COLOR_DELETE);
        }

        public Color GetNewColor()
        {
            return GetColorValue(KEY_COLOR_NEW, DataEditorConstants.COLOR_NEW);
        }

        public Color GetModifiedColor()
        {
            return GetColorValue(KEY_COLOR_MODIFIED, DataEditorConstants.COLOR_MODIFIED);
        }

        public Color GetErrorColor()
        {
            return GetColorValue(KEY_COLOR_ERROR, DataEditorConstants.COLOR_ERROR);
        }

        #endregion
    }
}