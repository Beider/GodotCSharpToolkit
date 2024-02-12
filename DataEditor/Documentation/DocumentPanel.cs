using Godot;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.ZimParser;
using System;
using System.Collections.Generic;
using System.IO;

public partial class DocumentPanel : Panel
{
    private const string META_IS_DIR = "IsDirectory";
    private const string ROOT_PATH = "res://Documentation/";
    private Tree DocumentsTree;
    private RichTextLabel DocumentView;
    private TreeItem Root;
    private Dictionary<string, TreeItem> FolderItems = new Dictionary<string, TreeItem>();
    private Dictionary<string, TreeItem> LeafItems = new Dictionary<string, TreeItem>();
    private bool Refreshed = false;
    private Color ColorDirectory = Colors.Ivory;
    private Color ColorFile = Colors.Green;

    private ZimParser ZimParser;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DocumentsTree = FindChild("DocumentsTree") as Tree;
        DocumentView = FindChild("DocumentView") as RichTextLabel;
        DocumentView.BbcodeEnabled = true;
        DocumentView.Connect("meta_clicked", Callable.From((Variant meta) => OnLinkClicked(meta)));

        DocumentsTree.HideRoot = true;
        DocumentsTree.Columns = 1;
        DocumentsTree.ColumnTitlesVisible = false;
        DocumentsTree.SetColumnTitle(0, "Document");

        DocumentsTree.Connect("item_selected", Callable.From(() => OnItemSelected()));
        ZimParser = new ZimParser();
    }

    private void OnLinkClicked(Variant data)
    {
        if (data.VariantType != Variant.Type.String)
        {
            return;
        }
        GotoLink((string)data.Obj);
    }

    private void GotoLink(string link)
    {
        var key = link.Replace(":", "/").Replace(" ", "_");
        if (LeafItems.ContainsKey(key))
        {
            SelectExpandAndScrollTo(LeafItems[key]);
        }
    }

    private void OnItemSelected()
    {
        var selectedItem = DocumentsTree.GetSelected();
        if (selectedItem == null) { return; }
        ShowItem(selectedItem);
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
        item.Select(0);
        DocumentsTree.ScrollToItem(item);
    }

    private void ShowItem(TreeItem selectedItem)
    {
        var fileData = selectedItem.GetMetadata(0);
        if (fileData.VariantType == Variant.Type.String)
        {
            var path = (string)fileData.Obj;
            if (path.IsNullOrEmpty()) { return; }

            string content = ParseZimFile(path);
            DocumentView.Text = content;
        }
    }

    private string ParseZimFile(string path)
    {
        var content = FileUtils.LoadTextFile(path);
        return ZimParser.ParseTextToBBCode(content, ROOT_PATH);
    }

    public void ToggleVisible()
    {
        if (Visible)
        {
            HideHelp();
        }
        else
        {
            ShowHelp();
        }
    }

    private void RefreshDocumentTree(bool force = false)
    {
        if (Refreshed && !force) { return; }
        Refreshed = true;
        DocumentsTree.Clear();
        FolderItems.Clear();
        LeafItems.Clear();
        Root = DocumentsTree.CreateItem(null);
        AddToDocumentTree(ROOT_PATH);

        SortTree(Root);
    }

    private void AddToDocumentTree(string documentationRoot)
    {
        var files = FileUtils.GetAllFilesInFolder(documentationRoot, true, ".txt");

        foreach (var file in files)
        {
            var shortPath = file.Substring(documentationRoot.Length);
            var parts = shortPath.Split('/', '\\');
            if (parts.Length == 0) { continue; }
            var lastDir = Root;
            var key = "";
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1)
                {
                    // Filename
                    AddFile(parts[i], file, key, lastDir);
                }
                else
                {
                    key += $"{parts[i]}/";
                    // Directory
                    lastDir = AddDirectory(parts[i], key, lastDir);
                }
            }
        }
    }

    private void AddFile(string name, string fullPath, string key, TreeItem parent)
    {
        var item = DocumentsTree.CreateItem(parent);
        var nameNoExtension = name.Replace(".txt", "", StringComparison.InvariantCultureIgnoreCase);
        var fixedName = nameNoExtension.Replace("_", " ", StringComparison.InvariantCultureIgnoreCase);
        item.SetText(0, fixedName);
        item.SetMetadata(0, fullPath);
        item.SetCustomColor(0, ColorFile);

        LeafItems[$"{key}{nameNoExtension}"] = item;
    }

    private TreeItem AddDirectory(string name, string key, TreeItem parent)
    {
        if (!FolderItems.ContainsKey(key))
        {
            var item = DocumentsTree.CreateItem(parent);
            item.SetText(0, name);
            item.SetMeta(META_IS_DIR, true);
            item.SetCustomColor(0, ColorDirectory);
            item.Collapsed = true;
            FolderItems[key] = item;
        }
        return FolderItems[key];
    }

    private void SortTree(TreeItem item)
    {
        SortTreeItemChildren(item);
        foreach (var child in item.GetChildren())
        {
            SortTree(child);
        }
    }

    private void SortTreeItemChildren(TreeItem parent)
    {
        var n = parent.GetChildCount();
        if (n == 0) { return; }
        for (int i = 0; i < n - 1; i++)
        {
            for (int inner = 0; inner < n - i - 1; inner++)
            {
                var child = parent.GetChild(inner);
                var child2 = parent.GetChild(inner + 1);
                if (child == null || child2 == null) { continue; }
                var childIsDir = (bool)child.GetMeta(META_IS_DIR, false).Obj;
                var child2IsDir = (bool)child2.GetMeta(META_IS_DIR, false).Obj;
                if (!childIsDir && child2IsDir)
                {
                    child2.MoveBefore(child);
                }
                else if (childIsDir && !child2IsDir)
                {
                    continue;
                }
                if (child.GetText(0).CompareTo(child2.GetText(0)) > 0)
                {
                    child2.MoveBefore(child);
                }
            }
        }
    }

    public void HideHelp()
    {
        Visible = false;
    }

    public void ShowHelp()
    {
        Visible = true;
        RefreshDocumentTree();
    }
}
