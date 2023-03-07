using Godot;
using System;
using System.Collections.Generic;
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

        protected List<string> ModPaths;

        public TreeItem TreeItemSelf { get; set; } = null;


        public string Key { get; set; } = "Tree Item";
        public string Name { get; set; } = "Name";
        public string ModName { get; set; } = "ModName";
        public Color Color { get; set; } = DataEditorConstants.COLOR_DEFAULT;
        public Color ColorBg { get; set; } = DataEditorConstants.COLOR_BG_DEFAULT;
        public bool Collapsed { get; set; } = false;

        public AbstractEditorTreeItem()
        {

        }

        /// <summary>
        /// Setup things we need at startup
        /// </summary>
        public void Init(TreeItem parent, IDataEditor editor, List<string> modPaths, string modName)
        {
            this.Parent = parent;
            this.Editor = editor;
            this.ModPaths = modPaths;
            this.ModName = modName;
        }

        /// <summary>
        /// Called when this item is selected
        /// </summary>
        public virtual void OnItemSelected()
        {
            Logger.Warning($"OnItemSelected not implemented for tree item {Name}");
        }

        /// <summary>
        /// Should return true to show menu, false if not
        /// </summary>
        public virtual bool FillContextMenu()
        {
            return false;
        }
    }
}