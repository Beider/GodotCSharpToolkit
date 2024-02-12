using Godot;
using GodotCSharpToolkit.DebugMenu;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GodotCSharpToolkit.ZimParser
{
    public partial class ZimParser
    {
        private static List<Type> RuleTypes = new List<Type>();
        private List<ZimWikiRuleAbstract> Rules = new List<ZimWikiRuleAbstract>();
        public Dictionary<string, string> ExtractedData = new Dictionary<string, string>();

        public string RootPath = "";
        public ZimParser()
        {

        }

        /// <summary>
        /// Resolve all rule classes with reflection
        /// </summary>
        private void ResolveRuleTypes()
        {
            if (RuleTypes.Count > 0) { return; }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (DebugReflectionUtil.IsInGodotOrSystemNamespace(type)) { continue; }

                if (typeof(ZimWikiRuleAbstract).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    RuleTypes.Add(type);
                }
            }
        }

        private void InitRules()
        {
            ResolveRuleTypes();
            Rules.Clear();
            foreach (var type in RuleTypes)
            {
                var inst = Activator.CreateInstance(type) as ZimWikiRuleAbstract;
                inst.Parser = this;
                Rules.Add(inst);
            }
        }

        public string ParseTextToBBCode(string wikiText, string rootPath)
        {
            RootPath = rootPath;
            InitRules();
            ExtractedData.Clear();
            string parsedText = wikiText;
            foreach (var rule in Rules)
            {
                parsedText = rule.Apply(parsedText);
                if (rule.HasExtractedData())
                {
                    var data = rule.GetExtractedData();
                    foreach (var key in data.Keys)
                    {
                        ExtractedData[key] = data[key];
                    }
                }
            }
            return parsedText;
        }
    }
}