using Godot;
using System;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public class AbstractEditorTreeItem
    {
        /// <summary>
        /// The editor we belong to
        /// </summary>
        protected IDataEditor Editor;

        /// <summary>
        /// The item that is our parent
        /// </summary>
        protected TreeItem Parent;

        public TreeItem TreeItemSelf { get; set; } = null;

        public string Name { get; set; } = "Tree Item";
        public Color Color { get; set; } = DataEditorConstants.COLOR_DEFAULT;
        public Color ColorBg { get; set; } = DataEditorConstants.COLOR_BG_DEFAULT;
        public bool Collapsed { get; set; } = false;

        public AbstractEditorTreeItem()
        {

        }

        /// <summary>
        /// Setup things we need at startup
        /// </summary>
        public void Init(TreeItem parent, IDataEditor editor)
        {
            this.Parent = parent;
            this.Editor = editor;
        }

        /// <summary>
        /// Called when this item is selected
        /// </summary>
        public virtual void OnItemSelected()
        {
            Logger.Warning($"OnItemSelected not implemented for tree item {Name}");
        }
    }
}