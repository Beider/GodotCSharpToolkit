using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace GodotCSharpToolkit.Logging
{
    public enum LogLevel
    {
        Info,
        Debug,
        Warning,
        Error
    }
    public class Logger : Node
    {
        public static Logger Instance;

        public override void _Ready()
        {
            Instance = this;
        }

        public static void Info(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        public static void Warning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void Error(string message, Exception ex = null)
        {
            Log(message, LogLevel.Error);
            if (ex != null)
            {
                Log(ex.Message, LogLevel.Error);
                Log(ex.StackTrace, LogLevel.Error);
            }
        }

        public static void Log(string message, LogLevel level)
        {
            GD.Print(message);
        }
    }
}