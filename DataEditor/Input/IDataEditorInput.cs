using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public interface IDataEditorInput
    {
        void SetInputData(JsonDefWithName data, JsonGenericEditorInputRow input, IDataEditor editor);

        void Refresh();

        void DataUpdated();

        void Disable(bool disabled);

        void TakeFocus();
    }
}