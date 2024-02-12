using Godot;
using System;
using System.Text.RegularExpressions;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using System.Collections.Generic;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimList : ZimWikiRuleAbstract
    {
        private List<string> OpenTags = new List<string>();
        private int CurrentIndent = -1;

        public ZimList()
        {
            Options = RegexOptions.Multiline;
            Pattern = @"^(?<tabs>\t*?)(?:(?<prefix>\d+\.|\*|\[\s\]|\[\*\]?)\s*(?<item>.*?)(?:\r?\n|$))";
        }

        public override string Apply(string wikiText)
        {
            OpenTags = new List<string>();
            CurrentIndent = -1;
            return Regex.Replace(wikiText, Pattern, (match) => Match(match), Options);
        }

        private string Match(Match match)
        {
            int tabsCount = match.Groups["tabs"].Length;
            string item = match.Groups["item"].Value.Trim();
            bool isOrdered = match.Groups["prefix"].Value.Contains(".");

            var prefix = isOrdered ? "[ol]" : "[ul]";
            var postfix = isOrdered ? "[/ol]" : "[/ul]";

            // Add the item to the parsed text
            string parsedItem = CurrentIndent == tabsCount ? item : $"{prefix}{item}";

            CurrentIndent = tabsCount;

            var nextMatch = match.NextMatch();
            int tabsCount2 = nextMatch.Groups["tabs"].Length;

            int end = match.Index + match.Length;

            if (nextMatch.Index > end || !nextMatch.Success)
            {
                // End of current section close all open list tags
                parsedItem += postfix;
                foreach (var ending in OpenTags)
                {
                    parsedItem += ending;
                }
                OpenTags.Clear();
                CurrentIndent = -1;
            }
            else
            {
                if (tabsCount == tabsCount2)
                {
                    parsedItem += "\n";
                }
                else if (tabsCount < tabsCount2)
                {
                    OpenTags.Add(postfix);
                }
                else
                {
                    parsedItem += postfix;
                    int diff = tabsCount - tabsCount2;
                    for (int i = 0; i < diff; i++)
                    {
                        parsedItem += OpenTags[OpenTags.Count - 1];
                        OpenTags.RemoveAt(OpenTags.Count - 1);
                    }
                }
            }

            return parsedItem;
        }

    }
}