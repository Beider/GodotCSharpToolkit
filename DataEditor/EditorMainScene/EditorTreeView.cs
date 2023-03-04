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
        private TreeItem Root;
        private IDataEditor Editor;
        private Dictionary<string, AbstractEditorTreeItem> TreeItemLookup = new Dictionary<string, AbstractEditorTreeItem>();
        private List<AbstractEditorRootItem> RootItems = new List<AbstractEditorRootItem>();
        private Dictionary<string, TreeItem> Categories = new Dictionary<string, TreeItem>();

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
            Clear();
            TreeItemLookup.Clear();
            InCode = true;
            HideRoot = true;
            Root = CreateItem(null);
            foreach (var item in RootItems)
            {
                item.Init(GetParent(item), Editor);
                if (reload) { item.Reload(); }
                item.CreateRootItem();
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