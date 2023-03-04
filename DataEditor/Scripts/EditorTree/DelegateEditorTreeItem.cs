using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public class DelegateEditorTreeItem : AbstractEditorTreeItem
    {
        public Action<DelegateEditorTreeItem> OnSelection { get; set; } = null;

        public object RelatedData;

        public override void OnItemSelected()
        {
            if (OnSelection != null)
            {
                OnSelection.Invoke(this);
            }
            else
            {
                Logger.Warning($"OnSelection delegate not set for tree item {Name}");
            }
        }
    }
}