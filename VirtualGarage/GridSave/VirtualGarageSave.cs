using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Sandbox.Definitions;
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
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace VirtualGarage
{
    public class VirtualGarageSave
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static VirtualGarageSave Instance = new VirtualGarageSave();

        public void SaveGrid(IMyCharacter character, long identityId, string gridName, CommandContext context = null)
        {
            var IsItSaved = false;
            MyCubeGrid SelectedGrid = null;
            MyCubeGrid LastGrid = null;
            Matrix headMatrix = character.GetHeadMatrix(true, true, false);
            Vector3D vector3D = headMatrix.Translation + headMatrix.Forward * 0.5f;
            Vector3D worldEnd = headMatrix.Translation + headMatrix.Forward * 5000.5f;
            List<MyPhysics.HitInfo> mRaycastResult = new List<MyPhysics.HitInfo>();
            HashSet<IMyEntity> GridSets = new HashSet<IMyEntity>();
            List<MyCubeGrid> GridsGroup;

            if (gridName != string.Empty)
            {
                MyAPIGateway.Entities.GetEntities(GridSets, (IMyEntity Entity) => Entity is IMyCubeGrid && Entity.DisplayName.Equals(gridName, StringComparison.InvariantCultureIgnoreCase));
                if (!GridSets.Any())
                {
                    context.Respond("No such grid exist with name '" + gridName + "' .", "VirtualGarage", "Red");
                    return;
                }
                foreach (var IEntity in GridSets)
                {
                    if (IEntity is null)
                        continue;

                    // reset velocity
                    if (IEntity.Physics != null)
                    {
                        IEntity.Physics.AngularVelocity = new Vector3();
                        IEntity.Physics.LinearVelocity = new Vector3();
                    }

                    GridsGroup = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes((MyCubeGrid)IEntity);

                    if (SaveGridToVirtualGarage(identityId, GridsGroup, context))
                    {
                        IsItSaved = true;
                        LastGrid = (MyCubeGrid)IEntity;
                    }
                }
            }
            else
            {
                MyPhysics.CastRay(vector3D, worldEnd, mRaycastResult, 15);

                foreach (var hitInfo in new HashSet<MyPhysics.HitInfo>(mRaycastResult))
                {
                    if (hitInfo.HkHitInfo.GetHitEntity() is MyCubeGrid grid)
                    {
                        if (grid is null)
                            continue;

                        // ignore projected grid.
                        if (grid.IsPreview)
                            continue;

                        // make sure not to run again on same grid, mRaycastResult contains atleast 6 times same grid, why KEEN why!
                        if (SelectedGrid != null && SelectedGrid.EntityId == grid.EntityId)
                            continue;

                        // reset velocity
                        if (grid.Physics != null)
                        {
                            grid.Physics.AngularVelocity = new Vector3();
                            grid.Physics.LinearVelocity = new Vector3();
                        }

                        GridsGroup = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(grid);

                        if (SaveGridToVirtualGarage(identityId, GridsGroup, context))
                        {
                            IsItSaved = true;
                            SelectedGrid = grid;
                            if (grid.BlocksCount > 100)
                                LastGrid = grid;
                            break;
                        }
                    }
                }
            }

            if (IsItSaved)
                context?.Respond("Grid/Cтруктура " + LastGrid?.DisplayName + $" {Plugin.Instance.Config.GridSavedToVirtualGarageResponce}");
            else
                context?.Respond(Plugin.Instance.Config.NoGridInViewResponce);
        }

        public bool SaveGridToVirtualGarage(long identityId, List<MyCubeGrid> myCubeGridList, CommandContext context = null)
        {
            // check ownership
            if (myCubeGridList.FirstOrDefault().BigOwners.Count > 0 && !myCubeGridList.FirstOrDefault().BigOwners.Contains(identityId))
            {
                context?.Respond($"{Plugin.Instance.Config.OnlyOwnerCanSaveResponce} " + myCubeGridList.FirstOrDefault().DisplayName);
                return false;
            }

            // check distance from player to grid.
            if (Vector3D.DistanceSquared(myCubeGridList.FirstOrDefault().PositionComp.GetPosition(), context.Player.Character.GetPosition()) > Plugin.Instance.Config.MaxRangeToGrid * Plugin.Instance.Config.MaxRangeToGrid)
            {
                context?.Respond($"{Plugin.Instance.Config.GridToFarResponce} " + myCubeGridList.FirstOrDefault().DisplayName);
                return false;
            }

            context?.Respond($"{Plugin.Instance.Config.SavingGridResponce} " + myCubeGridList.FirstOrDefault().DisplayName);

            var pathToVirtualGarage = Plugin.Instance.Config.PathToVirtualGarage;

            int totalpcu = 0;
            int totalblocks = 0;
            List<MyObjectBuilder_CubeGrid> gridsOB = new List<MyObjectBuilder_CubeGrid>();

            foreach (MyCubeGrid сubeGrid in myCubeGridList)
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

            if (totalpcu > Plugin.Instance.Config.MaxPCUForGridOnSave)
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
            string filenameexported = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + "_P-" + totalpcu + "_B-" + totalblocks + "_" + gridName;

            MyObjectBuilder_ShipBlueprintDefinition newObject1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
            newObject1.Id = new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(filenameexported));
            newObject1.DLCs = GetDLCs(newObject1.CubeGrids);
            newObject1.CubeGrids = gridsOB.ToArray();
            newObject1.RespawnShip = false;
            newObject1.DisplayName = MyGameService.UserName;
            newObject1.OwnerSteamId = Sync.MyId;
            newObject1.CubeGrids[0].DisplayName = myCubeGridList.FirstOrDefault().DisplayName;
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
               foreach (MyEntity myEntity in myCubeGridList)
                   myEntity.Close();
           });

            return true;
        }

        public bool SaveOldGridToVirtualGarage(long identityId, List<MyCubeGrid> myCubeGridList)
        {
            var pathToVirtualGarage = Plugin.Instance.Config.PathToVirtualGarage;

            int totalpcu = 0;
            int totalblocks = 0;
            List<MyObjectBuilder_CubeGrid> gridsOB = new List<MyObjectBuilder_CubeGrid>();

            foreach (MyCubeGrid сubeGrid in myCubeGridList)
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

            string gridName = gridsOB[0].DisplayName.Length <= 30
                ? gridsOB[0].DisplayName
                : gridsOB[0].DisplayName.Substring(0, 30);
            string filenameexported = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + "_P-" + totalpcu + "_B-" + totalblocks + "_" + gridName;

            MyObjectBuilder_ShipBlueprintDefinition newObject1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
            newObject1.Id = new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(filenameexported));
            newObject1.DLCs = GetDLCs(newObject1.CubeGrids);
            newObject1.CubeGrids = gridsOB.ToArray();
            newObject1.RespawnShip = false;
            newObject1.DisplayName = MyGameService.UserName;
            newObject1.OwnerSteamId = Sync.MyId;
            newObject1.CubeGrids[0].DisplayName = myCubeGridList.FirstOrDefault().DisplayName;
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
                foreach (MyEntity myEntity in myCubeGridList)
                    myEntity.Close();
            });

            return true;
        }

        private static string[] GetDLCs(MyObjectBuilder_CubeGrid[] cubeGrids)
        {
            if (cubeGrids.IsNullOrEmpty())
                return null;

            var hashSet = new HashSet<string>();
            foreach (var GridEntity in cubeGrids)
            {
                foreach (var cubeBlock in GridEntity.CubeBlocks)
                {
                    var GetBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(cubeBlock);
                    if (GetBlockDefinition is null || GetBlockDefinition.DLCs is null)
                        continue;

                    if (GetBlockDefinition.DLCs.Length > 0)
                    {
                        foreach (var DLCName in GetBlockDefinition.DLCs)
                            hashSet.Add(DLCName);
                    }
                }
            }
            return hashSet.ToArray();
        }
    }
}