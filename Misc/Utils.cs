using Godot;
using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GodotCSharpToolkit.Logging;
using System.Linq;

namespace GodotCSharpToolkit.Misc
{
    public static class Utils
    {
        /// <summary>
        /// Load all json files in a folder
        /// </summary>
        /// <param name="path">Path should be relative</param>
        /// <param name="includeSubFolders">Should we include sub folders</param>
        /// <returns>A dictionary of file path, content</returns>
        public static Dictionary<string, List<U>> LoadJsonFileContent<T, U>(string path, bool includeSubFolders) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            // Load a generic json file
            var resultList = new Dictionary<string, List<U>>();
            Dictionary<string, T> files = FileUtils.LoadAllJsonFilesInFolder<T>(path, includeSubFolders);
            if (files != null)
            {
                foreach (var key in files.Keys)
                {
                    T sDefJsonFile = files[key];
                    var list = new List<U>();
                    foreach (U def in sDefJsonFile.GetValues())
                    {
                        def.SetSource(key);
                        list.Add(def);
                    }
                    resultList.Add(key, list);
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
            return FileUtils.NormalizeDirectory(setting.ToString()); ;
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

        public static List<string> GetEnumValuesAsString(Type enumType)
        {
            var returnList = new List<string>();
            foreach (var val in Enum.GetValues(enumType))
            {
                returnList.Add(val.ToString());
            }
            return returnList;
        }

        /// <summary>
        /// Used primarily for the editor
        /// </summary>
        public static Dictionary<object, string> EnumToDictionary(Type enumType, bool sorted = true)
        {
            var returnList = new Dictionary<object, string>();
            foreach (var val in Enum.GetValues(enumType))
            {
                returnList.Add(val, val.ToString());
            }
            return returnList.OrderBy(pair => pair.Key.ToString()).ToDictionary(obj => obj.Key, obj => obj.Value); ;
        }

        /// <summary>
        /// Convert a list of items into an array using a conversion function.
        /// You could also use list.Select(x=>convert(x)).ToArray() in Linq
        /// </summary>
        public static T[] ConvertListToArray<T, U>(List<U> list, Func<U, T> convert)
        {
            var array = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = convert(list[i]);
            }
            return array;
        }

        private static NumberFormatInfo nfi = null;

        public static NumberFormatInfo NFormatInfo
        {
            get
            {
                if (nfi == null)
                {
                    nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";
                }
                return nfi;
            }
        }

        public static string ToNumericString(this object obj)
        {
            if (obj == null) { return null; }
            if (obj is float f)
            {
                return f.ToString(NFormatInfo);
            }
            if (obj is int i)
            {
                return i.ToString();
            }
            return obj.ToString();
        }
    }
}