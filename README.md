# Twitch2Key

**This is the project's source code repository. If you're not interested in the source code, binaries can be found at https://akaagar.itch.io/twitch2key**

A C# command-line tool which listens to a Twitch channel's live chat and simulates keyboard presses when certain messages are posted. Can be used to create "Twitch plays XXX" streams where commands from the chat ("forward", "back", etc.) are turned into game inputs.

**Features**

* Easy to setup and configure, no Twitch plugin required
* Can turn any Twitch chat message to any key press
* Can give "admin" powers to some users so only them can press certain keys
* Can create teams of users, so two groups of users in the chat can fight each other in a versus-fighting game, or be assigned different task in a complex game with many keys

**Please read /bin/Release/README.html for additional information**

Created by Ambroise Garel (@akaAgar), released under the GNU General Public License.

Requires the following libraries (DLLs are provided in /bin/Release/) :
* INIPlusPlus, by myself: https://github.com/akaAgar/ini-plus-plus
* InputManager, by shynet : https://www.codeproject.com/Articles/117657/InputManager-library-Track-user-input-and-simulate
