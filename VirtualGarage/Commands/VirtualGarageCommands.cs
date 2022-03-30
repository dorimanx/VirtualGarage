using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                IMyPlayer player = this.Context.Player;
                if (player == null)
                    return;
                long identityId = player.IdentityId;
                if (!Directory.Exists(Path.Combine(Plugin.Instance.Config.PathToVirtualGarage,
                        MyAPIGateway.Players.TryGetSteamId(identityId).ToString())))
                {
                    this.Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond, (string) null,
                        (string) null);
                }
                else
                {
                    string[] files =
                        Directory.GetFiles(
                            Path.Combine(Plugin.Instance.Config.PathToVirtualGarage,
                                this.Context.Player.SteamUserId.ToString()), "*.sbc");
                    System.Collections.Generic.List<string> all =
                        new System.Collections.Generic.List<string>((IEnumerable<string>) files).FindAll(
                            (Predicate<string>) (s => s.EndsWith(".sbc")));
                    if (files.Length == 0 || all.Count<string>() == 0)
                    {
                        this.Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond, (string) null,
                            (string) null);
                    }
                    else
                    {
                        System.Collections.Generic.List<string> resultListFiles =
                            new System.Collections.Generic.List<string>();
                        all.SortNoAlloc<string>((Comparison<string>) ((s, s1) =>
                            string.Compare(s, s1, StringComparison.Ordinal)));
                        all.ForEach((Action<string>) (s => resultListFiles.Add(s.Replace(".sbc", ""))));
                        string str = Plugin.Instance.Config.GridsInVirtualGarageRespond + " \n";
                        for (int index = 1; index < resultListFiles.Count + 1; ++index)
                            str = string.Format("{0}{1}. {2}\n", (object) str, (object) index,
                                (object) Path.GetFileName(resultListFiles[index - 1]));
                        this.Context.Respond(str, (string) null, (string) null);
                    }
                }
            }
            catch (Exception ex)
            {
                VirtualGarageCommands.Log.Error<Exception>(ex);
            }
        }

        [Command("save", "Save grid by looking at its position", null)]
        [Permission(MyPromoteLevel.None)]
        public void SaveGridToStorage(string gridName = "")
        {
            DoSaveGrid(gridName);
        }

        private void DoSaveGrid(string gridName)
        {
            IMyPlayer player = this.Context.Player;
            if (player == null)
                return;
            System.Collections.Generic.List<IMyPlayer> players = new System.Collections.Generic.List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (IMyPlayer myPlayer in players)
            {
                if (myPlayer.GetRelationTo(player.IdentityId) == MyRelationsBetweenPlayerAndBlock.Enemies)
                {
                    IMyCharacter character = myPlayer.Character;
                    if (!myPlayer.IsBot && character != null && !character.IsDead && character.IsPlayer &&
                        Vector3D.Distance(myPlayer.GetPosition(), player.Character.GetPosition()) <
                        Plugin.Instance.Config.EnemyPlayerInRange)
                    {
                        VirtualGarageCommands.Log.Warn("Enemy:" + myPlayer.DisplayName + myPlayer.IsBot.ToString());
                        this.Context.Respond(Plugin.Instance.Config.EnemyNearByChatRespond, (string) null,
                            (string) null);
                        return;
                    }
                }
            }

            try
            {
                if (player.Character == null)
                    return;
                VirtualGarageCommands.Log.Warn("VirtualGarage:" + this.Context.Player.DisplayName + " send *!g save " +
                                               gridName + "*");
                VirtualGarageSave.Instance.SaveGrid(player.Character, player.IdentityId, gridName, this.Context);
            }
            catch (Exception ex)
            {
                VirtualGarageCommands.Log.Error<Exception>(ex);
            }
        }

        [Command("loadbase", "Load grid from VirtualGarage by number in the same coordinates", null)]
        [Permission(MyPromoteLevel.None)]
        public void LoadBase(int index) => this.Load(index, loadbase: true);

        [Command("load", "Load grid from VirtualGarage by number", null)]
        [Permission(MyPromoteLevel.None)]
        public void Load(int index, bool spawnDynamic = false, bool loadbase = false)
        {
            if (!Directory.Exists(Path.Combine(Plugin.Instance.Config.PathToVirtualGarage,
                    this.Context.Player.SteamUserId.ToString())))
            {
                this.Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond, (string) null,
                    (string) null);
            }
            else
            {
                Path.Combine(Plugin.Instance.Config.PathToVirtualGarage, this.Context.Player.SteamUserId.ToString());
                string[] files =
                    Directory.GetFiles(
                        Path.Combine(Plugin.Instance.Config.PathToVirtualGarage,
                            this.Context.Player.SteamUserId.ToString()), "*.sbc");
                System.Collections.Generic.List<string> all =
                    new System.Collections.Generic.List<string>((IEnumerable<string>) files).FindAll(
                        (Predicate<string>) (s => s.EndsWith(".sbc")));
                if (files.Length == 0 || all.Count<string>() == 0)
                {
                    this.Context.Respond(Plugin.Instance.Config.NoGridsInVirtualGarageRespond, (string) null,
                        (string) null);
                }
                else
                {
                    all.SortNoAlloc<string>((Comparison<string>) ((s, s1) =>
                        string.Compare(s, s1, StringComparison.Ordinal)));
                    string str = all[index - 1];
                    IMyPlayer player = this.Context.Player;
                    if (player == null)
                        return;
                    long identityId = player.IdentityId;
                    IMyCharacter character1 = player.Character;
                    float naturalGravityMultiplier;
                    MyGravityProviderSystem.CalculateNaturalGravityInPoint(character1.GetPosition(),
                        out naturalGravityMultiplier);
                    if (!loadbase && (double) naturalGravityMultiplier >
                        (double) Plugin.Instance.Config.MinAllowedGravityToLoad)
                    {
                        this.Context.Respond(
                            string.Format("{0} > {1}",
                                (object) Plugin.Instance.Config.VirtualGarageNotAllowedInGravityMoreThanResponce,
                                (object) Plugin.Instance.Config.MinAllowedGravityToLoad), (string) null, (string) null);
                    }
                    else
                    {
                        System.Collections.Generic.List<IMyPlayer> players =
                            new System.Collections.Generic.List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(players);
                        foreach (IMyPlayer myPlayer in players)
                        {
                            if (myPlayer.GetRelationTo(identityId) == MyRelationsBetweenPlayerAndBlock.Enemies)
                            {
                                IMyCharacter character2 = myPlayer.Character;
                                if (!myPlayer.IsBot && character2 != null && !character2.IsDead &&
                                    character2.IsPlayer &&
                                    Vector3D.Distance(myPlayer.GetPosition(), character1.GetPosition()) <
                                    Plugin.Instance.Config.EnemyPlayerInRange)
                                {
                                    VirtualGarageCommands.Log.Warn("Enemy:" + myPlayer.DisplayName +
                                                                   myPlayer.IsBot.ToString());
                                    this.Context.Respond(Plugin.Instance.Config.EnemyNearByChatRespond, (string) null,
                                        (string) null);
                                    return;
                                }
                            }
                        }

                        Vector3D? spawnPosition = new Vector3D?();
                        if (!loadbase)
                            spawnPosition = VirtualGarageLoad.SpawnPosition(character1);
                        if (loadbase || spawnPosition.HasValue)
                        {
                            VirtualGarageLoad.DoSpawnGrids(identityId, str, spawnPosition,
                                (Delegate.AddListenerDelegate) ((grid, identity) =>
                                {
                                    VirtualGarageLoad.AddGps(grid, identity);
                                    foreach (var myCubeBlock in grid.GetFatBlocks())
                                    {
                                        if (myCubeBlock is IMyMotorStator)
                                        {
                                            ((IMyMotorStator)myCubeBlock).Attach();
                                        }
                                        if (myCubeBlock is IMyShipDrill)
                                        {
                                            ((IMyShipDrill)myCubeBlock).Enabled = false;
                                        }
                                    }
                                }), spawnDynamic);
                            foreach (MyObjectBuilder_CubeGrid cubeGrid in MyBlueprintUtils.LoadPrefab(str)
                                         .ShipBlueprints[0].CubeGrids)
                            {
                                this.Context.Respond(
                                    Plugin.Instance.Config.GridSpawnedToWorldRespond + " :" + cubeGrid?.DisplayName,
                                    (string) null, (string) null);
                                VirtualGarageCommands.Log.Info("Структура: " + cubeGrid?.DisplayName +
                                                               " перенесена в мир");
                            }

                            Task.Run(() =>
                            {
                                if (File.Exists(str + "_spawned"))
                                    File.Delete(str + "_spawned");
                                File.Move(str, str + "_spawned");
                            });

                        }
                        else
                        {
                            this.Context.Respond(Plugin.Instance.Config.NoRoomToSpawnRespond, (string) null,
                                (string) null);
                            VirtualGarageCommands.Log.Info("Слишком много всего вокруг, найдите место посвободнее " +
                                                           str);
                        }
                    }
                }
            }
        }
    }
}