using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;

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
                    await Task.Run(() => { CheckAllGrids(myCubeGrids); });
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

        private void CheckAllGrids(IEnumerable<MyCubeGrid> myCubeGrids)
        {
            foreach (var myCubeGrid in myCubeGrids)
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    try
                    {
                        if (myCubeGrid.DisplayName.Contains("@"))
                            return;

                        var bigOwners = myCubeGrid.BigOwners;
                        if (bigOwners == null || bigOwners.Count == 0)
                            return;

                        var owner = bigOwners[0];
                        var steamId = MySession.Static.Players.TryGetSteamId(owner);
                        if (steamId == 0)
                            return;

                        var identityById = Sync.Players.TryGetIdentity(owner);
                        var lastLogoutTime = identityById.LastLogoutTime;
                        var totalDays = (DateTime.Now - lastLogoutTime).TotalDays;
                        if (totalDays > Plugin.Instance.Config.OldGridDays)
                        {
                            Log.Warn("Товарища " + owner + " нет с нами уже " + totalDays +
                                     " дней, приберём его грид " +
                                     myCubeGrid.DisplayName + " в гараж");
                            VirtualGarageSave.Instance.SaveGridToVirtualGarage(owner, myCubeGrid);
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