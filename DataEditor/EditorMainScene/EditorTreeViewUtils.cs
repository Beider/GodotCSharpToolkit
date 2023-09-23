using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorTreeView : Tree
    {
        /// <summary>
        /// Creates a tree item using the stored preferences from storage
        /// </summary>
        public TreeItem CreateTreeItem(TreeItem parent, AbstractEditorTreeItem item)
        {
            // Add so we can find it again
            if (TreeItemLookup.ContainsKey(item.Key))
            {
                Logger.Error($"Duplicate key added {item.Key}");
            }
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
            var item = CreateDelegateTreeItem(parent, name, key, true,
                    Editor.Preferences.GetModColor(), DataEditorConstants.COLOR_BG_DEFAULT,
                    (dte) => Editor.NotifyOnModuleTreeItemPressed(name, dte), modPaths, name, "");
            item.OnContextMenuFill = (a) => { return FillModContextMenu(name); };

            return item;
        }

        protected DelegateEditorTreeItem CreateModFeatureItem(TreeItem parent, string name, string modName, string featureName, List<string> modPaths)
        {
            var key = GetUniqueKey(parent, name);
            var item = CreateDelegateTreeItem(parent, name, key, true,
                    Colors.White, DataEditorConstants.COLOR_BG_DEFAULT,
                    null, modPaths, modName, featureName);
            item.OnContextMenuFill = (a) => { return FillModFeatureContextMenu(modName, name); };

            return item;
        }

        public DelegateEditorTreeItem CreateDelegateTreeItem(TreeItem parent, string name, string key, bool collapsed,
                    Color defaultColor, Color defaultBgColor, Action<DelegateEditorTreeItem> onSelection, List<string> modPaths,
                    string modName, string feature, object relatedData = null)
        {
            var newItem = new DelegateEditorTreeItem();
            newItem.Init(parent, Editor, modPaths, modName, feature);
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
                // Find abstract tree items, but skip the delegate item
                if (typeof(AbstractEditorRootItem).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    RootItemTypes.Add(type);
                }
                else if (typeof(AbstractEditorTreeModFolderProvider).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    ModProvider = Activator.CreateInstance(type) as AbstractEditorTreeModFolderProvider;
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
            if (key != "" && TreeItemLookup.ContainsKey(key))
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

        /// <summary>
        /// Get the root item of the given type
        /// </summary>
        public AbstractEditorRootItem GetRootItemByType(string modName, Type type)
        {
            string rootKey = $"{modName}_{type.Name}";
            if (!RootItems.ContainsKey(rootKey)) { return null; }
            return RootItems[rootKey];
        }

        /// <summary>
        /// Get all the root items of the given type
        /// </summary>
        public List<T> GetAllItemsByRootItemType<T>()
        {
            var returnList = new List<T>();
            foreach (var item in RootItems.Values)
            {
                if (item is T rItem)
                {
                    returnList.Add(rItem);
                }
            }

            return returnList;
        }

        public void SetFilter(string query, bool exactMatch = false)
        {
            Filter.Clear();
            FilterActive = true;

            var results = Editor.Search(query, exactMatch);

            foreach (var jDef in results)
            {
                Filter.Add(jDef.GetUniqueId());
            }

            RefreshTree(false);
            Editor.SearchWindow.Visible = true;
        }

        public void ClearFilter()
        {
            Filter.Clear();
            FilterActive = false;
        }

        /// <summary>
        /// Returns if this item is filtered or not
        /// </summary>
        public bool IsFiltered(string identifier)
        {
            if (!FilterActive) { return false; }
            return !Filter.Contains(identifier);
        }
    }
}