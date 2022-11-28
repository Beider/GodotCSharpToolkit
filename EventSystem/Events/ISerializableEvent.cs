using Godot;
using System;

namespace GodotCSharpToolkit.EventSystem.Events
{
    public interface ISerializableEvent
    {
        /// <summary>
        /// Unless you inherit from the base abstract script you should always call base first.
        /// Also always end with a separator. Example:
        /// 
        ///     return $"{base.Serialize()}{ID}{EventManager.SEPARATOR}";
        /// 
        /// </summary>
        /// <returns>Serialized string that can be deserialized again</returns>
        String Serialize();

        /// <summary>
        /// Should deserialize the list, the pos is the position we are currently on
        /// It is assumed that you add +1 to this and return it when you consume a value from the list.
        /// 
        /// And that you call your parent before working on the list yourself.
        /// </summary>
        /// <param name="list">List of values</param>
        /// <param name="pos">The current postion</param>
        /// <returns></returns>
        int Deserialize(string[] list, int pos = 0);
    }

}