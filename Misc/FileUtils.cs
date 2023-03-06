using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

/// <summary>
/// The file utils in this class work on both godot paths (res:// or user://) and absolute file paths.
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class FileUtils
    {
        /// <summary>
        /// Will fix the slashes in the path name
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (IsGodotPath(path))
            {
                return path.Replace("\\", "/");
            }
            return path.Replace("/", "\\");
        }
        /// <summary>
        /// Get all subdirectories of the given path.
        /// </summary>
        public static List<string> GetSubDirectories(string parentPath)
        {
            List<string> retList = new List<string>();
            Directory dir = new Directory();

            // Release builds do not like backslashes
            String path = parentPath.Replace("\\", "/");
            dir.Open(path);
            dir.ListDirBegin(true, true);
            while (true)
            {
                String filePath = dir.GetNext();
                if (filePath == "")
                {
                    break;
                }
                if (dir.CurrentIsDir())
                {
                    // Grab the subfolders
                    var subfolder = path + filePath + "/";
                    if (!IsGodotPath(subfolder))
                    {
                        subfolder = subfolder.Replace("/", "\\");
                    }
                    retList.Add(subfolder);
                }
            }
            dir.ListDirEnd();
            return retList;
        }

        /// <summary>
        /// Checks if the directory exists
        /// </summary>
        public static bool DirectoryExists(string path)
        {
            if (IsGodotPath(path))
            {
                return (new Godot.Directory()).DirExists(path);
            }
            return System.IO.Directory.Exists(path);
        }

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