﻿using Bloodstone.API;
using Bloody.Core;
using Bloody.Core.GameData.v1;
using Bloody.Core.Helper.v1;
using Bloody.Core.Models.v1;
using BloodyPoints.DB;
using BloodyPoints.Helpers;
using ProjectM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace BloodyPoints.Command
{

    [CommandGroup(name: "bloodypoint", shortHand: "bls")]
    internal class Bloodypoint
    {
        public static int WaypointLimit = 3;
        public static bool DraculaRoom = false;

        private static EntityManager entityManager = VWorld.Server.EntityManager;

        /*[Command(name: "test", shortHand: "t", adminOnly: false, usage: "", description: "")]
        public static void test(ChatCommandContext ctx)
        {
            var userModel = GameData.Users.GetUserByCharacterName(ctx.User.CharacterName.Value);

            if (userModel.Entity.Has<CurrentMapZone>())
            {
                var currentMapZone = userModel.Entity.Read<CurrentMapZone>();
                var chunWorldIndex = currentMapZone.TerrainChunk.X;
                Plugin.Logger.LogInfo($"{currentMapZone.TerrainChunk.X} {currentMapZone.TerrainChunk.Y}");
            } else
            {
                Plugin.Logger.LogInfo($"NOOOOOO");
            }
        }*/

        [Command(name: "teleport", shortHand: "tp", adminOnly: false, usage: ".使用傳點", description: "將您傳送到特定路徑點.")]
        public static void WaypoinCommand(ChatCommandContext ctx, string name)
        {
            var PlayerEntity = ctx.Event.SenderCharacterEntity;
            var SteamID = ctx.Event.User.PlatformId;
            if (Helper.IsPlayerInCombat(PlayerEntity))
            {
                throw ctx.Error("你還在戰鬥中,無法使用傳點!");
            }

            var wp = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name);

            if (wp != null)
            {
                if(!DraculaRoom)
                {
                    if (Helper.checkDracualaRoom(wp))
                    {
                        throw ctx.Error($"你不能把傳送至古拉爵的家阿!");
                    }
                }
                Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                return;
            }

            wp = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name && waypoint.Owner == SteamID);

            if (wp != null)
            {
                if (!DraculaRoom)
                {
                    if (Helper.checkDracualaRoom(wp))
                    {
                        throw ctx.Error($"你不能把傳送至古拉爵的家阿!");
                    }
                }
                Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                return;
            }

            throw ctx.Error($"找不到為 {name} 的傳點!");
        }

        [Command(name: "teleportplayer", shortHand: "tpp", adminOnly: true, usage: "<Name> <PlayerName>", description: "Teleports player to the specific waypoint. If we type \"all\" instead of the player's name it will teleport all online players to the specified point.")]
        public static void WaypointPlayerCommand(ChatCommandContext ctx, string name, string PlayerName)
        {
            if(PlayerName == "all")
            {
                var users = GameData.Users.Online;

                foreach(var user in users)
                {
                    var PlayerEntity = user.Character.Entity;
                    var SteamID = ctx.Event.User.PlatformId;
                    if (Helper.IsPlayerInCombat(PlayerEntity))
                    {
                        ctx.Reply($"Unable to use waypoint! {user.CharacterName} in combat!");
                    }

                    UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.Event.User.CharacterName.Value);


                    if (BuffUtility.TryGetBuff(Core.SystemsCore.EntityManager, user.Character.Entity, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff, out Entity buffEntity))
                    {
                        throw ctx.Error($"You cannot create a waypoint while flying");
                    }


                    var wp = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name);

                    if (wp != null)
                    {
                        if (!DraculaRoom)
                        {
                            if (Helper.checkDracualaRoom(wp))
                            {
                                throw ctx.Error($"You can't teleport to Dracula's room!");
                            }
                        }
                        Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                        return;
                    }

                    wp = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name && waypoint.Owner == SteamID);

                    if (wp != null)
                    {
                        if (!DraculaRoom)
                        {
                            if (Helper.checkDracualaRoom(wp))
                            {
                                throw ctx.Error($"You can't teleport to Dracula's room!");
                            }
                        }
                        Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                        return;
                    }
                }

            } else
            {
                var user = GameData.Users.GetUserByCharacterName(PlayerName);
                var PlayerEntity = user.Character.Entity;
                var SteamID = ctx.Event.User.PlatformId;
                if (Helper.IsPlayerInCombat(PlayerEntity))
                {
                    throw ctx.Error("Unable to use waypoint! You're in combat!");
                }


                if (BuffUtility.TryGetBuff(Core.SystemsCore.EntityManager, user.Character.Entity, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff, out Entity buffEntity))
                {
                    throw ctx.Error($"You cannot create a waypoint while flying");
                }

                var findName = name + "_" + SteamID;

                var wp = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name && waypoint.Owner == SteamID);

                if (wp != null)
                {
                    if (!DraculaRoom)
                    {
                        if (Helper.checkDracualaRoom(wp))
                        {
                            throw ctx.Error($"You can't teleport to Dracula's room!");
                        }
                    }
                    Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                    return;
                }

                wp = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name && waypoint.Owner == SteamID);

                if (wp != null)
                {
                    if (!DraculaRoom)
                    {
                        if (Helper.checkDracualaRoom(wp))
                        {
                            throw ctx.Error($"You can't teleport to Dracula's room!");
                        }
                    }
                    Helper.TeleportTo(ctx.Event.SenderUserEntity, PlayerEntity, wp.getLocation());
                    return;
                }
            }

            
        }

        [Command(name: "waypoint", shortHand: "wp", adminOnly: false, usage: ".創建傳點", description: "已創建個人專屬的傳送點")]
        public static void WaypointSetCommand(ChatCommandContext ctx, string name)
        {

            ulong SteamID = ctx.Event.User.PlatformId;

            UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.Event.User.CharacterName.Value);

            if (BuffUtility.TryGetBuff(Core.SystemsCore.EntityManager, userModel.Character.Entity, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff, out Entity buffEntity))
            {
                throw ctx.Error($"飛行時無法創建傳點");
            }

            if (Database.waypoints_owned.TryGetValue(SteamID, out var total) && !ctx.Event.User.IsAdmin && total >= WaypointLimit)
            {
                if (total >= WaypointLimit)
                {
                    throw ctx.Error("你已經超出可以設置的傳點數量.");
                }
            }

            var item = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name);

            if (item != null)
            {
                throw ctx.Error($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
            }

            item = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name && waypoint.Owner == SteamID);

            if (item != null)
            {
                throw ctx.Error($"你已經創建相同名稱的傳點.");
            }

            float3 location = entityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            if (!DraculaRoom)
            {
                WaypointData testLocation = new WaypointData("test", 123456789123456789, location.x, location.y, location.z);
                if (Helper.checkDracualaRoom(testLocation))
                {
                    throw ctx.Error($"你不能把傳點設置在古拉爵的家阿!");
                }
            }
            
            var f2_location = new float3(location.x, location.y, location.z);
            AddWaypoint(SteamID, f2_location, name, false);
            ctx.Reply("已創建個人專屬的傳送點.");
        }

        [Command(name: "waypointglobal", shortHand: "wpg", adminOnly: true, usage: "<Name>", description: "Creates the specified global waypoint")]
        public static void WaypointSetGlobalCommand(ChatCommandContext ctx, string name)
        {

            UserModel userModel = GameData.Users.GetUserByCharacterName(ctx.Event.User.CharacterName.Value);


            if (BuffUtility.TryGetBuff(Core.SystemsCore.EntityManager, userModel.Character.Entity, Prefabs.AB_Shapeshift_Bat_TakeFlight_Buff, out Entity buffEntity))
            {
                throw ctx.Error($"You cannot create a waypoint while flying");
            }

            ulong SteamID = ctx.Event.User.PlatformId;
            var findName = name + "_" + SteamID;

            var item = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name);

            if (item != null)
            {
                throw ctx.Error($"你沒有這個 \"{name}\" 名字的傳點,請確認你的傳點名稱.");
            }

            item = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name);

            if (item != null)
            {
                throw ctx.Error($"Already have a personal waypoint with the same name.");
            }

            float3 location = entityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            if (!DraculaRoom)
            {
                WaypointData testLocation = new WaypointData("test", 123456789123456789, location.x, location.y, location.z);
                if (Helper.checkDracualaRoom(testLocation))
                {
                    throw ctx.Error($"You can't create a waypoint in Dracula's room!");
                }
            }

            var f2_location = new float3(location.x, location.y, location.z);
            AddWaypoint(SteamID, f2_location, name, true);
            ctx.Reply("Successfully added Global Waypoint.");
        }

        [Command(name: "waypointremoveglobal", shortHand: "wpgr", adminOnly: true, usage: "<Name>", description: "Removes the specified global waypoint")]
        public static void WaypointremoveGlobalCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;

            var item = Database.globalWaypoint.FirstOrDefault(waypoint => waypoint.Name == name);

            if (item != null)
            {
                RemoveWaypoint(SteamID, name, item, true);
                ctx.Reply("Successfully removed Waypoint.");
                return;
            }

            throw ctx.Error($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
            
        }

        [Command(name: "waypointremove", shortHand: "rm", adminOnly: false, usage: ".刪除傳點", description: "Removes the specified personal waypoint")]
        public static void WaypointRemoveCommand(ChatCommandContext ctx, string name)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            var findName = name + "_" + SteamID;
            var item = Database.waypoints.FirstOrDefault(waypoint => waypoint.Name == name);

            if (item != null)
            {
                RemoveWaypoint(SteamID, name, item, false);
                ctx.Reply("你已成功刪除傳點.");
                return;
            }

            throw ctx.Error($"A global waypoint with the \"{name}\" name existed. Please rename your waypoint.");
        }

        [Command(name: "list", shortHand: "ls", adminOnly: false, usage: "查看傳點", description: "Lists waypoints available to you")]
        public static void WaypointCommand(ChatCommandContext ctx)
        {
            int total_wp = 0;
            foreach (var global_wp in Database.globalWaypoint)
            {
                ctx.Reply($" - <color=#ffff00>{global_wp.Name}</color> [<color=#00dd00>Global</color>]");
                total_wp++;
            }
            foreach (var wp in Database.waypoints)
            {
                ctx.Reply($" - <color=#ffff00>{wp.Name}</color>");
                total_wp++;
            }
            if (total_wp == 0) throw ctx.Error("沒有可用的傳點.");
        }

        public static void AddWaypoint(ulong owner, float3 location, string name, bool isGlobal)
        {
            var WaypointData = new WaypointData(name, owner, location.x, location.y, location.z);
            if (isGlobal) Database.globalWaypoint.Add(WaypointData);
            else Database.waypoints.Add(WaypointData);
            if (!isGlobal && Database.waypoints_owned.TryGetValue(owner, out var total))
            {
                Database.waypoints_owned[owner] = total + 1;
            }
            else Database.waypoints_owned[owner] = 1;

        }

        public static void RemoveWaypoint(ulong owner, string name, WaypointData waypointData, bool global)
        {
            if (global)
            {
                Database.globalWaypoint.Remove(waypointData);
            }
            else
            {
                Database.waypoints_owned[owner] -= 1;
                if (Database.waypoints_owned[owner] < 0) Database.waypoints_owned[owner] = 0;
                Database.waypoints.Remove(waypointData);
            }

        }

        public static void LoadWaypoints()
        {
            if (!File.Exists(Plugin.WaypointsJson))
            {
                File.WriteAllText(Plugin.WaypointsJson,"[]");
            }

            string json = File.ReadAllText(Plugin.WaypointsJson);
            try
            {
                Database.waypoints = JsonSerializer.Deserialize<List<WaypointData>>(json);
                Plugin.Logger.LogWarning("Waypoints DB Populated");
            }
            catch
            {
                Database.waypoints = new List<WaypointData>();
                Plugin.Logger.LogWarning("Waypoints DB Created");
            }


            if (!File.Exists(Plugin.GlobalWaypointsJson))
            {
                File.WriteAllText(Plugin.GlobalWaypointsJson, "[]");
            }

            json = File.ReadAllText(Plugin.GlobalWaypointsJson);
            try
            {
                Database.globalWaypoint = JsonSerializer.Deserialize<List<WaypointData>>(json);
                Plugin.Logger.LogWarning("GlobalWaypoints DB Populated");
            }
            catch
            {
                Database.globalWaypoint = new List<WaypointData>();
                Plugin.Logger.LogWarning("GlobalWaypoints DB Created");
            }


            if (!File.Exists(Plugin.TotalWaypointsJson))
            {
                File.WriteAllText(Plugin.TotalWaypointsJson, "[]");
            }

            json = File.ReadAllText(Plugin.TotalWaypointsJson);
            try
            {
                Database.waypoints_owned = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("TotalWaypoints DB Populated");
            }
            catch
            {
                Database.waypoints_owned = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("TotalWaypoints DB Created");
            }

            
            
        }

        public static void SaveWaypoints()
        {
            File.WriteAllText(Plugin.WaypointsJson, JsonSerializer.Serialize(Database.waypoints));
            File.WriteAllText(Plugin.GlobalWaypointsJson, JsonSerializer.Serialize(Database.globalWaypoint));
            File.WriteAllText(Plugin.TotalWaypointsJson, JsonSerializer.Serialize(Database.waypoints_owned));
        }
    }
}
