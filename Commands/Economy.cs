using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using Sandbox.ModAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Managers;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ModAPI;
using VRageMath;

namespace N1ShittyCommands.Commands
{
    [Category("economy")]
    public class Economy : CommandModule
    {

        [ReflectedGetter(Name = "EconomyDefinition", Type = typeof(MySessionComponentEconomy))]
        private static Func<MySessionComponentEconomy, MySessionComponentEconomyDefinition> _econDef;

        [ReflectedSetter(Name = nameof(MySessionComponentEconomy.GenerateFactionsOnStart), Type = typeof(MySessionComponentEconomy))]
        private static Action<MySessionComponentEconomy, bool> _genFacOnStart;


        [Command("reset faction", "removes all current NPC trade factions and their station and allow new factions/stations to spawn")]
        [Permission(MyPromoteLevel.Admin)]
        public void EconomyFactionReset(string factionTag = null)
        {
            var meh = MySession.Static.GetComponent<MySessionComponentEconomy>();

            if (meh == null)
            {
                Context.Respond("Can't find MySessionComponentEconomy");
                return;
            }

            var factionList = MySession.Static.Factions.Select(x => x.Value).ToList();
            if (!string.IsNullOrEmpty(factionTag))
                factionList.RemoveAll(x => !x.Tag.Equals(factionTag, StringComparison.OrdinalIgnoreCase));
            var removeList = new List<MyFaction>(factionList.Where(faction => faction.IsEveryoneNpc() && faction.FactionType != MyFactionTypes.PlayerMade && faction.FactionType != MyFactionTypes.None));

            var removedFactions = removeList.Count;
            RemoveStations(meh,removeList,true);

            CleanupReputations();

            _genFacOnStart.Invoke(meh, true);
            meh.BeforeStart();
            Context.Respond($"Cleared {removedFactions} factions \n Faction reset complete");
        }


        [Command("reset station", "removes all NPC trade stations and reset faction to spawn new ones.  This will also increase NPC factions if maxfactioncount is set to 0 in world setting")]
        [Permission(MyPromoteLevel.Admin)]
        public void StationReset(string factionTag = null)
        {
            var meh = MySession.Static.GetComponent<MySessionComponentEconomy>();

            if (meh == null)
            {
                Context.Respond("Can't find MySessionComponentEconomy");
                return;
            }

            var factionList = MySession.Static.Factions.Select(x => x.Value).ToList();
            if (!string.IsNullOrEmpty(factionTag))
                factionList.RemoveAll(x => x.Tag.Equals(factionTag, StringComparison.OrdinalIgnoreCase));

            var removedStations = 0;
            var removeStationList = new List<MyFaction>(factionList.Where(faction => faction.IsEveryoneNpc() && faction.FactionType != MyFactionTypes.PlayerMade && faction.FactionType != MyFactionTypes.None));

            removedStations = RemoveStations(meh, removeStationList);

            _genFacOnStart.Invoke(meh, true);
            meh.BeforeStart();
            Task.Run(() =>
            {
                Thread.Sleep(100);
                var newFactionCreated = new List<MyFaction>();
                foreach (var (id, faction) in MySession.Static.Factions)
                {
                    if (removeStationList.Contains(faction)) continue;
                    newFactionCreated.Add(faction);
                }
                foreach (var faction in newFactionCreated)
                {
                    RemoveFaction(faction);
                }
                
                
            });
            Context.Respond($"Cleared {removedStations} stations \n Station reset complete");

        }


        private static int RemoveStations(MySessionComponentEconomy meh,List<MyFaction> factionList, bool deleteFaction = false)
        {
            var removedStations = 0;
            var removeStationList = new List<MyFaction>();
            var deletedStationPositions = new List<MyOrientedBoundingBoxD>();

            foreach (var faction in factionList)
            {
                
                foreach (var station in faction.Stations)
                {
                    MyEntities.TryGetEntityById(station.StationEntityId, out var entity);
                    if (entity != null)
                    {
                        deletedStationPositions.Add(new MyOrientedBoundingBoxD(entity.PositionComp.LocalAABB, entity.PositionComp.WorldMatrixRef));
                        entity.Close();
                    }

                    meh.RemoveStationGrid(station.Id);
                    meh.RemoveStationGrid(station.StationEntityId);
                    removeStationList.Add(faction);
                    removedStations++;
                }
            }

            RemoveStation(removeStationList);
            CleanupReputations();

            var safeZones = new HashSet<MySafeZone>(MySessionComponentSafeZones.SafeZones);
            var delSafeZones = new List<MySafeZone>();
            foreach (var zone in safeZones)
            {
                var zonePosition =
                    new MyOrientedBoundingBoxD(zone.PositionComp.LocalAABB, zone.PositionComp.WorldMatrixRef);

                foreach (var position in deletedStationPositions)
                {
                    if (!position.Intersects(ref zonePosition)) continue;
                    delSafeZones.Add(zone);
                    break;
                }
            }

            foreach (var zone in delSafeZones)
            {
                zone.Close();
            }

            if (!deleteFaction) return removedStations;

            foreach (var faction in factionList)
            {
                RemoveFaction(faction);
            }
            return removedStations;
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

        [ReflectedGetter(Name = "m_stations", Type = typeof(MyFaction))]
        private static Func<MyFaction, Dictionary<long, MyStation>> _stations;

        private static void RemoveStation(List<MyFaction> removeStationList)
        {
            var factionList = MySession.Static.Factions.Select(x => x.Value).ToList();

            foreach (var faction in factionList)
            { 
                if (!removeStationList.Contains(faction)) continue;
                 _stations(faction).Clear();
            }

        }

    }
}