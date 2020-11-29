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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TwitchChatExporter
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Dictionary of keyboard commands with the chat message that triggers them as a key.
        /// </summary>
        private static Dictionary<string, KeyboardCommand> KeyboardCommands;

        /// <summary>
        /// Stream writer to log all chat messages to a text file.
        /// </summary>
        private static StreamWriter LogWriter = null;

        /// <summary>
        /// Keyboard input sender class which will send simulated keyboard presses.
        /// </summary>
        private static KeyboardInputSender InputSender;
        
        /// <summary>
        /// Twitch listener class which will listen to the Twitch chat.
        /// </summary>
        private static TwitchChatListener Listener;

        /// <summary>
        /// Array of the names of users with admin privileges.
        /// </summary>
        private static string[] UserAdmins;

        /// <summary>
        /// Main application entry point.
        /// </summary>
        private static void Main(params string[] args)
        {
            if (args.Length == 0)
            {
#if DEBUG
                args = new string[] { "..\\Release\\DemoSettings.ini" };
#else
                Console.WriteLine("No settings .ini file provided. Syntax is: Twitch2Key.exe PATH_TO_INI_FILE_WITH_SETTINGS");
                return;
#endif
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File {args[0]} not found.");
#if DEBUG
                Console.ReadLine(); // So the console window doesn't close immediately when debugging
#endif
                return;
            }

            string twitchChannel, twitchIRCToken;

            KeyboardCommands = new Dictionary<string, KeyboardCommand>(StringComparer.InvariantCultureIgnoreCase);
            InputSender = new KeyboardInputSender();

            using (INIFile ini = new INIFile(args[0]))
            {
                twitchChannel = ini.GetValue<string>("Global", "Channel").Trim();
                twitchIRCToken = ini.GetValue<string>("Global", "Token").Trim();
                UserAdmins = (from string u in ini.GetValueArray<string>("Global", "UserAdmins")
                              where !string.IsNullOrEmpty(u.Trim()) select u.Trim()).Distinct().ToArray();

                if (ini.GetValue<bool>("Global", "LogToFile"))
                    LogWriter = File.AppendText($"{twitchChannel}.txt");

                if (string.IsNullOrEmpty(twitchChannel) || string.IsNullOrEmpty(twitchIRCToken))
                {
                    Console.WriteLine($"Twitch channel and twitch IRC token not set in file {args[0]}. Aborting.");
#if DEBUG
                    Console.ReadLine(); // So the console window doesn't close immediately when debugging
#endif
                    return;
                }

                foreach (string tlk in ini.GetTopLevelKeysInSection("Keyboard"))
                {
                    string command = ini.GetValue<string>("Keyboard", $"{tlk}.Message").Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(command) || KeyboardCommands.ContainsKey(command)) continue;

                    KeyboardCommands.Add(command, new KeyboardCommand(ini, tlk));
                }
            }

            Listener = new TwitchChatListener(twitchChannel, twitchIRCToken);
            Listener.OnTwitchChatMessage += OnTwitchChatMessage;
            Listener.OnTwitchChatUserMessage += OnTwitchChatUserMessage;
            Console.WriteLine("Beginning to listen to the Twitch chat, press CTRL+C to close");
            Listener.BeginListening();
        }

        /// <summary>
        /// Method called each time the Twitch IRC sends any message.
        /// </summary>
        /// <param name="rawMessage">The raw message, as outputed by the Twitch IRC server</param>
        private static void OnTwitchChatMessage(string rawMessage)
        {
            Console.WriteLine(rawMessage);
        }

        /// <summary>
        /// Method called each time the Twitch IRC sends an user messager.
        /// </summary>
        /// <param name="user">Name of the user</param>
        /// <param name="message">Messaged typed by the user</param>
        private static void OnTwitchChatUserMessage(string user, string message)
        {
            // Write output message to the log file.
            if (LogWriter != null)
            {
                LogWriter.WriteLine($"[{user}]: {message}");
                LogWriter.Flush();
            }

            // If posted message matches one of those in the [Keyboard] section of the .ini file,
            // send the appropriate key command.
            if (KeyboardCommands.ContainsKey(message))
            {
                KeyboardCommand command = KeyboardCommands[message];

                // Command is an admin command and user is not an admin, do nothing
                if (command.AdminOnly && !UserAdmins.Contains(user)) return;

                if (command.CancelledKey != Keys.None)
                    InputSender.ReleaseKey(command.CancelledKey);

                InputSender.PressKey(command.Key, command.Duration, command.Increment);
            }
        }
    }
}
