using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public static class DataEditorConstants
    {

        //
        // Default colors, can be overriden in prefs see EditorPrefsExtended.cs
        //
        public static readonly Color COLOR_OK = Colors.GreenYellow;
        public static readonly Color COLOR_WARN = Colors.Yellow;
        public static readonly Color COLOR_ERROR = Colors.LightCoral;
        public static readonly Color COLOR_DISABLED = Colors.Gray;
        public static readonly Color COLOR_DEFAULT = Colors.White;
        public static readonly Color COLOR_BG_DEFAULT = Colors.Transparent;
        public static readonly Color COLOR_ISLOCAL = Colors.Cyan;
        public static readonly Color COLOR_MODS = Colors.MediumPurple;
        public static readonly Color COLOR_DELETE = new Color(0.2f, 0, 0, 1);
        public static readonly Color COLOR_NEW = new Color(0.0f, 0.3f, 0.3f, 1);
        public static readonly Color COLOR_MODIFIED = new Color(0.3f, 0.24902f, 0, 1);

        //
        // DEFAULTS
        //
        // The items below can be used to override editor defaults in the preferences.
        //
        // Example (override category default color):
        //      Editor.Preferences.SetTreeItemColor(DataEditorConstants.ROOT_CATEGORIES_COLOR, myColor);
        //
        public const string ROOT_CATEGORIES_COLOR = "Tree_Root_Categories_Color";
        public const string ROOT_CATEGORIES_COLOR_BG = "Tree_Root_Categories_Color_BG";


        //
        // JsonDefWithName metadata
        //

        /// <summary>
        /// All JsonItemWithName will get this metadata added on load.
        /// Contains the name of the mod this came from
        /// </summary>
        public const string METADATA_KEY_MOD = "key_mod_name";
        public const string METADATA_KEY_FEATURE = "key_feature_name";

        public static string GetModName(this JsonDefWithName jDef)
        {
            var val = jDef.GetMetadata(METADATA_KEY_MOD);
            if (val == null) { return ""; }
            return val.ToString();
        }

        public static string GetFeatureName(this JsonDefWithName jDef)
        {
            var val = jDef.GetMetadata(METADATA_KEY_FEATURE);
            if (val == null) { return ""; }
            return val.ToString();
        }

        /// <summary>
        /// Added on load. Contains this editor
        /// </summary>
        public const string METADATA_KEY_EDITOR = "key_editor";

        public static IAbstractJsonEditor GetEditor(this JsonDefWithName jDef)
        {
            var val = jDef.GetMetadata(METADATA_KEY_EDITOR);
            if (val == null) { return null; }
            return (IAbstractJsonEditor)val;
        }


        //
        // ICONS
        //

        private static readonly String EDITOR_ROOT = "res://GodotCSharpToolkit/DataEditor/";

        public static readonly Texture2D ICON_DELETE =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/delete.png") as Texture2D;

        public static readonly Texture2D ICON_REVERT =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/revert.png") as Texture2D;

        public static readonly Texture2D ICON_DUPLICATE =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/duplicate.png") as Texture2D;

        public static readonly Texture2D ICON_EDIT =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/edit.png") as Texture2D;

        public static readonly Texture2D ICON_NEW =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/new.png") as Texture2D;

        public static readonly Texture2D ICON_COPY =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/copy.png") as Texture2D;

        public static readonly Texture2D ICON_PASTE =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/paste.png") as Texture2D;

        public static readonly Texture2D ICON_SEARCH =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/search.png") as Texture2D;

        public static readonly Texture2D ICON_HELP =
                ResourceLoader.Load($"{EDITOR_ROOT}Assets/Icons/help.png") as Texture2D;


        //
        // Scene paths
        //

        public static readonly PackedScene SCENE_SETTINGS =
                ResourceLoader.Load($"{EDITOR_ROOT}EditorSettingsScene/DataEditorSettings.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_CONFIRM =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/ConfirmDialog/DataEditorConfirmDialog.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_TEXT_ENTRY =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/TextEntryDialog/TextEntryDialog.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_ADVANCED_LIST =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/AdvancedListDialog/DataEditorAdvancedListDialog.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_FOLDER_MANAGER =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/FolderManager/FolderManager.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_GENERIC_EDITOR =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/GenericEditorDialog/GenericEditorDialog.tscn") as PackedScene;
        public static readonly PackedScene SCENE_POPUP_MENU =
                ResourceLoader.Load($"{EDITOR_ROOT}PopupMenu/EditorPopupMenu.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_ICON_SELECTION =
                ResourceLoader.Load($"{EDITOR_ROOT}Dialogs/IconSelectionDialog/IconSelectionDialog.tscn") as PackedScene;



        private static readonly PackedScene SCENE_INPUT_TEXT =
                        ResourceLoader.Load($"{EDITOR_ROOT}Input/InputText/DataEditorInputText.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_COMBO =
                ResourceLoader.Load($"{EDITOR_ROOT}Input/InputCombo/DataEditorInputCombo.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_LIST =
                ResourceLoader.Load($"{EDITOR_ROOT}Input/InputList/DataEditorInputList.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_TREE =
                ResourceLoader.Load($"{EDITOR_ROOT}Input/InputTree/DataEditorInputTree.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_CHECKBOX =
                ResourceLoader.Load($"{EDITOR_ROOT}Input/InputCheckbox/DataEditorInputCheckbox.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_BUTTON =
                ResourceLoader.Load($"{EDITOR_ROOT}Input/InputButton/DataEditorInputButton.tscn") as PackedScene;
        private static readonly PackedScene SCENE_EDITOR_CONTAINER =
                ResourceLoader.Load($"{EDITOR_ROOT}JsonEditor/EditorContainer/EditorContainer.tscn") as PackedScene;


        private static readonly PackedScene SCENE_GENERIC_EDITOR =
                        ResourceLoader.Load($"{EDITOR_ROOT}JsonEditor/JsonGenericEditor.tscn") as PackedScene;



        public static EditorContainer CreateEditorContainer()
        {
            return DataEditorConstants.SCENE_EDITOR_CONTAINER.Instantiate() as EditorContainer;
        }

        public static DataEditorInputText CreateInputText()
        {
            return DataEditorConstants.SCENE_INPUT_TEXT.Instantiate() as DataEditorInputText;
        }

        public static DataEditorInputCombo CreateInputCombo()
        {
            return DataEditorConstants.SCENE_INPUT_COMBO.Instantiate() as DataEditorInputCombo;
        }

        public static DataEditorInputList CreateInputList()
        {
            return DataEditorConstants.SCENE_INPUT_LIST.Instantiate() as DataEditorInputList;
        }

        public static DataEditorInputCheckbox CreateInputCheckbox()
        {
            return DataEditorConstants.SCENE_INPUT_CHECKBOX.Instantiate() as DataEditorInputCheckbox;
        }

        public static DataEditorInputButton CreateInputButton()
        {
            return DataEditorConstants.SCENE_INPUT_BUTTON.Instantiate() as DataEditorInputButton;
        }

        public static DataEditorInputTree CreateInputTree()
        {
            return DataEditorConstants.SCENE_INPUT_TREE.Instantiate() as DataEditorInputTree;
        }

        public static EditorPopupMenu CreatePopupMenu(Node parent)
        {
            var menu = DataEditorConstants.SCENE_POPUP_MENU.Instantiate() as EditorPopupMenu;
            parent.AddChild(menu);
            return menu;
        }

        public static JsonGenericEditor CreateJsonGenericEditor()
        {
            return DataEditorConstants.SCENE_GENERIC_EDITOR.Instantiate() as JsonGenericEditor;
        }

        /// <summary>
        /// Can override to provide item colors if you want
        /// </summary>
        public static Color GetItemColor(JsonDefWithName item, IDataEditor editor, bool isForeground)
        {
            if (!isForeground)
            {
                if (item.IsTaggedForDelete) { return editor.Preferences.GetDeleteColor(); }
                else if (item.IsInvalid) { return editor.Preferences.GetErrorColor(); }
                else if (item.IsNew) { return editor.Preferences.GetNewColor(); }
                else if (item.IsModified) { return editor.Preferences.GetModifiedColor(); }
                return editor.Preferences.GetDefaultBgColor();
            }
            if (item.IsLocal())
            {
                return editor.Preferences.GetIsLocalColor();
            }
            return editor.Preferences.GetDefaultColor();
        }
    }
}