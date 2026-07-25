using System;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.SessionComponents;
using Torch.API.Session;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Utils;
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ModAPI;
using N1ShittyCommands.Utility;

namespace N1ShittyCommands.Commands
{
    [Category("reputation")]
    public class Reputation : CommandModule
    {

		[Command("reset", "resets all reputations pairs to default")]
		[Permission(MyPromoteLevel.Moderator)]
		public void ResetRep()
        {
           var count = Utility.Reputation.CleanupReputations();
            if (count == 0)
            {
                Context.Respond("No reputations needed to be cleaned up.");
                return;
            }

			Context.Respond($"Cleaned up {count} reputaion.");
        }

        [Command("resetplayer", "resets player's NPC reputation pair to default")]
		[Permission(MyPromoteLevel.Moderator)]
		public void ResetPlayer(string playerName = null)
        {
            Utilities.TryGetPlayerByNameOrId(playerName, out var playerIdentity);
            if (playerName != null && playerIdentity == null)
            {
                Context.Respond($"Player {playerName} not found.");
                return;
            }
            var count = Utility.Reputation.ResetPlayerReputation(playerIdentity.IdentityId);
			Context.Respond($"Reset {playerIdentity.DisplayName}'s reputation with {count} factions.");
		}
    }
}