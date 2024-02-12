using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimMetaRemover : ZimWikiRuleAbstract
    {
        public ZimMetaRemover()
        {
            Pattern = @"(?<=^|\n)(Content-Type:|Wiki-Format:|Creation-Date:)\s*(?<value>.*?)(\r?\n|$)";
            Replacement = "${value}";
            Options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
        }

        public override string Apply(string wikiText)
        {
            ExtractedData.Clear();
            var text = Regex.Replace(wikiText, Pattern, (match) => Match(match),
                    RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            return text.Trim();
        }

        private string Match(Match match)
        {
            string header = match.Groups[1].Value.Replace(":", "");
            string value = match.Groups["value"].Value.Trim();
            ExtractedData[header] = value;
            return $"";
        }
    }
}