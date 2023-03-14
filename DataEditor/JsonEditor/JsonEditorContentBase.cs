using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Editor;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public abstract class JsonEditorContentBase : Panel, IDataEditorContent
    {
        protected IDataEditor Editor;
        protected JsonDefWithName Data;
        protected object Provider;

        protected GridContainer CreateRow(Control parent, int columns)
        {
            var container = new GridContainer();
            container.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            parent.AddChild(container);
            container.Columns = columns;
            container.AddConstantOverride("hseparation", 20);
            return container;
        }

        public virtual void Save()
        {
            // Do nothing
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        public virtual void SetData(object data, object provider)
        {
            Provider = provider;
            Data = (JsonDefWithName)data;
        }

        public string GetUniqueId()
        {
            return Data.GetUniqueId();
        }
    }
}