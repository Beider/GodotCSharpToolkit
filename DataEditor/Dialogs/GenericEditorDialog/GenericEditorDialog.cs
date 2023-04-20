using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public class GenericEditorDialogInput
    {
        public string Name { get; set; } = "List dialog";
        public JsonGenericEditorInput EditorInput { get; set; } = null;
        public JsonDefWithName Data { get; set; } = null;
        public Action<JsonDefWithName> OnDialogOk { get; set; } = null;
        public Func<JsonDefWithName, bool> IsOkEnabled { get; set; } = null;
        public Action OnClose { get; set; } = null;
        public Vector2 DialogSize = new Vector2(500f, 300f);
    }
    /// <summary>
    /// Dialog that displays a generic editor
    /// </summary>
    public class GenericEditorDialog : ColorRect
    {
        private WindowDialog Dialog;
        private IDataEditor Editor;
        private Control ContentParent;
        private Button BtnOk;
        private Button BtnCancel;
        private JsonGenericEditor GenericEditor;

        private GenericEditorDialogInput Input;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Dialog
            Dialog = FindNode("Dialog") as WindowDialog;
            Dialog.Connect("popup_hide", this, nameof(OnCancelPressed));
            Dialog.WindowTitle = Input.Name;

            // Root
            ContentParent = FindNode("Content") as Control;

            // Buttons
            BtnOk = FindNode("BtnOk") as Button;
            BtnCancel = FindNode("BtnCancel") as Button;
            BtnOk.Connect("pressed", this, nameof(OnOkPressed));
            BtnCancel.Connect("pressed", this, nameof(OnCancelPressed));
            CheckOkEnabled();

            Input.Data.OnStatusChange += OnDataChanged;

            // Create generic editor
            GenericEditor = DataEditorConstants.CreateJsonGenericEditor();
            GenericEditor.SetInput(Input.EditorInput);
            GenericEditor.SetData(Input.Data, this);
            GenericEditor.Init(Editor);
            ContentParent.AddChild(GenericEditor);

            Dialog.RectMinSize = Input.DialogSize;
            Dialog.RectSize = Input.DialogSize;

            Dialog.Popup_();
        }

        private void OnDataChanged(JsonDefWithName data)
        {
            CheckOkEnabled();
        }

        public void Init(GenericEditorDialogInput input, IDataEditor editor)
        {
            Input = input;
            Editor = editor;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Scancode == (int)KeyList.Escape)
                {
                    OnCancelPressed();
                }
                else if (key.Scancode == (int)KeyList.Enter && !BtnOk.Disabled)
                {
                    OnOkPressed();
                }
            }
        }

        private void CheckOkEnabled()
        {
            if (Input.IsOkEnabled != null)
            {
                BtnOk.Disabled = !Input.IsOkEnabled(Input.Data);
                return;
            }

            // Default is just enabled
            BtnOk.Disabled = false;
        }

        private void OnCancelPressed()
        {
            if (Input.OnClose != null)
            {
                Input.OnClose();
            }
            CloseDialog();
        }

        private void OnOkPressed()
        {
            if (BtnOk.Disabled) { return; }
            if (Input.OnDialogOk != null)
            {
                Input.OnDialogOk(Input.Data);
            }
            CloseDialog();
        }

        private void CloseDialog()
        {
            Input.Data.OnStatusChange -= OnDataChanged;
            QueueFree();
        }
    }
}