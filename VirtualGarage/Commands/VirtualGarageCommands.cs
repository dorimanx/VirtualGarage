using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GUI;
using Sandbox.ModAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace VirtualGarage
{
    [Category("g")]
    public class VirtualGarageCommands : CommandModule
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Command("list", "List grids in garage.", null)]
        [Permission(MyPromoteLevel.None)]
        public void List()
        {
            try
            {
                IMyPlayer player = Context.Player;
                if (player is null)
                    return;

                long identityId = player.IdentityId;
                var pathToVirtualGarage = Plugin.Instance.Config.PathToVirtualGarage;
                string str = Path.Combine(pathToVirtualGarage, MyAPIGateway.Players.TryGetSteamId(identityId).ToString());
                if (!Directory.Exists(str))
                {
                    Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond);
                    return;
                }

                var files = Directory.GetFiles(Path.Combine(Plugin.Instance.Config.PathToVirtualGarage, Context.Player.SteamUserId.ToString()), "*.sbc");
                var listFiles = new List<string>(files).FindAll(s => s.EndsWith(".sbc"));

                if (files.Length == 0 || listFiles.Count() == 0)
                {
                    Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond);
                    return;
                }

                var resultListFiles = new List<string>();

                listFiles.SortNoAlloc((s, s1) => string.Compare(s, s1, StringComparison.Ordinal));
                listFiles.ForEach(s => resultListFiles.Add(s.Replace(".sbc", "")));

                string respond = $"{Plugin.Instance.Config.GridsInVirtualGarageRespond} \n";

                for (var i = 1; i < resultListFiles.Count + 1; i++)
                {
                    respond = $"{respond}{i}. {Path.GetFileName(resultListFiles[i - 1])}\n";
                }

                Context.Respond(respond);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        [Command("save", "Save grid by looking at its position", null)]
        [Permission(MyPromoteLevel.None)]
        public void SaveGridToStorage(string gridName = "")
        {
            var player = Context.Player;
            if (player is null)
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var myPlayer in players)
            {
                if (myPlayer.GetRelationTo(player.IdentityId) != MyRelationsBetweenPlayerAndBlock.Enemies)
                    continue;

                var distance = Vector3D.Distance(myPlayer.GetPosition(), player.Character.GetPosition());
                if (distance < Plugin.Instance.Config.EnemyPlayerInRange)
                {
                    Context.Respond(Plugin.Instance.Config.EnemyNearByChatRespond);
                    return;
                }
            }

            try
            {
                if (player.Character == null)
                    return;

                Log.Warn("VirtualGarage:" + Context.Player.DisplayName + " send *!g save " + gridName + "*");

                VirtualGarageSave.Instance.SaveGrid(player.Character, player.IdentityId, gridName, Context);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        [Command("loadbase", "Load grid from VirtualGarage by number in the same coordinates", null)]
        [Permission(MyPromoteLevel.None)]
        public void LoadBase(int index)
        {
            Load(index, false, true);
        }

        [Command("load", "Load grid from VirtualGarage by number", null)]
        [Permission(MyPromoteLevel.None)]
        public void Load(int index, bool spawnDynamic = false, bool loadbase = false)
        {
            string str = Path.Combine(Plugin.Instance.Config.PathToVirtualGarage, Context.Player.SteamUserId.ToString());
            if (!Directory.Exists(str))
            {
                Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond);
                return;
            }

            var path = Path.Combine(Plugin.Instance.Config.PathToVirtualGarage, Context.Player.SteamUserId.ToString());
            var files = Directory.GetFiles(Path.Combine(Plugin.Instance.Config.PathToVirtualGarage, Context.Player.SteamUserId.ToString()), "*.sbc");
            var listFiles = new List<string>(files).FindAll(s => s.EndsWith(".sbc"));

            if (files.Length == 0 || listFiles.Count() == 0)
            {
                Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond);
                return;
            }

            listFiles.SortNoAlloc((s, s1) => string.Compare(s, s1, StringComparison.Ordinal));
            var gridNameToLoad = listFiles[index - 1];
            IMyPlayer player = Context.Player;

            if (player == null)
                return;

            long identityId = player.IdentityId;
            IMyCharacter character = player.Character;

            _ = MyGravityProviderSystem.CalculateNaturalGravityInPoint(character.GetPosition(), out float naturalGravityMultiplier);

            if (!loadbase && naturalGravityMultiplier > Plugin.Instance.Config.MinAllowedGravityToLoad)
            {
                Context.Respond($"{Plugin.Instance.Config.VirtualGarageNotAllowedInGravityMoreThanResponce} > {Plugin.Instance.Config.MinAllowedGravityToLoad}");
                return;
            }

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var myPlayer in players)
            {
                if (myPlayer.GetRelationTo(identityId) != MyRelationsBetweenPlayerAndBlock.Enemies)
                    continue;

                if (Vector3D.Distance(myPlayer.GetPosition(), character.GetPosition()) < Plugin.Instance.Config.EnemyPlayerInRange)
                {
                    Context.Respond(Plugin.Instance.Config.EnemyNearByChatRespond);
                    return;
                }
            }

            Vector3D? spawnPosition = null;
            if (!loadbase)
            {
                spawnPosition = VirtualGarageLoad.SpawnPosition(character);
            }
            
            if (loadbase || spawnPosition != null)
            {
                VirtualGarageLoad.DoSpawnGrids(identityId, gridNameToLoad, spawnPosition, (grid, identity) => VirtualGarageLoad.AddGps(grid, identity), spawnDynamic);

                MyObjectBuilder_Definitions PrefabToLoad = MyBlueprintUtils.LoadPrefab(gridNameToLoad);
                MyObjectBuilder_CubeGrid[] cubeGridsList = PrefabToLoad.ShipBlueprints[0].CubeGrids;
                
                foreach (var GridInList in cubeGridsList)
                {
                    Context.Respond($"{Plugin.Instance.Config.GridSpawnedToWorldRespond} :{GridInList?.DisplayName}");
                    Log.Info("Структура: " + GridInList?.DisplayName + " перенесена в мир");
                }

                if (File.Exists(gridNameToLoad + "_spawned"))
                    File.Delete(gridNameToLoad + "_spawned");

                File.Move(gridNameToLoad, gridNameToLoad + "_spawned");
                return;
            }

            Context.Respond(Plugin.Instance.Config.NoRoomToSpawnRespond);
            Log.Info("Слишком много всего вокруг, найдите место посвободнее " + gridNameToLoad);
        }
    }
}