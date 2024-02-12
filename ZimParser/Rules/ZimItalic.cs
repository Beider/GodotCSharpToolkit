using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimItalic : ZimWikiRuleAbstract
    {
        public ZimItalic()
        {
            Pattern = @"\/\/(?<item>.*?)\/\/";
            Replacement = "[i]${item}[/i]";
        }

    }
}