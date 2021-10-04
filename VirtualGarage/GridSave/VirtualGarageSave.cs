using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Torch.Commands;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace VirtualGarage
{
    public class VirtualGarageSave
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static VirtualGarageSave Instance = new VirtualGarageSave();

        public void SaveGrid(IMyCharacter character, long identityId, CommandContext context = null)
        {
            var IsItSaved = false;
            MyCubeGrid SelectedGrid = null;
            MyCubeGrid LastGrid = null;
            Matrix headMatrix = character.GetHeadMatrix(true, true, false);
            Vector3D vector3D = headMatrix.Translation + headMatrix.Forward * 0.5f;
            Vector3D worldEnd = headMatrix.Translation + headMatrix.Forward * 5000.5f;
            List<MyPhysics.HitInfo> mRaycastResult = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(vector3D, worldEnd, mRaycastResult, 15);

            foreach (var hitInfo in new HashSet<MyPhysics.HitInfo>(mRaycastResult))
            {
                if (hitInfo.HkHitInfo.GetHitEntity() is MyCubeGrid grid)
                {
                    if (grid is null)
                        continue;

                    // make sure not to run again on same grid, mRaycastResult contains atleast 6 times same grid, why KEEN why!
                    if (SelectedGrid != null && SelectedGrid.EntityId == grid.EntityId)
                        continue;

                    if (SaveGridToVirtualGarage(identityId, grid, context))
                    {
                        IsItSaved = true;
                        SelectedGrid = grid;
                        if (grid.BlocksCount > 100)
                            LastGrid = grid;
                    }
                }
            }

            if (IsItSaved)
                context?.Respond("Grid/Cтруктура " + LastGrid?.DisplayName + $" {Plugin.Instance.Config.GridSavedToVirtualGarageResponce}");
            else
                context?.Respond(Plugin.Instance.Config.NoGridInViewResponce);
        }

        public bool SaveGridToVirtualGarage(long identityId, MyCubeGrid myCubeGrid, CommandContext context = null)
        {
            if (!myCubeGrid.BigOwners.Contains(identityId))
            {
                context?.Respond($"{Plugin.Instance.Config.OnlyOwnerCanSaveResponce} " + myCubeGrid.DisplayName);
                return false;
            }

            context?.Respond($"{Plugin.Instance.Config.SavingGridResponce} " + myCubeGrid.DisplayName);

            var pathToVirtualGarage = Plugin.Instance.Config.PathToVirtualGarage;

            List<MyCubeGrid> grids = new List<MyCubeGrid>
            {
                myCubeGrid
            };
            int totalpcu = 0;
            int totalblocks = 0;
            List<MyObjectBuilder_CubeGrid> gridsOB = new List<MyObjectBuilder_CubeGrid>();

            foreach (MyCubeGrid сubeGrid in grids)
            {
                totalpcu += сubeGrid.BlocksPCU;
                totalblocks += сubeGrid.BlocksCount;

                try
                {
                    foreach (MyCubeBlock fatBlock in сubeGrid.GetFatBlocks())
                    {
                        MyCubeBlock c = fatBlock;
                        if (c is MyCockpit)
                            (c as MyCockpit).RemovePilot();

                        if (c is MyProgrammableBlock)
                        {
                            try
                            {
                                Plugin.m_myProgrammableBlockKillProgramm.Invoke(
                                    c as MyProgrammableBlock, new object[1]
                                    {
                                         MyProgrammableBlock.ScriptTerminationReason.None
                                    });
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "MyProgrammableBlock hack eval");
                            }
                        }

                        if (c is MyShipDrill)
                            (c as MyShipDrill).Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "(SaveGrid)Exception in block disables ex");
                }

                MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid)сubeGrid.GetObjectBuilder(true);
                gridsOB.Add(objectBuilder);
            }

            if  (totalpcu > Plugin.Instance.Config.MaxPCUForGridOnSave)
            {
                context?.Respond(Plugin.Instance.Config.GridPCUOverLimitResponce);
                return false;
            }

            if (totalblocks > Plugin.Instance.Config.MaxBlocksForGridOnSave)
            {
                context?.Respond(Plugin.Instance.Config.GridBlocksOverLimitResponce);
                return false;
            }

            string gridName = gridsOB[0].DisplayName.Length <= 30
                ? gridsOB[0].DisplayName
                : gridsOB[0].DisplayName.Substring(0, 30);
            string filenameexported = "Time-" + DateTime.Now.ToLongTimeString() + "_PCU-" + totalpcu + "_BL-" + totalblocks + "_" + gridName;

            MyObjectBuilder_ShipBlueprintDefinition newObject1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
            newObject1.Id = new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(filenameexported));
            newObject1.CubeGrids = gridsOB.ToArray();
            newObject1.RespawnShip = false;
            newObject1.DisplayName = MyGameService.UserName;
            newObject1.OwnerSteamId = Sync.MyId;
            newObject1.CubeGrids[0].DisplayName = myCubeGrid.DisplayName;
            MyObjectBuilder_Definitions newObject2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
            newObject2.ShipBlueprints = new MyObjectBuilder_ShipBlueprintDefinition[1];
            newObject2.ShipBlueprints[0] = newObject1;

            string str = Path.Combine(pathToVirtualGarage, MyAPIGateway.Players.TryGetSteamId(identityId).ToString());
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);

            foreach (char ch in ((IEnumerable<char>)Path.GetInvalidPathChars()).Concat(Path.GetInvalidFileNameChars()))
            {
                filenameexported = filenameexported.Replace(ch.ToString(), ".");
            }

            string path = Path.Combine(str, filenameexported + ".sbc");
            if (MyObjectBuilderSerializer.SerializeXML(path, false, newObject2))
                MyObjectBuilderSerializer.SerializePB(path + "B5", true, newObject2);

            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
           {
               foreach (MyEntity myEntity in grids)
                   myEntity.Close();
           });

            return true;
        }
    }
}