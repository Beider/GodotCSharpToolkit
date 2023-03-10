using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Simple dialog that lets you create a new item.
    /// Contains a name entry field and an optional field to show a list for selecting an object to clone.
    /// </summary>
    public class DataEditorTextEntryDialog : ColorRect
    {
        private Label LblTitle;
        private Button BtnConfirm;
        private Button BtnCancel;
        private GridContainer ControlGrid;

        private Action<string, string> Callback;
        private Func<string, bool> TextValidator;
        private string TextFieldName;
        private string ListFieldName;
        private DataEditorInputText TextInput;
        private DataEditorInputList ListInput;
        private string Title;
        private List<string> ItemList = null;

        private string TextEntryValue;
        private string ListInputValue = null;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LblTitle = FindNode("Title") as Label;
            BtnConfirm = FindNode("BtnConfirm") as Button;
            BtnCancel = FindNode("BtnCancel") as Button;
            ControlGrid = FindNode("ControlGrid") as GridContainer;
            LblTitle.Text = Title;

            BtnConfirm.Connect("pressed", this, nameof(OnConfirmPressed));
            BtnCancel.Connect("pressed", this, nameof(OnCancelPressed));

            TextInput = DataEditorConstants.CreateInputText();
            ControlGrid.AddChild(TextInput);
            TextInput.SetInputData(null, GetTextRowData());

            if (ItemList != null && ItemList.Count > 0)
            {
                ListInput = DataEditorConstants.CreateInputList();
                ControlGrid.AddChild(ListInput);
                TextInput.SetInputData(null, GetListRowData());
            }

        }

        private JsonGenericEditorInputRow GetTextRowData()
        {
            var genInput = new JsonGenericEditorInput();
            var val = genInput.AddTextRow(TextFieldName, 1, (v) => TextEntryValue,
            (n, d, value) => TextEntryValue = value.ToString(),
            NameValidation);
            val.EditorWidth = 155f;
            return val;
        }

        private JsonGenericEditorInputRowList GetListRowData()
        {
            var genInput = new JsonGenericEditorInput();
            var val = genInput.AddListRow(TextFieldName, 1, false, ItemList,
            (v) => TextEntryValue,
            (n, d, value) => TextEntryValue = value.ToString(),
            JsonGenericEditorInput.ValidateTextNotNullOrEmpty);
            val.EditorWidth = 155f;
            return val;
        }

        private bool NameValidation(string name, object data, object value)
        {
            if (value == null || !TextValidator.Invoke(value.ToString()))
            {
                BtnConfirm.Disabled = true;
                return false;
            }
            BtnConfirm.Disabled = false;
            return true;
        }

        public void SetupBeforeAddChild(string title, string textFieldName,
                     Action<string, string> callback, Func<string, bool> nameValidator,
                     string listFieldName = "", List<string> itemList = null)
        {
            Callback = callback;
            TextValidator = nameValidator;
            Title = title;
            ItemList = itemList;

            TextFieldName = textFieldName;
            ListFieldName = listFieldName;
        }

        private void OnConfirmPressed()
        {
            Callback.Invoke(TextEntryValue, ListInputValue);
            QueueFree();
        }

        private void OnCancelPressed()
        {
            QueueFree();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Scancode == (int)KeyList.Escape)
                {
                    OnCancelPressed();
                }
                else if (key.Scancode == (int)KeyList.Enter && !BtnConfirm.Disabled)
                {
                    OnConfirmPressed();
                }
            }
        }
    }
}