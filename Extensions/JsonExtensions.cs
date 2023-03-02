using System;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GodotCSharpToolkit.Misc;

namespace GodotCSharpToolkit.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJsonObject<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (System.Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, Utils.Settings), Utils.Settings);
        }

        public static object CloneJsonObject(this object source, Type objectType)
        {
            // Don't serialize a null object, simply return null
            if (System.Object.ReferenceEquals(source, null))
            {
                return null;
            }

            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source, Utils.Settings), objectType, Utils.Settings);
        }
    }
}