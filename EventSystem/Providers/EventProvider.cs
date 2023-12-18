using Godot;
using System;
using GodotCSharpToolkit.EventSystem.Events;
using GodotCSharpToolkit.Logging;


namespace GodotCSharpToolkit.EventSystem.Providers
{
    public partial class EventProvider : Node
    {
        public event Action<RecordableEvent> OnEvent = delegate { };

        public event Action OnComplete = delegate { };

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