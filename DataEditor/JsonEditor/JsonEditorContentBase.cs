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

        public override void _ExitTree()
        {
            base._ExitTree();
            if (Data != null)
            {
                Data.OnStatusChange -= DataUpdated;
            }
        }

        public virtual void Save()
        {
            // Do nothing
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        protected virtual void DataUpdated(JsonDefWithName data)
        {
            // Do nothing
        }

        public virtual void SetData(object data, object provider)
        {
            Provider = provider;
            Data = (JsonDefWithName)data;
            if (Data != null)
            {
                Data.OnStatusChange += DataUpdated;
            }
        }

        public string GetUniqueId()
        {
            return Data.GetUniqueId();
        }

        public virtual Action<string> GetOpenAction()
        {
            return (key) => { };
        }
        public virtual Color GetColor() { return Colors.Transparent; }
        public virtual string GetContentName() { return "Not Set"; }
        public virtual string GetContentID() { return null; }
    }
}