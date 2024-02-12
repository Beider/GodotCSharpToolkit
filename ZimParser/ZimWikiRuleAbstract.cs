using Godot;
using GodotCSharpToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GodotCSharpToolkit.ZimParser
{
    public abstract class ZimWikiRuleAbstract
    {
        protected Dictionary<string, string> ExtractedData = new Dictionary<string, string>();
        protected string Pattern = "";
        protected string Replacement = "";
        protected RegexOptions Options = RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace;
        public ZimParser Parser;
        public virtual string Apply(string wikiText)
        {
            if (Pattern.IsNullOrEmpty()) { return wikiText; }

            return Regex.Replace(wikiText, Pattern, Replacement, Options);
        }

        public virtual bool HasExtractedData()
        {
            return ExtractedData.Count > 0;
        }

        public virtual Dictionary<string, string> GetExtractedData()
        {
            return ExtractedData;
        }
    }
}