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
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ModAPI;

namespace N1ShittyCommands.Commands
{
    [Category("economy")]
    public class Economy : CommandModule
    {

        [ReflectedGetter(Name = "EconomyDefinition", Type = typeof(MySessionComponentEconomy))]
        private static Func<MySessionComponentEconomy, MySessionComponentEconomyDefinition> _econDef;

        [ReflectedSetter(Name = nameof(MySessionComponentEconomy.GenerateFactionsOnStart), Type = typeof(MySessionComponentEconomy))]
        private static Action<MySessionComponentEconomy, bool> _genFacOnStart;

        [ReflectedGetter(Name = nameof(MySessionComponentEconomy.GenerateFactionsOnStart), Type = typeof(MySessionComponentEconomy))]
        private static Func<MySessionComponentEconomy, bool> _wat;

        [Command("reset tradestations")]
        [Permission(MyPromoteLevel.Admin)]
        public void EconomyReset()
        {
            var meh = MySession.Static.GetComponent<MySessionComponentEconomy>();
            
            if (meh == null)
            {
                Context.Respond("Can't find MySessionComponentEconomy");
                return;
            }

            var factionList = MySession.Static.Factions.Select(x => x.Value).ToList();

            foreach (var faction in factionList)
            {
                if (!faction.IsEveryoneNpc() || faction.FactionType == MyFactionTypes.PlayerMade || faction.FactionType == MyFactionTypes.None) continue;
                foreach (var station in faction.Stations)
                {
                    MyEntities.TryGetEntityById(station.StationEntityId, out var entity);
                    if (entity is MySafeZone || entity is MyCubeGrid)
                        entity.Close();
                    meh.RemoveStationGrid(station.Id);
                    meh.RemoveStationGrid(station.StationEntityId);

                }
                RemoveFaction(faction);
            }

            CleanupReputations();

            _genFacOnStart.Invoke(meh,true);
            _genFacOnStart.Invoke(meh,true);
            meh.BeforeStart();
            var safeZones = new HashSet<MySafeZone>(MyEntities.GetEntities().OfType<MySafeZone>());

            var removed = 0;
            foreach (var safeZone in safeZones)
            {
                if (!safeZone.IsEmpty()) continue;
                safeZone.Close();
                removed++;
            }

            Context.Respond($"Cleared {removed} safezone");

            Context.Respond("Economy reset complete");
        }


        private static MethodInfo _factionChangeSuccessInfo = typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess", BindingFlags.NonPublic | BindingFlags.Static);

        private static void RemoveFaction(MyFaction faction)
        {
            //bypass the check that says the server doesn't have permission to delete factions
            //_applyFactionState(MySession.Static.Factions, MyFactionStateChange.RemoveFaction, faction.FactionId, faction.FactionId, 0L, 0L);
            //MyMultiplayer.RaiseStaticEvent(s =>
            //        (Action<MyFactionStateChange, long, long, long, long>) Delegate.CreateDelegate(typeof(Action<MyFactionStateChange, long, long, long, long>), _factionStateChangeReq),
            //    MyFactionStateChange.RemoveFaction, faction.FactionId, faction.FactionId, faction.FounderId, faction.FounderId);
            NetworkManager.RaiseStaticEvent(_factionChangeSuccessInfo, MyFactionStateChange.RemoveFaction, faction.FactionId, faction.FactionId, 0L, 0L);
            MyFactionCollection.RemoveFaction(faction.FactionId);

            if(!MyAPIGateway.Session.Factions.FactionTagExists(faction.Tag)) return;
            MyAPIGateway.Session.Factions.RemoveFaction(faction.FactionId); //Added to remove factions that got through the crack
        }
               
        [ReflectedGetter(Name = "m_relationsBetweenFactions", Type = typeof(MyFactionCollection))]
        private static Func<MyFactionCollection, Dictionary<MyFactionCollection.MyRelatablePair, Tuple<MyRelationsBetweenFactions, int>>> _relationsGet;
        [ReflectedGetter(Name = "m_relationsBetweenPlayersAndFactions", Type = typeof(MyFactionCollection))]
        private static Func<MyFactionCollection, Dictionary<MyFactionCollection.MyRelatablePair, Tuple<MyRelationsBetweenFactions, int>>> _playerRelationsGet;

        private static int CleanupReputations()
        {
            var collection = _relationsGet(MySession.Static.Factions);
            var collection2 = _playerRelationsGet(MySession.Static.Factions);


            var validIdentities = new HashSet<long>();

            //find all identities owning a block
            foreach (var entity in MyEntities.GetEntities())
            {
                var grid = entity as MyCubeGrid;
                if (grid == null)
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
            foreach (var faction in MySession.Static.Factions.Factions.Where(x=>x.Value.Members.Count > 0))
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

    }
}