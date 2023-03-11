using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorTreeView : Tree
    {
        public event Action<string> OnSelectItemRequest = delegate { };

        private TreeItem Root;
        private IDataEditor Editor;
        private Dictionary<string, AbstractEditorTreeItem> TreeItemLookup = new Dictionary<string, AbstractEditorTreeItem>();
        public Dictionary<string, List<string>> ModFolders = new Dictionary<string, List<string>>();
        private List<Type> RootItemTypes = new List<Type>();
        private Dictionary<string, AbstractEditorRootItem> RootItems = new Dictionary<string, AbstractEditorRootItem>();
        private AbstractEditorTreeModFolderProvider Provider = null;
        public Dictionary<string, Func<AbstractEditorTreeItem, string>> DisplayNameDelegates = new Dictionary<string, Func<AbstractEditorTreeItem, string>>();

        public JsonDefWithName CopiedObject { get; set; } = null;

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
            this.Connect("item_rmb_selected", this, nameof(OnItemRmbSelected));
            AllowReselect = true;
            AllowRmbSelect = true;

            DisplayNameDelegates.Add("Name", i => i.Name);
            //DisplayNameDelegates.Add("Key", i => i.Key);
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

        public void RefreshTree(bool reload = true, string reloadEditor = "")
        {
            if (Provider == null)
            {
                Logger.Error($"Please provide a AbstractEditorTreeModFolderProvider provider for the editor.");
                return;
            }

            // Current selection if any
            OnSelectItemRequest = delegate { };

            Clear();
            TreeItemLookup.Clear();
            InCode = true;
            HideRoot = true;
            Root = CreateItem(null);
            Root.SetMetadata(0, "root");

            // Get mod folders
            ModFolders.Clear();
            ModFolders = Provider.GetModFolders();

            if (reload)
            {
                RootItems.Clear();
            }

            // Add local paths
            AddLocalMods();

            // Ensure all paths end with a slash
            FixModPathEnding();

            // Create items
            var modKeys = new List<string>(ModFolders.Keys);
            modKeys.Sort();
            foreach (var modName in modKeys)
            {
                var delegateItem = CreateModItem(Root, modName, ModFolders[modName]);
                var modItem = CreateTreeItem(Root, delegateItem);

                foreach (var type in RootItemTypes)
                {
                    string rootKey = $"{modName}_{type.Name}";
                    AbstractEditorRootItem item = null;
                    if (!RootItems.ContainsKey(rootKey))
                    {
                        item = Activator.CreateInstance(type) as AbstractEditorRootItem;
                        item.Init(modItem, Editor, ModFolders[modName], modName);
                        item.Reload();
                        RootItems.Add(rootKey, item);
                    }
                    else
                    {
                        item = RootItems[rootKey];
                        item.Init(modItem, Editor, ModFolders[modName], modName);
                    }

                    item.CreateRootItem();
                }
            }
            InCode = false;
            if (reloadEditor != "")
            {
                OnSelectItemRequest(reloadEditor);
            }
        }

        private void AddLocalMods()
        {
            // Add local paths
            if (Editor.Preferences.ShouldUseLocalPath())
            {
                foreach (var key in ModFolders.Keys)
                {
                    ModFolders[key].Add($"{Editor.Preferences.SettingLocalSavePath}{key}");
                }

                // Add local only mods
                foreach (var path in FileUtils.GetSubDirectories(Editor.Preferences.SettingLocalSavePath))
                {

                    var folder = FileUtils.GetDirectoryName(path);
                    if (!ModFolders.ContainsKey(folder))
                    {
                        ModFolders.Add(folder, new List<string>());
                        ModFolders[folder].Add(path);
                    }
                }
            }
        }

        private void FixModPathEnding()
        {
            // Ensure all paths end with a slash
            foreach (var modN in ModFolders.Keys)
            {
                for (int i = 0; i < ModFolders[modN].Count; i++)
                {
                    ModFolders[modN][i] = FileUtils.NormalizeDirectory(ModFolders[modN][i]);
                }
            }
        }

        public bool HasUnsavedChanges()
        {
            foreach (var item in RootItems.Values)
            {
                if (item.HasUnsavedChanges())
                {
                    return true;
                }
            }
            return false;
        }

        public void Save()
        {
            foreach (var item in RootItems.Values)
            {
                item.Save();
            }
        }
    }
}