﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks.SafeZone;
using Torch.Commands;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using System.Text.RegularExpressions;

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

            var respond = new StringBuilder();

            respond.AppendLine($"Found {safeZones.Count}:");
            respond.Append(string.Join("\n", safeZones.Select(x => $"{x.DisplayName} - ({x.EntityId})")));
            //var safeZoneList = string.Join("\n", safeZones.Select(x => $"{x.DisplayName}({x.EntityId})"));

            Context.Respond(respond.ToString());
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
            //Blame the shittiness of this code on keen for having a shitty empty bool.
            var allGrids = new List<MyEntity>(MyEntities.GetEntities().OfType<MyCubeGrid>());
            var occupiedSpace = new List<MyOrientedBoundingBoxD>();
            foreach (var entity in allGrids)
            {
                occupiedSpace.Add(new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB,
                    entity.PositionComp.WorldMatrixRef));
            }

            var removed = 0;
            foreach (var safeZone in safeZones)
            {
                var zonePosition =
                    new MyOrientedBoundingBoxD(safeZone.PositionComp.LocalAABB, safeZone.PositionComp.WorldMatrixRef);

                if (!safeZone.IsEmpty() || (occupiedSpace.Count > 0 && occupiedSpace.Any(x=>x.Intersects(ref zonePosition))) ) continue;
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

            var _deletedList = new List<string>();

            foreach (var safeZone in safeZones)
            {
                if (!Regex.IsMatch(safeZone.DisplayName, name, RegexOptions.IgnoreCase)) continue;
                _deletedList.Add(safeZone.DisplayName);
                safeZone.Close();
                removed++;
            }

            if (removed == 0)
            {
                Context.Respond($"no safe zone containing name {name} found");
                return;
            }
            var respond = new StringBuilder();
            respond.AppendLine($"Removed {removed} safe zones");
            respond.Append(string.Join("\n", _deletedList));
            Context.Respond(respond.ToString());

        }


    }
}