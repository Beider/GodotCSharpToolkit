using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    public partial class DebugMenu
    {

        /// <summary>
        /// Find and initialize all tools in the assemly
        /// </summary>
        private void InitTools()
        {
            if (!OS.HasFeature("editor"))
            {
                Logger.Info("Running in export build, skipping tools");
                return;
            }
            Logger.Info("------ ACTIVATING TOOLS ------");
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && typeof(IDebugTool).IsAssignableFrom(type))
                {
                    ActivateTool(type);
                }
            }
            Logger.Info($"------ DONE ACTIVATING TOOLS ------");
        }

        private void ActivateTool(Type toolType)
        {
            Logger.Info($"--- INIT {toolType.Name}");
            IDebugTool tool = (IDebugTool)Activator.CreateInstance(toolType);
            tool.Initialize();
        }
    }

}