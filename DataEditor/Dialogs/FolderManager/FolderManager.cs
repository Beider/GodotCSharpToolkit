using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.Editor
{
    public partial class FolderManager : ColorRect
    {
        public static bool IsLoading { get; private set; } = false;
        private const string PREF_SIZE = "folder_manager_size_";
        private const string PREF_POS = "folder_manager_pos_";

        private Window Dialog;

        private FolderManagerTree Tree;
        private Button BtnOk;
        private Button BtnCancel;
        private TreeItem Root;
        private IDataEditor Editor;
        private FolderManagerTreeItem RootFolderItem;
        public Dictionary<string, FolderManagerTreeItem> ItemLookup = new Dictionary<string, FolderManagerTreeItem>();

        AdvancedDialogWindowInput WindowDialog;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Dialog = FindChild("Dialog") as Window;
            Dialog.Connect("close_requested", new Callable(this, nameof(OnCancelPressed)));

            // Tree
            Tree = FindChild("Tree") as FolderManagerTree;
            Tree.Connect("item_activated", new Callable(this, nameof(OnOkPressed)));
            Tree.FolderManager = this;

            // Buttons
            BtnOk = FindChild("BtnOk") as Button;
            BtnCancel = FindChild("BtnCancel") as Button;
            BtnOk.Connect("pressed", new Callable(this, nameof(OnOkPressed)));
            BtnCancel.Connect("pressed", new Callable(this, nameof(OnCancelPressed)));

            WindowDialog = FindChild("Dialog") as AdvancedDialogWindowInput;
            WindowDialog.OnCancelPressed += OnCancelPressed;
            WindowDialog.OnOkPressed += OnOkPressed;

            Dialog.Popup();
            LoadPrefs();
            Load();
        }

        private void Load()
        {
            if (Editor.Preferences.SettingLocalSavePath.IsNullOrEmpty())
            {
                // Can't load
                return;
            }
            IsLoading = true;
            Tree.Columns = 2;
            Tree.ColumnTitlesVisible = true;
            Tree.SetColumnTitle(0, "Name");
            Tree.SetColumnTitle(1, "Modified");
            RootFolderItem = new FolderManagerTreeItem();
            foreach (var mod in Editor.Tree.ModFolders.Keys)
            {
                // Module
                string modPath = GetLocalModPath(Editor.Tree.ModFolders[mod]);
                var modFolderItem = FolderManagerTreeItem.CreateModuleItem(mod, modPath, Editor.Preferences.GetModColor());
                RootFolderItem.AddChild(modFolderItem);

                // Features
                var features = Editor.Tree.GetFeatures(mod);

                foreach (var feature in features.Keys)
                {
                    // Add the feature item
                    string path = GetLocalModPath(features[feature]);
                    var featureItem = FolderManagerTreeItem.CreateTypeItem(feature, path, "Feature", feature, null, Colors.White);
                    featureItem.Collapsed = true;
                    modFolderItem.AddChild(featureItem);



                    // Feature children
                    foreach (var item in Editor.Tree.RootItems.Values)
                    {
                        //if (!item.ModName.Equals(mod)) { continue; }
                        if (!item.ModPaths.Contains(path)) { continue; }
                        var newItem = item.GetFolderManagerRootItem(mod, path);
                        featureItem.AddChild(newItem);
                    }
                }
            }
            IsLoading = false;
            Refresh();
        }

        private string GetLocalModPath(List<string> paths)
        {
            foreach (var path in paths)
            {
                if (path.StartsWith(Editor.Preferences.SettingLocalSavePath))
                {
                    return path;
                }
            }
            return "";
        }

        public void Refresh()
        {
            ItemLookup.Clear();
            Tree.Clear();
            RootFolderItem.UpdateModifiedState();
            Root = AddItem(null, RootFolderItem);
        }

        private TreeItem AddItem(TreeItem parent, FolderManagerTreeItem item)
        {
            ItemLookup.Add(item.Key, item);
            var newItem = Tree.CreateItem(parent);
            newItem.SetText(0, item.Name);
            newItem.SetMetadata(0, item.Key);
            newItem.SetCustomColor(0, item.Color);
            if (item.OriginalParent != item.Parent)
            {
                newItem.SetText(1, $"Moved From: {item.OriginalParent.GetFullPath()}");
                newItem.SetCustomBgColor(1, Colors.Yellow.Darkened(0.8f), false);
            }
            else if (item.IsModified)
            {
                newItem.SetText(1, "Children Modified");
                newItem.SetCustomBgColor(1, Colors.Orange.Darkened(0.8f), false);
            }
            newItem.Collapsed = item.Collapsed;
            foreach (var child in item.Children)
            {
                AddItem(newItem, child);
            }
            return newItem;
        }

        public void Init(IDataEditor editor)
        {
            Editor = editor;
        }

        private void OnCancelPressed()
        {
            SavePrefs();
            QueueFree();
        }

        private void OnOkPressed()
        {
            if (BtnOk.Disabled) { return; }
            Save();
            SavePrefs();
            QueueFree();
            Editor.Refresh();
        }

        private void SavePrefs()
        {
            if (Editor != null)
            {
                Editor.Preferences.SetValue(PREF_SIZE, GD.VarToStr(Dialog.Size));
                Editor.Preferences.SetValue(PREF_POS, GD.VarToStr(Dialog.Position));
            }
        }

        private void LoadPrefs()
        {
            if (Editor != null)
            {
                var size = Editor.Preferences.GetValue(PREF_SIZE, GD.VarToStr(Dialog.Size));
                var pos = Editor.Preferences.GetValue(PREF_POS, GD.VarToStr(Dialog.Position));

                Dialog.Size = ((Vector2)GD.StrToVar(size)).ToVector2I();
                Dialog.Position = ((Vector2)GD.StrToVar(pos)).ToVector2I();

                if (Dialog.Position.Y <= 10)
                {
                    Dialog.Position = new Vector2I(Dialog.Position.X, 10);
                }
            }
        }

        private void Save()
        {
            foreach (var module in RootFolderItem.Children)
            {
                if (!module.IsModified) { continue; }
                foreach (var feature in module.Children)
                {
                    if (!feature.IsModified) { continue; }
                    foreach (var dataType in feature.Children)
                    {
                        if (!dataType.IsModified) { continue; }
                        SaveDataType(dataType, feature.Path3D, dataType.SubPath);
                    }
                }
            }
        }

        private void SaveDataType(FolderManagerTreeItem dataType, string featurePath, string subPath)
        {
            // Get list of current files in the system
            var originalFiles = FileUtils.GetAllFilesInFolder(dataType.Path3D, false, FileUtils.JSON_EXTENSION);
            foreach (var fileObject in dataType.Children)
            {
                originalFiles.Remove(fileObject.Path3D);
                if (!fileObject.IsModified) { continue; }

                // Extract file name
                var name = System.IO.Path.GetFileName(fileObject.Path3D);
                var newPath = FileUtils.NormalizePath($"{featurePath}{subPath}{name}");

                if (fileObject.Children.Count == 0)
                {
                    // Delete the file
                    if (System.IO.File.Exists(newPath))
                    {
                        System.IO.File.Delete(newPath);
                    }
                }
                else
                {
                    // Save the file
                    SaveFile(newPath, fileObject);
                }
            }

            foreach (var file in originalFiles)
            {
                // Delete moved files
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }
        }

        private void SaveFile(string filePath, FolderManagerTreeItem item)
        {
            var defList = new List<JsonDefWithName>();
            item.Children.ForEach(i => defList.Add(i.Data));
            item.SaveAction(filePath, defList);
        }
    }
}
