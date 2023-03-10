using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Editor;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public abstract class JsonEditorContentBase<T, U> : Panel, IDataEditorContent where T : JsonDefWithName
    {
        protected IDataEditor Editor;
        protected T Data;
        protected U Provider;

        protected GridContainer CreateRow(Control parent, int columns)
        {
            var container = new GridContainer();
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

        public void SetData(JsonDefWithName data, object provider)
        {
            if (provider is U prov)
            {
                Provider = prov;
            }
            else
            {
                Logger.Error($"Provider is not of correct type ({typeof(U)}): {provider.GetType()}");
            }
            if (data is T dObj)
            {
                Data = dObj;
            }
            else
            {
                Logger.Error($"Data is not of correct type ({typeof(T)}): {data.GetType()}");
            }
        }
    }
}