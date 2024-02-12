using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimSuperscript : ZimWikiRuleAbstract
    {
        public ZimSuperscript()
        {
            // We can't align the text to top with BBcode so whatever
            Pattern = @"\^\{(?<item>.*?)\}";
            Replacement = "[font_size=7]${item}[/font_size]";
        }

    }
}