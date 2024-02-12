using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimSubscript : ZimWikiRuleAbstract
    {
        public ZimSubscript()
        {
            Pattern = @"_\{(?<item>.*?)\}";
            Replacement = "[font_size=7]${item}[/font_size]";
        }

    }
}