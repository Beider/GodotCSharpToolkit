using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorTreeView : Tree
    {

        /// <summary>
        /// Get the parent for a root item. If the item has a category that will be the parent.
        /// </summary>
        private TreeItem GetParent(AbstractEditorRootItem item)
        {
            if (item.Category.IsNullOrEmpty())
            {
                return Root;
            }

            if (!Categories.ContainsKey(item.Category))
            {
                var color = Editor.Preferences.GetTreeItemColor(DataEditorConstants.ROOT_CATEGORIES_COLOR, DataEditorConstants.COLOR_DEFAULT);
                var colorbg = Editor.Preferences.GetTreeItemColor(DataEditorConstants.ROOT_CATEGORIES_COLOR_BG, DataEditorConstants.COLOR_BG_DEFAULT);
                Categories.Add(item.Category, CreateTreeItem(Root, item.Category, color, colorbg, true));
            }

            return Categories[item.Category];
        }

        /// <summary>
        /// Creates a tree item using the stored preferences from storage
        /// </summary>
        public TreeItem CreateTreeItem(TreeItem parent, AbstractEditorTreeItem item)
        {
            // Add so we can find it again
            var key = Guid.NewGuid().ToString();
            TreeItemLookup.Add(key, item);
            var treeItem = CreateTreeItem(parent, item.Name, item.Color, item.ColorBg, item.Collapsed, key);
            item.TreeItemSelf = treeItem;
            return treeItem;
        }

        /// <summary>
        /// Create a tree item using preferences from the storage
        /// </summary>
        public TreeItem CreateTreeItem(TreeItem parent, string name, Color color, Color bgColor, bool collapsed, string key = "")
        {
            var treeItem = CreateItem(parent);
            var isCollapsed = Editor.Preferences.GetTreeItemCollapsedState(name, collapsed);
            var itemColor = Editor.Preferences.GetTreeItemColor(name, color);
            treeItem.SetText(0, name);
            treeItem.SetCustomColor(0, itemColor);
            treeItem.SetCustomBgColor(0, bgColor);
            treeItem.Collapsed = isCollapsed;
            treeItem.SetMetadata(0, key);
            return treeItem;
        }

        private void ResolveRootItems()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(AbstractEditorRootItem).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    RootItems.Add(Activator.CreateInstance(type) as AbstractEditorRootItem);
                }
            }
        }

        /// <summary>
        /// Get the abstract tree item that belongs to the item in the tree. Or null if none
        /// </summary>
        private AbstractEditorTreeItem GetAbstractTreeItem(TreeItem item)
        {
            object obj = item.GetMetadata(0);
            string key = obj == null ? "" : obj.ToString();
            if (key != "")
            {
                return TreeItemLookup[key];
            }
            return null;
        }
    }
}