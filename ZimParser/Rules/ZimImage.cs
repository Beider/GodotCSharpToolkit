using Godot;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using System;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimImage : ZimWikiRuleAbstract
    {
        public ZimImage()
        {
            Pattern = @"\{\{(?<item>.*?)\}\}";
            Options = RegexOptions.Singleline;
        }

        public override string Apply(string wikiText)
        {
            return Regex.Replace(wikiText, Pattern, (match) => Match(match), Options);
        }

        private string Match(Match match)
        {
            string text = match.Groups["item"].Value;

            // Remove the leading ..\
            text = text.Substring(3);
            text = $"{Parser.FilePath}/{text}";
            text = FileUtils.NormalizePath(text).Replace("res://", "").Replace("res:/", "");
            return $"[img]{text}[/img]";
        }

    }
}