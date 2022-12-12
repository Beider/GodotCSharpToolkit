using System;
using Godot;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.Extensions
{
    public static class MiscExtensions
    {
        public static Vector2 ToVector2(this Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return new Vector2(0, 0);
                case Direction.Up:
                    return new Vector2(0, -1);
                case Direction.UpRight:
                    return new Vector2(1, -1);
                case Direction.Right:
                    return new Vector2(1, 0);
                case Direction.DownRight:
                    return new Vector2(1, 1);
                case Direction.Down:
                    return new Vector2(0, 1);
                case Direction.DownLeft:
                    return new Vector2(-1, 1);
                case Direction.Left:
                    return new Vector2(-1, 0);
                case Direction.UpLeft:
                    return new Vector2(-1, -1);
                default:
                    return new Vector2(0, 0);
            }
        }

        public static float ToAngle(this Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return 0f;
                case Direction.Right:
                    return 0f;
                case Direction.DownRight:
                    return 45f;
                case Direction.Down:
                    return 90f;
                case Direction.DownLeft:
                    return 135f;
                case Direction.Left:
                    return 180f;
                case Direction.UpLeft:
                    return 225f;
                case Direction.Up:
                    return 270f;
                case Direction.UpRight:
                    return 315f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Print the value of a stopwatch
        /// </summary>
        /// <param name="watch">The stopwatch</param>
        /// <param name="name">Name of the watch</param>
        public static void PrintStopwatch(this System.Diagnostics.Stopwatch watch, string name)
        {
            Logger.Info($"{name} took {watch.ElapsedMilliseconds} milliseconds ({watch.ElapsedTicks} ticks)");
        }

        /// <summary>
        /// Compare two floats with a margin of error
        /// </summary>
        /// <param name="value1">First float</param>
        /// <param name="value2">Second float</param>
        /// <param name="epsilon">Margin of error</param>
        /// <returns>True if within the margin</returns>
        public static bool CompareFloats(this float value1, float value2, float epsilon = 0.0001f)
        {
            return Mathf.Abs(value1 - value2) <= epsilon;
        }
    }
}