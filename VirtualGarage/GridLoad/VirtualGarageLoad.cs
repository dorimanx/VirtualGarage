using System;
using Sandbox.Common.ObjectBuilders;
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

    public static void DoSpawnGrids(
      long masterIdentityId,
      string str,
      Vector3D? spawnPosition,
      Delegate.AddListenerDelegate addListenerDelegate = null,
      bool convertToDynamic = false)
    {
      VirtualGarageLoad.SpawnSomeGrids(MyBlueprintUtils.LoadPrefab(str).ShipBlueprints[0].CubeGrids, spawnPosition, masterIdentityId, addListenerDelegate, convertToDynamic);
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
      Random random = new Random();
      MyEntity ignoreEnt = (MyEntity) null;
      BoundingSphereD boundingSphere = new BoundingSphereD(character.GetPosition(), (double) Plugin.Instance.Config.MaxSpawnRadius);
      foreach (MyEntity myEntity in MyEntities.GetEntitiesInSphere(ref boundingSphere))
      {
        if (myEntity is MySafeZone)
          ignoreEnt = myEntity;
      }
      return MyEntities.FindFreePlaceCustom(boundingSphere.RandomToUniformPointOnSphere(random.NextDouble(), random.NextDouble()), Plugin.Instance.Config.MaxSpawnRadius, ignoreEnt: ignoreEnt);
    }

    public static void AddGps(MyCubeGrid grid, long myPlayerIdentity)
    {
      IMyGps gps = MyAPIGateway.Session?.GPS.Create(grid.DisplayName, grid.DisplayName, grid.PositionComp.GetPosition(), true, true);
      gps.GPSColor = Color.Yellow;
      MyAPIGateway.Session?.GPS.AddGps(myPlayerIdentity, gps);
    }

    public static void SpawnSomeGrids(
      MyObjectBuilder_CubeGrid[] cubeGrids,
      Vector3D? position,
      long masterIdentityId,
      Delegate.AddListenerDelegate addListenerDelegate = null,
      bool convertToDynamic = false)
    {
      MyAPIGateway.Entities.RemapObjectBuilderCollection(cubeGrids);
      VirtualGarageLoad.RemapOwnership(cubeGrids, masterIdentityId);
      Vector3D vector3D1 = (Vector3D) cubeGrids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
      Vector3D vector3D2 = (Vector3D) cubeGrids[0].PositionAndOrientation.Value.Position + Vector3D.Zero;
      if (Plugin.Instance.Config.ConvertToStatic && Plugin.Instance.Config.ConvertToDynamic)
        Plugin.Instance.Config.ConvertToDynamic = false;
      for (int index = 0; index < cubeGrids.Length; ++index)
      {
        MyObjectBuilder_CubeGrid cubeGrid = cubeGrids[index];
        if (index == 0)
        {
          if (cubeGrid.GridSizeEnum == MyCubeSize.Large)
          {
            if (Plugin.Instance.Config.ConvertToStatic && !Plugin.Instance.Config.ConvertToDynamic)
            {
              cubeGrid.IsStatic = true;
              cubeGrid.IsUnsupportedStation = true;
            }
            if (((!Plugin.Instance.Config.ConvertToDynamic ? 0 : (!Plugin.Instance.Config.ConvertToStatic ? 1 : 0)) | (convertToDynamic ? 1 : 0)) != 0)
            {
              cubeGrid.IsStatic = false;
              cubeGrid.IsUnsupportedStation = false;
            }
          }
          if (cubeGrid.PositionAndOrientation.HasValue)
          {
            MyPositionAndOrientation valueOrDefault = cubeGrid.PositionAndOrientation.GetValueOrDefault();
            if (position.HasValue)
              valueOrDefault.Position = (SerializableVector3D) position.Value;
            cubeGrid.PositionAndOrientation = new MyPositionAndOrientation?(valueOrDefault);
            vector3D1 = (Vector3D) cubeGrid.PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
          }
        }
        else
        {
          MyPositionAndOrientation valueOrDefault = cubeGrid.PositionAndOrientation.GetValueOrDefault();
          valueOrDefault.Position = (SerializableVector3D) (vector3D1 + (Vector3D) valueOrDefault.Position - vector3D2);
          cubeGrid.PositionAndOrientation = new MyPositionAndOrientation?(valueOrDefault);
        }
        cubeGrid.AngularVelocity = (SerializableVector3) new Vector3();
        cubeGrid.LinearVelocity = (SerializableVector3) new Vector3();
      }

      foreach (var cubeGrid in cubeGrids)
      {
        foreach (var myObjectBuilderCubeBlock in cubeGrid.CubeBlocks)
        {
          if (myObjectBuilderCubeBlock is MyObjectBuilder_Drill)
          {
            ((MyObjectBuilder_Drill) myObjectBuilderCubeBlock).Enabled = false;
          }
        }

        MyAPIGateway.Entities.CreateFromObjectBuilderParallel(cubeGrid, completionCallback: ((Action<IMyEntity>) (entity =>
        {
          ((MyCubeGrid) entity).DetectDisconnectsAfterFrame();
          MyAPIGateway.Entities.AddEntity(entity);
          addListenerDelegate((MyCubeGrid) entity, masterIdentityId);
        })));
      }
    }
  }
}
