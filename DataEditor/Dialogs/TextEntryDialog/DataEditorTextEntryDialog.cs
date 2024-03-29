using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Simple dialog that lets you create a new item.
    /// Contains a name entry field and an optional field to show a list for selecting an object to clone.
    /// </summary>
    public partial class DataEditorTextEntryDialog : ColorRect
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
        private DataEditorInputCombo ComboInput;
        private string Title;
        private Func<Dictionary<object, string>> GetItemList = null;

        private string TextEntryValue;
        private string ListInputValue = null;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LblTitle = FindChild("Title") as Label;
            BtnConfirm = FindChild("BtnConfirm") as Button;
            BtnCancel = FindChild("BtnCancel") as Button;
            ControlGrid = FindChild("ControlGrid") as GridContainer;
            LblTitle.Text = Title;

            BtnConfirm.Connect("pressed", new Callable(this, nameof(OnConfirmPressed)));
            BtnCancel.Connect("pressed", new Callable(this, nameof(OnCancelPressed)));

            if (!TextFieldName.IsNullOrEmpty())
            {
                TextInput = DataEditorConstants.CreateInputText();
                ControlGrid.AddChild(TextInput);
                TextInput.SetInputData(null, GetTextRowData(), null);
                TextInput.TakeFocus();
            }

            if (GetItemList != null && GetItemList != null)
            {
                ComboInput = DataEditorConstants.CreateInputCombo();
                ControlGrid.AddChild(ComboInput);
                ComboInput.SetInputData(null, GetListRowData(), null);
                ComboInput.TakeFocus();
            }

        }

        private JsonGenericEditorInputRow GetTextRowData()
        {
            var genInput = new JsonGenericEditorInput();
            var val = genInput.AddTextField(TextFieldName, 1, (v) => TextEntryValue,
            (n, d, value) => TextEntryValue = value.ToString(),
            NameValidation);
            val.EditorWidth = 155f;
            return val;
        }

        private JsonGenericEditorInputRowCombo GetListRowData()
        {
            var genInput = new JsonGenericEditorInput();
            var val = genInput.AddComboField(ListFieldName, 1, false, GetItemList,
            (v) => ListInputValue,
            (n, d, value) => ListInputValue = value.ToString(),
            null);
            val.EditorWidth = 155f;
            return val;
        }

        private bool NameValidation(string name, object data, object value)
        {
            if (TextValidator == null) { return true; }
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
                     string listFieldName = "", Func<Dictionary<object, string>> getItemList = null)
        {
            Callback = callback;
            TextValidator = nameValidator;
            Title = title;
            GetItemList = getItemList;

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
                if (key.Keycode == Key.Escape)
                {
                    OnCancelPressed();
                    GetViewport().SetInputAsHandled();
                }
                else if ((key.Keycode == Key.Enter || key.Keycode == Key.KpEnter) && !BtnConfirm.Disabled)
                {
                    OnConfirmPressed();
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
}