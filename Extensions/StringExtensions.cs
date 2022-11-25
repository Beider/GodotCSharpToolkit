using Godot;
using System;

namespace GodotCSharpToolkit.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            if (value == null || value.Trim() == "")
            {
                return true;
            }

            return false;
        }
    }
}