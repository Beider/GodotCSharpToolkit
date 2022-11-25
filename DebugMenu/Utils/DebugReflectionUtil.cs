using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DebugMenu
{
    /// <summary>
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    public class DebugReflectionUtil
    {
        public static BindingFlags BindFlagsAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static;

        // These two can be useful for checking how often the cache is hit or who is hitting the cache
        #region Developer Helper Methods
        private static int CacheHits
        {
            get
            {
                return _cacheHits;
            }
            set
            {
                _cacheHits = value;
                if (_cacheHits % 100 == 0)
                {
                    GD.Print(String.Format("Cache hit {0} times", _cacheHits));
                }
            }
        }

        private static int _cacheHits = 0;

        private static Dictionary<string, int> CallerCounter = new Dictionary<string, int>();
        private static void RecordCaller()
        {
            StackTrace stackTrace = new StackTrace();
            string callerMethod = stackTrace.GetFrame(2).GetMethod().Name;
            if (!CallerCounter.ContainsKey(callerMethod))
            {
                CallerCounter.Add(callerMethod, 0);
            }

            CallerCounter[callerMethod]++;
        }

        #endregion

        #region GetCustomAttributes
        private static Dictionary<string, object[]> GetCustomAttributesCache = new Dictionary<string, object[]>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static object[] GetCustomAttributes<T>(MemberInfo member, bool inherit) where T : Attribute
        {
            string key = $"{member.DeclaringType.Name}#{member.Name}#{typeof(T).Name}";
            if (!GetCustomAttributesCache.ContainsKey(key))
            {
                GetCustomAttributesCache.Add(key, member.GetCustomAttributes(typeof(T), inherit));
            }

            return GetCustomAttributesCache[key];
        }
        #endregion

        #region GetCustomAttribute

        private static Dictionary<string, Attribute> GetCustomAttributeCache = new Dictionary<string, Attribute>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Attribute GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            string key = $"{member.DeclaringType.Name}#{member.Name}#{typeof(T).Name}";
            if (!GetCustomAttributeCache.ContainsKey(key))
            {
                GetCustomAttributeCache.Add(key, member.GetCustomAttribute(typeof(T)));
            }

            return GetCustomAttributeCache[key];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Attribute GetCustomAttribute<T>(Type type) where T : Attribute
        {
            string key = $"{type.Name}#{typeof(T).Name}";
            if (!GetCustomAttributeCache.ContainsKey(key))
            {
                GetCustomAttributeCache.Add(key, type.GetCustomAttribute(typeof(T)));
            }

            return GetCustomAttributeCache[key];
        }
        #endregion

        #region GetMethodInfos
        private static Dictionary<string, IList<MethodInfo>> GetMethodInfosCache = new Dictionary<string, IList<MethodInfo>>();

        /// <summary>
        /// Returns a list of all the unique methods for a Node, including the hierarchy
        /// </summary>
        /// <param name="Instance">The object type to find for</param>
        /// <returns>List of methodss</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IList<MethodInfo> GetMethodInfos(Type Instance)
        {
            string key = Instance.Name;
            if (!GetMethodInfosCache.ContainsKey(key))
            {
                List<MethodInfo> Methods = new List<MethodInfo>();
                while (Instance != null && !IsInGodotOrSystemNamespace(Instance))
                {
                    Methods.AddRange(Instance.GetMethods(BindFlagsAll));
                    Instance = Instance.BaseType;
                }

                List<MethodInfo> DeDupedMethods = new List<MethodInfo>();
                foreach (MethodInfo Method in Methods)
                {
                    bool IsUnique = DeDupedMethods.All(DeDupedMethod => DeDupedMethod.DeclaringType != Method.DeclaringType || DeDupedMethod.Name != Method.Name);

                    if (IsUnique)
                    {
                        DeDupedMethods.Add(Method);
                    }
                }

                GetMethodInfosCache.Add(key, DeDupedMethods.AsReadOnly());
            }

            return GetMethodInfosCache[key];
        }

        #endregion

        #region GetMemberInfos
        private static Dictionary<string, IList<MemberInfo>> GetMemberInfosCache = new Dictionary<string, IList<MemberInfo>>();
        /// <summary>
        /// Returns a list of all the unique members for a Node, including the hierarchy
        /// </summary>
        /// <param name="Instance">The object type to find for</param>
        /// <returns>List of members</returns>
        /// 
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IList<MemberInfo> GetMemberInfos(Type Instance)
        {
            string key = Instance.Name;
            if (!GetMemberInfosCache.ContainsKey(key))
            {
                List<MemberInfo> Members = new List<MemberInfo>();
                while (Instance != null && !IsInGodotOrSystemNamespace(Instance))
                {
                    Members.AddRange(Instance.GetFields(BindFlagsAll));
                    Members.AddRange(Instance.GetProperties(BindFlagsAll));
                    Instance = Instance.BaseType;
                }

                List<MemberInfo> DeDupedMembers = new List<MemberInfo>();
                foreach (MemberInfo Member in Members)
                {
                    bool IsUnique = DeDupedMembers.All(
                        DeDupedMember =>
                            DeDupedMember.DeclaringType != Member.DeclaringType || DeDupedMember.Name != Member.Name);

                    if (IsUnique)
                    {
                        DeDupedMembers.Add(Member);
                    }
                }

                // Convert to read only so our cache is never modified
                GetMemberInfosCache.Add(key, DeDupedMembers.AsReadOnly());
            }

            return GetMemberInfosCache[key];
        }

        #endregion

        #region FindClassAttributeInNode

        private static Dictionary<string, Attribute> ClassAttributeCache = new Dictionary<string, Attribute>();
        /// <summary>
        /// Returns the attribute object for the specified type, 
        /// climbing the hierarchy until Node is reached or the attribute is found
        /// </summary>
        /// <param name="InstanceType">The type to search</param>
        /// <typeparam name="T">The type to find</typeparam>
        /// <returns>The attribute object for the specified type or null if not found</returns>
        /// 
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T FindClassAttributeInNode<T>(Type InstanceType) where T : Attribute
        {
            // Check the buffer
            string key = $"{InstanceType.Name}#{typeof(T).Name}";
            if (ClassAttributeCache.ContainsKey(key))
            {
                return (T)ClassAttributeCache[key];
            }

            if (IsSameOrSubclass(InstanceType, typeof(Node)) == false)
            {
                return null;
            }

            T FoundAtr = Attribute.GetCustomAttribute(InstanceType, typeof(T)) as T;

            if (FoundAtr == null && InstanceType != typeof(Node) && InstanceType.BaseType != null)
            {
                FoundAtr = FindClassAttributeInNode<T>(InstanceType.BaseType);
            }

            if (FoundAtr != null)
            {
                // Add to buffer if found
                ClassAttributeCache.Add(key, FoundAtr);
                return FoundAtr;
            }

            ClassAttributeCache.Add(key, null);

            return null;
        }

        #endregion

        /// <summary>
        /// Check if a type is in the godot namespace
        /// </summary>
        public static bool IsInGodotOrSystemNamespace(Type type)
        {
            if (type.Namespace != null &&
                (type.Namespace == "Godot" || type.Namespace.StartsWith("Godot.")
                || type.Namespace == "System" || type.Namespace.StartsWith("System.")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if subclass is a subclass of the base 
        /// </summary>
        /// <param name="SubClass">The sube class</param>
        /// <param name="Base">The base class</param>
        /// <returns>true if the types are equal or is SubClass is a a subclass of Base</returns>
        public static bool IsSameOrSubclass(Type SubClass, Type Base)
        {
            return SubClass.IsSubclassOf(Base) || SubClass == Base;
        }
    }
}
