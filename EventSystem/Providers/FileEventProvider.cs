using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.EventSystem.Providers
{
    public class FileEventProvider : EventProvider
    {
        private static readonly int BUFFER_SIZE = 20;
        private static readonly int BUFFER_READ_TRIGGER = 10;
        private StreamReader Reader;
        private string Path;

        private Dictionary<ulong, List<string[]>> Buffer = new Dictionary<ulong, List<string[]>>();

        public FileEventProvider() : base()
        {

        }

        /// <summary>
        /// Load data from disk
        /// </summary>
        /// <param name="path">The path to the file to load (godot relative path)</param>
        /// <param name="settingsMethod">A method callback to handle settings, or null if you don't want settings</param>
        /// <returns></returns>
        public string Load(String path, Func<List<string>, String> settingsMethod)
        {
            string relPath = path;
            var file = new Godot.File();
            if (relPath == null || relPath == "")
            {
                return $"File provider path is null or empty";
            }
            else if (!file.FileExists(relPath))
            {
                return $"File provider recording path does not exist";
            }
            Path = GetFileAbsPath(relPath);

            return OpenFile(settingsMethod);
        }

        public override void Stop()
        {
            if (Reader != null)
            {
                Reader.Close();
            }
            base.Stop();
        }

        private String GetFileAbsPath(String relPath)
        {
            var file = new Godot.File();

            // Make file
            file.Open(relPath, Godot.File.ModeFlags.Read);

            // Get real path
            string realPath = file.GetPathAbsolute();
            file.Close();
            return realPath;
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="settingsMethod">A method to pass any settings to</param>
        /// <returns>An error message or null</returns>
        private string OpenFile(Func<List<string>, String> settingsMethod)
        {
            Logger.Info($"Playing back from file: {Path}");

            // Get the last tick first
            LastTickInProvider = GetTick(System.IO.File.ReadAllLines(Path).Last());

            // Open file
            var fileStream = new FileStream(Path, FileMode.Open);
            Reader = new StreamReader(fileStream);

            // Read settings
            string message = ReadSettings(settingsMethod);
            if (message != null)
            {
                // Abort we can't load this file
                Stop();
                return message;
            }

            FillBuffer();

            return null;
        }

        private string ReadSettings(Func<List<string>, String> settingsMethod)
        {
            if (settingsMethod != null)
            {
                List<string> settings = new List<string>();
                string line = Reader.ReadLine();
                while (line != Constants.END_OF_SETTINGS)
                {
                    settings.Add(line);
                    line = Reader.ReadLine();
                }

                return settingsMethod(settings);
            }
            return null;
        }

        protected override void OnTick(ulong tick)
        {
            if (Buffer.Count == 0)
            {
                PlaybackComplete();
            }
            // Check for event
            if (Buffer.ContainsKey(tick))
            {
                // Send event
                Buffer[tick].ForEach(splitLine => HandleEvent(splitLine));
                Buffer.Remove(tick);

                // Read more events if buffer is running low
                FillBuffer();
            }
        }

        private void HandleEvent(string[] splitLine)
        {
            var evnt = EventManager.DeserializeEvent(splitLine);
            evnt.IsLocal = false;
            EventManager.SendEvent(evnt, true);
        }

        private void FillBuffer()
        {
            if (Reader.EndOfStream || Buffer.Count > BUFFER_READ_TRIGGER)
            {
                return;
            }
            while (Buffer.Count < BUFFER_SIZE && !Reader.EndOfStream)
            {
                string line = Reader.ReadLine();
                AddTobuffer(line);
            }
        }

        private ulong GetTick(string line)
        {
            string[] lineSplit = line.Split(Constants.SEPARATOR);
            return ulong.Parse(lineSplit[EventManager.POS_TICK]);
        }

        private void AddTobuffer(string line)
        {
            string[] lineSplit = line.Split(Constants.SEPARATOR);
            ulong tick = ulong.Parse(lineSplit[EventManager.POS_TICK]);
            ulong sequence = ulong.Parse(lineSplit[EventManager.POS_SEQUENCE]);
            if (!Buffer.ContainsKey(tick))
            {
                Buffer.Add(tick, new List<string[]>());
            }
            int pos = 0;
            for (int i = 0; i < Buffer[tick].Count; i++)
            {
                ulong tmpSeq = ulong.Parse(Buffer[tick][i][EventManager.POS_SEQUENCE]);
                if (tmpSeq < sequence)
                {
                    pos++;
                }
                else
                {
                    break;
                }
            }


            Buffer[tick].Insert(pos, lineSplit);
        }
    }
}