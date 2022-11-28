using Godot;
using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GodotCSharpToolkit.Misc
{
    public static class Utils
    {
        private static IToolkitSettings ToolkitSettings;

        /// <summary>
        /// Load all json files in a folder
        /// </summary>
        /// <param name="path">Path should be relative</param>
        /// <param name="includeSubFolders">Should we include sub folders</param>
        /// <returns>A list of content</returns>
        public static List<U> LoadJsonFileContent<T, U>(string path, bool includeSubFolders) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            // Load a generic json file
            List<U> resultList = new List<U>();
            List<T> files = Utils.LoadAllJsonFilesInFolderGodot<T>(path, includeSubFolders);
            foreach (T sDefJsonFile in files)
            {
                foreach (U def in sDefJsonFile.GetValues())
                {
                    resultList.Add(def);
                }
            }

            return resultList;
        }

        /// <summary>
        /// Get the toolkit settings
        /// </summary>
        /// <returns></returns>
        public static IToolkitSettings GetToolkitSettings()
        {
            if (ToolkitSettings == null)
            {
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (typeof(IToolkitSettings).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        ToolkitSettings = Activator.CreateInstance(type) as IToolkitSettings;
                        break;
                    }
                }
                if (ToolkitSettings == null)
                {
                    Logging.Logger.Error("Could not find toolkit settings, please implement IToolkitSettings");
                }
            }
            return ToolkitSettings;
        }

        /// <summary>
        /// Uses the godot file reader and directory browser to read all json files of a given type in a folder.
        /// </summary>
        public static List<T> LoadAllJsonFilesInFolderGodot<T>(string path, bool includeSubFolders)
        {
            List<T> retList = new List<T>();
            Directory dir = new Directory();
            dir.Open(path);
            dir.ListDirBegin(true, true);
            while (true)
            {
                String filePath = dir.GetNext();
                if (filePath == "")
                {
                    break;
                }
                if (includeSubFolders && dir.CurrentIsDir())
                {
                    // Go into all subfolder
                    retList.AddRange(LoadAllJsonFilesInFolderGodot<T>(path + filePath + "/", includeSubFolders));
                }
                else if (filePath.ToLower().EndsWith(".json"))
                {
                    // Load all scenes found
                    string fileContent = LoadTextFile(path + filePath);
                    T jsonObj = (T)Utils.FromJson(fileContent, typeof(T));
                    retList.Add(jsonObj);
                }
            }
            dir.ListDirEnd();
            return retList;
        }

        /// <summary>
        /// Uses godot file to load a text file (supports res:// and user:// paths)
        /// </summary>
        public static string LoadTextFile(string path)
        {
            string text = "";
            var f = new Godot.File();
            if (f.FileExists(path))
            {
                f.Open(path, Godot.File.ModeFlags.Read);
                text = f.GetAsText();
                f.Close();
            }
            return text;
        }

        /// <summary>
        /// Only for use with JSON util, should not be used in the real game
        /// </summary>
        public static List<T> LoadAllJsonFilesInFolder<T>(string path)
        {
            List<T> returnList = new List<T>();
            try
            {
                foreach (var file in System.IO.Directory.GetFiles(path, "*.json", System.IO.SearchOption.AllDirectories))
                {
                    string fileContent = System.IO.File.ReadAllText(file);
                    T jsonObj = (T)Utils.FromJson(fileContent, typeof(T));
                    returnList.Add(jsonObj);
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.Write(ex);
            }
            return returnList;
        }

        public static object FromJson(string json, Type type) => JsonConvert.DeserializeObject(json, type, Settings);

        public static string ToJson(object JsonObject) => JsonConvert.SerializeObject(JsonObject, Settings);

        /// <summary>
        /// Settings for JSON serializer
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            Formatting = Formatting.Indented,
            DateParseHandling = DateParseHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
        };
    }
}