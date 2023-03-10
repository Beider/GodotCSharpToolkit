using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public interface IDataEditorContent
    {
        void Save();

        void QueueFree();

        void SetData(JsonDefWithName data, object provider);

        void Init(IDataEditor editor);
    }
}