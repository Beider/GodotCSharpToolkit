using Godot;
using System;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// An extension for accessability methods
    /// </summary>
    public partial class EditorPrefsExtended : EditorPrefsStorage
    {
        public event Action OnPrefsChanged = delegate { };

        private const string KEY_COLOR_DEFAULT = "default_tree_item_color";
        private const string KEY_COLOR_BG_DEFAULT = "default_bg_tree_item_color";
        private const string KEY_COLOR_ISLOCAL = "islocal_tree_item_color";
        private const string KEY_COLOR_DELETE = "delete_tree_item_color";
        private const string KEY_COLOR_NEW = "new_tree_item_color";
        private const string KEY_COLOR_MODIFIED = "modified_tree_item_color";
        private const string KEY_COLOR_ERROR = "error_tree_item_color";
        private const string KEY_COLOR_MODS = "mods_tree_item_color";
        private const string PREF_EDITOR_SORT = "sort_tree_items";
        private const string PREF_EDITOR_IS_LOCAL_ONLY = "is_local_only";
        private const string PREFIX_TREE_COLLAPSED_STATE_PREF = "tree_col_state_";
        private const string PREFIX_TREE_COLOR_PREF = "tree_color_";
        private const string SETTING_AUTOLOAD = "editor_pref_autoload";
        private const string SPLIT_OFFSET = "editor_pref_split_offset";
        private const string SETTING_SAVE_PATH = "editor_pref_save_path";
        private const string SETTING_DISABLE_VS_CONTEXT = "editor_disable_context";
        public EditorPrefsExtended(string path = null) : base(path)
        {

        }

        public void SetTreeItemColor(string name, Color value)
        {
            SetValue($"{PREFIX_TREE_COLOR_PREF}{name}", value);
        }

        public Color GetTreeItemColor(string name, Color defaultValue)
        {
            return GetValue($"{PREFIX_TREE_COLOR_PREF}{name}", defaultValue);
        }

        public void SetTreeItemCollapsedState(TreeItem item)
        {
            SetValue($"{PREFIX_TREE_COLLAPSED_STATE_PREF}{GetItemPath(item)}", item.Collapsed);
        }

        public bool GetTreeItemCollapsedState(TreeItem item, bool defaultValue)
        {
            return GetValue($"{PREFIX_TREE_COLLAPSED_STATE_PREF}{GetItemPath(item)}", defaultValue);
        }

        private string GetItemPath(TreeItem item)
        {
            var name = item.GetText(0);
            var parent = item.GetParent();
            while (parent != null)
            {
                name = $"{parent.GetText(0)}.{name}";
                parent = parent.GetParent();
            }
            return name;
        }

        public bool IsPathValid(string path)
        {
            if (path.IsNullOrEmpty()) { return false; }
            if (path.StartsWith("user:")) { return true; }
            if (path.Equals("/") || path.Equals("\\")) { return false; }
            return DirAccess.DirExistsAbsolute(path);
        }

        #region Settings

        public int SettingEditorSplitOffset
        {
            get { return GetValue(SPLIT_OFFSET, 0); }
            set { SetValue(SPLIT_OFFSET, value); }
        }

        public bool SettingIsLoadLocalData
        {
            get { return GetValue(SETTING_AUTOLOAD, true); }
            set { SetValue(SETTING_AUTOLOAD, value); }
        }

        public string SettingLocalSavePath
        {
            get { return GetValue(SETTING_SAVE_PATH, ""); }
            set { SetValue(SETTING_SAVE_PATH, value); }
        }

        public bool DisableContextScriptEditor
        {
            get { return GetValue(SETTING_DISABLE_VS_CONTEXT, false); }
            set { SetValue(SETTING_DISABLE_VS_CONTEXT, value); }
        }

        public bool PrefSortTree
        {
            get { return GetValue(PREF_EDITOR_SORT, true); }
            set { SetValue(PREF_EDITOR_SORT, value); OnPrefsChanged(); }
        }

        public bool PrefIsLocalOnly
        {
            get { return GetValue(PREF_EDITOR_IS_LOCAL_ONLY, false); }
            set { SetValue(PREF_EDITOR_IS_LOCAL_ONLY, value); OnPrefsChanged(); }
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
            return GetValue(KEY_COLOR_DEFAULT, DataEditorConstants.COLOR_DEFAULT);
        }

        public Color GetDefaultBgColor()
        {
            return GetValue(KEY_COLOR_BG_DEFAULT, DataEditorConstants.COLOR_BG_DEFAULT);
        }

        public Color GetIsLocalColor()
        {
            return GetValue(KEY_COLOR_ISLOCAL, DataEditorConstants.COLOR_ISLOCAL);
        }

        public Color GetDeleteColor()
        {
            return GetValue(KEY_COLOR_DELETE, DataEditorConstants.COLOR_DELETE);
        }

        public Color GetNewColor()
        {
            return GetValue(KEY_COLOR_NEW, DataEditorConstants.COLOR_NEW);
        }

        public Color GetModifiedColor()
        {
            return GetValue(KEY_COLOR_MODIFIED, DataEditorConstants.COLOR_MODIFIED);
        }

        public Color GetErrorColor()
        {
            return GetValue(KEY_COLOR_ERROR, DataEditorConstants.COLOR_ERROR);
        }

        public Color GetModColor()
        {
            return GetValue(KEY_COLOR_MODS, DataEditorConstants.COLOR_MODS);
        }

        #endregion
    }
}