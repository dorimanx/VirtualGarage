using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace VirtualGarage
{
    public class VirtualGarageOldGridProcessor
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static VirtualGarageOldGridProcessor OldGridProcessor = new VirtualGarageOldGridProcessor();

        public void OnLoaded()
        {
            CheckOldGridAsync();
        }

        public async void CheckOldGridAsync()
        {
            try
            {
                Log.Info("Check old grids started");
                try
                {
                    await Task.Delay(30000);
                    var myCubeGrids = MyEntities.GetEntities().OfType<MyCubeGrid>();
                    var PlayersList = MySession.Static.Players.GetAllIdentities().ToList();
                    await Task.Run(() => { CheckAllGrids(myCubeGrids, PlayersList); });
                }
                catch (Exception e)
                {
                    Log.Error(e, "Check old grids Error");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Check old grids start Error");
            }
        }

        private void CheckAllGrids(IEnumerable<MyCubeGrid> myCubeGrids, List<MyIdentity> PlayersList)
        {
            List<MyCubeGrid> GridsGroup = new List<MyCubeGrid>();

            foreach (var myCubeGrid in myCubeGrids)
            {
                if (myCubeGrid is null || myCubeGrid.Closed || myCubeGrid.MarkedForClose)
                    continue;

                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    try
                    {
                        if (myCubeGrid.DisplayName.Contains("@"))
                            return;

                        var bigOwners = myCubeGrid.BigOwners;
                        if (bigOwners == null || bigOwners.Count < 1)
                            return;

                        var owner = bigOwners.FirstOrDefault();

                        if (MySession.Static.Players.IdentityIsNpc(owner))
                            return;

                        var steamId = MySession.Static.Players.TryGetSteamId(owner);
                        if (steamId == 0)
                            return;

                        var MyidentityById = Sync.Players.TryGetIdentity(owner);
                        if (MyidentityById is null)
                            return;

                        var lastLogoutTime = MyidentityById.LastLogoutTime;
                        var totalDays = (DateTime.Now - lastLogoutTime).TotalDays;

                        if (totalDays > Plugin.Instance.Config.OldGridDays)
                        {
                            GridsGroup = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(myCubeGrid);
                            if (GridsGroup is null)
                                return;

                            var PlayerName = owner.ToString();

                            foreach (var PlayerEntity in PlayersList)
                            {
                                if (PlayerEntity != null && PlayerEntity.IdentityId == MyidentityById.IdentityId)
                                    PlayerName = PlayerEntity.DisplayName;
                            }

                            if (PlayerName == string.Empty)
                                PlayerName = "Unknown BOB";

                            Log.Warn("Товарища " + PlayerName + " нет с нами уже " + totalDays + " дней, приберём его грид " + myCubeGrid.DisplayName + " в гараж");

                            VirtualGarageSave.Instance.SaveOldGridToVirtualGarage(owner, GridsGroup);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Check old grid EXCEPTION");
                    }
                });
            }
        }
    }
}