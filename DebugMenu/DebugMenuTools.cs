using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace DebugMenu
{
    public partial class DebugMenu
    {

        /// <summary>
        /// Find and initialize all tools in the assemly
        /// </summary>
        private void InitTools()
        {
            GD.Print("------ ACTIVATING TOOLS ------");
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && typeof(IDebugTool).IsAssignableFrom(type))
                {
                    ActivateTool(type);
                }
            }
            GD.Print($"------ DONE ACTIVATING TOOLS ------");
        }

        private void ActivateTool(Type toolType)
        {
            GD.Print($"--- INIT {toolType.Name}");
            IDebugTool tool = (IDebugTool)Activator.CreateInstance(toolType);
            tool.Initialize();
        }
    }

}