using Godot;
using GodotCSharpToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GodotCSharpToolkit.Extensions
{
    public static class StringExtensions
    {
        private static CultureInfo commaCulture = new CultureInfo("en")
        {
            NumberFormat =
        {
            NumberDecimalSeparator = ","
        }
        };

        private static CultureInfo pointCulture = new CultureInfo("en")
        {
            NumberFormat =
        {
            NumberDecimalSeparator = "."
        }
        };

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Should parse floats for any culture
        /// </summary>
        public static bool TryParseFloat(this string value, out float outValue)
        {
            outValue = 0f;
            if (value.IsNullOrEmpty()) { return false; }
            value = value.Trim();
            var culture = pointCulture;

            if (value.Contains(",") && value.Split(',').Length == 2)
            {
                culture = commaCulture;
            }

            if (float.TryParse(value, NumberStyles.Float, culture, out outValue))
            {
                return true;
            }

            Logger.Error($"Failed to convert value to decimal '{value}'. Returning 0.");
            return false;
        }

        public static string SanitizeFileName(this string name, bool allowSpace = false)
        {
            var invalids = new List<char>(System.IO.Path.GetInvalidFileNameChars());
            if (!allowSpace)
            {
                invalids.Add(' ');
            }
            return String.Join("_", name.Split(invalids.ToArray(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}