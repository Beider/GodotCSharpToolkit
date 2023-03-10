using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public class JsonGenericEditorInput
    {
        public List<JsonGenericEditorInputRow> Rows { get; } = new List<JsonGenericEditorInputRow>();

        /// <summary>
        /// Decides how far we space the rows in the grid
        /// </summary>
        public int RowSpacing { get; set; } = 10;

        /// <summary>
        /// If onUpdate is not set it will use default onUpdate method
        /// </summary>
        public JsonGenericEditorInputRow AddTextRow(string name, int rowNum,
                        Func<JsonDefWithName, object> getValue,
                        Action<string, object, object> onSave,
                        Func<string, object, object, bool> onValidate)
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

        public JsonGenericEditorInputRowList AddListRow(string name, int rowNum,
                        bool sort, List<string> values,
                        Func<JsonDefWithName, object> getValue,
                        Action<string, object, object> onSave,
                        Func<string, object, object, bool> onValidate)
        {
            var row = new JsonGenericEditorInputRowList();
            row.Name = name;
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.List;
            row.RowNumber = rowNum;
            row.GetValue = getValue;
            row.OnSave = onSave;
            row.OnValidate = onValidate;
            row.Sort = sort;
            row.Values = values;
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
            int outValue = 0;
            return int.TryParse(value.ToString(), out outValue);
        }
    }


    public class JsonGenericEditorInputRow
    {
        public enum EditorTypes
        {
            Text, List, Custom
        }

        /// <summary>
        /// The name of this field, used to show to the user and as a key for some of the callbacks.
        /// </summary>
        public string Name { get; set; } = "";

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
        /// If custom this will be instanced, must implement IDataEditorInput
        /// </summary>
        public PackedScene CustomEditorPackedScene { get; set; } = null;

        /// <summary>
        /// You get the data object and are expected to return the value (usually a string)
        /// </summary>
        public Func<JsonDefWithName, object> GetValue { get; set; } = null;

        /// <summary>
        /// You get the name, data object and current value of the control as input
        /// </summary>
        public Action<string, JsonDefWithName, object> OnSave { get; set; } = null;

        /// <summary>
        /// Input is the name of this JsonGenericEditorInputRow, the data object and the control value.
        /// Expects true in return if valid, false if not.
        /// If null we do no validation
        /// </summary>
        public Func<string, JsonDefWithName, object, bool> OnValidate { get; set; } = null;
    }

    public class JsonGenericEditorInputRowList : JsonGenericEditorInputRow
    {
        public List<string> Values { get; set; } = new List<string>();

        public bool Sort { get; set; } = true;
    }
}