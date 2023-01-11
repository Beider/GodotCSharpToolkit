using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.DataManager
{

    public class JsonDataManager : Node
    {
        public static JsonDataManager Instance;

        public event Action OnReloadJson = delegate { };

        /// <summary>
        /// Outer key = Key for the data type
        /// Inner key = name from the json def
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, IJsonDefWithName>> JsonData = new Dictionary<string, Dictionary<string, IJsonDefWithName>>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Instance = this;
        }

        public static void LoadJsonFile<T, U>(string key, string relativePath, bool includeSubFolders) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            var dict = Instance.FillDictionary<T, U>(relativePath, includeSubFolders);
            Instance.JsonData.Add(key, dict);
        }

        /// <summary>
        /// Get value from a static dictionary by name.
        /// Will log error in event of a problem.
        /// </summary>
        /// <param name="key">The storage to look in</param>
        /// <param name="name">The name to look for</param>
        /// <typeparam name="T">The type of item to get</typeparam>
        /// <returns>The item or default (probably null)</returns>
        public static T GetByName<T>(string key, string name) where T : IJsonDefWithName
        {
            if (!Instance.JsonData.ContainsKey(key))
            {
                Logger.Error($"Could not find JsonData of type '{typeof(T).Name}' with key '{key}'");
                return default(T);
            }
            var dict = Instance.JsonData[key];
            if (!dict.ContainsKey(name))
            {
                Logger.Error($"Could not find JsonData of type '{typeof(T).Name}' with name '{name}'");
                return default(T);
            }

            return (T)dict[name];
        }


        /// <summary>
        /// Fill the given dictionary
        /// </summary>
        private Dictionary<string, IJsonDefWithName> FillDictionary<T, U>(string relativePath, bool includeSubFolders) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            var dict = new Dictionary<string, IJsonDefWithName>();
            var list = Utils.LoadJsonFileContent<T, U>(relativePath, includeSubFolders);
            list.ForEach(value => dict.Add(value.GetName(), value));
            return dict;
        }
    }

}