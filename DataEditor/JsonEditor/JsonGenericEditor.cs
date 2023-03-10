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
    public class JsonGenericEditor<T, U> : JsonEditorContentBase<T, U> where T : JsonDefWithName
    {
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

            foreach (var row in Input.Rows)
            {
                EnsureRowExists(row);
                CreateControl(row);
            }
        }

        private void CreateControl(JsonGenericEditorInputRow data)
        {
            var parent = Rows[data.RowNumber];
            IDataEditorInput input = null;
            if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Text)
            {
                input = DataEditorConstants.CreateInputText();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.List)
            {
                input = DataEditorConstants.CreateInputList();
            }
            else if (data.EditorType == JsonGenericEditorInputRow.EditorTypes.Custom &&
                    data.CustomEditorPackedScene != null)
            {
                input = data.CustomEditorPackedScene.Instance() as IDataEditorInput;
            }

            if (input == null)
            {
                Logger.Error($"Could not instance input named '{data.Name}'");
                return;
            }

            parent.AddChild((Control)input);
            input.SetInputData(Data, data);

            InputFields.Add(data.Name, input);
        }

        private void EnsureRowExists(JsonGenericEditorInputRow data)
        {
            while (CurrentRow < data.RowNumber)
            {
                CurrentRow++;
                var row = CreateRow(ControlContainer, 100);
                Rows.Add(CurrentRow, row);
            }
        }
    }
}