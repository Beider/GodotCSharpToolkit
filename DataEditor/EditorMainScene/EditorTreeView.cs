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
        private TreeItem Root;
        private IDataEditor Editor;
        private Dictionary<string, AbstractEditorTreeItem> TreeItemLookup = new Dictionary<string, AbstractEditorTreeItem>();
        private List<AbstractEditorRootItem> RootItems = new List<AbstractEditorRootItem>();
        private Dictionary<string, TreeItem> Categories = new Dictionary<string, TreeItem>();
        private AbstractEditorTreeModFolderProvider Provider = null;
        public Dictionary<string, Func<AbstractEditorTreeItem, string>> DisplayNameDelegates = new Dictionary<string, Func<AbstractEditorTreeItem, string>>();

        /// <summary>
        /// Workaround for some godot bugs with signals triggering from code on trees
        /// </summary>
        private bool InCode = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ResolveRootItems();
            this.Connect("item_collapsed", this, nameof(OnTreeItemCollapsed));
            this.Connect("item_selected", this, nameof(OnTreeItemSelected));

            DisplayNameDelegates.Add("Name", i => i.Name);
        }

        public void Init(IDataEditor editor)
        {
            this.Editor = editor;
            Editor.Preferences.OnPrefsChanged += OnPrefsChanged;
            RefreshTree();
        }

        private void OnPrefsChanged()
        {
            RefreshTree(false);
        }

        public AbstractEditorTreeItem GetTreeItemById(string key)
        {
            if (TreeItemLookup.ContainsKey(key))
            {
                return TreeItemLookup[key];
            }
            return null;
        }

        public void RefreshTree(bool reload = true)
        {
            if (Provider == null)
            {
                Logger.Error($"Please provide a AbstractEditorTreeModFolderProvider provider for the editor.");
                return;
            }
            Clear();
            TreeItemLookup.Clear();
            InCode = true;
            HideRoot = true;
            Root = CreateItem(null);

            // Get mod folders
            var mods = Provider.GetRootItemList();

            // Add local path
            if (Editor.Preferences.ShouldUseLocalPath())
            {
                foreach (var key in mods.Keys)
                {
                    mods[key].Add($"{Editor.Preferences.SettingLocalSavePath}{key}");
                }
            }

            // Create items
            foreach (var key in mods.Keys)
            {
                var delegateItem = CreateModItem(Root, key, mods[key]);
                var modItem = CreateTreeItem(Root, delegateItem);
                foreach (var item in RootItems)
                {
                    item.Init(GetParent(item, modItem), Editor, mods[key]);
                    if (reload) { item.Reload(); }
                    item.CreateRootItem();
                }
            }
            InCode = false;
        }

        public bool HasUnsavedChanges()
        {
            return RootItems.Find(item => item.HasUnsavedChanges()) != null;
        }

        public void Save()
        {
            RootItems.ForEach(item => item.Save());
        }
    }
}