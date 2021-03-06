﻿/*
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

namespace Twitch2Key
{
    /// <summary>
    /// Main application class.
    /// </summary>

    public static class Program
    {
        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        private static void Main(string[] args)
        {
            if (args.Length == 0) // No command-line parameters
            {
#if DEBUG
                // No parameters while debugging? Load a default .ini file.
                args = new string[] { "..\\Release\\AllLetters.ini" };
#else
                Console.WriteLine("No settings .ini file provided. Syntax is: Twitch2Key.exe PATH_TO_INI_FILE_WITH_SETTINGS");
                return;
#endif
            }

            using (T2K t2kApp = new T2K())
                t2kApp.Run(args[0]);

#if DEBUG
            // Make sure console window doesn't close immediately when debugging
            Console.WriteLine();
            Console.WriteLine("Press ENTER to close this window");
            Console.ReadLine();
#endif
        }
    }
}