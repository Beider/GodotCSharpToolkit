using System;
using Godot;
using System.Reflection;

namespace DebugMenu
{
    /// <summary>
    /// Extension class to provide useful framework methods
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    public static class DebugMemberInfoExtensions
    {
        public const string LOG_CAT = "LogMemberInfoExtension";

        /// <summary>
        /// Sets the value of this member
        /// </summary>
        /// <param name="member">The member</param>
        /// <param name="Instance">The instance to set the value for</param>
        /// <param name="Value">The value</param>
        public static void SetValue(this MemberInfo member, object Instance, object Value)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(Instance, Value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(Instance, Value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get the value of this member
        /// </summary>
        /// <param name="member">The member</param>
        /// <param name="Instance">The instance to get the value of</param>
        /// <returns>The value of the member in the instance</returns>
        public static object GetValue(this MemberInfo member, object Instance)
        {
            if (Instance == null)
            {
                return null;
            }
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(Instance);
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetValue(Instance);
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Gets the underlying type of a member
        /// </summary>
        /// <param name="member">The member to find the type for</param>
        /// <returns>The underlying type</returns>
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Clear all the children of a node
        /// </summary>
        public static void ClearNodeChildren(this Node Instance)
        {
            foreach (Node child in Instance.GetChildren())
            {
                child.QueueFree();
            }
        }
    }
}