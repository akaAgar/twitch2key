/*
==========================================================================
This file is part of Twitch2Key, a tool which can turn messages from
a Twitch chat into simulated keyboard presses,
by @akaAgar (https://github.com/akaAgar/twitch2key)

Twitch2Key is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Twitch2Key is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Twitch2Key. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Twitch2Key
{
    using STimer = System.Timers.Timer;

    /// <summary>
    /// Sends keyboard inputs.
    /// Uses the InputManager library by ShyNet.
    /// (https://www.codeproject.com/Articles/117657/InputManager-library-Track-user-input-and-simulate)
    /// </summary>
    public sealed class KeyboardInputSender : IDisposable
    {
        /// <summary>
        /// Interval between key up/down check.
        /// </summary>
        private const double TIMER_INTERVAL = 50.0;

        /// <summary>
        /// Dictionary of duration during which each key must still stay pressed.
        /// </summary>
        private readonly Dictionary<Keys, double> TimeLeftForKey = new Dictionary<Keys, double>();
        
        /// <summary>
        /// Timer used to check for time left before key release.
        /// </summary>
        private readonly STimer Timer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public KeyboardInputSender()
        {
            TimeLeftForKey = new Dictionary<Keys, double>();
            foreach (Keys key in (Keys[])Enum.GetValues(typeof(Keys)))
            {
                if (TimeLeftForKey.ContainsKey(key)) continue;
                TimeLeftForKey.Add(key, 0.0);
            }
            
            Timer = new STimer();
            Timer.Elapsed += OnTimerElapsed;
            Timer.AutoReset = true;
            Timer.Interval = TIMER_INTERVAL;
            Timer.Start();
        }

        /// <summary>
        /// Presses down the specified key.
        /// </summary>
        /// <param name="key">Key to press down</param>
        /// <param name="milliseconds">How long should the key be pressed, in millseconds?</param>
        /// <param name="increment">If true, and the key is already pressed, duration will be added</param>
        public void PressKey(Keys key, double milliseconds = 100, bool increment = false)
        {
            milliseconds = Math.Max(1, milliseconds);

            if (TimeLeftForKey[key] <= 0.0)
            {
                Console.WriteLine($"Key {key} is DOWN");
                InputManager.Keyboard.KeyDown(key);
            }

            TimeLeftForKey[key] = Math.Max(0.0, TimeLeftForKey[key]);

            if (increment)
                TimeLeftForKey[key] = Math.Max(0.0, TimeLeftForKey[key]) + milliseconds;
            else
                TimeLeftForKey[key] = milliseconds;

            Console.WriteLine($"Pressing down {key} for {milliseconds}ms (total: {TimeLeftForKey[key]}ms)");
        }

        /// <summary>
        /// Release the specified key.
        /// </summary>
        /// <param name="key">Key to release</param>
        public void ReleaseKey(Keys key)
        {
            if (TimeLeftForKey[key] <= 0.0) return;

            TimeLeftForKey[key] = 0.0;
            InputManager.Keyboard.KeyUp(key);
        }

        /// <summary>
        /// Called each time the timer 
        /// </summary>
        /// <param name="sender">Sender timer</param>
        /// <param name="e">Timer parameters</param>
        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Keys[] keys = TimeLeftForKey.Keys.ToArray();

            foreach (Keys key in keys)
            {
                if (TimeLeftForKey[key] <= 0.0) continue;

                TimeLeftForKey[key] -= TIMER_INTERVAL;
                if (TimeLeftForKey[key] <= 0.0)
                {
                    Console.WriteLine($"Key {key} is UP");
                    InputManager.Keyboard.KeyUp(key);
                }
            }
        }

        /// <summary>
        /// IDisposable implemenration.
        /// </summary>
        public void Dispose()
        {
            // Destroy the timer
            Timer.Stop();
            Timer.Elapsed -= OnTimerElapsed;
            Timer.Dispose();

            // Release all keys
            foreach (Keys key in TimeLeftForKey.Keys)
                InputManager.Keyboard.KeyUp(key);
        }
    }
}
