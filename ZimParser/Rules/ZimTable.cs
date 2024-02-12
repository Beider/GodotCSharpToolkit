using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimTable : ZimWikiRuleAbstract
    {
        private const string HEADER_SEPARATOR = ":-------";
        private bool TableOpen = false;
        public ZimTable()
        {
            //Pattern = @"^\|\s*(?<item>.+?)\s*\|$";
            Pattern = @"^\|\s*(?<item>.+)$";
            Options = RegexOptions.Multiline;
        }

        public override string Apply(string wikiText)
        {
            TableOpen = false;
            return Regex.Replace(wikiText, Pattern, (match) => Match(match), Options);
        }

        private string Match(Match match)
        {
            string item = match.Groups["item"].Value.Trim();
            if (item.EndsWith("|"))
            {
                if (item.Contains(HEADER_SEPARATOR))
                {
                    return "";
                }
                var nextMatch = match.NextMatch();
                var items = item.Split("|", false);
                var returnString = TableOpen ? "" : $"[table={items.Length}]";
                TableOpen = true;

                bool isHeader = nextMatch.Groups["item"].Value.Contains(HEADER_SEPARATOR);
                foreach (var cell in items)
                {
                    if (isHeader)
                    {
                        returnString += $"[cell][center][b][font_size=18]{cell.Trim()}[/font_size][/b][/center][/cell]";
                    }
                    else
                    {
                        returnString += $"[cell][center]{cell.Trim()}[/center][/cell]";
                    }
                }
                int end = match.Index + match.Length + 2;
                if (nextMatch.Index > end || !nextMatch.Success)
                {
                    returnString += "[/table]";
                    TableOpen = false;
                }

                return returnString;
            }
            else
            {
                return item;
            }
        }

    }
}