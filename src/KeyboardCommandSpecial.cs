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

namespace Twitch2Key
{
    /// <summary>
    /// Enumerates special functions for keyboard commands.
    /// </summary>
    public enum KeyboardCommandSpecial
    {
        /// <summary>
        /// This command does nothing special
        /// </summary>
        None,
        /// <summary>
        /// Toggles the admin (only admins can use commands) mode OFF
        /// </summary>
        AdminModeOff,
        /// <summary>
        /// Toggles the admin (only admins can use commands) mode ON
        /// </summary>
        AdminModeOn,
        /// <summary>
        /// This command sets the team user who called it belongs to, if he/she doesn't belong to any team yet
        /// </summary>
        SetTeam,
        /// <summary>
        /// This command sets the team user who called it belongs to, event if he/she already belongs to a team (old value is overwritten)
        /// </summary>
        SetTeamOverwrite,
        /// <summary>
        /// This command resets all teams (remove all players from all teams)
        /// </summary>
        ResetAllTeams
    }
}
