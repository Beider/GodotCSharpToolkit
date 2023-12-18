using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class FolderManagerTreeItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        public bool AcceptDrop { get; set; } = false;
        public bool AllowMove { get; set; } = false;
        public bool Collapsed { get; set; } = false;
        public bool IsModified { get; set; } = false;

        /// <summary>
        /// The type of this item, used to compare for drop
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// The type of children this accepts
        /// </summary>
        public string ChildrenType { get; set; } = "";

        public string Name { get; set; } = null;
        public string Path3D { get; set; } = null;
        public string SubPath { get; set; } = null;
        public Color Color { get; set; } = Colors.White;
        public Action<string, List<JsonDefWithName>> SaveAction;

        public JsonDefWithName Data { get; set; } = null;
        public FolderManagerTreeItem Parent { get; set; } = null;
        public FolderManagerTreeItem OriginalParent { get; set; } = null;
        public List<FolderManagerTreeItem> Children { get; set; } = new List<FolderManagerTreeItem>();
        public List<FolderManagerTreeItem> OriginalChildren { get; set; } = new List<FolderManagerTreeItem>();

        public void AddChild(FolderManagerTreeItem item)
        {
            if (!Children.Contains(item))
            {
                if (item.OriginalParent == null) { item.OriginalParent = this; }
                item.Parent = this;
                Children.Add(item);
                item.SetModified();
                if (FolderManager.IsLoading)
                {
                    OriginalChildren.Add(item);
                }
            }
        }

        public void RemoveChild(FolderManagerTreeItem item)
        {
            if (Children.Contains(item))
            {
                if (item.Parent == this) { item.Parent = null; }
                Children.Remove(item);
                SetModified(false);
            }
        }

        public void RemoveFromParent()
        {
            if (Parent != null)
            {
                Parent.SetModified(false);
                Parent.RemoveChild(this);
                SetModified();
            }
        }

        private void SetModified(bool isFirst = true)
        {
            if (FolderManager.IsLoading) { return; }
            if (isFirst)
            {
                IsModified = Parent != OriginalParent;
            }
            else
            {
                IsModified = true;
            }
            Parent?.SetModified(false);
        }

        public static FolderManagerTreeItem CreateModuleItem(string name, string path, Color color)
        {
            var item = new FolderManagerTreeItem();
            item.Name = name;
            item.Type = "Module";
            item.Path3D = path;
            item.AllowMove = false;
            item.Color = color;
            item.ChildrenType = "Feature";
            item.AcceptDrop = true;
            return item;
        }

        public static FolderManagerTreeItem CreateTypeItem(string name, string path, string type, object source, string childrenType, Color color)
        {
            var item = new FolderManagerTreeItem();
            item.Name = name;
            item.Type = type;
            item.Path3D = path;
            item.AllowMove = false;
            item.Color = color;
            if (childrenType != null)
            {
                item.ChildrenType = childrenType;
                item.AcceptDrop = true;
            }
            return item;
        }

        public static FolderManagerTreeItem CreateJsonItem(string name, string path, string type, JsonDefWithName data, Color color)
        {
            var item = new FolderManagerTreeItem();
            item.Name = name;
            item.Type = type;
            item.Data = data;
            item.Path3D = path;
            item.AllowMove = true;
            item.AcceptDrop = false;
            item.Color = color;
            return item;
        }

        public FolderManagerTreeItem GetDropTarget(FolderManagerTreeItem item)
        {
            switch (ShouldAcceptDropRequest(item))
            {
                case Tree.DropModeFlagsEnum.OnItem:
                    return this;
                case Tree.DropModeFlagsEnum.Inbetween:
                    return Parent;
            }
            return null;
        }

        public Tree.DropModeFlagsEnum ShouldAcceptDropRequest(FolderManagerTreeItem item)
        {
            if (item.Parent != this && item.Type.Equals(ChildrenType) && AcceptDrop)
            {
                return Tree.DropModeFlagsEnum.OnItem;
            }
            if (Parent == null) { return Tree.DropModeFlagsEnum.Disabled; }

            if (item.Parent != Parent && item.Type.Equals(Parent.ChildrenType) && Parent.AcceptDrop)
            {
                return Tree.DropModeFlagsEnum.Inbetween;
            }

            return Tree.DropModeFlagsEnum.Disabled;
        }

        public string GetFullPath()
        {
            return Parent != null && !Parent.Name.IsNullOrEmpty() ? $"{Parent.GetFullPath()}/{Name}" : Name;
        }

        public bool UpdateModifiedState()
        {
            var childModified = false;
            foreach (var child in Children)
            {
                if (child.UpdateModifiedState())
                {
                    childModified = true;
                }
            }
            var selfModified = OriginalParent != Parent;
            if (Children.Count > 0 || OriginalChildren.Count > 0)
            {
                // Might not be most efficient but should work
                var childrenModified = !(Children.All(OriginalChildren.Contains) && OriginalChildren.All(Children.Contains));
                selfModified = childrenModified || selfModified;
            }
            IsModified = childModified ? true : selfModified;
            return IsModified;
        }
    }
}