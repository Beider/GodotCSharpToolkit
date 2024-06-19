using Godot;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using System;
using System.IO;
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
            text = FileUtils.NormalizePath(text, true);
            if (!FileUtils.FileExists(text))
            {
                // Backup is to just try the images folder
                var name = Path.GetFileName(text);
                text = $"{Parser.RootPath}Images/{name}";
                text = FileUtils.NormalizePath(text);
            }
            text = text.Replace("res://", "").Replace("res:/", "");
            return $"[img]{text}[/img]";
        }

    }
}