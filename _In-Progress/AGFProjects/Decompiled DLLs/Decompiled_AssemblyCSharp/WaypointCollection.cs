using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WaypointCollection
{
	public const int cCurrentSaveVersion = 7;

	public HashSetList<Waypoint> Collection = new HashSetList<Waypoint>();

	public void Read(BinaryReader _br)
	{
		Collection.Clear();
		int version = _br.ReadByte();
		int num = _br.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			Waypoint waypoint = new Waypoint();
			waypoint.Read(_br, version);
			Collection.Add(waypoint);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)7);
		int num = 0;
		for (int i = 0; i < Collection.list.Count; i++)
		{
			if (Collection.list[i].IsSaved)
			{
				num++;
			}
		}
		_bw.Write((ushort)num);
		for (int j = 0; j < Collection.list.Count; j++)
		{
			if (Collection.list[j].IsSaved)
			{
				Collection.list[j].Write(_bw);
			}
		}
	}

	public WaypointCollection Clone()
	{
		WaypointCollection waypointCollection = new WaypointCollection();
		for (int i = 0; i < Collection.list.Count; i++)
		{
			waypointCollection.Collection.Add(Collection.list[i].Clone());
		}
		return waypointCollection;
	}

	public bool ContainsWaypoint(Waypoint _wp)
	{
		return Collection.hashSet.Contains(_wp);
	}

	public bool ContainsLastKnownPositionWaypoint(int _entityId)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownPositionEntityId == _entityId)
			{
				return true;
			}
		}
		return false;
	}

	public Waypoint GetLastKnownPositionWaypoint(int _entityID)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownPositionEntityId == _entityID)
			{
				return item;
			}
		}
		return null;
	}

	public void UpdateEntityVehicleWayPoint(EntityVehicle vehicle, bool unloaded = false)
	{
		if (!vehicle.LocalPlayerIsOwner())
		{
			return;
		}
		Waypoint waypoint = null;
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownPositionEntityId == vehicle.entityId)
			{
				waypoint = item;
				break;
			}
		}
		if (waypoint != null)
		{
			Vector3i vector3i = Vector3i.FromVector3Rounded(vehicle.position);
			if (waypoint.pos != vector3i)
			{
				Collection.Remove(waypoint);
				waypoint.pos = vector3i;
				waypoint.navObject.TrackedPosition = vehicle.position;
				Collection.Add(waypoint);
			}
			waypoint.navObject.hiddenOnCompass = !unloaded;
			SetWaypointHiddenOnMap(vehicle.entityId, !unloaded);
		}
		else
		{
			((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshVehiclePositionWaypoint(vehicle, _unloaded: false);
		}
	}

	public void SetEntityVehicleWaypointFromVehicleManager(List<(int entityId, Vector3 position)> _positions)
	{
		List<Waypoint> list = new List<Waypoint>();
		for (int i = 0; i < Collection.list.Count; i++)
		{
			if (Collection.list[i].lastKnownPositionEntityId == -1 || Collection.list[i].lastKnownPositionEntityType != eLastKnownPositionEntityType.Vehicle)
			{
				continue;
			}
			Waypoint waypoint = Collection.list[i];
			bool flag = false;
			for (int j = 0; j < _positions.Count; j++)
			{
				if (waypoint.lastKnownPositionEntityId == _positions[j].entityId)
				{
					flag = true;
					Vector3i vector3i = Vector3i.FromVector3Rounded(_positions[j].position);
					if (waypoint.pos != vector3i)
					{
						Collection.Remove(waypoint);
						waypoint.pos = vector3i;
						waypoint.navObject.TrackedPosition = _positions[j].position;
						Collection.Add(waypoint);
					}
					break;
				}
			}
			if (!flag)
			{
				list.Add(waypoint);
			}
		}
		foreach (Waypoint item in list)
		{
			Collection.Remove(item);
			NavObjectManager.Instance.UnRegisterNavObject(item.navObject);
		}
	}

	public void SetDroneWaypointsFromDroneManager(List<(int entityId, Vector3 position)> _drones)
	{
		for (int i = 0; i < _drones.Count; i++)
		{
			Waypoint waypoint = null;
			foreach (Waypoint item in Collection.list)
			{
				if (item.lastKnownPositionEntityId == _drones[i].entityId)
				{
					waypoint = item;
					break;
				}
			}
			if (waypoint != null)
			{
				Vector3i vector3i = Vector3i.FromVector3Rounded(_drones[i].position);
				if (waypoint.pos != vector3i)
				{
					Collection.Remove(waypoint);
					waypoint.pos = vector3i;
					waypoint.navObject.TrackedPosition = _drones[i].position;
					Collection.Add(waypoint);
				}
			}
			else
			{
				((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(_drones[i].entityId, Vector3i.FromVector3Rounded(_drones[i].position), _unloaded: true);
			}
		}
		List<Waypoint> list = new List<Waypoint>();
		for (int j = 0; j < Collection.list.Count; j++)
		{
			if (Collection.list[j].lastKnownPositionEntityId == -1 || Collection.list[j].lastKnownPositionEntityType != eLastKnownPositionEntityType.Drone)
			{
				continue;
			}
			Waypoint waypoint2 = Collection.list[j];
			bool flag = false;
			for (int k = 0; k < _drones.Count; k++)
			{
				if (waypoint2.lastKnownPositionEntityId == _drones[k].entityId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(waypoint2);
			}
		}
		foreach (Waypoint item2 in list)
		{
			Collection.Remove(item2);
			NavObjectManager.Instance.UnRegisterNavObject(item2.navObject);
		}
	}

	public void UpdateEntityDroneWayPoint(EntityDrone drone, bool following, bool unloaded = false)
	{
		if (!drone.LocalPlayerIsOwner())
		{
			return;
		}
		Waypoint waypoint = null;
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownPositionEntityId == drone.entityId)
			{
				waypoint = item;
				break;
			}
		}
		if (waypoint != null)
		{
			Vector3i vector3i = Vector3i.FromVector3Rounded(drone.position);
			if (waypoint.pos != vector3i)
			{
				Collection.Remove(waypoint);
				waypoint.pos = vector3i;
				waypoint.navObject.TrackedPosition = drone.position;
				Collection.Add(waypoint);
			}
			waypoint.navObject.hiddenOnCompass = !unloaded;
			SetWaypointHiddenOnMap(drone.entityId, !unloaded);
		}
		else
		{
			((XUiC_MapArea)GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(drone, _unloaded: false);
		}
	}

	public void SetWaypointHiddenOnMap(int _entityId, bool _hidden)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownPositionEntityId == _entityId)
			{
				item.HiddenOnMap = _hidden;
				item.navObject.hiddenOnMap = _hidden;
				break;
			}
		}
	}

	public bool TryRemoveLastKnownPositionWaypoint(int _entityId)
	{
		Waypoint lastKnownPositionWaypoint = GetLastKnownPositionWaypoint(_entityId);
		if (lastKnownPositionWaypoint != null)
		{
			Collection.Remove(lastKnownPositionWaypoint);
			NavObjectManager.Instance.UnRegisterNavObject(lastKnownPositionWaypoint.navObject);
			return true;
		}
		return false;
	}

	public Waypoint GetWaypointForNavObject(NavObject nav)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (nav == item.navObject)
			{
				return item;
			}
		}
		return null;
	}
}
