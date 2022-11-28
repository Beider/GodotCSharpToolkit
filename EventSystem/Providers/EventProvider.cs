using Godot;
using System;
using GodotCSharpToolkit.EventSystem.Events;
using GodotCSharpToolkit.Logging;


namespace GodotCSharpToolkit.EventSystem.Providers
{
    public class EventProvider : Node
    {
        public delegate void EventSendWorldInteractionEvent(RecordableEvent rEvent);
        public event EventSendWorldInteractionEvent OnEvent = delegate { };

        public delegate void EventComplete();
        public event EventComplete OnComplete = delegate { };

        /// <summary>
        /// Should be set to the last tick this provider provides
        /// </summary>
        protected ulong LastTickInProvider = 0;

        public EventProvider()
        {
            GameTicker.Instance.OnTick += OnTick;
        }

        public virtual void Stop()
        {
            GameTicker.Instance.OnTick -= OnTick;
        }

        protected virtual void OnTick(ulong tick)
        {

        }

        protected void PlaybackComplete()
        {
            Stop();
            Logger.Info("File playback complete");
            OnComplete();
        }

        public virtual ulong GetLastTick()
        {
            return LastTickInProvider;
        }
    }
}