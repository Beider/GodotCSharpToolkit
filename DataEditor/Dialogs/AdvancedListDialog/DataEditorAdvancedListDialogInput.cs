using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public class DataEditorAdvancedListDialogInput
    {
        public string Name { get; set; } = "List dialog";

        public string Description { get; set; } = "If you want to filter by column value use <col>:<val>. Example: name:tomato\n" +
                                                  "You can use semicolon (;) to separate search terms.";


        /// <summary>
        /// Used for user preferences, change if you want settings to be for this dialog only
        /// </summary>
        public string UserPrefsKey { get; set; } = "ALD";

        /// <summary>
        /// The column to initially sort by
        /// </summary>
        public int SortedColumn { get; set; } = 0;

        /// <summary>
        /// Text used in search label
        /// </summary>
        public string SearchLabelText { get; set; } = "Search: ";

        /// <summary>
        /// You get the search string and the current selected object if any
        /// </summary>
        public Action<string, object> OnDialogOk { get; set; } = null;

        /// <summary>
        /// Can check if the OK button should be enabled, by default it is only enabled if you have a selection in the list.
        /// With this you could for instance enable it for list selection or simply by entering something in the search bar.
        /// </summary>
        public Func<string, object, bool> IsOkEnabled { get; set; } = null;

        /// <summary>
        /// Called when the user uses the X in the corner or the cancel button
        /// </summary>
        public Action OnClose { get; set; } = null;

        /// <summary>
        /// List of objects for the dialog
        /// </summary>
        public List<object> ObjectList { get; set; } = new List<object>();

        /// <summary>
        /// Columns for the tree
        /// </summary>
        public List<DEA_TreeColumn> Columns { get; set; } = new List<DEA_TreeColumn>();

        /// <summary>
        /// If disabled the default object column filter search will not be enabled.
        /// Instead we will only use the filters provided in the filter list
        /// </summary>
        public bool FilterByColumnValues { get; set; } = true;

        /// <summary>
        /// List of filters that are used when searching. Should return true if the object matches the current filter and should be included.
        /// You get the search string and object as input. If you set FilterByColumnValues to false only these filters will be used.
        /// </summary>
        public List<Func<string, object, bool>> FilterList { get; set; } = new List<Func<string, object, bool>>();

        /// <summary>
        /// The selection mode, will determine what you get back.
        /// <para>Default: Row you get the object associated with that row</para>
        /// <para>Multi: you will get a list of object when ok is pressed.</para>
        /// <para>Single: NOT SUPPORTED, will then default to row</para>
        /// </summary>
        public Tree.SelectModeEnum SelectionMode { get; set; } = Tree.SelectModeEnum.Row;

    }

    public class DEA_TreeColumn
    {
        /// <summary>
        /// Name of the column
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Allow the column to be resized by the user?
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Minimum width of the column in case user can resize
        /// </summary>
        public int MinWidth { get; set; } = 10;

        /// <summary>
        /// Used to get the value for this column, 
        /// you will get passed the object from the object list.
        /// </summary>
        public Func<object, string> GetValue { get; set; } = null;

        /// <summary>
        /// Get foreground color for the column, if not set it will use default
        /// </summary>
        public Func<object, Color> GetForegroundColor { get; set; } = null;

        /// <summary>
        /// Get background color for the column, if not set it will use default
        /// </summary>
        public Func<object, Color> GetBackgroundColor { get; set; } = null;


    }
}