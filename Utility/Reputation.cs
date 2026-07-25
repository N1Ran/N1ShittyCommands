using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Fluent;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.Utils;
using VRage.Game;
using VRage.Game.ModAPI;

namespace N1ShittyCommands.Utility
{
	public static class Reputation
	{
		[ReflectedGetter(Name = "m_relationsBetweenFactions", Type = typeof(MyFactionCollection))]
		private static Func<MyFactionCollection, Dictionary<MyFactionCollection.MyRelatablePair, Tuple<MyRelationsBetweenFactions, int>>> _relationsGet;
		[ReflectedGetter(Name = "m_relationsBetweenPlayersAndFactions", Type = typeof(MyFactionCollection))]
		private static Func<MyFactionCollection, Dictionary<MyFactionCollection.MyRelatablePair, Tuple<MyRelationsBetweenFactions, int>>> _playerRelationsGet;

		public static int CleanupReputations()
		{
			var collection = _relationsGet(MySession.Static.Factions);
			var collection2 = _playerRelationsGet(MySession.Static.Factions);


			var validIdentities = new HashSet<long>();

			//find all identities owning a block
			foreach (var entity in MyEntities.GetEntities())
			{
                if (!(entity is MyCubeGrid grid))
                    continue;
                validIdentities.UnionWith(grid.SmallOwners);
			}


			//find online identities
			foreach (var online in MySession.Static.Players.GetOnlinePlayers())
			{
				validIdentities.Add(online.Identity.IdentityId);
			}

			foreach (var identity in MySession.Static.Players.GetAllIdentities().ToList())
			{
				if (MySession.Static.Players.IdentityIsNpc(identity.IdentityId))
				{
					validIdentities.Add(identity.IdentityId);
				}
			}

			//Add Factions with at least one member to valid identities
			foreach (var faction in MySession.Static.Factions.Factions.Where(x => x.Value.Members.Count > 0))
			{
				validIdentities.Add(faction.Key);
			}

			//might not be necessary, but just in case
			validIdentities.Remove(0);
			var result = 0;

			var collection0List = collection.Keys.ToList();
			var collection1List = collection2.Keys.ToList();

			foreach (var pair in collection0List)
			{
				if (validIdentities.Contains(pair.RelateeId1) && validIdentities.Contains(pair.RelateeId2))
					continue;
				collection.Remove(pair);
			}

			foreach (var pair in collection1List)
			{
				if (validIdentities.Contains(pair.RelateeId1) && validIdentities.Contains(pair.RelateeId2))
					continue;
				collection2.Remove(pair);
			}


			return result;
		}

		public static int ResetPlayerReputation(long playerId)
		{
			var relationCollection = _relationsGet(MySession.Static.Factions);
			var playerRelationCollection = _playerRelationsGet(MySession.Static.Factions);
			var playerFaction = MySession.Static.Factions.TryGetPlayerFaction(playerId);
			var result = 0;

			var relationCollectionList = relationCollection.Keys.ToList();
			var playerRelationCollectionList = playerRelationCollection.Keys.ToList();

			foreach (var pair in relationCollectionList)
			{
				if (pair.RelateeId1 == playerFaction.FactionId || pair.RelateeId2 == playerFaction.FactionId)
				{
					relationCollection.Remove(pair);
					result++;
				}
			}

			foreach (var pair in playerRelationCollectionList)
			{
				if (pair.RelateeId1 == playerId || pair.RelateeId2 == playerId)
				{
					playerRelationCollection.Remove(pair);
					result++;
				}
			}
			SetNPCDefaultRelationAndReputation(playerId);
			return result;
		}

		public static void SetNPCDefaultRelationAndReputation(long playerId)
		{
			var npcFactions = MySession.Static.Factions.GetNpcFactions();
			var playerFaction = MySession.Static.Factions.TryGetPlayerFaction(playerId);
			if (npcFactions == null || npcFactions.Count == 0)
			{
				Core.Log.Warn($"No NPC factions found. Cannot set default reputation for player {playerId}.");
				return;	
			}

			foreach (var faction in npcFactions)
			{
				if (faction?.StaticReputation == false) continue;
				var statingReputation = (int)faction.StartingReputation;
				MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(playerId, faction.FactionId, statingReputation, ReputationChangeReason.None);
				if (playerFaction == null)
					continue;
				MySession.Static.Factions.SetReputationBetweenFactions(playerFaction.FactionId, faction.FactionId, statingReputation);
			}
		}

		public static void ResetPlayerToFaction(long playerId, long factionId)
		{
			var playerRelationCollection = _playerRelationsGet(MySession.Static.Factions);
			var relationCollection = _relationsGet(MySession.Static.Factions);

			MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(playerId, factionId, 0,ReputationChangeReason.Admin);
		}

	}
}
