using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Managers;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Components;


namespace N1ShittyCommands.Commands
{
    
    [Category("player")]
    public class PlayerModule : CommandModule
    {
        [Command("reset container", "resets playercontainerdata so Unknown Signals can be re-enabled")]
        public void ResetContainer()
        {
            if (!(MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().GetObjectBuilder() is MyObjectBuilder_SessionComponentContainerDropSystem containerDropSystem))
            {
                Context.Respond("DropSys is null");
                return;
            }

            var result = 0;
            foreach (var data in containerDropSystem.PlayerData)
            {
                if (data.Active || MyEntities.TryGetEntityById(data.ContainerId, out _)) continue;
                data.Active = true;
                result++;
            }
            Context.Respond($"Reset {result} player container data");
        }
    }
    
}

