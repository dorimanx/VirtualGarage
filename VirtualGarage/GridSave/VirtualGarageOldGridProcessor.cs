using System;
using System.Collections.Generic;
using System.IO;
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
                    await Task.Run(() =>
                    {
                        CheckAllGrids(myCubeGrids, PlayersList);
                    });
                    await Task.Delay(new Random().Next(60000, 180000));
                    await Task.Run(() =>
                    {
                        RemoveTrash();
                    });
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

        private void RemoveTrash()
        {
            var path = Plugin.Instance.Config.PathToVirtualGarage;
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    if (file.EndsWith(".sbcB5"))
                    {
                        File.Delete(file);
                        continue;
                    }
                    if (file.EndsWith(".sbc_spawned") && (DateTime.Now - File.GetCreationTime(file)).TotalDays > 7)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        private void CheckAllGrids(IEnumerable<MyCubeGrid> myCubeGrids, List<MyIdentity> PlayersList)
        {
            foreach (MyCubeGrid myCubeGrid1 in myCubeGrids)
            {
                MyCubeGrid myCubeGrid = myCubeGrid1;
                List<MyCubeGrid> GridsGroup = new List<MyCubeGrid>();
                if (myCubeGrid != null && !myCubeGrid.Closed && !myCubeGrid.MarkedForClose)
                    MyAPIGateway.Utilities.InvokeOnGameThread((Action) (() =>
                    {
                        try
                        {
                            if (myCubeGrid.DisplayName.Contains("@"))
                                return;
                            List<long> bigOwners = myCubeGrid.BigOwners;
                            if (bigOwners == null || bigOwners.Count < 1)
                                return;
                            long identityId = bigOwners.FirstOrDefault<long>();
                            if (MySession.Static.Players.IdentityIsNpc(identityId) ||
                                MySession.Static.Players.TryGetSteamId(identityId) == 0UL)
                                return;
                            MyIdentity identity = Sync.Players.TryGetIdentity(identityId);
                            if (identity == null)
                                return;
                            try
                            {
                                if (Plugin.Instance.Config.OldGridDays == 0)
                                {
                                    Log.Warn("Приберём его грид " +
                                                                           myCubeGrid.DisplayName + " в гараж");
                                    GridsGroup = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Mechanical)
                                        .GetGroupNodes(myCubeGrid);
                                    VirtualGarageSave.Instance.SaveOldGridToVirtualGarage(identityId, GridsGroup);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ошибка", ex);
                            }

                            double totalDays = (DateTime.Now - identity.LastLogoutTime).TotalDays;
                            if (totalDays <= (double) Plugin.Instance.Config.OldGridDays)
                                return;
                            GridsGroup = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Mechanical)
                                .GetGroupNodes(myCubeGrid);
                            if (GridsGroup == null)
                                return;
                            string str = identityId.ToString();
                            foreach (MyIdentity players in PlayersList)
                            {
                                if (players != null && players.IdentityId == identity.IdentityId)
                                    str = players.DisplayName;
                            }

                            if (str == string.Empty)
                                str = "Unknown BOB";
                            Log.Warn("Товарища " + str + " нет с нами уже " +
                                                                   totalDays + " дней, приберём его грид " +
                                                                   myCubeGrid.DisplayName + " в гараж");
                            VirtualGarageSave.Instance.SaveOldGridToVirtualGarage(identityId, GridsGroup);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Check old grid EXCEPTION");
                        }
                    }));
            }
        }
    }
}