using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

/// <summary>
/// Some tween methods
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class TweenUtils
    {
        public static float Flip(float x)
        {
            return 1 - x;
        }

        public static float EaseOut(float t)
        {
            return Flip(Mathf.Sqrt(Flip(t)));
        }
    }
}