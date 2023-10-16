using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;
using System.Globalization;

namespace GodotCSharpToolkit.Editor
{
    public class JsonGenericEditorInput
    {
        /// <summary>
        /// Called when we want to save
        /// </summary>
        /// <param name="name">The name of this field</param>
        /// <param name="dataObject">The data object related to this field</param>
        /// <param name="currentValue">The current value that the user has entered / selected</param>
        public delegate void OnSaveCallback(string name, JsonDefWithName dataObject, object currentValue);

        /// <summary>
        /// Called when we want to validate a value
        /// </summary>
        /// <param name="name">The name of this field</param>
        /// <param name="dataObject">The data object related to this field</param>
        /// <param name="currentValue">The current value that the user has entered / selected</param>
        /// <value>true if valid, false if not</value>
        public delegate bool OnValidateCallback(string name, JsonDefWithName dataObject, object currentValue);

        /// <summary>
        /// Called when a button is pressed
        /// </summary>
        /// <param name="dataObject">The data object related to this field</param>
        /// <param name="dataEditor">The data editor so you can refresh if needed</param>
        public delegate void OnPressedCallback(JsonDefWithName dataObject, IDataEditorInput dataEditor);

        public List<JsonGenericEditorInputRow> Rows { get; } = new List<JsonGenericEditorInputRow>();

        /// <summary>
        /// Decides how far we space the rows in the grid
        /// </summary>
        public int RowSpacing { get; set; } = 10;

        public float LabelWidth = 0f;

