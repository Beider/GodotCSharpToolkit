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
        public const string IMPORT_EXTENSION = ".import";
        public const string JSON_EXTENSION = ".json";

        /// <summary>
        /// Will fix the slashes in the path name
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (IsGodotPath(path) || Utils.IsMacOS())
            {
                return path.Replace("\\", "/");
            }
            return path.Replace("/", "\\");
        }

        /// <summary>
        /// Removes the last slash from a path if it has one
        /// </summary>
        public static string RemoveLastSlash(string path)
        {
            if (path == null) { return ""; }
            if (path.EndsWith("/") || path.EndsWith("\\"))
            {
                return path.Substr(0, path.Length - 1);
            }
            return path;
        }

        /// <summary>
        /// Will normalize the slashes and ensure we have an ending slash
        /// </summary>
        public static string NormalizeDirectory(string pathToDir)
        {
            if (pathToDir == null) { return null; }
            if (pathToDir == "") { return pathToDir; }
            string path = pathToDir;
            if (!path.EndsWith("/") && !path.EndsWith("\\")) { path += "/"; }
            return NormalizePath(path);
        }

        /// <summary>
        /// Get the name of a directory
        /// </summary>
        public static string GetDirectoryName(string path)
        {
            if (IsGodotPath(path))
            {
                var split = path.Split("/");
                int dist = 1;
                if (path.EndsWith("/")) { dist++; }
                return split[split.Length - dist];
            }
            else
            {
                return new System.IO.DirectoryInfo(path).Name;
            }
        }

        public static void CreateDirectory(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create directory", ex);
            }
        }

        public static void CreateDirectory(string path, string dirName)
        {
            var dir = DirAccess.Open(path);
            if (dir == null) { return; }
            if (!dir.DirExists(dirName))
            {
                dir.MakeDir(dirName);
            }
        }

        /// <summary>
        /// Get all subdirectories of the given path.
        /// </summary>
        public static List<string> GetSubDirectories(string parentPath)
        {
            List<string> retList = new();

            // Release builds do not like backslashes
            String path = NormalizeDirectory(parentPath);
            DirAccess dir = DirAccess.Open(path);
            if (dir == null)
            {
                return retList;
            }
            dir.ListDirBegin();
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
                    if (!IsGodotPath(subfolder) || Utils.IsMacOS())
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
        /// Uses the godot file reader and directory browser to read all files of the given type in a folder
        /// </summary>
        public static List<string> GetAllFilesInFolder(string pathToFolder, bool includeSubFolders, string extension = "")
        {
            List<string> retList = new();

            // Release builds do not like backslashes
            String path = NormalizeDirectory(pathToFolder);
            DirAccess dir = DirAccess.Open(path);

            if (dir == null)
            {
                return retList;
            }
            dir.ListDirBegin();
            while (true)
            {
                String filePath = dir.GetNext();
                if (filePath == "")
                {
                    break;
                }
                if (includeSubFolders && dir.CurrentIsDir() && !filePath.Equals(IMPORT_EXTENSION))
                {
                    // Go into all subfolder
                    retList.AddRange(GetAllFilesInFolder(path + filePath + "/", includeSubFolders, extension));
                }
                else if ((extension == "" && !dir.CurrentIsDir()) ||
                         (extension != "" && filePath.ToLower().EndsWith(extension)))
                {
                    // Grab name of all files found
                    if (!Engine.IsEditorHint() && filePath.EndsWith(IMPORT_EXTENSION))
                    {
                        // In export builds get rid of the .import at the end
                        retList.Add(path + filePath.Replace(IMPORT_EXTENSION, ""));
                    }
                    else
                    {
                        retList.Add(path + filePath);
                    }
                }
                else if (!Engine.IsEditorHint() && extension != "" && filePath.ToLower().EndsWith($"{extension}{IMPORT_EXTENSION}"))
                {
                    // This is an export build so we need to look for .Import files
                    retList.Add(path + filePath.Replace(IMPORT_EXTENSION, ""));
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
                return DirAccess.DirExistsAbsolute(path);
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
                var exists = FileAccess.FileExists(path);
                if (!exists && !Engine.IsEditorHint())
                {
                    exists = FileAccess.FileExists($"{path}{IMPORT_EXTENSION}");
                }
                return exists;
            }
            return System.IO.File.Exists(path);
        }

        public static void Delete(string path, bool recursive = false)
        {
            if (!DirAccess.DirExistsAbsolute(path))
            {
                if (FileAccess.FileExists(path))
                {
                    DirAccess.RemoveAbsolute(path);
                    return;
                }
            }
            var dir = DirAccess.Open(path);
            if (recursive && dir != null)
            {
                // Delete all files
                foreach (var file in GetAllFilesInFolder(path, false))
                {
                    dir.Remove(file);
                }

                // Deal with subfolders
                foreach (var folder in GetSubDirectories(path))
                {
                    Delete(folder, recursive);
                }
            }

            // Remove self
            dir?.Remove(path);
        }

        /// <summary>
        /// Loads the file content into a string. 
        /// Returns an empty string if file is not found.
        /// </summary>
        public static string LoadTextFile(string path)
        {
            string text = "";
            if (FileAccess.FileExists(path))
            {
                var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                text = f.GetAsText();
                f.Close();
            }
            return text;
        }

        /// <summary>
        /// Saves the string content to the file.
        /// </summary>
        public static string SaveToFile(string content, string filePath)
        {
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(content);
                file.Close();
                return null;
            }
            else
            {
                var error = FileAccess.GetOpenError();
                Logger.Error($"Failed to write to file, error code: {error}");
                return error.ToString();
            }
        }

        /// <summary>
        /// Uses the godot file reader and directory browser to read all json files of a given type in a folder.
        /// </summary>
        public static Dictionary<string, T> LoadAllJsonFilesInFolder<T>(string path, bool includeSubFolders)
        {
            Dictionary<string, T> retList = new Dictionary<string, T>();
            var fileList = FileUtils.GetAllFilesInFolder(path, includeSubFolders, ".json");

            // Load json files
            foreach (var file in fileList)
            {
                string fileContent = FileUtils.LoadTextFile(file);
                T jsonObj = (T)Utils.FromJson(fileContent, typeof(T));
                if (jsonObj == null)
                {
                    Logger.Info($"Failed to deserialize json to {typeof(T).Name}");
                }
                else
                {
                    retList.Add(file, jsonObj);
                }
            }

            return retList;
        }

        /// <summary>
        /// Check if the path is a godot internal path or not (starts with res:// or user://)
        /// </summary>
        /// <returns>True if it is a godot path</returns>
        public static bool IsGodotPath(string path)
        {
            return IsResPath(path) || path.ToLower().StartsWith("user:");
        }

        /// <summary>
        /// Check if this is from res://
        /// </summary>
        public static bool IsResPath(string path)
        {
            return path.ToLower().StartsWith("res:");
        }
    }
}