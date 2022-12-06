using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace GodotCSharpToolkit.Logging
{
    public enum LogLevel
    {
        Trace,
        Info,
        Debug,
        Warning,
        Error
    }

    /// <summary>
    /// Implement this interface to add custom log handling
    /// </summary>
    public interface LogWriter
    {
        void Log(string message, LogLevel level, Exception ex);
    }

    public class Logger : Node
    {
        public static Logger Instance;

        private List<LogWriter> Loggers = new List<LogWriter>();

        private bool HasLoggers = false;

        public override void _Ready()
        {
            Instance = this;
        }

        public static void AddLogger(LogWriter logger)
        {
            Instance.HasLoggers = true;
            Instance.Loggers.Add(logger);
        }

        public static void Trace(string message)
        {
            Log(message, LogLevel.Trace);
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
            Log(message, LogLevel.Error, ex);
        }

        public static void Log(string message, LogLevel level, Exception ex = null)
        {
            if (!Instance.HasLoggers)
            {
                return;
            }

            Instance.Loggers.ForEach(l => l.Log(message, level, ex));
        }
    }
}