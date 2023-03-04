using Godot;
using System;
using GodotCSharpToolkit.Logging;

/// <summary>
/// The file utils in this class work on both godot paths (res:// or user://) and absolute file paths.
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class FileUtils
    {
        /// <summary>
        /// Checks if the file exists
        /// </summary>
        public static bool FileExists(string path)
        {
            if (IsGodotPath(path))
            {
                return (new Godot.File()).FileExists(path);
            }
            return System.IO.File.Exists(path);
        }
        /// <summary>
        /// Loads the file content into a string. 
        /// Returns an empty string if file is not found.
        /// </summary>
        public static string LoadTextFile(string path)
        {
            if (IsGodotPath(path)) { return LoadTextFileGodot(path); }
            else { return LoadTextFileCSharp(path); }
        }

        private static string LoadTextFileGodot(string path)
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

        public static string LoadTextFileCSharp(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }
            return "";
        }

        /// <summary>
        /// Saves the string content to the file.
        /// </summary>
        public static void SaveToFile(string content, string filePath)
        {
            if (IsGodotPath(filePath)) { SaveToFileGodot(content, filePath); }
            else { SaveToFileCSharp(content, filePath); }
        }

        public static void SaveToFileGodot(string content, string filePath)
        {
            var file = new Godot.File();
            var error = file.Open(filePath, File.ModeFlags.Write);
            if (error == Error.Ok)
            {
                file.StoreString(content);
                file.Close();
            }
            else
            {
                Logger.Error($"Failed to write to file, error code: {error.ToString()}");
            }
        }

        private static void SaveToFileCSharp(string content, string filePath)
        {
            try
            {
                System.IO.File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to write to file", ex);
            }
        }

        /// <summary>
        /// Check if the path is a godot internal path or not (starts with res:// or user://)
        /// </summary>
        /// <returns>True if it is a godot path</returns>
        public static bool IsGodotPath(string path)
        {
            return path.ToLower().StartsWith("res://") || path.ToLower().StartsWith("user://");
        }
    }
}