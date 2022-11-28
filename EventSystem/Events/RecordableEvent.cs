using Godot;
using System;

namespace GodotCSharpToolkit.EventSystem.Events
{

    /// <summary>
    /// Base class for all events
    /// </summary>
    public abstract class RecordableEvent : ISerializableEvent
    {
        /// <summary>
        /// True if this event was created locally, false if it comes from a provider
        /// </summary>
        public bool IsLocal = true;

        /// <summary>
        /// The NodeExtensions.GetPeerId() of the creator of this event (1 is server)
        /// Does not get sent over the network as it can not be trusted, for local use only
        /// </summary>
        public int Sender = 1;

        /// <summary>
        /// The tick this event was created
        /// </summary>
        public ulong Tick;

        /// <summary>
        /// The number of this event in the event sequence from this peer
        /// </summary>
        public ulong Sequence;

        public RecordableEvent()
        {
            Tick = GameTicker.Tick;
            Sequence = EventManager.GetNextSequenceNumber();
        }

        /// <summary>
        /// Unless you inherit from the base abstract script you should always call base first.
        /// Also always end with a separator. Example:
        /// 
        ///     return $"{base.Serialize()}{ID}{EventManager.SEPARATOR}";
        /// 
        /// </summary>
        /// <returns>Serialized string that can be deserialized again</returns>
        public abstract String Serialize();

        /// <summary>
        /// Should deserialize the list, the pos is the position we are currently on
        /// It is assumed that you add +1 to this and return it when you consume a value from the list.
        /// 
        /// And that you call your parent before working on the list yourself.
        /// </summary>
        /// <param name="list">List of values</param>
        /// <param name="pos">The current postion</param>
        /// <returns></returns>
        public abstract int Deserialize(string[] list, int pos = 0);
    }

}