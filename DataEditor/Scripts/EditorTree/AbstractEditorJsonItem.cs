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
        public List<String> Categories = new List<string>();

        protected bool AllowReplaceOfLocalData = true;
        protected bool SortList = true;

        /// <summary>
        /// Get the data path relative to res://
        /// We will use the same relative paths in the local directory
        /// </summary>
        protected abstract string GetRelativeDataPath();

        /// <summary>
        /// Get the editor for the given json def
        /// </summary>
        protected abstract Control GetEditor(JsonDefWithName jDef);

        public override void Reload()
        {
            ChangedObjects.Clear();
            Values.Clear();
            Categories.Clear();
            var paths = GetDataPaths();
            var data = JsonDataManager.FillDictionary<T, U>(paths, true, AllowReplaceOfLocalData);
            foreach (var key in data.Keys)
            {
                var jDef = (JsonDefWithName)data[key];
                jDef.OnStatusChange += OnStatusChange;
                if (!Categories.Contains(jDef.GetCategory()))
                {
                    Categories.Add(jDef.GetCategory());
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

            var item = Editor.Tree.GetTreeItemById(def.TreeIdentifier);
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

            System.IO.Directory.CreateDirectory(path);

            foreach (var cat in Categories)
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
            var rootItem = base.CreateRootItem();

            var cats = new List<string>(Categories);
            if (Editor.Preferences.PrefSortTree) { cats.Sort(); }

            foreach (var cat in cats)
            {
                // Get items, skip if we have none, if not create category
                var list = GetFilteredListInCategory<JsonDefWithName>(cat, Editor.Preferences.PrefIsLocalOnly, true);
                if (list.Count == 0) { continue; }
                var catItem = CreateCategoryItem(rootItem, cat);

                // Create tree items
                var itemList = new List<DelegateEditorTreeItem>();
                foreach (var jDef in list)
                {
                    var aTreeItem = Editor.Tree.CreateDelegateTreeItem(catItem, jDef.GetName(), jDef.GetKey(), true, GetItemColor(jDef, true),
                                    GetItemColor(jDef, false), OnJsonItemSelected, ModPaths, jDef);
                    itemList.Add(aTreeItem);
                }
                if (Editor.Preferences.PrefSortTree) { SortTreeItemList(itemList); }

                foreach (var aItem in itemList)
                {
                    var treeItem = CreateJsonEntryItem(catItem, aItem);
                    ((JsonDefWithName)aItem.RelatedData).TreeIdentifier = (string)treeItem.GetMetadata(0);
                }

            }

            return rootItem;
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
            var editor = GetEditor(data);
            var content = (IDataEditorContent)editor;
            content.SetData(data, this);
            Editor.ShowEditor(editor);
        }

        private void RefreshItem(AbstractEditorTreeItem item, JsonDefWithName def)
        {
            item.TreeItemSelf.SetCustomColor(0, GetItemColor(def, true));
            item.TreeItemSelf.SetCustomBgColor(0, GetItemColor(def, false));
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

        private TreeItem CreateCategoryItem(TreeItem parent, string name)
        {
            var item = Editor.Tree.CreateDelegateTreeItem(parent, name, name, true,
                    GetCategoryColor(name, true), GetCategoryColor(name, false), OnCategorySelected, ModPaths);
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
    }
}