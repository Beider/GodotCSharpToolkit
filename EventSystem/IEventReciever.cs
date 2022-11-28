using Godot;
using System;
using GodotCSharpToolkit.EventSystem.Events;

namespace GodotCSharpToolkit.EventSystem
{
    public interface IEventReciever
    {
        void HandleEvent(RecordableEvent rEvent);
    }
}