using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimStrikethrough : ZimWikiRuleAbstract
    {
        public ZimStrikethrough()
        {
            Pattern = @"~~(?<item>.*?)~~";
            Replacement = "[s]${item}[/s]";
        }

    }
}