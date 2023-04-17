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

        public static string GetModName(this JsonDefWithName jDef)
        {
            var val = jDef.GetMetadata(METADATA_KEY_MOD);
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

        public static readonly Texture ICON_DELETE =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/delete.png") as Texture;

        public static readonly Texture ICON_REVERT =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/revert.png") as Texture;

        public static readonly Texture ICON_DUPLICATE =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/duplicate.png") as Texture;

        public static readonly Texture ICON_EDIT =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/edit.png") as Texture;

        public static readonly Texture ICON_NEW =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/new.png") as Texture;

        public static readonly Texture ICON_COPY =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/copy.png") as Texture;

        public static readonly Texture ICON_PASTE =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Assets/Icons/paste.png") as Texture;

        //
        // Scene paths
        //

        public static readonly PackedScene SCENE_SETTINGS =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/EditorSettingsScene/DataEditorSettings.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_CONFIRM =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Dialogs/ConfirmDialog/DataEditorConfirmDialog.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_TEXT_ENTRY =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Dialogs/TextEntryDialog/TextEntryDialog.tscn") as PackedScene;

        public static readonly PackedScene SCENE_DIALOG_ADVANCED_LIST =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Dialogs/AdvancedListDialog/DataEditorAdvancedListDialog.tscn") as PackedScene;


        private static readonly PackedScene SCENE_INPUT_TEXT =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputText/DataEditorInputText.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_COMBO =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputCombo/DataEditorInputCombo.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_LIST =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputList/DataEditorInputList.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_TREE =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputTree/DataEditorInputTree.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_CHECKBOX =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputCheckbox/DataEditorInputCheckbox.tscn") as PackedScene;
        private static readonly PackedScene SCENE_INPUT_BUTTON =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/Input/InputButton/DataEditorInputButton.tscn") as PackedScene;
        private static readonly PackedScene SCENE_EDITOR_CONTAINER =
                ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/JsonEditor/EditorContainer/EditorContainer.tscn") as PackedScene;


        private static readonly PackedScene SCENE_GENERIC_EDITOR =
                        ResourceLoader.Load("res://GodotCSharpToolkit/DataEditor/JsonEditor/JsonGenericEditor.tscn") as PackedScene;



        public static EditorContainer CreateEditorContainer()
        {
            return DataEditorConstants.SCENE_EDITOR_CONTAINER.Instance() as EditorContainer;
        }

        public static DataEditorInputText CreateInputText()
        {
            return DataEditorConstants.SCENE_INPUT_TEXT.Instance() as DataEditorInputText;
        }

        public static DataEditorInputCombo CreateInputCombo()
        {
            return DataEditorConstants.SCENE_INPUT_COMBO.Instance() as DataEditorInputCombo;
        }

        public static DataEditorInputList CreateInputList()
        {
            return DataEditorConstants.SCENE_INPUT_LIST.Instance() as DataEditorInputList;
        }

        public static DataEditorInputCheckbox CreateInputCheckbox()
        {
            return DataEditorConstants.SCENE_INPUT_CHECKBOX.Instance() as DataEditorInputCheckbox;
        }

        public static DataEditorInputButton CreateInputButton()
        {
            return DataEditorConstants.SCENE_INPUT_BUTTON.Instance() as DataEditorInputButton;
        }

        public static DataEditorInputTree CreateInputTree()
        {
            return DataEditorConstants.SCENE_INPUT_TREE.Instance() as DataEditorInputTree;
        }

        public static JsonGenericEditor CreateJsonGenericEditor()
        {
            return DataEditorConstants.SCENE_GENERIC_EDITOR.Instance() as JsonGenericEditor;
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