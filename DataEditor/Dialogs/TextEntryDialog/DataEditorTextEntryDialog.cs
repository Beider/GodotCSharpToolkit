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
            TextInput.SetInputWidth(155f);
            TextInput.SetValidator(NameValidation);
            TextInput.SetSaveAction(UpdateName);
            TextInput.SetValue("");
            TextInput.SetText(TextFieldName);
            TextInput.SetKey(TextFieldName);

            if (ItemList != null && ItemList.Count > 0)
            {
                ListInput = DataEditorConstants.CreateInputList();
                ControlGrid.AddChild(ListInput);
                ListInput.SetItemList(ItemList, false);
                ListInput.SetValue(ItemList[0]);
                ListInput.SetInputWidth(155f);
                ListInput.SetSaveAction(UpdateListValue);
                ListInput.SetText(ListFieldName);
                ListInput.SetKey(ListFieldName);
            }

        }

        private void UpdateName(object value, string key)
        {
            TextEntryValue = value.ToString();
        }

        private void UpdateListValue(object value, string key)
        {
            ListInputValue = value.ToString();
        }

        private bool NameValidation(object value, string key)
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