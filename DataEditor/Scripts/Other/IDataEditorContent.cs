using Godot;
using System;

namespace GodotCSharpToolkit.Editor
{
    public interface IDataEditorContent
    {
        void Save();

        void QueueFree();

        void SetData(object data, object provider);

        void Init(IDataEditor editor);

        string GetUniqueId();

        /// <summary>
        /// Get the opening action to reopen this
        /// </summary>
        Action<string> GetOpenAction();

        /// <summary>
        /// The color that represents the object opened
        /// </summary>
        Color GetColor();

        /// <summary>
        /// Name of the object opened
        /// </summary>
        string GetContentName();

        /// <summary>
        /// Get the unique ID
        /// </summary>
        string GetContentID();
    }
}