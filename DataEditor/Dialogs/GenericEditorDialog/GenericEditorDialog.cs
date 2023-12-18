using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class GenericEditorDialogInput
    {
        public event Action RebuildDialog = delegate { };
        public event Action CheckOk = delegate { };
        public string Name { get; set; } = "List dialog";
        public Func<JsonGenericEditorInput> GetEditorInput { get; set; } = null;
        public JsonDefWithName Data { get; set; } = null;
        public Action<JsonDefWithName> OnDialogOk { get; set; } = null;
        public Func<JsonDefWithName, bool> IsOkEnabled { get; set; } = null;
        public Action OnClose { get; set; } = null;
        public Vector2 DialogSize = new Vector2(500f, 300f);

        public void NotifyRebuildDialog()
        {
            RebuildDialog();
        }

        public void NotifyCheckOk()
        {
            CheckOk();
        }
    }
    /// <summary>
    /// Dialog that displays a generic editor
    /// </summary>
    public partial class GenericEditorDialog : ColorRect
    {
        private Window Dialog;
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
            Dialog = FindChild("Dialog") as Window;
            Dialog.Connect("close_requested", new Callable(this, nameof(OnCancelPressed)));
            Dialog.Title = Input.Name;

            // Root
            ContentParent = FindChild("Content") as Control;

            // Buttons
            BtnOk = FindChild("BtnOk") as Button;
            BtnCancel = FindChild("BtnCancel") as Button;
            BtnOk.Connect("pressed", new Callable(this, nameof(OnOkPressed)));
            BtnCancel.Connect("pressed", new Callable(this, nameof(OnCancelPressed)));
            CheckOkEnabled();

            Input.Data.OnStatusChange += OnDataChanged;

            // Create generic editor
            Input.RebuildDialog += ReBuildGenericEditor;
            Input.CheckOk += CheckOkEnabled;
            ReBuildGenericEditor();

            Dialog.MinSize = Input.DialogSize.ToVector2I();
            Dialog.Size = Input.DialogSize.ToVector2I();

            Dialog.Popup();
        }

        public override void _ExitTree()
        {
            Input.RebuildDialog -= ReBuildGenericEditor;
            Input.CheckOk -= CheckOkEnabled;
        }

        private void ReBuildGenericEditor()
        {
            GenericEditor?.QueueFree();
            GenericEditor = DataEditorConstants.CreateJsonGenericEditor();
            GenericEditor.SetInput(Input.GetEditorInput());
            GenericEditor.SetData(Input.Data, this);
            GenericEditor.Init(Editor);
            ContentParent.AddChild(GenericEditor);
            CheckOkEnabled();
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
                if (key.Keycode == Key.Escape)
                {
                    OnCancelPressed();
                    GetViewport().SetInputAsHandled();
                }
                else if ((key.Keycode == Key.Enter || key.Keycode == Key.KpEnter) && !BtnOk.Disabled)
                {
                    OnOkPressed();
                    GetViewport().SetInputAsHandled();
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