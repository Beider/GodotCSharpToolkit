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
        private TreeItem Root;
        private IDataEditor Editor;
        private Dictionary<string, AbstractEditorTreeItem> TreeItemLookup = new Dictionary<string, AbstractEditorTreeItem>();
        private Dictionary<string, List<string>> ModFolders = new Dictionary<string, List<string>>();
        private List<Type> RootItemTypes = new List<Type>();
        private Dictionary<string, AbstractEditorRootItem> RootItems = new Dictionary<string, AbstractEditorRootItem>();
        private Dictionary<string, TreeItem> Categories = new Dictionary<string, TreeItem>();
        private AbstractEditorTreeModFolderProvider Provider = null;
        public Dictionary<string, Func<AbstractEditorTreeItem, string>> DisplayNameDelegates = new Dictionary<string, Func<AbstractEditorTreeItem, string>>();

        public Dictionary<string, Action> ContextMenuDelegates = new Dictionary<string, Action>();

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
            this.Connect("empty_tree_rmb_selected", this, nameof(OnEmptyTreeRmbSelected));
            this.Connect("empty_rmb", this, nameof(OnEmptyTreeRmbSelected));
            AllowReselect = true;
            AllowRmbSelect = true;

            DisplayNameDelegates.Add("Name", i => i.Name);
            DisplayNameDelegates.Add("Key", i => i.Key);
        }

        private void OnEmptyTreeRmbSelected(Vector2 pos)
        {
            if (!Editor.Preferences.ShouldUseLocalPath()) { return; }
            var menu = Editor.PopupMenu;
            menu.Clear();
            ContextMenuDelegates.Clear();
            menu.RectSize = menu.RectMinSize;
            FillMenu();

            menu.RectPosition = GetViewport().GetMousePosition();
            menu.Popup_();
        }

        private void OnItemRmbSelected(Vector2 pos)
        {
            if (!Editor.Preferences.ShouldUseLocalPath()) { return; }
            if (GetSelected() == null) { return; }
            var data = GetSelected().GetMetadata(0);
            if (data == null) { return; }
            var item = TreeItemLookup.ContainsKey(data.ToString()) ? TreeItemLookup[data.ToString()] : null;
            if (item == null) { return; }

            var menu = Editor.PopupMenu;
            ContextMenuDelegates.Clear();
            menu.Clear();
            menu.RectSize = menu.RectMinSize;
            if (item.FillContextMenu())
            {
                menu.RectPosition = GetViewport().GetMousePosition();
                menu.Popup_();
            }

            FillMenu();
        }
        public void AddContextMenuSeparator(string name)
        {
            Editor.PopupMenu.AddSeparator($"- {name} -");
        }

        public void AddContextMenuEntry(string name, Action action)
        {
            Editor.PopupMenu.AddItem(name);
            ContextMenuDelegates.Add(name, action);
        }

        private void FillMenu()
        {
            AddContextMenuSeparator("General");
            AddContextMenuEntry("Add new mod", () =>
            {
                Func<string, bool> nameValidator = (name) =>
                {
                    if (name.IsNullOrEmpty()) { return false; }
                    foreach (var mod in ModFolders.Keys)
                    {
                        if (mod.ToLower().Equals(name.ToLower()))
                        {
                            return false;
                        }
                    }
                    return true;
                };
                Editor.ShowTextEntryDialog("Please enter the mod name", "Name", AddNewModule, nameValidator);
            });
        }

        private void AddNewModule(string name, string listValue)
        {
            System.IO.Directory.CreateDirectory($"{Editor.Preferences.SettingLocalSavePath}{name}");
            RefreshTree(false);
        }

        private void OnPopupMenuPressed(int index)
        {
            string text = Editor.PopupMenu.GetItemText(index);
            if (ContextMenuDelegates.ContainsKey(text))
            {
                // Invoke the menu item
                ContextMenuDelegates[text]();
            }
        }

        private void HandlePopupMenuClick(string itemName)
        {
            GD.Print($"{itemName} clicked");
        }

        public void Init(IDataEditor editor)
        {
            this.Editor = editor;
            Editor.Preferences.OnPrefsChanged += OnPrefsChanged;
            Editor.PopupMenu.Connect("id_pressed", this, nameof(OnPopupMenuPressed));
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
            Root.SetMetadata(0, "root");

            // Get mod folders
            ModFolders.Clear();
            ModFolders = Provider.GetModFolders();

            if (reload)
            {
                RootItems.Clear();
            }

            // Add local paths
            if (Editor.Preferences.ShouldUseLocalPath())
            {
                foreach (var key in ModFolders.Keys)
                {
                    ModFolders[key].Add($"{Editor.Preferences.SettingLocalSavePath}{key}");
                }

                // Add local only mods
                foreach (var path in System.IO.Directory.GetDirectories(Editor.Preferences.SettingLocalSavePath))
                {
                    var folder = (new System.IO.DirectoryInfo(path)).Name;
                    if (!ModFolders.ContainsKey(folder))
                    {
                        ModFolders.Add(folder, new List<string>());
                        ModFolders[folder].Add(path);
                    }
                }
            }

            // Ensure all paths end with a slash
            foreach (var modN in ModFolders.Keys)
            {
                for (int i = 0; i < ModFolders[modN].Count; i++)
                {
                    var path = ModFolders[modN][i];
                    if (!path.EndsWith("/") || !path.EndsWith("\\")) { path += "/"; }
                    ModFolders[modN][i] = FileUtils.NormalizePath(path);
                }
            }

            // Create items
            foreach (var modName in ModFolders.Keys)
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
                        item.Init(GetParent(item, modItem), Editor, ModFolders[modName], modName);
                        item.Reload();
                        RootItems.Add(rootKey, item);
                    }
                    else
                    {
                        item = RootItems[rootKey];
                        item.Init(GetParent(item, modItem), Editor, ModFolders[modName], modName);
                    }

                    item.CreateRootItem();
                }
            }
            InCode = false;
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