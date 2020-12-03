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

namespace Twitch2Key
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed class T2K : IDisposable
    {
        /// <summary>
        /// Is the admin mode (only admin can use commands at the time) enabled?
        /// </summary>
        private bool AdminModeEnabled = false;

        /// <summary>
        /// Dictionary of keyboard commands with the chat message that triggers them as a key.
        /// </summary>
        private Dictionary<string, KeyboardCommand> KeyboardCommands;

        /// <summary>
        /// Stream writer to log all chat messages to a text file.
        /// </summary>
        private StreamWriter LogWriter = null;

        /// <summary>
        /// Keyboard input sender class which will send simulated keyboard presses.
        /// </summary>
        private KeyboardInputSender InputSender;
        
        /// <summary>
        /// Twitch listener class which will listen to the Twitch chat.
        /// </summary>
        private TwitchChatListener Listener;

        /// <summary>
        /// Dictionary of teams each players belong to.
        /// </summary>
        private Dictionary<string, int> Teams;

        /// <summary>
        /// Array of the names of users with admin privileges.
        /// </summary>
        private string[] UserAdmins;

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="settingINIFile">Path to the settings .ini file to use</param>
        public void Run(string settingINIFile)
        {
            if (!File.Exists(settingINIFile))
            {
                Console.WriteLine($"File {settingINIFile} not found.");
                return;
            }

            string twitchChannel, twitchIRCToken;

            AdminModeEnabled = false;
            KeyboardCommands = new Dictionary<string, KeyboardCommand>(StringComparer.InvariantCultureIgnoreCase);
            InputSender = new KeyboardInputSender();
            Teams = new Dictionary<string, int>();

            using (INIFile ini = new INIFile(settingINIFile))
            {
                twitchChannel = ini.GetValue<string>("Global", "Channel").Trim();
                twitchIRCToken = ini.GetValue<string>("Global", "Token").Trim();
                UserAdmins = (from string u in ini.GetValueArray<string>("Global", "UserAdmins")
                              where !string.IsNullOrEmpty(u.Trim()) select u.Trim().ToLowerInvariant()).Distinct().ToArray();
                Console.WriteLine($"The following users are admins: {string.Join(", ", UserAdmins)}");

                if (ini.GetValue<bool>("Global", "LogToFile"))
                    LogWriter = File.AppendText($"{twitchChannel}.txt");

                if (string.IsNullOrEmpty(twitchChannel) || string.IsNullOrEmpty(twitchIRCToken))
                {
                    Console.WriteLine($"Twitch channel and twitch IRC token not set in file {settingINIFile}. Aborting.");
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
        private void OnTwitchChatMessage(string rawMessage)
        {
            Console.WriteLine(rawMessage);
        }

        /// <summary>
        /// Method called each time the Twitch IRC sends an user messager.
        /// </summary>
        /// <param name="user">Name of the user</param>
        /// <param name="message">Messaged typed by the user</param>
        private void OnTwitchChatUserMessage(string user, string message)
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

                // Command is an admin command, or we are currently in admin-only mode, and user is not an admin: do nothing
                if ((command.AdminsOnly || AdminModeEnabled) && !UserAdmins.Contains(user.ToLowerInvariant()))
                {
                    Console.WriteLine($"User {user} tried to call admin-only commmand {message}");
                    return;
                }

                // Check for special command functions
                switch (command.Special)
                {
                    case KeyboardCommandSpecial.AdminModeOff:
                        AdminModeEnabled = false;
                        Console.WriteLine("Admin mode turned OFF.");
                        return;
                    case KeyboardCommandSpecial.AdminModeOn:
                        AdminModeEnabled = true;
                        Console.WriteLine("Admin mode turned ON.");
                        return;
                    case KeyboardCommandSpecial.ResetAllTeams:
                        Teams.Clear();
                        Console.WriteLine("All teams cleared.");
                        return;
                    case KeyboardCommandSpecial.SetTeam:
                    case KeyboardCommandSpecial.SetTeamOverwrite:
                        if ((command.Special != KeyboardCommandSpecial.SetTeamOverwrite) && Teams.ContainsKey(user))
                            return; // User already belongs to a team
                        if (Teams.ContainsKey(user)) Teams.Remove(user); // Remove the current value
                        if (command.Team > 0) // Set the new team (if team is not zero)
                        {
                            Teams.Add(user, command.Team);
                            Console.WriteLine($"User {user} joined team {command.Team}.");
                        }
                        else
                            Console.WriteLine($"User {user} removed from all teams.");
                        return;
                }

                // Command is a team command, check players belongs to the correct team
                // Must be checked after Command.Special because SetTeam special functions also use the Command.Team property,
                // and we don't want to restrict these commands to players already belonging to the team they which to join
                // (which would be pointless).
                if ((command.Team != 0) && (!Teams.ContainsKey(user) || (Teams[user] != command.Team)))
                    return;

                // If an old key press should be cancelled, do it now
                if (command.CancelledKey != Keys.None)
                    InputSender.ReleaseKey(command.CancelledKey);

                // Press the new key
                InputSender.PressKey(command.Key, command.Duration, command.Increment);
            }
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }
    }
}
