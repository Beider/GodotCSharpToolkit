using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GodotCSharpToolkit.Misc
{

    public interface IJsonFile<T> where T : IJsonDefWithName
    {
        T[] GetValues();
    }

    public interface IJsonDefWithName
    {
        string GetName();
    }

    public class Vector2Json
    {
        public Vector2Json() { }

        [JsonProperty("X")]
        public float X = 1f;

        [JsonProperty("Y")]
        public float Y = 1f;

        public Vector2 AsVector2()
        {
            return new Vector2(X, Y);
        }
    }
}