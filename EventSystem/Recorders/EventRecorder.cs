using Godot;
using System;
using GodotCSharpToolkit.EventSystem.Events;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.EventSystem.Recorders
{
    public partial class EventRecorder
    {
        public EventRecorder()
        {

        }

        public virtual void Stop()
        {
            Logger.Error($"Type {GetType().Name} does not implement Stop");
        }

        public virtual void RecordEvent(RecordableEvent rEvent)
        {
            Logger.Error($"Type {GetType().Name} does not implement RecordEvent");
        }
    }
}