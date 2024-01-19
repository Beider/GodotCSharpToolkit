using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Collections.Generic;

public partial class EditorPopupMenu : PopupMenu
{
    public Dictionary<string, Action> PopupMenuDelegates = new Dictionary<string, Action>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Connect("id_pressed", new Callable(this, nameof(OnPopupMenuPressed)));
        Connect("popup_hide", Callable.From(() => QueueFree()));
    }



    public void ClearPopupMenu()
    {
        Clear();
        this.ClearChildren();
        PopupMenuDelegates.Clear();
    }

    public void PositionInParent(Vector2 position)
    {
        var parentWindow = GetParentWindow(GetParent());
        if (parentWindow != null && parentWindow.Name != "root")
        {
            Position = (parentWindow.Position + position).ToVector2I();
        }
        else
        {
            Position = position.ToVector2I();
        }
    }

    private Window GetParentWindow(Node node)
    {
        if (node is Window pm) { return pm; }
        if (node.GetParent() != null) { return GetParentWindow(node.GetParent()); }
        return null;
    }

    public void AddPopupMenuSeparator(string name)
    {
        AddSeparator($" {name} ");
    }

    public void AddPopupMenuEntry(string name, Action action, Texture2D icon = null, string subMenuName = "")
    {
        PopupMenu menu = this;
        if (!subMenuName.IsNullOrEmpty())
        {
            foreach (Node child in GetChildren())
            {
                if (child.Name.Equals(subMenuName))
                {
                    menu = (PopupMenu)child;
                    break;
                }
            }
        }
        if (icon != null)
        {
            menu.AddIconItem(icon, name);
        }
        else
        {
            menu.AddItem(name);
        }
        PopupMenuDelegates.Add(name, action);
    }

    public void CreatePopupSubMenu(string name)
    {
        PopupMenu subMenu = new PopupMenu();
        subMenu.Name = name;
        AddChild(subMenu);
        AddSubmenuItem(name, name);
    }

    private void OnPopupMenuPressed(int index)
    {
        string text = GetItemText(index);
        if (PopupMenuDelegates.ContainsKey(text))
        {
            // Invoke the menu item
            PopupMenuDelegates[text]();
        }
    }
}
