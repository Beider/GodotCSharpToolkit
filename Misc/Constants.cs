using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Misc
{
    //
    // Enums
    //


    /// <summary>
    /// Directional enum for 8-way movement. Has related extensions to convert into Vector2 or Angles.
    /// </summary>
    public enum Direction
    {
        None,
        Up,
        UpRight,
        Right, // Forward
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }
}