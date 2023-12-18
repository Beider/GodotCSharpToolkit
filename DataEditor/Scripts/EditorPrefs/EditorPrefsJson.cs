using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GodotCSharpToolkit.Editor
{
    public partial class EditorPrefsJson
    {
        [JsonProperty("Values")]
        public List<EditorPrefEntry> Values = new List<EditorPrefEntry>();
    }

    public partial class EditorPrefEntry
    {
        [JsonProperty("Name")]
        public string Name = "";

        [JsonProperty("Value")]
        public string Value = "";
    }
}