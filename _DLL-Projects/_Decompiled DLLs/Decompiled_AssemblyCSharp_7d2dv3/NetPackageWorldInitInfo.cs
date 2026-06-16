using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInitInfo : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance.Serializable> eventPrefabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<(int, WallVolume)> wallVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dataLength;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWorldInitInfo Setup(List<PrefabInstance.Serializable> _eventPrefabs, List<(int, WallVolume)> _wallVolumes)
	{
		eventPrefabs = _eventPrefabs;
		foreach (PrefabInstance.Serializable eventPrefab in eventPrefabs)
		{
			dataLength += eventPrefab.GetLength();
		}
		wallVolumes = _wallVolumes;
		dataLength += _wallVolumes.Count * 29;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		if (eventPrefabs == null)
		{
			eventPrefabs = new List<PrefabInstance.Serializable>();
		}
		eventPrefabs.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			eventPrefabs.Add(new PrefabInstance.Serializable(_br));
		}
		if (wallVolumes == null)
		{
			wallVolumes = new List<(int, WallVolume)>();
		}
		eventPrefabs.Clear();
		int num2 = _br.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			wallVolumes.Add((_br.ReadInt32(), WallVolume.Read(_br)));
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(eventPrefabs.Count);
		foreach (PrefabInstance.Serializable eventPrefab in eventPrefabs)
		{
			eventPrefab.Write(_bw);
		}
		_bw.Write(wallVolumes.Count);
		foreach (var (value, wallVolume) in wallVolumes)
		{
			_bw.Write(value);
			wallVolume.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Error("[NetPackageWorldInitInfo] world is null");
			return;
		}
		EventPrefabsClient eventPrefabsClient = _world.m_EventPrefabsClient;
		if (eventPrefabsClient != null)
		{
			foreach (PrefabInstance.Serializable eventPrefab in eventPrefabs)
			{
				eventPrefabsClient.TryAdd(eventPrefab.id, eventPrefab.prefabName, eventPrefab.rotation, eventPrefab.position);
			}
		}
		else
		{
			Log.Error("[NetPackageWorldInitInfo] EventPrefabsClient not available");
		}
		_world.SetWallVolumesForClient(wallVolumes);
		_callbacks.worldInitInfoReceived = true;
	}

	public override int GetLength()
	{
		return dataLength;
	}
}
