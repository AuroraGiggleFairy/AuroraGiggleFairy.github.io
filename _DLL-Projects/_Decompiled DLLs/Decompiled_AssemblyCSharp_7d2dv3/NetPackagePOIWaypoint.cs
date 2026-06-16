using UnityEngine.Scripting;

[Preserve]
public class NetPackagePOIWaypoint : NetPackage
{
	public enum OperationType : byte
	{
		TrySet,
		Remove,
		ClearAll
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public OperationType operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hiddenOnCompass;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePOIWaypoint Setup(OperationType _operation, int _entityId, int _prefabInstanceId = -1, bool _hiddenOnCompass = true)
	{
		operation = _operation;
		entityId = _entityId;
		prefabInstanceId = _prefabInstanceId;
		hiddenOnCompass = _hiddenOnCompass;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		operation = (OperationType)_br.ReadByte();
		entityId = _br.ReadInt32();
		switch (operation)
		{
		case OperationType.TrySet:
			prefabInstanceId = _br.ReadInt32();
			hiddenOnCompass = _br.ReadBoolean();
			break;
		case OperationType.Remove:
			prefabInstanceId = _br.ReadInt32();
			break;
		case OperationType.ClearAll:
			break;
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)operation);
		_bw.Write(entityId);
		switch (operation)
		{
		case OperationType.TrySet:
			_bw.Write(prefabInstanceId);
			_bw.Write(hiddenOnCompass);
			break;
		case OperationType.Remove:
			_bw.Write(prefabInstanceId);
			break;
		case OperationType.ClearAll:
			break;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayer entityPlayer = _world.GetEntity(entityId) as EntityPlayer;
		if (entityPlayer == null)
		{
			Log.Warning($"NetPackagePOIWaypoint: Could not find player with entityId {entityId}");
			return;
		}
		switch (operation)
		{
		case OperationType.TrySet:
			POIWaypoint.TrySet(entityPlayer, prefabInstanceId, hiddenOnCompass);
			break;
		case OperationType.Remove:
			POIWaypoint.Remove(entityPlayer, prefabInstanceId);
			break;
		case OperationType.ClearAll:
			POIWaypoint.ClearAll(entityPlayer);
			break;
		}
	}

	public override int GetLength()
	{
		return 10;
	}
}
