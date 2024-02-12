using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimHeading : ZimWikiRuleAbstract
    {
        public ZimHeading()
        {
            Pattern = @"(?<equals>=+)\s*(?<heading>.*?)\s*\k<equals>";
        }

        public override string Apply(string wikiText)
        {
            return Regex.Replace(wikiText, Pattern, (match) => Match(match),
                    RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        }

        private string Match(Match match)
        {
            int level = match.Groups["equals"].Length;
            string headingText = match.Groups["heading"].Value.Trim();
            string bbCodeStart = $"[font_size={20 + (level * 2)}]";
            string bbCodeEnd = $"[/font_size]";
            return $"{bbCodeStart}{headingText}{bbCodeEnd}";
        }

    }
}
