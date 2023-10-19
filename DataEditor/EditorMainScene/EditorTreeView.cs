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

        private List<string> Filter = new List<string>();
        public bool FilterActive { private set; get; } = false;

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

        public void RefreshTree(bool reload = true)
        {
            if (RefreshInProgress) { return; }
            if (ModProvider == null)
            {
                Logger.Error($"Please provide a AbstractEditorTreeModFolderProvider provider for the editor.");
                return;
            }

            string selectedItem = "";

            if (GetSelected() != null)
            {
                selectedItem = (string)GetSelected().GetMetadata(0);
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

                // Add features
                var features = GetFeatures(modName);

                foreach (var feature in features.Keys)
                {
                    var delegateItem = CreateModFeatureItem(modItem, feature, modName, feature, features[feature]);
                    var delTree = CreateTreeItem(modItem, delegateItem);

                    // Fill the tree
                    foreach (var type in RootItemTypes)
                    {
                        string rootKey = $"{modName}?{feature}?{type.Name}";
                        AbstractEditorRootItem item = null;
                        if (!RootItems.ContainsKey(rootKey))
                        {
                            item = Activator.CreateInstance(type) as AbstractEditorRootItem;
                            item.Init(delTree, Editor, features[feature], modName, feature);
                            item.Reload();
                            item.Key = rootKey;
                            RootItems.Add(rootKey, item);
                        }
                        else
                        {
                            item = RootItems[rootKey];
                            item.Init(delTree, Editor, features[feature], modName, feature);
                        }

                        item.CreateRootItem();
                    }
                }


            }
            if (FilterActive)
            {
                // Prune empty items
                PruneEmptyTreeItems(Root);
            }

            FindAndSelect(selectedItem);

            InCode = false;
            RefreshInProgress = false;
        }

        private void FindAndSelect(string id)
        {
            if (id.IsNullOrEmpty()) { return; }
            var tItem = GetTreeItemById(id);
            if (tItem == null) { return; }
            ScrollToItem(tItem.TreeItemSelf);
            tItem.TreeItemSelf.Select(0);
        }

        public Dictionary<string, List<string>> GetFeatures(string modName)
        {
            var returnDict = new Dictionary<string, List<string>>();
            var subfolderList = new List<string>();
            var lowerCaseList = new List<string>();
            foreach (var folder in ModFolders[modName])
            {
                var subs = FileUtils.GetSubDirectories(folder);
                foreach (var subFolder in subs)
                {
                    var subName = System.IO.Path.GetFileName(FileUtils.RemoveLastSlash(subFolder));
                    if (!lowerCaseList.Contains(subName.ToLower()))
                    {
                        subfolderList.Add(subName);
                        lowerCaseList.Add(subName.ToLower());
                    }
                }
            }
            foreach (var sub in subfolderList)
            {
                returnDict.Add(sub, new List<string>());
                foreach (var folder in ModFolders[modName])
                {
                    returnDict[sub].Add(FileUtils.NormalizeDirectory($"{FileUtils.NormalizeDirectory(folder)}{sub}"));
                }
            }

            return returnDict;
        }

        /// <summary>
        /// Return true if it should be pruned
        /// </summary>
        private bool PruneEmptyTreeItems(TreeItem item)
        {
            var removeList = new List<TreeItem>();
            bool hasLeafs = false;
            // Return false if this is a leaf
            var abs = GetAbstractTreeItem(item);
            if (abs != null && abs.IsLeaf)
            {
                return false;
            }

            var child = item.GetChildren();
            while (child != null)
            {
                if (PruneEmptyTreeItems(child))
                {
                    removeList.Add(child);
                }
                else
                {
                    hasLeafs = true;
                }
                child = child.GetNext();
            }

            foreach (var c in removeList)
            {
                item.RemoveChild(c);
            }

            return !hasLeafs;
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