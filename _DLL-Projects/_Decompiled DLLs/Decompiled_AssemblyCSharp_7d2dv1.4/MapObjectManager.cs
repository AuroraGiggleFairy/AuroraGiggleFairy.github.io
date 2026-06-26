using System.Collections.Generic;
using UnityEngine;

public class MapObjectManager
{
	public delegate void MapObjectListChangedDelegate(EnumMapObjectType _type, MapObject _mapObject, bool _bAdded);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DictionaryList<int, MapObject>> mapObjects = new List<DictionaryList<int, MapObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MapObject> entityList = new List<MapObject>();

	public event MapObjectListChangedDelegate ChangedDelegates;

	public static void Reset()
	{
		entityList.Clear();
	}

	public MapObjectManager()
	{
		for (int i = 0; i < 17; i++)
		{
			mapObjects.Add(new DictionaryList<int, MapObject>());
		}
		foreach (MapObject entity in entityList)
		{
			if (entity is MapObjectVehicle)
			{
				Add(new MapObjectVehicle(entity as MapObjectVehicle));
			}
			else
			{
				Add(new MapObject(entity));
			}
		}
	}

	public static void ClearEntityList()
	{
		entityList.Clear();
	}

	public void Add(MapObject _mapObject)
	{
		if (mapObjects[(int)_mapObject.type].dict.ContainsKey((int)_mapObject.key))
		{
			Remove(_mapObject.type, (int)_mapObject.key);
		}
		mapObjects[(int)_mapObject.type].Add((int)_mapObject.key, _mapObject);
		if (this.ChangedDelegates != null)
		{
			this.ChangedDelegates(_mapObject.type, _mapObject, _bAdded: true);
		}
		if (_mapObject.type == EnumMapObjectType.Entity && !entityList.Contains(_mapObject))
		{
			entityList.Add(_mapObject);
		}
	}

	public void Remove(EnumMapObjectType _type, int _key)
	{
		if (mapObjects[(int)_type].dict.ContainsKey(_key))
		{
			MapObject mapObject = mapObjects[(int)_type].dict[_key];
			if (mapObject.type == EnumMapObjectType.Entity && entityList.Contains(mapObject))
			{
				entityList.Remove(mapObject);
			}
			mapObjects[(int)_type].Remove(_key);
			if (this.ChangedDelegates != null)
			{
				this.ChangedDelegates(_type, mapObject, _bAdded: false);
			}
		}
	}

	public void RemoveByPosition(EnumMapObjectType _type, Vector3 _position)
	{
		for (int num = mapObjects[(int)_type].list.Count - 1; num >= 0; num--)
		{
			Vector3 position = mapObjects[(int)_type].list[num].GetPosition();
			if (position.x == _position.x && position.z == _position.z)
			{
				mapObjects[(int)_type].list.RemoveAt(num);
			}
		}
	}

	public void RemoveByType(EnumMapObjectType _type)
	{
		for (int num = mapObjects[(int)_type].list.Count - 1; num >= 0; num--)
		{
			if (mapObjects[(int)_type].list[num].type == _type)
			{
				mapObjects[(int)_type].list.RemoveAt(num);
			}
		}
	}

	public void Clear()
	{
		mapObjects.Clear();
	}

	public List<MapObject> GetList(EnumMapObjectType _type)
	{
		return mapObjects[(int)_type].list;
	}
}
