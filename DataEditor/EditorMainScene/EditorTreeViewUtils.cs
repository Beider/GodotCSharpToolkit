using Godot;
using System;
using System.Linq;
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
        private TreeItem GetParent(AbstractEditorRootItem item, TreeItem defaultParent)
        {
            if (item.Category.IsNullOrEmpty())
            {
                return defaultParent;
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
            TreeItemLookup.Add(item.Key, item);
            var nameDelegate = Editor.Tree.GetDisplayNameDelegate();
            var treeItem = CreateTreeItem(parent, nameDelegate(item), item.Color, item.ColorBg, item.Collapsed, item.Key);
            item.TreeItemSelf = treeItem;
            return treeItem;
        }

        /// <summary>
        /// Create a tree item using preferences from the storage
        /// </summary>
        public TreeItem CreateTreeItem(TreeItem parent, string name, Color color, Color bgColor, bool collapsed, string key = "")
        {
            var treeItem = CreateItem(parent);
            var itemColor = Editor.Preferences.GetTreeItemColor(name, color);
            treeItem.SetText(0, name);
            var isCollapsed = Editor.Preferences.GetTreeItemCollapsedState(treeItem, collapsed);
            treeItem.SetCustomColor(0, itemColor);
            treeItem.SetCustomBgColor(0, bgColor);
            treeItem.Collapsed = isCollapsed;
            treeItem.SetMetadata(0, key);
            return treeItem;
        }

        protected DelegateEditorTreeItem CreateModItem(TreeItem parent, string name, List<string> modPaths)
        {
            var key = GetUniqueKey(parent, name);
            return CreateDelegateTreeItem(parent, name, key, true,
                    DataEditorConstants.COLOR_DEFAULT, DataEditorConstants.COLOR_BG_DEFAULT, null, modPaths, name);
        }

        public DelegateEditorTreeItem CreateDelegateTreeItem(TreeItem parent, string name, string key, bool collapsed,
                    Color defaultColor, Color defaultBgColor, Action<DelegateEditorTreeItem> onSelection, List<string> modPaths,
                    string modName, object relatedData = null)
        {
            var newItem = new DelegateEditorTreeItem();
            newItem.Init(parent, Editor, modPaths, modName);
            newItem.Name = name;
            newItem.Key = key;
            newItem.Collapsed = collapsed;
            newItem.Color = defaultColor;
            newItem.ColorBg = defaultBgColor;
            newItem.OnSelection = onSelection;
            newItem.RelatedData = relatedData;
            return newItem;
        }

        private void ResolveRootItems()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(AbstractEditorRootItem).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    RootItemTypes.Add(type);
                }
                else if (typeof(AbstractEditorTreeModFolderProvider).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    Provider = Activator.CreateInstance(type) as AbstractEditorTreeModFolderProvider;
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

        /// <summary>
        /// Adds a delegate that is used to get the name of an AbstractEditorTreeItem,
        /// will automatically let you cycle through any you add in the toolbar menu.
        /// </summary>
        public void AddDisplayDelegate(string name, Func<AbstractEditorTreeItem, string> del)
        {
            DisplayNameDelegates.Add(name, del);
        }

        public Func<AbstractEditorTreeItem, string> GetDisplayNameDelegate()
        {
            var key = Editor.Preferences.PrefDisplayNameDelegateName;
            if (!DisplayNameDelegates.ContainsKey(key))
            {
                key = DisplayNameDelegates.Keys.First();
                Editor.Preferences.PrefDisplayNameDelegateName = key;
            }
            return DisplayNameDelegates[key];
        }

        /// <summary>
        /// Switch to the next display name mode
        /// </summary>
        public void NextDisplayName()
        {
            var key = Editor.Preferences.PrefDisplayNameDelegateName;
            if (DisplayNameDelegates.ContainsKey(key))
            {
                bool pick = false;

                foreach (var keyName in DisplayNameDelegates.Keys)
                {
                    if (keyName.Equals(key))
                    {
                        pick = true;
                    }
                    else if (pick)
                    {
                        Editor.Preferences.PrefDisplayNameDelegateName = keyName;
                        return;
                    }
                }
            }

            Editor.Preferences.PrefDisplayNameDelegateName = DisplayNameDelegates.Keys.First();
            return;
        }

        /// <summary>
        /// Creates a unique key for the tree item based on parents keys.
        /// Should only be used for things where a unique UUID is not available
        /// </summary>
        public string GetUniqueKey(TreeItem parent, string itemName)
        {
            string key = itemName;
            var item = parent;
            while (item != null)
            {
                key = $"{item.GetMetadata(0)}.{key}";
                item = item.GetParent();
            }

            return key;
        }
    }
}