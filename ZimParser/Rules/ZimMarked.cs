using Godot;
using System;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimMarked : ZimWikiRuleAbstract
    {
        public ZimMarked()
        {
            Pattern = @"__(?<item>.*?)__";
            Replacement = "[bgcolor=CornflowerBlue]${item}[/bgcolor]";
        }

    }
}