using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.DataManager
{

    public partial class JsonDataManager : Node
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

        public static void LoadJsonFile<T, U>(string key, string path, bool includeSubFolders) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            var dict = new Dictionary<string, U>();
            FillDictionary<T, U>(dict, path, includeSubFolders);
            var storeDict = new Dictionary<string, IJsonDefWithName>();
            foreach (var val in dict.Keys)
            {
                storeDict.Add(val, dict[val]);
            }
            Instance.JsonData.Add(key, storeDict);
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
        /// Creates a dictionary by combining all json files found in the underlying paths.
        /// </summary>
        public static Dictionary<string, U> GetDictionary<T, U>(List<string> paths, bool includeSubFolders, bool replaceDuplicates = true) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            var dict = new Dictionary<string, U>();
            foreach (string path in paths)
            {
                FillDictionary<T, U>(dict, path, includeSubFolders, replaceDuplicates);
            }
            return dict;
        }

        /// <summary>
        /// Fill the given dictionary.
        /// If replace duplicates is set then subsequent folders will override the first one
        /// </summary>
        public static void FillDictionary<T, U>(Dictionary<string, U> dict, string path, bool includeSubFolders, bool replaceDuplicates = true) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            var list = Utils.LoadJsonFileContent<T, U>(path, includeSubFolders);
            foreach (string key in list.Keys)
            {
                foreach (var value in list[key])
                {
                    if (dict.ContainsKey(value.GetUniqueId()))
                    {
                        if (replaceDuplicates)
                        {
                            dict[value.GetUniqueId()] = value;
                        }
                    }
                    else
                    {
                        dict.Add(value.GetUniqueId(), value);
                    }
                }
            }
        }
    }

}