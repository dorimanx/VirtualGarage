using System;
using Sandbox.Game.Entities;
using Sandbox.Game.GUI;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace VirtualGarage
{
    public class Delegate
    {
        public delegate void AddListenerDelegate(MyCubeGrid grid, long myPlayerIdentity);
    }

    public class VirtualGarageLoad
    {
        public static VirtualGarageLoad Instance = new VirtualGarageLoad();

        public static void DoSpawnGrids(long masterIdentityId, string str, Vector3D spawnPosition, Delegate.AddListenerDelegate addListenerDelegate = null, bool convertToDynamic = false)
        {
            MyObjectBuilder_Definitions loadedPrefab = MyBlueprintUtils.LoadPrefab(str);
            MyObjectBuilder_CubeGrid[] cubeGrids = loadedPrefab.ShipBlueprints[0].CubeGrids;

            SpawnSomeGrids(cubeGrids, spawnPosition, masterIdentityId, addListenerDelegate, convertToDynamic);
        }

        public static void RemapOwnership(MyObjectBuilder_CubeGrid[] cubeGrids, long new_owner)
        {
            foreach (MyObjectBuilder_CubeGrid cubeGrid in cubeGrids)
            {
                foreach (MyObjectBuilder_CubeBlock cubeBlock in cubeGrid.CubeBlocks)
                {
                    if (Plugin.Instance.Config.ChangeBuiltBy)
                        cubeBlock.BuiltBy = new_owner;

                    if (Plugin.Instance.Config.ChangeOwner)
                        cubeBlock.Owner = new_owner;

                    cubeBlock.ShareMode = MyOwnershipShareModeEnum.Faction;
                }
            }
        }

        public static Vector3D? SpawnPosition(IMyCharacter character)
        {
            var rand = new Random();
            MyEntity safezone = null;
            var spawnPosition = character.GetPosition();

            var boundingSphere = new BoundingSphereD(spawnPosition, Plugin.Instance.Config.MaxSpawnRadius);
            var topMostEntitiesInSphere = MyEntities.GetEntitiesInSphere(ref boundingSphere);

            foreach (var myentity in topMostEntitiesInSphere)
            {
                if (myentity is MySafeZone)
                    safezone = myentity;
            }

            return MyEntities.FindFreePlaceCustom(boundingSphere.RandomToUniformPointInSphere(rand.NextDouble(), rand.NextDouble(), rand.NextDouble()),
                                      Plugin.Instance.Config.MaxSpawnRadius, 20, 5, 1, 0, safezone);
        }

        public static void AddGps(MyCubeGrid grid, long myPlayerIdentity)
        {
            var gridGPS = MyAPIGateway.Session?.GPS.Create(grid.DisplayName, grid.DisplayName, grid.PositionComp.GetPosition(), true, true);
            gridGPS.GPSColor = Color.Yellow;
            MyAPIGateway.Session?.GPS.AddGps(myPlayerIdentity, gridGPS);
        }

        public static void SpawnSomeGrids(MyObjectBuilder_CubeGrid[] cubeGrids, Vector3D position, long masterIdentityId, Delegate.AddListenerDelegate addListenerDelegate = null, bool convertToDynamic = false)
        {
            MyAPIGateway.Entities.RemapObjectBuilderCollection(cubeGrids);
            RemapOwnership(cubeGrids, masterIdentityId);

            var NewVector3D = cubeGrids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
            var delta = cubeGrids[0].PositionAndOrientation!.Value.Position + Vector3D.Zero;

            // make sure admin didnt failed!
            if (Plugin.Instance.Config.ConvertToStatic && Plugin.Instance.Config.ConvertToDynamic)
            {
                Plugin.Instance.Config.ConvertToDynamic = false;
            }

            for (int index = 0; index < cubeGrids.Length; ++index)
            {
                var cubeGrid = cubeGrids[index];

                if (index == 0)
                {
                    if (cubeGrid.GridSizeEnum == MyCubeSize.Large)
                    {
                        if (Plugin.Instance.Config.ConvertToStatic && !Plugin.Instance.Config.ConvertToDynamic)
                        {
                            cubeGrid.IsStatic = true;
                            cubeGrid.IsUnsupportedStation = true;
                        }

                        if ((Plugin.Instance.Config.ConvertToDynamic && !Plugin.Instance.Config.ConvertToStatic) || convertToDynamic)
                        {
                            cubeGrid.IsStatic = false;
                            cubeGrid.IsUnsupportedStation = false;
                        }
                    }

                    if (cubeGrid.PositionAndOrientation.HasValue)
                    {
                        var valueOrDefault = cubeGrid.PositionAndOrientation.GetValueOrDefault();
                        valueOrDefault.Position = position;
                        cubeGrid.PositionAndOrientation = new MyPositionAndOrientation?(valueOrDefault);
                        NewVector3D = cubeGrid.PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
                    }
                }
                else
                {
                    var valueOrDefault = cubeGrid.PositionAndOrientation.GetValueOrDefault();
                    valueOrDefault.Position = NewVector3D + valueOrDefault.Position - delta;
                    cubeGrid.PositionAndOrientation = valueOrDefault;
                }

                // reset velocity
                cubeGrid.AngularVelocity = new Vector3();
                cubeGrid.LinearVelocity = new Vector3();
            }

            foreach (var GridEntity in cubeGrids)
            {
                MyAPIGateway.Entities.CreateFromObjectBuilderParallel(GridEntity, completionCallback: new Action<IMyEntity>(entity =>
                {
                    ((MyCubeGrid)entity).DetectDisconnectsAfterFrame();
                    MyAPIGateway.Entities.AddEntity(entity);
                    addListenerDelegate.Invoke(((MyCubeGrid)entity), masterIdentityId);
                }));
            }
        }
    }
}
