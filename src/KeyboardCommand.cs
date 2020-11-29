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

using INIPlusPlus;
using System;
using System.Windows.Forms;

namespace TwitchChatExporter
{
    /// <summary>
    /// Stores information about a key command to execute when a certain message is typed in the chat.
    /// </summary>
    public struct KeyboardCommand
    {
        /// <summary>
        /// Is this command only available to "admin" users?
        /// </summary>
        public bool AdminOnly { get; }

        /// <summary>
        /// Keypress to cancel when this key is pressed
        /// (to make sure "move forward" and "move backward" key are not pressed at the same time, etc.)
        /// </summary>
        public Keys CancelledKey { get; }
        
        /// <summary>
        /// How long (in milliseconds) should the key be pressed?
        /// </summary>
        public double Duration { get; }
        
        /// <summary>
        /// The key code to press.
        /// </summary>
        public Keys Key { get; }
        
        /// <summary>
        /// If true, key press <see cref="Duration"/> will be added to the total duration of the key press.
        /// If false, duration will be set to the valued specified in <see cref="Duration"/>.
        /// </summary>
        public bool Increment { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ini">.ini file to read the command from</param>
        /// <param name="iniKey">.ini key from which to read the command</param>
        public KeyboardCommand(INIFile ini, string iniKey)
        {
            AdminOnly = ini.GetValue("Keyboard", $"{iniKey}.AdminOnly", false);
            CancelledKey = ini.GetValue("Keyboard", $"{iniKey}.CancelledKey", Keys.None);
            Duration = Math.Max(1, ini.GetValue("Keyboard", $"{iniKey}.Duration", 100));
            Key = ini.GetValue("Keyboard", $"{iniKey}.Key", Keys.None);
            Increment = ini.GetValue("Keyboard", $"{iniKey}.Increment", false);
        }
    }
}
