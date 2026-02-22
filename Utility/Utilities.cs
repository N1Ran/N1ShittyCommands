using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.API.Managers;
using Torch.Managers.ChatManager;
using Torch.Utils;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Network;
using VRage.Utils;
using VRageMath;

namespace N1ShittyCommands.Utility
{
	public static class Utilities
	{
		[ReflectedStaticMethod(Type = typeof(MyCubeBuilder), Name = "SpawnGridReply", OverrideTypes = new[] { typeof(bool), typeof(ulong) })]
		private static Action<bool, ulong> _spawnGridReply;


		public static string GetMessage(string msg, List<string> blockList, string limitName, int count = 1)
		{
			var returnMsg = "";


			returnMsg = msg.Replace("{BC}", count.ToString()).Replace("{L}", limitName).Replace("{BL}", string.Join("\n", blockList));


			return returnMsg;
		}

		public static string GetPlayerNameFromId(long id)
		{
			var playerName = "";
			if (id == 0) return playerName;
			var identity = MySession.Static.Players.TryGetIdentity(id);
			playerName = identity?.DisplayName;
			return playerName;
		}
		public static string GetPlayerNameFromSteamId(ulong steamId)
		{
			var pid = MySession.Static.Players.TryGetIdentityId(steamId);
			if (pid == 0)
				return "";
			var id = MySession.Static.Players.TryGetIdentity(pid);
			return id?.DisplayName;
		}

		public static MyIdentity GetPlayerIdentityFromSteamId(ulong steamId)
		{
			MyIdentity identity = MySession.Static.Players.TryGetIdentity(MySession.Static.Players.TryGetIdentityId(steamId, 0));
			return identity;
		}


		public static long GetPlayerIdFromSteamId(ulong steamId)
		{
			return MySession.Static.Players.TryGetIdentityId(steamId);
		}

		public static ulong GetSteamIdFromPlayerId(long playerId)
		{
			return MySession.Static.Players.TryGetSteamId(playerId);
		}

		public static void SendFailSound(ulong target)
		{
			if (target == 0) return;
			_spawnGridReply(false, target);
		}

		/// <summary>
		/// Gets the entity with name or ID given
		/// </summary>
		/// <param name="nameOrId"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static bool TryGetEntityByNameOrId(string nameOrId, out IMyEntity entity)
		{

			if (long.TryParse(nameOrId, out var id))
				return MyAPIGateway.Entities.TryGetEntityById(id, out entity);


			foreach (var ent in MyEntities.GetEntities())
			{
				if (string.IsNullOrEmpty(ent.DisplayName)) continue;
				if (!ent.DisplayName.Equals(nameOrId)) continue;
				entity = ent;
				return true;
			}

			entity = null;
			return false;
		}

		public static bool TryGetPlayerByNameOrId(string nameOrId, out MyIdentity identity)
		{
			identity = null;
			if (ulong.TryParse(nameOrId, out var steamId))
			{
				var id0 = MySession.Static.Players.TryGetIdentityId(steamId);
				identity = MySession.Static.Players.TryGetIdentity(id0);

				return identity != null;
			}

			if (long.TryParse(nameOrId, out var id1))
			{
				identity = MySession.Static.Players.TryGetIdentity(id1);

				return identity != null;
			}

			foreach (var id3 in MySession.Static.Players.GetAllIdentities())
			{
				if (string.IsNullOrEmpty(id3.DisplayName) || !id3.DisplayName.Equals(nameOrId)) continue;
				identity = id3;
				return identity != null;
			}

			identity = null;
			return false;

		}



		public static void ValidationFailed(ulong id = 0)
		{
			var user = id > 0 ? id : MyEventContext.Current.Sender.Value;
			if (user == 0) return;
			((MyMultiplayerServerBase)MyMultiplayer.Static).ValidationFailed(user);
		}

		public static MyCubeBlockDefinition GetDefinition(MyObjectBuilder_CubeBlock block)
		{
			return MyDefinitionManager.Static.GetCubeBlockDefinition(block);
		}


		public static int ClearSafeZones()
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

				if (!safeZone.IsEmpty() || (occupiedSpace.Count > 0 && occupiedSpace.Any(x => x.Intersects(ref zonePosition)))) continue;
				safeZone.Close();
				removed++;
			}
			return removed;
		}



		}
	}
