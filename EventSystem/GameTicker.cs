using Godot;
using System;

namespace GodotCSharpToolkit.EventSystem
{
    public partial class GameTicker : Node
    {
        public static GameTicker Instance;

        public event Action<ulong> OnTick = delegate { };

        public static ulong Tick { get; private set; } = 0;

        public bool IsActive { get; set; } = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Instance = this;
        }

        public void SetTick(ulong tick)
        {
            Tick = tick;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsActive)
            {
                return;
            }

            // Super simple implementation for now
            OnTick(Tick);
            Tick++;
        }

        /// <summary>
        /// Converts the amount of seconds to amount of ticks, by default phsycis frame rate * seconds
        /// </summary>
        /// <param name="seconds">The seconds to convert</param>
        /// <returns>The ticks</returns>
        public static ulong GetSecondsAsTicks(ulong seconds)
        {
            return (ulong)Engine.PhysicsTicksPerSecond * seconds;
        }
    }
}