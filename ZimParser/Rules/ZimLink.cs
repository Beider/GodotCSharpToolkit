using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimLink : ZimWikiRuleAbstract
    {
        public ZimLink()
        {
            Pattern = @"\[\[(?<item>.*?)\]\]";
            Options = RegexOptions.Singleline;
        }

        public override string Apply(string wikiText)
        {
            return Regex.Replace(wikiText, Pattern, (match) => Match(match), Options);
        }

        private string Match(Match match)
        {
            string text = match.Groups["item"].Value;
            var splitText = text.Split("|");
            if (splitText.Length == 1)
            {
                return $"[url={splitText[0]}]{splitText[0]}[/url]";
            }
            return $"[url={splitText[0]}]{splitText[1]}[/url]";
        }

    }
}