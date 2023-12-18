using Godot;
using System;
using System.Reflection;
using GodotCSharpToolkit.EventSystem;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.DataManager;


public partial class ToolkitLoader : Node
{

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddAutloadFromType(typeof(Logger));
        AddAutloadFromType(typeof(GameTicker));
        AddAutloadFromType(typeof(JsonDataManager));
        AddAutloadFromType(typeof(EventManager));
    }

    private void AddAutloadFromType(Type autoloadType)
    {
        Node node = Activator.CreateInstance(autoloadType) as Node;
        node.Name = autoloadType.Name;
        AddChild(node);
    }
}