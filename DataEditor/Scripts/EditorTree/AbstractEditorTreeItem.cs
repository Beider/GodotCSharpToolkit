using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Basic tree item to be inherited from
    /// </summary>
    public abstract class AbstractEditorTreeItem
    {
        /// <summary>
        /// The editor we belong to
        /// </summary>
        protected IDataEditor Editor;

        /// <summary>
        /// The item that is our parent
        /// </summary>
        protected TreeItem Parent;

        public List<string> ModPaths;

        protected WeakRef TreeItemSelf { get; set; } = null;


        public string Key { get; set; } = "Tree Item";
        public string Name { get; set; } = "Name";
        public string ModName { get; set; } = "ModName";
        public string FeatureName { get; set; } = "FeatureName";
        public Color Color { get; set; } = DataEditorConstants.COLOR_DEFAULT;
        public Color ColorBg { get; set; } = DataEditorConstants.COLOR_BG_DEFAULT;
        public bool Collapsed { get; set; } = false;
        public bool IsLeaf { get; set; } = false;

        public AbstractEditorTreeItem()
        {

        }

        /// <summary>
        /// Setup things we need at startup
        /// </summary>
        public void Init(TreeItem parent, IDataEditor editor, List<string> modPaths = null, string modName = "", string featureName = "")
        {
            this.Parent = parent;
            this.Editor = editor;
            this.ModPaths = modPaths;
            this.ModName = modName;
            this.FeatureName = featureName;
            Editor.Tree.OnSelectItemRequest += OnSelectItemRequest;
        }

        protected virtual void OnSelectItemRequest(string uniqueId)
        {

        }

        public void SetTreeItemReference(WeakRef weakref)
        {
            TreeItemSelf = weakref;
        }

        public TreeItem ResolveSelfItem()
        {
            if (TreeItemSelf == null) { return null; }
            var refItem = TreeItemSelf.GetRef();
            if (refItem.VariantType != Variant.Type.Nil)
            {
                return (TreeItem)refItem.Obj;
            }
            return null;
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
        public virtual bool FillContextMenu(EditorPopupMenu menu)
        {
            return false;
        }

        public virtual void Dispose()
        {
            TreeItemSelf?.Dispose();
            TreeItemSelf = null;
        }
    }
}