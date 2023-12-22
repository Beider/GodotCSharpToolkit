using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Extensions;
using ScriptSystem.Data;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Generic editor for any type of json object. Simply define the input and it should work.
    /// </summary>
    public partial class JsonGenericEditor : JsonEditorContentBase
    {
        public event Action OnDataUpdated = delegate { };
        public event Action<JsonDefWithName, JsonGenericEditor> OnRequestInputUpdate = delegate { };

        private GridContainer ControlContainer;

        private JsonGenericEditorInput Input = null;

        private Dictionary<int, GridContainer> Rows = new Dictionary<int, GridContainer>();
        private Dictionary<string, IDataEditorInput> InputFields = new Dictionary<string, IDataEditorInput>();
        int CurrentRow = 0;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            CreateGridContainer();
            BuildControls();
        }

        protected override void DataUpdated(JsonDefWithName data)
        {
            OnDataUpdated();
            foreach (var field in InputFields.Values)
            {
                field.DataUpdated();
            }
        }

        public void Refresh()
        {
            ControlContainer.Visible = false;
            ControlContainer.QueueFree();
            CurrentRow = 0;
            Rows.Clear();
            InputFields.Clear();
            OnRequestInputUpdate(Data, this);
            CreateGridContainer();
            BuildControls();
        }

        private void CreateGridContainer()
        {
            ControlContainer = new GridContainer();
            ControlContainer.OffsetTop = 10;
            ControlContainer.OffsetLeft = 10;
            ControlContainer.OffsetRight = -10;
            ControlContainer.OffsetBottom = -10;
            ControlContainer.AnchorRight = 1;
            ControlContainer.AnchorBottom = 1;
            ControlContainer.AddThemeConstantOverride("vseparation", Input.RowSpacing);
            AddChild(ControlContainer);
        }

        public void SetInput(JsonGenericEditorInput input)
        {
            this.Input = input;
        }

        private void BuildControls()
        {
            if (Input == null) { return; }

            Boolean isFirst = true;

            foreach (var row in Input.Rows)
            {
                EnsureRowExists(row);
                CreateControl(row, isFirst);
                isFirst = false;
            }
        }

        private void CreateControl(JsonGenericEditorInputRow data, bool setFocus)
        {
            var parent = Rows[data.RowNumber];
            parent.Columns = parent.GetChildCount() + 1;
            IDataEditorInput input = null;
            if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Text)
            {
                input = DataEditorConstants.CreateInputText();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Combo)
            {
                input = DataEditorConstants.CreateInputCombo();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.List)
            {
                input = DataEditorConstants.CreateInputList();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.CheckBox)
            {
                input = DataEditorConstants.CreateInputCheckbox();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Button)
            {
                input = DataEditorConstants.CreateInputButton();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Tree)
            {
                input = DataEditorConstants.CreateInputTree();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Custom &&
                    data.GetCustomEditor != null)
            {
                input = data.GetCustomEditor();
            }

            if (input == null)
            {
                Logger.Error($"Could not instance input named '{data.Name}'");
                return;
            }

            if (data.LabelWidth == 0) { data.LabelWidth = Input.LabelWidth; }
            parent.AddChild((Control)input);
            if (setFocus)
            {
                input.TakeFocus();
            }
            input.SetInputData(Data, data, Editor);

            InputFields.Add(data.Name, input);
        }

        private void EnsureRowExists(JsonGenericEditorInputRow data)
        {
            while (CurrentRow < data.RowNumber)
            {
                CurrentRow++;
                var row = CreateRow(ControlContainer, 1);
                Rows.Add(CurrentRow, row);
            }
        }

        public override int GetTypeId()
        {
            return 1;
        }

        public override Color GetColor()
        {
            return JsonItemListDialog.GetItemColor(Data);
        }

        public override string GetContentName()
        {
            return Data.GetName();
        }

        public override string GetContentID()
        {
            return Data.GetUniqueId();
        }
    }
}
