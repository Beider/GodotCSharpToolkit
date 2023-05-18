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
        public Dictionary<string, AbstractEditorRootItem> RootItems = new Dictionary<string, AbstractEditorRootItem>();
        public Dictionary<string, DelegateEditorTreeItem> ModItems = new Dictionary<string, DelegateEditorTreeItem>();

        private AbstractEditorTreeModFolderProvider ModProvider = null;
        public Dictionary<string, Func<AbstractEditorTreeItem, string>> DisplayNameDelegates = new Dictionary<string, Func<AbstractEditorTreeItem, string>>();

        public JsonDefWithName CopiedObject { get; set; } = null;
        private bool RefreshInProgress = false;

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
            if (RefreshInProgress) { return; }
            if (ModProvider == null)
            {
                Logger.Error($"Please provide a AbstractEditorTreeModFolderProvider provider for the editor.");
                return;
            }

            // Mark as refresh starting
            RefreshInProgress = true;

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
            ModFolders = ModProvider.GetModFolders();

            if (reload)
            {
                foreach (var val in RootItems.Values) { val.Dispose(); }
                RootItems.Clear();
                foreach (var val in ModItems.Values) { val.Dispose(); }
                ModItems.Clear();
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
                // Resolve the mod item
                TreeItem modItem = null;
                if (!ModItems.ContainsKey(modName))
                {
                    var delegateItem = CreateModItem(Root, modName, ModFolders[modName]);
                    modItem = CreateTreeItem(Root, delegateItem);
                    ModItems[modName] = delegateItem;
                }
                else
                {
                    // Recreate the tree item
                    modItem = CreateTreeItem(Root, ModItems[modName]);
                }

                // Fill the tree
                foreach (var type in RootItemTypes)
                {
                    string rootKey = $"{modName}?{type.Name}";
                    AbstractEditorRootItem item = null;
                    if (!RootItems.ContainsKey(rootKey))
                    {
                        item = Activator.CreateInstance(type) as AbstractEditorRootItem;
                        item.Init(modItem, Editor, ModFolders[modName], modName);
                        item.Reload();
                        item.Key = rootKey;
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

            RefreshInProgress = false;
        }

        private void AddLocalMods()
        {
            // Add local paths
            if (Editor.Preferences.ShouldUseLocalPath())
            {
                foreach (var key in ModFolders.Keys)
                {
                    var normalized = FileUtils.NormalizeDirectory($"{Editor.Preferences.SettingLocalSavePath}{key}");
                    if (!ModFolders[key].Contains(normalized))
                    {
                        ModFolders[key].Add(normalized);
                    }
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
            foreach (var item in ModItems.Values)
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
            foreach (var item in ModItems.Values)
            {
                item.Save();
            }
        }
    }
}