using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks.SafeZone;
using Torch.Commands;
using VRage.Game.ModAPI;

namespace N1ShittyCommands.Commands
{
    [Category("safezone")]
    public class SafeZone : CommandModule
    {
        [Command("list", "Lists current safe zones")]
        public void ListSafeZone()
        {
            var safeZones = new HashSet<MySafeZone>(MyEntities.GetEntities().OfType<MySafeZone>());

            if (safeZones.Count == 0)
            {
                Context.Respond("No safe zone found");
                return;
            }
            var safeZoneList = string.Join("\n", safeZones.Select(x => $"{x.DisplayName}({x.EntityId})"));

            Context.Respond(safeZoneList);
        }

        [Command("locate", "Lists current safe zones")]
        public void ViewSafeZone()
        {

            if (Context.Player == null || Context.Player.SteamUserId == 0)
            {
                Context.Respond("command cannot be use in this method.");
                return;
            }
            var safeZones = new HashSet<MySafeZone>(MySessionComponentSafeZones.SafeZones);

            
            if (safeZones.Count == 0)
            {
                Context.Respond("No safe zone found");
                return;
            }

            foreach (var safeZone in safeZones)
            {
                var gps = MyAPIGateway.Session?.GPS.Create(safeZone.DisplayName, ($"{safeZone.EntityId} "), safeZone.LocationForHudMarker, true);

                MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gps);
                
            }


        }

        [Command("clear", "removes empty safezones")]
        public void RemoveSafeZones()
        {
            var safeZones = new HashSet<MySafeZone>(MySessionComponentSafeZones.SafeZones);

            var removed = 0;
            foreach (var safeZone in safeZones)
            {
                if (!safeZone.IsEmpty()) continue;
                safeZone.Close();
                removed++;
            }

            Context.Respond($"Cleared {removed} safezone");
        }

        [Command("delete", "deletes safe zone by matching name")]
        public void DeleteSafeZone(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Context.Respond("Provide a name of target safe zone to delete");
                return;
            }
            var safeZones = new HashSet<MySafeZone>(MySessionComponentSafeZones.SafeZones);
            if (long.TryParse(name, out var id))
            {
                var target = safeZones.FirstOrDefault(x => x.EntityId == id);
                if (target == null)
                {
                    Context.Respond($"no safe zone containing Id {name} found");
                    return;
                }
                target.Close();
                Context.Respond("Removed 1 safe zone");
                return;
            }
            var removed = 0;
            foreach (var safeZone in safeZones)
            {
                if (!safeZone.DisplayName.Contains(name,StringComparison.OrdinalIgnoreCase)) continue;
                safeZone.Close();
                removed++;
            }

            if (removed == 0)
            {
                Context.Respond($"no safe zone containing name {name} found");
                return;
            }

            Context.Respond($"Removed {removed} safe zones");
        }


    }
}