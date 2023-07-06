using Godot;
using GodotCSharpToolkit.Editor;
using ScriptSystem.Data;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public class EditorContainerData : JsonGenericEditorInputRow
    {
        /// <summary>
        /// The input used for the generic editor inside this editor
        /// </summary>
        public JsonGenericEditorInput Input { get; set; }

        /// <summary>
        /// The title of the container
        /// </summary>
        public string ContainerTitle { get; set; }

        /// <summary>
        /// Min size
        /// </summary>
        public Vector2 RectMinSize = Vector2.Zero;
    }

    public class EditorContainer : Panel, IDataEditorInput
    {
        private JsonDefWithName Data;
        private IDataEditor Editor;
        private EditorContainerData Input;

        private JsonGenericEditor GenericEditor;

        private GridContainer Grid;
        private Label NameLabel;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Grid = FindNode("Grid") as GridContainer;
            NameLabel = FindNode("Label") as Label;

        }

        public void Refresh()
        {

        }

        public void DataUpdated()
        {

        }
        public virtual void Disable(bool disabled)
        {
        }

        public void SetInputData(JsonDefWithName data, JsonGenericEditorInputRow input, IDataEditor editor)
        {
            Data = data;
            Editor = editor;
            Input = (EditorContainerData)input;

            GenericEditor = DataEditorConstants.CreateJsonGenericEditor();
            GenericEditor.SetInput(Input.Input);
            GenericEditor.SetData(Data, this);
            GenericEditor.Init(Editor);

            Grid.AddChild(GenericEditor);
            NameLabel.Text = Input.ContainerTitle;
            RectMinSize = Input.RectMinSize;
        }

        private static EditorContainer GetEditor()
        {
            return DataEditorConstants.CreateEditorContainer();
        }

        public static EditorContainerData CreateInput(JsonGenericEditorInput parent, string containerTitle,
                                                        int rowNum, JsonGenericEditorInput input,
                                                        Vector2 rectMinSize)
        {
            var row = new EditorContainerData();
            row.Name = System.Guid.NewGuid().ToString();
            row.EditorType = JsonGenericEditorInputRow.EditorTypes.Custom;
            row.ContainerTitle = containerTitle;
            row.RectMinSize = rectMinSize;
            row.Input = input;
            row.GetCustomEditor = GetEditor;
            row.RowNumber = rowNum;
            parent.Rows.Add(row);
            return row;
        }

        public void TakeFocus()
        {

        }
    }
}