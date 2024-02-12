using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimVerbatim : ZimWikiRuleAbstract
    {
        public ZimVerbatim()
        {
            Pattern = @"''(?<item>.*?)''";
            Replacement = "[code]${item}[/code]";
        }

    }
}