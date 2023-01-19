using Godot;
using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Misc
{
    public static class Utils
    {
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
            List<T> files;
            if (path.ToLower().StartsWith("res://") || path.ToLower().StartsWith("user://"))
            {
                Logger.Info($"Godot path: {path}");
                files = Utils.LoadAllJsonFilesInFolderGodot<T>(path, includeSubFolders);
            }
            else
            {
                Logger.Info($"Absolute path: {path}");
                files = Utils.LoadAllJsonFilesInFolder<T>(path, includeSubFolders);
            }
            if (files != null)
            {
                foreach (T sDefJsonFile in files)
                {
                    foreach (U def in sDefJsonFile.GetValues())
                    {
                        resultList.Add(def);
                    }
                }
            }
            return resultList;
        }

        /// <summary>
        /// Get the absolute project path
        /// </summary>
        public static string GetAbsoluteProjectPath()
        {
            return ValidatePathSetting(ProjectSettings.GetSetting(Constants.SETTING_PATH_NAME));
        }

        /// <summary>
        /// Get the relative data path
        /// </summary>
        public static string GetRelativeDataPath()
        {
            return ValidatePathSetting(ProjectSettings.GetSetting(Constants.SETTING_DATA_PATH));
        }

        private static string ValidatePathSetting(object setting)
        {
            if (setting == null)
            {
                return null;
            }
            string path = setting.ToString();
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        /// <summary>
        /// Uses the godot file reader and directory browser to read all json files of a given type in a folder.
        /// </summary>
        public static List<T> LoadAllJsonFilesInFolderGodot<T>(string pathToFile, bool includeSubFolders)
        {
            List<T> retList = new List<T>();
            var fileList = Utils.LoadAllFilesInFolderGodot(pathToFile, includeSubFolders, ".json");

            // Load json files
            foreach (var file in fileList)
            {
                string fileContent = LoadTextFile(file);
                T jsonObj = (T)Utils.FromJson(fileContent, typeof(T));
                if (jsonObj == null)
                {
                    Logger.Info($"Failed to deserialize json to {typeof(T).Name}");
                }
                else
                {
                    retList.Add(jsonObj);
                }
            }

            return retList;
        }

        /// <summary>
        /// Uses the godot file reader and directory browser to read all files of the given type in a folder
        /// </summary>
        public static List<string> LoadAllFilesInFolderGodot(string pathToFile, bool includeSubFolders, string extension)
        {
            List<string> retList = new List<string>();
            Directory dir = new Directory();

            // Release builds do not like backslashes
            String path = pathToFile.Replace("\\", "/");
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
                    retList.AddRange(LoadAllFilesInFolderGodot(path + filePath + "/", includeSubFolders, extension));
                }
                else if (filePath.ToLower().EndsWith(extension))
                {
                    // Grab name of all files found
                    retList.Add(path + filePath);
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
        /// Only use for loading files from an absolute path
        /// </summary>
        public static List<T> LoadAllJsonFilesInFolder<T>(string path, bool includeSubFolders)
        {
            List<T> returnList = new List<T>();
            var fileList = LoadAllFilesInFolder(path, includeSubFolders, "*.json");
            try
            {
                foreach (var file in fileList)
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

        public static List<string> LoadAllFilesInFolder(string path, bool includeSubFolders, string extension)
        {
            List<string> returnList = new List<string>();
            try
            {
                System.IO.SearchOption option = includeSubFolders ? System.IO.SearchOption.AllDirectories :
                                                                    System.IO.SearchOption.TopDirectoryOnly;

                foreach (var file in System.IO.Directory.GetFiles(path, $"*{extension}", option))
                {
                    returnList.Add(file);
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.Write(ex);
                throw ex;
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

        public static TEnum GetEnumValue<TEnum>(string value, TEnum defValue) where TEnum : struct
        {
            TEnum enumValue;
            if (Enum.TryParse<TEnum>(value, out enumValue))
            {
                return enumValue;
            }
            return defValue;
        }
    }
}