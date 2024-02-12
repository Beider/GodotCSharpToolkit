using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimBold : ZimWikiRuleAbstract
    {
        public ZimBold()
        {
            Pattern = @"\*\*(?<item>.*?)\*\*";
            Replacement = "[b]${item}[/b]";
        }

    }
}