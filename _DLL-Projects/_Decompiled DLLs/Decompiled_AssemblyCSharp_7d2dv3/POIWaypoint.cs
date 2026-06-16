using Platform;
using UnityEngine;

public class POIWaypoint : Waypoint
{
	public int prefabInstanceId;

	public POIWaypoint()
	{
		IsSaved = false;
	}

	public static bool TrySet(EntityPlayer player, int prefabInstanceId, bool hiddenOnCompass)
	{
		if (player.isEntityRemote)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(player.entityId);
				if (clientInfo != null)
				{
					clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePOIWaypoint>().Setup(NetPackagePOIWaypoint.OperationType.TrySet, player.entityId, prefabInstanceId, hiddenOnCompass));
					return true;
				}
			}
			return false;
		}
		PrefabInstance prefab = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPrefab(prefabInstanceId);
		if (prefab == null)
		{
			Log.Error(string.Format("{0} not set: could not find prefab instance with id {1}", "POIWaypoint", prefabInstanceId));
			return false;
		}
		POIWaypoint pOIWaypoint = new POIWaypoint();
		pOIWaypoint.prefabInstanceId = prefab.id;
		pOIWaypoint.pos = prefab.boundingBoxPosition + prefab.boundingBoxSize / 2;
		pOIWaypoint.name.Update(prefab.prefab.PrefabName, PlatformManager.MultiPlatform.User.PlatformUserId);
		pOIWaypoint.icon = "ui_game_symbol_map_trader";
		pOIWaypoint.ownerId = null;
		pOIWaypoint.lastKnownPositionEntityId = -1;
		if (!player.Waypoints.ContainsWaypoint(pOIWaypoint))
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", pOIWaypoint.pos, pOIWaypoint.icon, hiddenOnCompass);
			navObject.UseOverrideColor = true;
			navObject.OverrideColor = Color.cyan;
			navObject.IsActive = true;
			navObject.name = pOIWaypoint.name.Text;
			pOIWaypoint.navObject = navObject;
			player.Waypoints.Collection.Add(pOIWaypoint);
			return true;
		}
		return false;
	}

	public static void Remove(int prefabInstanceId)
	{
		foreach (EntityPlayer item in GameManager.Instance.World.Players.list)
		{
			Remove(item, prefabInstanceId);
		}
	}

	public static void Remove(EntityPlayer player, int prefabInstanceId)
	{
		if (player.isEntityRemote)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(player.entityId)?.SendPackage(NetPackageManager.GetPackage<NetPackagePOIWaypoint>().Setup(NetPackagePOIWaypoint.OperationType.Remove, player.entityId, prefabInstanceId));
			}
			return;
		}
		for (int num = player.Waypoints.Collection.list.Count - 1; num >= 0; num--)
		{
			Waypoint waypoint = player.Waypoints.Collection.list[num];
			if (waypoint is POIWaypoint pOIWaypoint && pOIWaypoint.prefabInstanceId == prefabInstanceId)
			{
				NavObjectManager.Instance.UnRegisterNavObject(pOIWaypoint.navObject);
				player.Waypoints.Collection.Remove(waypoint);
			}
		}
	}

	public static void ClearAll(EntityPlayer localPlayer)
	{
		if (localPlayer.isEntityRemote)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(localPlayer.entityId)?.SendPackage(NetPackageManager.GetPackage<NetPackagePOIWaypoint>().Setup(NetPackagePOIWaypoint.OperationType.ClearAll, localPlayer.entityId));
			}
			return;
		}
		NavObjectManager instance = NavObjectManager.Instance;
		for (int num = localPlayer.Waypoints.Collection.list.Count - 1; num >= 0; num--)
		{
			Waypoint waypoint = localPlayer.Waypoints.Collection.list[num];
			if (waypoint is POIWaypoint pOIWaypoint)
			{
				instance.UnRegisterNavObject(pOIWaypoint.navObject);
				localPlayer.Waypoints.Collection.Remove(waypoint);
			}
		}
	}
}
