using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Library.Utils;

namespace N1ShittyCommands.Commands
{
    #if DEBUG
    [Category("settings")]
    public class Settings : CommandModule
    {
        [Command("view", "Lists current server settings")]
        [Permission(MyPromoteLevel.None)]
        public void ListSettings()
        {
            var info = new StringBuilder();
            var list = new List<string>();
            var yes = GetResource("Yes");
            var no = GetResource("No");

            info.AppendFormat("{0}: {1}\r\n", GetResource("Name"), MyAPIGateway.Session.Name);
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_Description"), MyAPIGateway.Session.Description);
            info.AppendFormat("{0}: {1:%d} days {1:hh\\:mm\\:ss}\r\n", "Session Time", MyAPIGateway.Session.ElapsedPlayTime); // This is the local session, not the server.
            info.AppendFormat("{0}: {1:%d} days {1:hh\\:mm\\:ss}\r\n", "Game Time", MySession.Static.ElapsedGameTime); // Total game time. Still in debate about sync with the server.

            info.AppendFormat("\r\n");

            var gameMode = "Unknown";
            switch (MyAPIGateway.Session.SessionSettings.GameMode)
            {
                case MyGameModeEnum.Creative: gameMode = GetResource("WorldSettings_GameModeCreative"); break;
                case MyGameModeEnum.Survival: gameMode = GetResource("WorldSettings_GameModeSurvival"); break;
            }
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_GameMode"), gameMode);


            var onlineMode = "Unknown";
            switch (MyAPIGateway.Session.OnlineMode)
            {
                case MyOnlineModeEnum.FRIENDS: onlineMode = GetResource("WorldSettings_OnlineModeFriends"); break;
                case MyOnlineModeEnum.OFFLINE: onlineMode = GetResource("WorldSettings_OnlineModeOffline"); break;
                case MyOnlineModeEnum.PRIVATE: onlineMode = GetResource("WorldSettings_OnlineModePrivate"); break;
                case MyOnlineModeEnum.PUBLIC: onlineMode = GetResource("WorldSettings_OnlineModePublic"); break;
            }
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_OnlineMode"), onlineMode);
            info.AppendFormat("{0}: {1}\r\n", GetResource("MaxPlayers"), MyAPIGateway.Session.MaxPlayers);

            var environmentHostility = "Unknown";
            switch (MyAPIGateway.Session.EnvironmentHostility)
            {
                case MyEnvironmentHostilityEnum.CATACLYSM: environmentHostility = GetResource("WorldSettings_EnvironmentHostilityCataclysm"); break;
                case MyEnvironmentHostilityEnum.CATACLYSM_UNREAL: environmentHostility = GetResource("WorldSettings_EnvironmentHostilityCataclysmUnreal"); break;
                case MyEnvironmentHostilityEnum.NORMAL: environmentHostility = GetResource("WorldSettings_EnvironmentHostilityNormal"); break;
                case MyEnvironmentHostilityEnum.SAFE: environmentHostility = GetResource("WorldSettings_EnvironmentHostilitySafe"); break;
            }
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_EnvironmentHostility"), environmentHostility);
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_AutoSave"), MyAPIGateway.Session.AutoSaveInMinutes > 0 ? yes : no);
            info.AppendFormat("Auto Save In Minutes: {0}\r\n", MyAPIGateway.Session.AutoSaveInMinutes);
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_ScenarioEditMode"), MyAPIGateway.Session.SessionSettings.ScenarioEditMode ? yes : no);

            info.AppendFormat("\r\n");

            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_InventorySize"), MyAPIGateway.Session.InventoryMultiplier);
            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_AssemblerEfficiency"), MyAPIGateway.Session.AssemblerEfficiencyMultiplier);
            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_RefinerySpeed"), MyAPIGateway.Session.RefinerySpeedMultiplier);
            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_WelderSpeed"), MyAPIGateway.Session.WelderSpeedMultiplier);
            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_GrinderSpeed"), MyAPIGateway.Session.GrinderSpeedMultiplier);
            info.AppendFormat("{0}: {1:##,##0}\r\n", GetResource("MaxFloatingObjects"), MyAPIGateway.Session.MaxFloatingObjects);
            info.AppendFormat("{0}: {1:##,##0}\r\n", GetResource("MaxBackupSaves"), MyAPIGateway.Session.MaxBackupSaves);
            
            if (MyAPIGateway.Session.SessionSettings.WorldSizeKm == 0)
                info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_LimitWorldSize"), GetResource("WorldSettings_WorldSizeUnlimited"));
            else
                info.AppendFormat("{0}: {1:##,##0} Km\r\n", GetResource("WorldSettings_LimitWorldSize"), MyAPIGateway.Session.SessionSettings.WorldSizeKm);
            info.AppendFormat("{0}: x {1}\r\n", GetResource("WorldSettings_RespawnShipCooldown"), MyAPIGateway.Session.SessionSettings.SpawnShipTimeMultiplier);
            info.AppendFormat("{0}: {1:##,###} m\r\n", GetResource("WorldSettings_ViewDistance"), MyAPIGateway.Session.SessionSettings.ViewDistance);
            info.AppendFormat("{0}: {1:##,###} m\r\n", GetResource("WorldSettings_SyncDistance"), MyAPIGateway.Session.SessionSettings.SyncDistance);
            info.AppendFormat("{0}: {1}\r\n", GetResource("WorldSettings_EnableSunRotation"), MyAPIGateway.Session.SessionSettings.EnableSunRotation ? yes : no);
            info.AppendFormat("{0}: {1:N} minutes\r\n", GetResource("SunRotationPeriod"), MyAPIGateway.Session.SessionSettings.SunRotationIntervalMinutes);
            if (MySession.Static.Settings.AFKTimeountMin > 0)
                info.AppendFormat("{0}: {1:N} minutes\r\n", GetResource("AFKTimeoutMin"), MyAPIGateway.Session.SessionSettings.AFKTimeountMin);

            info.AppendFormat("\r\n");

            list.Add(
                $"{GetResource("WorldSettings_AutoHealing")}: {(MyAPIGateway.Session.AutoHealing ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableCopyPaste")}: {(MyAPIGateway.Session.EnableCopyPaste ? yes : no)}");
            list.Add($"{GetResource("WorldSettings_EnableEconomy")}: {(MySession.Static.Settings.EnableEconomy ? yes : no)}");
            list.Add($"{GetResource("WorldSettings_EnableBountyContracts")}: {(MySession.Static.Settings.EnableBountyContracts ? yes : no)}");
            list.Add($"{GetResource("WorldSettings_EnableContainerDrops")}: {(MySession.Static.Settings.EnableContainerDrops ? yes : no)}");
            list.Add($"{GetResource("MinDropContainerRespawnTime")}: {MyAPIGateway.Session.SessionSettings.MinDropContainerRespawnTime:N} minutes\r\n");
            list.Add(
                $"{GetResource("MaxDropContainerRespawnTime")}: {MyAPIGateway.Session.SessionSettings.MaxDropContainerRespawnTime:N} minutes\r\n");
            list.Add(
                $"{GetResource("WorldSettings_EnableWeapons")}: {(MyAPIGateway.Session.WeaponsEnabled ? yes : no)}");
            list.Add(
                $"{GetResource("World_Settings_EnableOxygen")}: {(MyAPIGateway.Session.SessionSettings.EnableOxygen ? yes : no)}");
            list.Add(
                $"{GetResource("World_Settings_EnableOxygenPressurization")}: {(MyAPIGateway.Session.SessionSettings.EnableOxygenPressurization ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableRespawnShips")}: {(MyAPIGateway.Session.SessionSettings.EnableRespawnShips ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableJetpack")}: {(MyAPIGateway.Session.SessionSettings.EnableJetpack ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableVoxelDestruction")}: {(MyAPIGateway.Session.SessionSettings.EnableVoxelDestruction ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_RespawnShipDelete")}: {(MyAPIGateway.Session.SessionSettings.RespawnShipDelete ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_ShowPlayerNamesOnHud")}: {(MyAPIGateway.Session.SessionSettings.ShowPlayerNamesOnHud ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_ThrusterDamage")}: {(MyAPIGateway.Session.SessionSettings.ThrusterDamage ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableCargoShips")}: {(MyAPIGateway.Session.SessionSettings.CargoShipsEnabled ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableIngameScripts")}: {(MyAPIGateway.Session.SessionSettings.EnableIngameScripts ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_Enable3rdPersonCamera")}: {(MyAPIGateway.Session.SessionSettings.Enable3rdPersonView ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_SpawnWithTools")}: {(MyAPIGateway.Session.SessionSettings.SpawnWithTools ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableDrones")}: {(MyAPIGateway.Session.SessionSettings.EnableDrones ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableSpectator")}: {(MyAPIGateway.Session.SessionSettings.EnableSpectator ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_PermanentDeath")}: {(MyAPIGateway.Session.SessionSettings.PermanentDeath.HasValue ? (MyAPIGateway.Session.SessionSettings.PermanentDeath.Value ? yes : no) : no)}");
            list.Add(
                $"{GetResource("WorldSettings_DestructibleBlocks")}: {(MyAPIGateway.Session.SessionSettings.DestructibleBlocks ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableToolShake")}: {(MyAPIGateway.Session.SessionSettings.EnableToolShake ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_Encounters")}: {(MyAPIGateway.Session.SessionSettings.EnableEncounters ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableConvertToStation")}: {(MyAPIGateway.Session.SessionSettings.EnableConvertToStation ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableWolfs")}: {(MyAPIGateway.Session.SessionSettings.EnableWolfs ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_EnableSpiders")}: {(MyAPIGateway.Session.SessionSettings.EnableSpiders ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_StartInRespawnScreen")}: {(MyAPIGateway.Session.SessionSettings.StartInRespawnScreen ? yes : no)}");
            list.Add($"Maximum Drones: {MyAPIGateway.Session.SessionSettings.MaxDrones}");
            list.Add(
                $"{GetResource("WorldSettings_SoundMode") + " " + GetResource("WorldSettings_RealisticSound")}: {(MyAPIGateway.Session.SessionSettings.RealisticSound ? yes : no)}");
            list.Add(
                $"{GetResource("WorldSettings_StationVoxelSupport")}: {(MyAPIGateway.Session.SessionSettings.StationVoxelSupport ? yes : no)}");

            list.Add(
                $"{GetResource("WorldSettings_WeatherSystem")}: {(MyAPIGateway.Session.SessionSettings.WeatherSystem ? yes : no)}");

            // add the remaining settings as a sorted list (according to the localizaed labels).
            foreach (var str in list.OrderBy(e => e))
                info.AppendLine(str);

            info.AppendFormat("\r\n");

            info.AppendFormat("{0}: {1}\r\n", GetResource("Block Limits Enabled"),
                MySession.Static.Settings.BlockLimitsEnabled.ToString());
            if (MySession.Static.BlockTypeLimits.Any())
            {
                foreach (var limit in MySession.Static.BlockTypeLimits)
                {
                    info.AppendFormat("{0}: {1}\r\n", GetResource(limit.Key), limit.Value);
                }
            }

            info.AppendFormat("\r\n");

            var mods = MyAPIGateway.Session.Mods;
            info.AppendFormat("{0}: {1:#,###0}\r\n", GetResource("WorldSettings_Mods"), mods.Count);
            foreach (var mod in mods.OrderBy(e => e.FriendlyName))
                info.AppendFormat("#{0} : '{1}'\r\n", mod.PublishedFileId, mod.FriendlyName);
            if (Context.Player == null)
                Context.Respond(info.ToString());
            else if (Context?.Player?.SteamUserId > 0)
            {
                ModCommunication.SendMessageTo(new DialogMessage("Game Settings", "", info.ToString()), Context.Player.SteamUserId);
            }
            //MyAPIGateway.Utilities.ShowMissionScreen("Game Settings", "", " ", info.ToString());
        }

        #if DEBUG
        [Command("change")]
        public void ChangeSettings()
        {
            var settings = MySession.Static.Settings;

            
            foreach (var arg in Context.Args)
            {
                if (arg.StartsWith("-gamemode"))
                {
                    if (!Enum.TryParse(arg.Replace("-gamemode=", ""),true, out MyGameModeEnum mode))
                    {
                        Context.Respond("Mode not possible, check your case and spelling");
                    }

                    settings.GameMode = mode;
                    Context.Respond($"GameMode set to {settings.GameMode}");
                }

                if (arg.StartsWith("-destructibleblocks"))
                {
                    if (!bool.Parse(arg.Replace("-gamemode=", "")))
                    {
                        Context.Respond("Mode not possible, check your case and spelling");
                    }

                    settings.DestructibleBlocks = bool.Parse(arg.Replace("-gamemode=", ""));
                    Context.Respond($"GameMode set to {settings.GameMode}");
                }

                if (arg.StartsWith("-destructibleblocks"))
                {
                    if (!bool.Parse(arg.Replace("-gamemode=", "")))
                    {
                        Context.Respond("Mode not possible, check your case and spelling");
                    }

                    settings.DestructibleBlocks = bool.Parse(arg.Replace("-gamemode=", ""));

                    Context.Respond($"GameMode set to {settings.GameMode}");
                }

                if (arg.StartsWith("-autosave"))
                {
                    if (!bool.Parse(arg.Replace("-autosave=", "")))
                    {
                        if (!uint.TryParse(arg.Replace("-autosave=",""), out var time))
                            Context.Respond("Mode not possible, check your case and spelling");
                        settings.AutoSaveInMinutes = time;
                        Context.Respond($"AutoSave in Minutes set to {settings.AutoSaveInMinutes}");
                    }

                    settings.AutoSave = bool.Parse(arg.Replace("-gamemode=", ""));

                    Context.Respond($"Aut0save set to {settings.AutoSave}");
                }

            }
        }
        #endif
        private static string GetResource(string stringId, params object[] args)
        {
            return args.Length == 0 ? MyTexts.GetString(stringId) : string.Format(MyTexts.GetString(stringId), args);
        }
    }
#endif
}