using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.EventSystem.Events;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.EventSystem.Recorders
{
    /// <summary>
    /// Writes events to a csv file
    /// 
    /// TODO: In the future we either need to guarantee event numbers between releases or write them to recording file.
    /// </summary>
    public class FileEventRecorder : EventRecorder
    {
        private readonly String Path;
        private StreamWriter Writer = null;

        /// <summary>
        /// Records to a file, will start with recording any settings
        /// </summary>
        /// <param name="path">The path to record to</param>
        /// <param name="settings">A list of settings</param>
        public FileEventRecorder(String path, List<String> settings)
        {
            string relPath = path;
            if (relPath == null || relPath == "")
            {
                Logger.Error($"File recorder path is null or empty");
                return;
            }
            Path = CreateFile(relPath);
            OpenFile(settings);
        }

        public override void Stop()
        {
            // Close file
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
            }
        }

        /// <summary>
        /// Create a new file and get relative path so we can open with System.IO
        /// </summary>
        /// <param name="relPath">The relative path to the file</param>
        /// <returns>The real path to the created file</returns>
        private String CreateFile(String relPath)
        {
            var file = new Godot.File();
            var dir = new Godot.Directory();

            // Make directories
            string directory = relPath.Substr(0, relPath.LastIndexOf("/"));
            if (!directory.EndsWith(":/"))
            {
                dir.MakeDirRecursive(directory);
            }

            // Make file
            file.Open(relPath, Godot.File.ModeFlags.Write);

            // Get real path
            string realPath = file.GetPathAbsolute();
            file.Close();
            return realPath;
        }

        private void OpenFile(List<String> settings)
        {
            // Open file
            var fileStream = new FileStream(Path, FileMode.Open);
            Writer = new StreamWriter(fileStream);

            // Record settings
            if (settings != null && settings.Count > 0)
            {
                foreach (string line in settings)
                {
                    Writer.WriteLine(line);
                }
                Writer.WriteLine(Constants.END_OF_SETTINGS);
                Writer.Flush();
            }

            Logger.Info($"Recording to file: {Path}");
        }

        public override void RecordEvent(RecordableEvent rEvent)
        {
            Writer.WriteLine(EventManager.SerializeEvent(rEvent));
        }
    }
}