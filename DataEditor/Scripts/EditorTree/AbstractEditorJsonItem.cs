using Godot;
using System;
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
    public abstract class AbstractEditorJsonItem<T, U> : AbstractEditorRootItem where T : IJsonFile<U> where U : JsonDefWithName
    {
        private List<JsonDefWithName> Values = new List<JsonDefWithName>();
        private List<JsonDefWithName> ChangedObjects = new List<JsonDefWithName>();
        public List<String> FileNames = new List<string>();
        public TreeItem RootItem = null;

        /// <summary>
        /// Get the data path relative to res://
        /// We will use the same relative paths in the local directory
        /// </summary>
        protected abstract string GetRelativeDataPath();

        /// <summary>
        /// Get the editor for the given json def
        /// </summary>
        protected abstract Control GetEditor(JsonDefWithName jDef);

        /// <summary>
        /// Called to revert the data. Returns the new data.
        /// </summary>
        public abstract JsonDefWithName Revert(JsonDefWithName item);

        /// <summary>
        /// Duplicates a piece of data and sets the new name
        /// </summary>
        public abstract JsonDefWithName Duplicate(JsonDefWithName item, string category = null);

        /// <summary>
        /// Create a new data with the given name
        /// </summary>
        public abstract JsonDefWithName CreateNew(string currentCategory);

        protected string JsonItemName = "item";

        public override void Reload()
        {
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
                Values.Add(jDef);
            }
        }

        public override bool HasUnsavedChanges()
        {
            return ChangedObjects.Count > 0;
        }

        /// <summary>
        /// Simply track how many changed objects we got
        /// </summary>
        private void OnStatusChange(JsonDefWithName def)
        {
            if ((def.IsModified || def.IsNew || def.IsTaggedForDelete) && !ChangedObjects.Contains(def))
            {
                ChangedObjects.Add(def);
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

            System.IO.Directory.CreateDirectory(path);

            foreach (var cat in FileNames)
            {
                var fileName = path + GetFileName(cat);
                var list = GetFilteredListInCategory<R>(cat, localOnly);

                if (list.Count == 0)
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    continue;
                }

                var jsonFileContent = Activator.CreateInstance<X>();
                jsonFileContent.SetValues(list.ToArray());
                var jsonContent = Utils.ToJson(jsonFileContent);
                FileUtils.SaveToFile(jsonContent, fileName);
            }
            return true;
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

            var cats = new List<string>(FileNames);
            if (Editor.Preferences.PrefSortTree) { cats.Sort(); }

            foreach (var cat in cats)
            {
                // Get items, skip if we have none, if not create category
                var list = GetFilteredListInCategory<JsonDefWithName>(cat, Editor.Preferences.PrefIsLocalOnly, true);
                var catItem = CreateCategoryItem(RootItem, cat, FillContextMenuForFile);
                if (list.Count == 0) { continue; }


                // Create tree items
                var itemList = new List<DelegateEditorTreeItem>();
                foreach (var jDef in list)
                {
                    var aTreeItem = Editor.Tree.CreateDelegateTreeItem(catItem, jDef.GetName(), jDef.GetUniqueId(), true, GetItemColor(jDef, true),
                                    GetItemColor(jDef, false), OnJsonItemSelected, ModPaths, ModName, jDef);
                    aTreeItem.OnContextMenuFill = FillContextMenuForItem;
                    itemList.Add(aTreeItem);
                }
                if (Editor.Preferences.PrefSortTree) { SortTreeItemList(itemList); }

                foreach (var aItem in itemList)
                {
                    var treeItem = CreateJsonEntryItem(catItem, aItem);
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
        protected virtual void OnCategorySelected(DelegateEditorTreeItem cateogry)
        {

        }

        /// <summary>
        /// Can override to change behavior. You can find the JsonDefWithName in the
        /// treeItem.RelatedData
        /// </summary>
        protected virtual void OnJsonItemSelected(DelegateEditorTreeItem treeItem)
        {
            var data = (JsonDefWithName)treeItem.RelatedData;
            ShowEditor(data);
        }

        private void ShowEditor(JsonDefWithName jsonDef)
        {
            var editor = GetEditor(jsonDef);
            var content = (IDataEditorContent)editor;
            content.SetData(jsonDef, this);
            Editor.ShowEditor(editor);
        }

        private void RefreshItem(AbstractEditorTreeItem item, JsonDefWithName def)
        {
            item.TreeItemSelf.SetCustomColor(0, GetItemColor(def, true));
            item.TreeItemSelf.SetCustomBgColor(0, GetItemColor(def, false));
            item.TreeItemSelf.SetText(0, def.GetName());
            item.Name = def.GetName();
        }

        /// <summary>
        /// Can override to provide item colors if you want
        /// </summary>
        protected virtual Color GetItemColor(JsonDefWithName item, bool isForeground)
        {
            if (!isForeground)
            {
                if (item.IsTaggedForDelete) { return Editor.Preferences.GetDeleteColor(); }
                else if (item.IsInvalid) { return Editor.Preferences.GetErrorColor(); }
                else if (item.IsNew) { return Editor.Preferences.GetNewColor(); }
                else if (item.IsModified) { return Editor.Preferences.GetModifiedColor(); }
                return Editor.Preferences.GetDefaultBgColor();
            }
            if (item.IsLocal())
            {
                return Editor.Preferences.GetIsLocalColor();
            }
            return Editor.Preferences.GetDefaultColor();
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

        private TreeItem CreateCategoryItem(TreeItem parent, string name, Func<DelegateEditorTreeItem, bool> contextMenu = null)
        {
            var key = Editor.Tree.GetUniqueKey(parent, name);
            var item = Editor.Tree.CreateDelegateTreeItem(parent, name, key, true,
                    GetCategoryColor(name, true), GetCategoryColor(name, false), OnCategorySelected, ModPaths, ModName);
            item.OnContextMenuFill = contextMenu;
            return Editor.Tree.CreateTreeItem(parent, item);
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

        public bool FillContextMenuForItem(DelegateEditorTreeItem item)
        {
            var data = (JsonDefWithName)item.RelatedData;
            Editor.Tree.AddContextMenuSeparator(item.Name);
            Editor.Tree.AddContextMenuEntry("Duplicate", () =>
            {
                AddAndShowNewItem(Duplicate(data));
            });
            Editor.Tree.AddContextMenuEntry("Copy", () =>
            {
                Editor.Tree.CopiedObject = data;
            });
            if (data.IsLocal())
            {
                Editor.Tree.AddContextMenuEntry("Revert", () => { GD.Print($"Revert"); });
                var deleteString = data.IsTaggedForDelete ? "Remove delete tag" : "Tag for delete";
                Editor.Tree.AddContextMenuEntry(deleteString, () =>
                {
                    data.IsTaggedForDelete = !data.IsTaggedForDelete;
                    RefreshItem(item, data);
                });
            }

            var parentKey = item.TreeItemSelf.GetParent().GetMetadata(0).ToString();
            var parent = (DelegateEditorTreeItem)Editor.Tree.GetTreeItemById(parentKey);
            FillContextMenuForFile(parent);

            return true;
        }

        public bool FillContextMenuForFile(DelegateEditorTreeItem item)
        {
            Editor.Tree.AddContextMenuSeparator(item.Name);
            Editor.Tree.AddContextMenuEntry($"Add new {JsonItemName}", () =>
            {
                AddAndShowNewItem(CreateNew(item.Name));
            });
            if (Editor.Tree.CopiedObject != null && typeof(U) == Editor.Tree.CopiedObject.GetType())
            {
                Editor.Tree.AddContextMenuEntry($"Paste {Editor.Tree.CopiedObject.GetName()}", () =>
                {
                    AddAndShowNewItem(Duplicate(Editor.Tree.CopiedObject, item.Name));
                });
            }
            FillContextMenu();
            return true;
        }

        private void AddAndShowNewItem(JsonDefWithName item)
        {
            item.OnStatusChange += OnStatusChange;
            Values.Add(item);
            Editor.Tree.RefreshTree(false);
            ShowEditor(item);
        }

        /// <summary>
        /// Our own context menu
        /// </summary>
        public override bool FillContextMenu()
        {
            Editor.Tree.AddContextMenuSeparator(Name);
            Editor.Tree.AddContextMenuEntry("Add new file", () =>
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


            });
            return true;
        }

        protected string GetSourceFile(string category)
        {
            return FileUtils.NormalizePath(Editor.Preferences.SettingLocalSavePath + $"{ModName}\\" + GetRelativeDataPath() + GetFileName(category));
        }
    }
}