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
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Twitch2Key
{
    /// <summary>
    /// Listens to a Twitch channel chat and raises events when a message is posted.
    /// </summary>
    public sealed class TwitchChatListener : IDisposable
    {
        /// <summary>
        /// How long (in milliseconds) to wait for 
        /// </summary>
        private const int RESTART_DELAY_ON_ERROR = 3000;

        /// <summary>
        /// Adress for the Twitch chat IRC server.
        /// </summary>
        private const string TWITCH_IRC_SERVER = "irc.chat.twitch.tv";

        /// <summary>
        /// Adress for the Twitch chat IRC port.
        /// </summary>
        private const int TWITCH_IRC_PORT = 6667;

        /// <summary>
        /// Delegate for events raised when the Twitch IRC sends any message.
        /// </summary>
        /// <param name="rawMessage">The raw message, as outputed by the Twitch IRC server</param>
        public delegate void TwitchChatMessage(string rawMessage);

        /// <summary>
        /// Delegate for events raised when the Twitch IRC sends an user messager.
        /// </summary>
        /// <param name="user">Name of the user</param>
        /// <param name="message">Messaged typed by the user</param>
        public delegate void TwitchChatUserMessage(string user, string message);

        /// <summary>
        /// Event raised when the Twitch IRC sends any message.
        /// </summary>
        public event TwitchChatMessage OnTwitchChatMessage = null;

        /// <summary>
        /// Event raised when the Twitch IRC sends an user messager.
        /// </summary>
        public event TwitchChatUserMessage OnTwitchChatUserMessage = null;

        /// <summary>
        /// Twitch channel to listen to.
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// Password/token to use to sign to the Twitch chat IRC.
        /// A password can be generated here: https://twitchapps.com/tmi/
        /// </summary>
        private string Token { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="channel">Twitch channel to listen to</param>
        /// <param name="token">Password/token to use to sign to the Twitch chat IRC</param>
        public TwitchChatListener(string channel, string token)
        {
            Channel = channel;
            Token = token;
        }

        /// <summary>
        /// Begins listening to the Twitch chat.
        /// </summary>
        public void BeginListening()
        {
            TcpClient irc;
            string returnedLine;
            StreamReader reader;
            NetworkStream stream;
            StreamWriter writer;

            try
            {
                irc = new TcpClient(TWITCH_IRC_SERVER, TWITCH_IRC_PORT);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.WriteLine($"PASS {Token}");
                writer.WriteLine("NICK TwitchChatToText");
                writer.WriteLine($"JOIN #{Channel}");
                writer.Flush();

                while (true)
                {
                    while ((returnedLine = reader.ReadLine()) != null)
                    {
                        OnTwitchChatMessage?.Invoke(returnedLine);

                        if (returnedLine.Contains("PRIVMSG"))
                        {
                            string user = "Unknown";

                            if (returnedLine.Contains("!"))
                                user = returnedLine.Substring(1, returnedLine.IndexOf("!") - 1);

                            string message = returnedLine.Substring(
                                returnedLine.IndexOf("PRIVMSG")
                                + "PRIVMSG".Length + 1 + Channel.Length + 3).Trim();

                            OnTwitchChatUserMessage?.Invoke(user, message);
                        }
                    }

                    writer.Close();
                    reader.Close();
                    irc.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(RESTART_DELAY_ON_ERROR);
                BeginListening();
            }
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {

        }
    }
}