        public JsonGenericEditorInputRow AddTextField(string name, int rowNum,
                        Func<JsonDefWithName, object> getValue,
                        OnSaveCallback onSave,
                        OnValidateCallback onValidate)
        {
            var row = new JsonGenericEditorInputRow();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Text;
            row.RowNumber = rowNum;
            row.GetValue = getValue;
            row.OnSave = onSave;
            row.OnValidate = onValidate;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRow AddButton(string name, int rowNum,
                        Func<JsonDefWithName, object> getValue,
                        OnPressedCallback onPressed)
        {
            var row = new JsonGenericEditorInputRowButton();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Button;
            row.RowNumber = rowNum;
            row.GetValue = getValue;
            row.OnPressed = onPressed;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRowList AddListField(string name, int rowNum,
                        bool sort, Func<Dictionary<object, string>> getListValues,
                        Action<IDataEditorInput> onAdd, Action<object, IDataEditorInput> onRemove,
                        Action<object> onDoubleClick = null)
        {
            var row = new JsonGenericEditorInputRowList();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.List;
            row.RowNumber = rowNum;
            row.OnAdd = onAdd;
            row.OnRemove = onRemove;
            row.OnDoubleClick = onDoubleClick;
            row.Sort = sort;
            row.GetListValues = getListValues;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRowTree AddTreeField(string name, int rowNum,
                        List<DEA_TreeColumn> columns, Func<List<object>> getObjectList,
                        Action<IDataEditorInput> onAdd, Action<object, IDataEditorInput> onEdit,
                        Action<object, IDataEditorInput> onRemove,
                        Action<object, IDataEditorInput> onDoubleClick = null,
                        List<JsonTreeMenuItem> menuItems = null)
        {
            var row = new JsonGenericEditorInputRowTree();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Tree;
            row.RowNumber = rowNum;
            row.OnAdd = onAdd;
            row.OnEdit = onEdit;
            row.OnRemove = onRemove;
            row.OnDoubleClick = onDoubleClick;
            row.MenuItems = menuItems;
            row.Columns = columns;
            row.GetObjectList = getObjectList;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRowCombo AddComboField(string name, int rowNum,
                        bool sort, Func<Dictionary<object, string>> getListValues,
                        Func<JsonDefWithName, object> getValue,
                        OnSaveCallback onSave,
                        OnValidateCallback onValidate)
        {
            var row = new JsonGenericEditorInputRowCombo();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Combo;
            row.RowNumber = rowNum;
            row.GetValue = getValue;
            row.OnSave = onSave;
            row.OnValidate = onValidate;
            row.Sort = sort;
            row.GetListValues = getListValues;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRow AddCheckboxField(string name, int rowNum,
                        string toolTip,
                        Func<JsonDefWithName, object> getValue,
                        OnSaveCallback onSave,
                        OnValidateCallback onValidate = null)
        {
            var row = new JsonGenericEditorInputRow();
            row.Name = name;
            row.ToolTip = toolTip;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.CheckBox;
            row.RowNumber = rowNum;
            row.GetValue = getValue;
            row.OnSave = onSave;
            row.OnValidate = onValidate;
            Rows.Add(row);
            return row;
        }

        public JsonGenericEditorInputRow AddCustomField(string name, int rowNum,
                        string toolTip, Func<IDataEditorInput> getCustom)
        {
            var row = new JsonGenericEditorInputRow();
            row.Name = name;
            row.ToolTip = toolTip;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Custom;
            row.RowNumber = rowNum;
            row.GetCustomEditor = getCustom;
            Rows.Add(row);
            return row;
        }


        public static bool ValidateTextNotNullOrEmpty(string name, object data, object value)
        {
            if (value == null) { return false; }
            if (value.ToString().IsNullOrEmpty()) { return false; }

            return true;
        }

        public static bool ValidateNumberValue(string name, object data, object value)
        {
            return int.TryParse(value.ToString(), out var outValue);
        }

        public static bool ValidateDecimalValue(string name, object data, object value)
        {
            return float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var outValue);
        }
    }


    public class JsonGenericEditorInputRow
    {
        public enum EditorTypes
        {
            Text, Combo, List, CheckBox, Button, Tree, Custom
        }

        /// <summary>
        /// The name of this field, used to show to the user and as a key for some of the callbacks.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// For controls that support it you can fill tooltip text here to get a tooltip
        /// </summary>
        public string ToolTip { get; set; } = "";

        /// <summary>
        /// If custom this will be instanced, must implement IDataEditorInput
        /// </summary>
        public EditorTypes EditorType { get; set; } = EditorTypes.Text;

        /// <summary>
        /// The row this item should be added to
        /// </summary>
        public int RowNumber { get; set; } = 1;

        /// <summary>
        /// Set the width of the editor
        /// </summary>
        public float EditorWidth { get; set; } = 100f;

        /// <summary>
        /// If this is 0 it will be overridden by the JsonGenericEditorInput.LabelWidth
        /// </summary>
        public float LabelWidth { get; set; } = 0f;

        /// <summary>
        /// If custom this will be instanced, must implement IDataEditorInput
        /// </summary>
        public Func<IDataEditorInput> GetCustomEditor { get; set; } = null;

        /// <summary>
        /// You get the data object and are expected to return the value (usually a string)
        /// </summary>
        public Func<JsonDefWithName, object> GetValue { get; set; } = null;

        /// <summary>
        /// Called whenever data has been updated, can do refresh etc...
        /// </summary>
        public Action<DataEditorInput> OnDataUpdated { get; set; } = null;

        /// <summary>
        /// You get the name, data object and current value of the control as input
        /// </summary>
        public JsonGenericEditorInput.OnSaveCallback OnSave { get; set; } = null;

        /// <summary>
        /// Input is the name of this JsonGenericEditorInputRow, the data object and the control value.
        /// Expects true in return if valid, false if not.
        /// If null we do no validation
        /// </summary>
        public JsonGenericEditorInput.OnValidateCallback OnValidate { get; set; } = null;

        /// <summary>
        /// For controls that respect this will set the control as read only
        /// </summary>
        public bool Disabled { get; set; } = false;
    }

    public class JsonGenericEditorInputRowButton : JsonGenericEditorInputRow
    {
        public JsonGenericEditorInput.OnPressedCallback OnPressed { get; set; } = null;
    }

    public class JsonGenericEditorInputRowCombo : JsonGenericEditorInputRow
    {
        /// <summary>
        /// First param is the key, second is display name
        /// </summary>
        public Func<Dictionary<object, string>> GetListValues { get; set; } = null;

        public bool Sort { get; set; } = true;
    }

    public class JsonGenericEditorInputRowList : JsonGenericEditorInputRowCombo
    {
        public float EditorHeight { get; set; } = 100f;

        /// <summary>
        /// The object is the key value of the item.
        /// You need to manually call refresh on the IDataEditorInput
        /// </summary>
        public Action<object, IDataEditorInput> OnRemove { get; set; } = null;

        /// <summary>
        /// After adding you need to manually call refresh on the IDataEditorInput
        /// </summary>
        public Action<IDataEditorInput> OnAdd { get; set; } = null;

        /// <summary>
        /// Triggered when this item is double clicked
        /// </summary>
        public Action<object> OnDoubleClick { get; set; } = null;
    }

    public class JsonTreeMenuItem
    {
        public JsonTreeMenuItem(string name, Texture icon, Action<object, IDataEditorInput> action)
        {
            this.Name = name;
            this.Icon = icon;
            this.Action = action;
        }

        public string Name;
        public Action<object, IDataEditorInput> Action;
        public Texture Icon;
    }

    public class JsonGenericEditorInputRowTree : JsonGenericEditorInputRow
    {
        public float EditorHeight { get; set; } = 100f;

        /// <summary>
        /// List of objects for the dialog
        /// </summary>
        public Func<List<object>> GetObjectList { get; set; }

        /// <summary>
        /// Triggered when this item is double clicked
        /// </summary>
        public Action<object, IDataEditorInput> OnDoubleClick { get; set; } = null;

        /// <summary>
        /// The object is the key value of the item.
        /// You need to manually call refresh on the IDataEditorInput
        /// </summary>
        public Action<object, IDataEditorInput> OnRemove { get; set; } = null;

        /// <summary>
        /// Same as OnRemove except you are expected to modify the object accordingly
        /// </summary>
        public Action<object, IDataEditorInput> OnEdit { get; set; } = null;

        /// <summary>
        /// After adding you need to manually call refresh on the IDataEditorInput
        /// </summary>
        public Action<IDataEditorInput> OnAdd { get; set; } = null;

        /// <summary>
        /// Columns for the tree
        /// </summary>
        public List<DEA_TreeColumn> Columns { get; set; } = new List<DEA_TreeColumn>();

        public List<JsonTreeMenuItem> MenuItems { get; set; } = null;
    }
}