using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using GodotCSharpToolkit.DataManager;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.EventSystem
{
    public partial class EventIdJsonFile : IJsonFile<EventIdJsonDef>
    {
        [JsonProperty("Values")]
        public List<EventIdJsonDef> Values { get; set; }

        public EventIdJsonDef[] GetValues()
        {
            return Values.ToArray();
        }
    }

    public class EventIdJsonDef : JsonDefWithName
    {
        public EventIdJsonDef() { }

        [JsonProperty("Id")]
        public byte Id = 0;

        [JsonProperty("ClassName")]
        public string ClassName = "";

        public override string GetName()
        {
            return ClassName;
        }
    }

}