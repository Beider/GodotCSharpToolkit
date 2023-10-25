using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Generic editor for any type of json object. Simply define the input and it should work.
    /// </summary>
    public class JsonGenericEditor : JsonEditorContentBase
    {
        public event Action OnDataUpdated = delegate { };

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

        private void CreateGridContainer()
        {
            ControlContainer = new GridContainer();
            ControlContainer.MarginTop = 10;
            ControlContainer.MarginLeft = 10;
            ControlContainer.MarginRight = -10;
            ControlContainer.MarginBottom = -10;
            ControlContainer.AnchorRight = 1;
            ControlContainer.AnchorBottom = 1;
            ControlContainer.AddConstantOverride("vseparation", Input.RowSpacing);
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

        public override Action<string> GetOpenAction()
        {
            return (key) =>
            {
                var data = EditorScene.GetJsonDefById<JsonDefWithName>(key);
                EditorUtils.ShowEditor(data);
            };
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
