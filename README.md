# Twitch2Key

A C# command-line tool which listens to a Twitch channel's live chat and simulates keyboard presses when certain messages are posted. Can be used to create "Twitch plays XXX" streams where commands from the chat ("forward", "back", etc.) will be turned into game inputs.

To be able to connect to the Twitch chat IRC server, you'll have to generate a Twitch IRC password (https://twitchapps.com/tmi/) and copy it into the Twitch2Key.ini file.

Created by Ambroise Garel (@akaAgar), released under the GNU General Public License.

Requires the following libraries (DLLs are provided) :
* INIPlusPlus: https://github.com/akaAgar/ini-plus-plus
* InputManager : https://www.codeproject.com/Articles/117657/InputManager-library-Track-user-input-and-simulate
