using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WaypointCollection
{
	public const int cCurrentSaveVersion = 6;

	public HashSetList<Waypoint> Collection = new HashSetList<Waypoint>();

	public void Read(BinaryReader _br)
	{
		Collection.Clear();
		int version = _br.ReadByte();
		int num = _br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			Waypoint waypoint = new Waypoint();
			waypoint.Read(_br, version);
			Collection.Add(waypoint);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)6);
		int num = 0;
		for (int i = 0; i < Collection.list.Count; i++)
		{
			if (Collection.list[i].IsSaved)
			{
				num++;
			}
		}
		_bw.Write((ushort)num);
		for (int j = 0; j < num; j++)
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

	public bool ContainsLastKnownPositionWaypointForVehicle(EntityVehicle _vehicle)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownVehiclePositionEntityId == _vehicle.entityId)
			{
				return true;
			}
		}
		return false;
	}

	public Waypoint GetEntityVehicleWaypoint(int _entityID)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownVehiclePositionEntityId == _entityID)
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
			if (item.lastKnownVehiclePositionEntityId == vehicle.entityId)
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
		}
	}

	public void SetEntityVehicleWaypointFromVehicleManager(List<(int entityId, Vector3 position)> _positions)
	{
		List<Waypoint> list = new List<Waypoint>();
		for (int i = 0; i < Collection.list.Count; i++)
		{
			if (Collection.list[i].lastKnownVehiclePositionEntityId == -1)
			{
				continue;
			}
			Waypoint waypoint = Collection.list[i];
			bool flag = false;
			for (int j = 0; j < _positions.Count; j++)
			{
				if (waypoint.lastKnownVehiclePositionEntityId == _positions[j].entityId)
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

	public void SetWaypointHiddenOnMap(int _entityId, bool _hidden)
	{
		foreach (Waypoint item in Collection.list)
		{
			if (item.lastKnownVehiclePositionEntityId == _entityId)
			{
				item.HiddenOnMap = _hidden;
				break;
			}
		}
	}

	public bool TryRemoveVehicleLastKnownWaypoint(EntityVehicle _vehicle)
	{
		Waypoint entityVehicleWaypoint = GetEntityVehicleWaypoint(_vehicle.entityId);
		if (entityVehicleWaypoint != null)
		{
			Collection.Remove(entityVehicleWaypoint);
			NavObjectManager.Instance.UnRegisterNavObject(entityVehicleWaypoint.navObject);
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
