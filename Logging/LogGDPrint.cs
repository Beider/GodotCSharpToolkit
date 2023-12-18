using Godot;
using System;
using System.Diagnostics;
using GodotCSharpToolkit.Extensions;

namespace GodotCSharpToolkit.Logging
{
    public partial class LogGDPrint : LogWriter
    {
        int Level = 0;
        public LogGDPrint(LogLevel minLevel)
        {
            Level = (int)minLevel;
        }

        public void Log(string message, LogLevel level, Exception ex)
        {
            if ((int)level < Level)
            {
                return;
            }

            StackTrace trace = new StackTrace(true);
            StackFrame frame = trace.GetFrame(5);
            string msg = message;

            if (frame != null)
            {
                msg = $"[{ShortenFileName(frame.GetFileName())}][{frame.GetMethod().Name}][{frame.GetFileLineNumber()}] {message}";
            }

            if (level == LogLevel.Error)
            {
                LogError(msg, ex);
            }
            else
            {
                GD.Print(msg);
            }
        }

        private string ShortenFileName(string name)
        {
            if (name.IsNullOrEmpty())
            {
                return "";
            }
            int index = name.LastIndexOf("\\");
            if (index >= 0)
            {
                return name.Substring(index + 1);
            }
            return name;
        }

        private void LogError(string message, Exception ex)
        {
            Exception curEx = ex;

            GD.PrintErr(message);
            if (curEx != null)
            {
                GD.PrintErr("------------");
                while (curEx != null)
                {
                    GD.PrintErr(curEx.Message);
                    GD.PrintErr(curEx.StackTrace);
                    GD.PrintErr("------------");
                    curEx = curEx.InnerException;
                }
            }
        }
    }
}