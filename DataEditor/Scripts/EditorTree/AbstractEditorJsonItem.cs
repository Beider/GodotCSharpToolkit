using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.DataManager;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// This is a class made to make it easy to use the tree with standard CSDO json classes
    /// </summary>
    public abstract class AbstractEditorJsonItem<T, U> : AbstractEditorRootItem, IAbstractJsonEditor where T : IJsonFile<U> where U : JsonDefWithName
    {
        public List<JsonDefWithName> Values { get; protected set; } = new List<JsonDefWithName>();
        public List<JsonDefWithName> ChangedObjects = new List<JsonDefWithName>();
        public List<String> FileNames = new List<string>();
        public TreeItem RootItem = null;

        /// <summary>
        /// Workaround for TreeItem.Select()
        /// </summary>
        private bool InCode = false;

        /// <summary>
        /// Used to build the context menu so we can say "Add new <JsonItemName>"
        /// </summary>
        protected string ContextMenuAddNewItemName = "item";

        /// <summary>
        /// Get the data path relative to res://
        /// We will use the same relative paths in the local directory
        /// </summary>
        protected abstract string GetRelativeDataPath();

        /// <summary>
        /// Get the editor for the given json def
        /// </summary>
        protected abstract IDataEditorContent GetEditor(U jDef);

        /// <summary>
        /// Called to revert the data. Returns the new data.
        /// </summary>
        public virtual JsonDefWithName Revert(JsonDefWithName item)
        {
            if (item.Original == null) { return item; }
            var newItem = (U)item.Original.Clone(typeof(U));
            newItem.Metadata = item.Metadata;
            return newItem;
        }

        public override void Dispose()
        {
            base.Dispose();
            Values.Clear();
        }

        public override FolderManagerTreeItem GetFolderManagerRootItem(string moduleName, string featurePath)
        {
            var childrenType = typeof(U).Name;
            var fileType = $"File{childrenType}";
            var assetTypePath = FileUtils.NormalizePath(featurePath + GetRelativeDataPath());
            var root = FolderManagerTreeItem.CreateTypeItem(Name, assetTypePath, GetType().Name, this, fileType, Color);
            root.SubPath = GetRelativeDataPath();
            root.Collapsed = true;
            var fileNames = new Dictionary<string, FolderManagerTreeItem>();

            // We only want what is in this feature
            if (featurePath == null) { return root; }

            foreach (var val in Values)
            {
                if (!val.GetModName().Equals(moduleName)) { continue; }
                if (!val.SourceFile.StartsWith(featurePath)) { continue; }

                if (!fileNames.ContainsKey(val.GetCategory()))
                {
                    var fileItem = FolderManagerTreeItem.CreateTypeItem(val.GetCategory(), val.SourceFile, fileType, this, childrenType, Color);
                    fileItem.Collapsed = true;
                    fileItem.AllowMove = true;
                    fileItem.SaveAction = (name, list) => SaveFileContent<T, U>(list.Cast<U>().ToList(), name);
                    root.AddChild(fileItem);
                    fileNames.Add(val.GetCategory(), fileItem);
                }
                // Insert file names
                var listItem = FolderManagerTreeItem.CreateJsonItem(val.GetName(), val.SourceFile, childrenType, val, GetItemColor(val, true));
                fileNames[val.GetCategory()].AddChild(listItem);
            }
            return root;
        }


        /// <summary>
        /// Duplicates a piece of data and sets the new name
        /// </summary>
        public virtual JsonDefWithName Duplicate(JsonDefWithName item, string category = null)
        {
            var newItem = (U)item.Clone(typeof(U));
            var originalCopy = (U)item.Clone(typeof(U));

            newItem.SetName($"{item.GetName()}_dup");
            newItem.SourceFile = GetSourceFile(category != null ? category : item.GetCategory());
            newItem.SetUniqueId(newItem.CreateNewUniqueID());

            originalCopy.SetUniqueId(newItem.GetUniqueId());
            originalCopy.SetName(newItem.GetName());
            originalCopy.SourceFile = newItem.SourceFile;

            newItem.Original = originalCopy;
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_MOD, ModName);
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_FEATURE, FeatureName);
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_EDITOR, this);
            newItem.IsModified = true;
            newItem.IsNew = true;
            return newItem;
        }

        /// <summary>
        /// Create a new data with the given name
        /// </summary>
        public virtual JsonDefWithName CreateNew(string currentCategory)
        {
            JsonDefWithName newItem = Activator.CreateInstance(typeof(U)) as JsonDefWithName;
            newItem.SetName($"New {ContextMenuAddNewItemName}");
            if (newItem.GetUniqueId().IsNullOrEmpty())
            {
                newItem.SetUniqueId(System.Guid.NewGuid().ToString());
            }
            newItem.SourceFile = GetSourceFile(currentCategory);
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_MOD, ModName);
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_FEATURE, FeatureName);
            newItem.AddMetadata(DataEditorConstants.METADATA_KEY_EDITOR, this);
            newItem.IsModified = true;
            newItem.IsNew = true;
            return newItem;
        }

        public override void Reload()
        {
            AboutToReload();
            ChangedObjects.Clear();
            Values.Clear();
            FileNames.Clear();
            var paths = GetDataPaths();
            var data = JsonDataManager.GetDictionary<T, U>(paths, true, true);
            foreach (var key in data.Keys)
            {
                var jDef = (JsonDefWithName)data[key];
                jDef.Original = (U)jDef.Clone(typeof(U));
                jDef.OnStatusChange += OnStatusChange;
                if (!FileNames.Contains(jDef.GetCategory()))
                {
                    FileNames.Add(jDef.GetCategory());
                }
                jDef.AddMetadata(DataEditorConstants.METADATA_KEY_MOD, ModName);
                jDef.AddMetadata(DataEditorConstants.METADATA_KEY_FEATURE, FeatureName);
                jDef.AddMetadata(DataEditorConstants.METADATA_KEY_EDITOR, this);
                Values.Add(jDef);
            }
            FinishedReload();
        }

        /// <summary>
        /// Can be used to do stuff before reload
        /// </summary>
        protected virtual void AboutToReload() { }

        /// <summary>
        /// Can be used to do stuff after reload
        /// </summary>
        protected virtual void FinishedReload() { }

        public override bool HasUnsavedChanges()
        {
            return ChangedObjects.Count > 0;
        }

        /// <summary>
        /// Simply track how many changed objects we got
        /// </summary>
        private void OnStatusChange(JsonDefWithName def)
        {
            if ((def.IsModified || def.IsNew || def.IsTaggedForDelete))
            {
                if (!ChangedObjects.Contains(def))
                {
                    ChangedObjects.Add(def);
                }
            }
            else if (ChangedObjects.Contains(def))
            {
                ChangedObjects.Remove(def);
            }

            var item = Editor.Tree.GetTreeItemById(def.GetUniqueId());
            if (item != null)
            {
                RefreshItem(item, def);
            }
        }

        public override void Save()
        {
            if (Editor.Preferences.ShouldUseLocalPath())
            {
                SaveToDisk<T, U>(Editor.Preferences.SettingLocalSavePath, true);
            }
        }

        protected bool SaveToDisk<X, R>(string rootPath, bool localOnly) where X : IJsonFile<R> where R : JsonDefWithName
        {
            if (!Editor.Preferences.ShouldUseLocalPath()) { return false; }
            var path = ModPaths.Find(p => p.StartsWith(Editor.Preferences.SettingLocalSavePath));
            if (path.IsNullOrEmpty()) { return false; }

            path += GetRelativeDataPath();
            path = FileUtils.NormalizePath(path);

            foreach (var cat in FileNames)
            {
                var fileName = path + GetFileName(cat);
                var list = GetFilteredListInCategory<R>(cat, localOnly);

                if (list.Count == 0)
                {
                    if (FileUtils.FileExists(fileName))
                    {
                        FileUtils.Delete(fileName);
                    }
                    continue;
                }

                FileUtils.CreateDirectory(path);
                SaveFileContent<X, R>(list, fileName);
            }
            return true;
        }

        private void SaveFileContent<X, R>(List<R> list, string fileName) where X : IJsonFile<R> where R : JsonDefWithName
        {
            var jsonFileContent = Activator.CreateInstance<X>();
            jsonFileContent.SetValues(list.ToArray());
            var jsonContent = Utils.ToJson(jsonFileContent);
            FileUtils.SaveToFile(jsonContent, fileName);
        }

        /// <summary>
        /// Can be overriden if you want different logic such as adding mod paths
        /// </summary>
        protected virtual List<string> GetDataPaths()
        {
            var relativePath = GetRelativeDataPath();
            var list = new List<string>();
            foreach (var path in ModPaths)
            {
                list.Add(FileUtils.NormalizePath($"{path}{relativePath}"));
            }

            return list;
        }

        protected virtual string GetFileName(string category)
        {
            return $"{category}.json";
        }

        public List<JsonDefWithName> GetAllInCategory(string category)
        {
            var returnList = new List<JsonDefWithName>();
            foreach (var data in Values)
            {
                if (data.GetCategory().Equals(category))
                {
                    returnList.Add(data);
                }
            }
            return returnList;
        }

        protected List<R> GetFilteredListInCategory<R>(string category, bool localOnly, bool includeDelete = false) where R : JsonDefWithName
        {
            var returnList = new List<R>();
            foreach (var def in GetAllInCategory(category))
            {
                if (localOnly && !(def.IsLocal() || def.IsModified || def.IsNew))
                {
                    continue;
                }
                if (includeDelete || !def.IsTaggedForDelete)
                {
                    returnList.Add((R)def);
                }
            }
            return returnList;
        }

        public override TreeItem CreateRootItem()
        {
            Key = Editor.Tree.GetUniqueKey(Parent, Name);
            RootItem = base.CreateRootItem();

            var files = new List<string>(FileNames);
            if (Editor.Preferences.PrefSortTree) { files.Sort(); }

            foreach (var file in files)
            {
                // Get items, skip if we have none, if not create category
                var list = GetFilteredListInCategory<JsonDefWithName>(file, Editor.Preferences.PrefIsLocalOnly, true);
                var fileItem = CreateFileItem(RootItem, file, FillContextMenuForFile);
                if (list.Count == 0) { continue; }


                // Create tree items
                var itemList = new List<DelegateEditorTreeItem>();
                foreach (var jDef in list)
                {
                    if (Editor.Tree.IsFiltered(jDef.GetUniqueId())) { continue; }
                    var aTreeItem = Editor.Tree.CreateDelegateTreeItem(fileItem, jDef.GetName(), jDef.GetUniqueId(), true, GetItemColor(jDef, true),
                                    GetItemColor(jDef, false), OnJsonItemSelected, ModPaths, ModName, FeatureName, jDef);
                    aTreeItem.OnContextMenuFill = FillContextMenuForItem;
                    aTreeItem.IsLeaf = true;
                    itemList.Add(aTreeItem);
                }
                if (Editor.Preferences.PrefSortTree) { SortTreeItemList(itemList); }

                foreach (var aItem in itemList)
                {
                    var treeItem = CreateJsonEntryItem(fileItem, aItem);
                }

            }

            return RootItem;
        }

        protected virtual void SortTreeItemList(List<DelegateEditorTreeItem> list)
        {
            var nameDelegate = Editor.Tree.GetDisplayNameDelegate();
            list.Sort((i1, i2) =>
            {
                return nameDelegate(i1).CompareTo(nameDelegate(i2));
            });
        }

        /// <summary>
        /// Can override to change behavior
        /// </summary>
        protected virtual void OnFileItemSelected(DelegateEditorTreeItem cateogry)
        {

        }

        /// <summary>
        /// Can override to change behavior. You can find the JsonDefWithName in the
        /// treeItem.RelatedData
        /// </summary>
        protected virtual void OnJsonItemSelected(DelegateEditorTreeItem treeItem)
        {
            if (InCode) { return; }
            var data = (JsonDefWithName)treeItem.RelatedData;
            ShowEditor(data);
        }

        public void ShowEditor(JsonDefWithName jsonDef)
        {
            var editor = GetEditor((U)jsonDef);
            if (editor == null)
            {
                return;
            }
            editor.SetData(jsonDef, this);
            var item = Editor.Tree.GetTreeItemById(jsonDef.GetUniqueId());
            if (item != null && item.ResolveSelfItem() != null)
            {
                SelectExpandAndScrollTo(item.ResolveSelfItem());
            }
            Editor.ShowEditor((Control)editor);
        }

        private void SelectExpandAndScrollTo(TreeItem item)
        {
            // Expand
            var parent = item;
            while (parent != null)
            {
                parent.Collapsed = false;
                parent = parent.GetParent();
            }

            // Workaround so we don't go into an infinite loop
            InCode = true;
            item.Select(0);
            InCode = false;

            Editor.Tree.ScrollToItem(item);
        }

        protected override void OnSelectItemRequest(string uniqueId)
        {
            foreach (var val in Values)
            {
                if (val.GetUniqueId().Equals(uniqueId))
                {
                    ShowEditor(val);
                }
            }
        }

        private void RefreshItem(AbstractEditorTreeItem item, JsonDefWithName def)
        {
            var itm = item.ResolveSelfItem();
            itm.SetCustomColor(0, GetItemColor(def, true));
            itm.SetCustomBgColor(0, GetItemColor(def, false));
            itm.SetText(0, def.GetName());
            item.Name = def.GetName();
        }

        /// <summary>
        /// Can override to provide item colors if you want
        /// </summary>
        protected virtual Color GetItemColor(JsonDefWithName item, bool isForeground)
        {
            return DataEditorConstants.GetItemColor(item, Editor, isForeground);
        }

        /// <summary>
        /// Can override to provide category colors if you want
        /// </summary>
        protected virtual Color GetCategoryColor(string cat, bool isForeground)
        {
            if (isForeground)
            {
                return Editor.Preferences.GetDefaultColor();
            }
            return Editor.Preferences.GetDefaultBgColor();
        }

        private TreeItem CreateJsonEntryItem(TreeItem parent, AbstractEditorTreeItem item)
        {
            var treeItem = Editor.Tree.CreateTreeItem(parent, item);
            return treeItem;
        }

        private TreeItem CreateFileItem(TreeItem parent, string name, Func<DelegateEditorTreeItem, EditorPopupMenu, bool> contextMenu = null)
        {
            var key = Editor.Tree.GetUniqueKey(parent, name);
            var item = Editor.Tree.CreateDelegateTreeItem(parent, name, key, true,
                    GetCategoryColor(name, true), GetCategoryColor(name, false), OnFileItemSelected, ModPaths, FeatureName, ModName);
            item.OnContextMenuFill = contextMenu;
            item.Color = Color;
            return Editor.Tree.CreateTreeItem(parent, item); ;
        }

        public bool AnyMatchInData(Func<JsonDefWithName, bool> MatchingFunction)
        {
            foreach (var def in Values)
            {
                if (MatchingFunction.Invoke(def))
                {
                    return true;
                }
            }
            return false;
        }

        public bool FillContextMenuForItem(DelegateEditorTreeItem item, EditorPopupMenu menu)
        {
            var data = (JsonDefWithName)item.RelatedData;
            menu.AddPopupMenuSeparator(item.Name);
            menu.AddPopupMenuEntry("Filter For Usage", () =>
            {
                Editor.SearchWindow.Search(item.Key, false);
            }, DataEditorConstants.ICON_SEARCH);
            menu.AddPopupMenuEntry("Duplicate", () =>
            {
                AddAndShowNewItem(Duplicate(data));
            }, DataEditorConstants.ICON_DUPLICATE);
            menu.AddPopupMenuEntry("Copy", () =>
            {
                Editor.Tree.CopiedObject = data;
            }, DataEditorConstants.ICON_COPY);
            if (data.IsLocal())
            {
                if ((data.IsModified || data.IsTaggedForDelete) && !data.IsNew)
                {
                    menu.AddPopupMenuEntry("Revert", () =>
                    {
                        Editor.ShowConfirmDialog($"Are you sure you wish to revert {data.GetName()}? All unsaved changes will be lost.",
                        accept =>
                        {
                            if (accept)
                            {
                                Values.Remove(data);
                                AddAndShowNewItem(Revert(data));
                            }
                        });
                    }, DataEditorConstants.ICON_REVERT);
                }
                var deleteString = data.IsTaggedForDelete ? "Remove delete tag " : "Tag for delete ";
                menu.AddPopupMenuEntry(deleteString, () =>
                {
                    data.IsTaggedForDelete = !data.IsTaggedForDelete;
                    RefreshItem(item, data);
                }, DataEditorConstants.ICON_DELETE);
            }

            var parentKey = item.ResolveSelfItem().GetParent().GetMetadata(0).ToString();
            var parent = (DelegateEditorTreeItem)Editor.Tree.GetTreeItemById(parentKey);
            FillContextMenuForFile(parent, menu);

            return true;
        }

        public bool FillContextMenuForFile(DelegateEditorTreeItem item, EditorPopupMenu menu)
        {
            menu.AddPopupMenuSeparator(item.Name);
            menu.AddPopupMenuEntry($"Add new {ContextMenuAddNewItemName}", () =>
            {
                AddAndShowNewItem(CreateNew(item.Name));
            }, DataEditorConstants.ICON_NEW);
            if (Editor.Tree.CopiedObject != null && typeof(U) == Editor.Tree.CopiedObject.GetType())
            {
                menu.AddPopupMenuEntry($"Paste '{Editor.Tree.CopiedObject.GetName()}' ", () =>
                {
                    AddAndShowNewItem(Duplicate(Editor.Tree.CopiedObject, item.Name));
                }, DataEditorConstants.ICON_PASTE);
            }
            FillContextMenu(menu);
            return true;
        }

        private void AddAndShowNewItem(JsonDefWithName item)
        {
            item.OnStatusChange += OnStatusChange;
            Values.Add(item);
            OnStatusChange(item);
            Editor.Tree.RefreshTree(false);
            ShowEditor(item);
        }

        /// <summary>
        /// Our own context menu
        /// </summary>
        public override bool FillContextMenu(EditorPopupMenu menu)
        {
            menu.AddPopupMenuSeparator(Name);
            menu.AddPopupMenuEntry("Add new file", () =>
            {
                Action<string, string> addNew = (name, listValue) =>
                {
                    FileNames.Add(name);
                    Editor.Tree.RefreshTree(false);

                };
                Func<string, bool> nameValidator = (name) =>
                {
                    if (name.IsNullOrEmpty()) { return false; }
                    foreach (var file in FileNames)
                    {
                        if (file.ToLower().Equals(name.ToLower()))
                        {
                            return false;
                        }
                    }
                    return true;
                };
                Editor.ShowTextEntryDialog("Please enter the file name", "Name", addNew, nameValidator);


            }, DataEditorConstants.ICON_NEW);
            return true;
        }

        protected string GetSourceFile(string category)
        {
            return FileUtils.NormalizePath(Editor.Preferences.SettingLocalSavePath + $"{ModName}\\{FeatureName}\\" + GetRelativeDataPath() + GetFileName(category));
        }

        #region Validators

        /// <summary>
        /// Validates that the name of the JsonDefWithName is unique.
        /// Can be used with the JsonGenericEditor
        /// </summary>
        public bool ValidateNameUnique<X>(string name, object data, object value) where X : IAbstractJsonEditor
        {
            if (!JsonGenericEditorInput.ValidateTextNotNullOrEmpty(name, data, value))
            {
                return false;
            }

            var rootItems = Editor.Tree.GetAllItemsByRootItemType<X>();
            var strName = value.ToString();
            var jDef = (JsonDefWithName)data;

            foreach (var eItem in rootItems)
            {
                if (!eItem.IsNameUnique(strName, jDef))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsNameUnique(string name, JsonDefWithName original)
        {
            return Values.Find(v =>
            {
                if (original == v) { return false; }
                return v.GetName().Equals(name, StringComparison.OrdinalIgnoreCase);
            }) == null;
        }

        #endregion

    }
}