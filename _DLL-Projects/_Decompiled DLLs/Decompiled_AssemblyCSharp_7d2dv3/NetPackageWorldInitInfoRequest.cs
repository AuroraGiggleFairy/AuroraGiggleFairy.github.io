using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInitInfoRequest : NetPackage
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageWorldInitInfoRequest Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Error("[NetPackageWorldInitInfoRequest] world is null");
			return;
		}
		List<PrefabInstance.Serializable> list = null;
		EventPrefabs eventPrefabs = _world.ChunkCache.ChunkProvider?.GetEventPrefabs();
		if (eventPrefabs != null)
		{
			list = eventPrefabs.GetPrefabsSerialized();
		}
		else
		{
			list = new List<PrefabInstance.Serializable>();
			Log.Error("[NetPackageWorldInitInfoRequest] EventPrefabs not available");
		}
		List<(int, WallVolume)> allWallVolumes = _world.GetAllWallVolumes();
		base.Sender.SendPackage(NetPackageManager.GetPackage<NetPackageWorldInitInfo>().Setup(list, allWallVolumes));
	}

	public override int GetLength()
	{
		return 1;
	}
}
