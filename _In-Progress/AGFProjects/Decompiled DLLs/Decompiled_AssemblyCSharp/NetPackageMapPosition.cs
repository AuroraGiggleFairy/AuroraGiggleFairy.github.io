using UnityEngine.Scripting;

[Preserve]
public class NetPackageMapPosition : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i mapMiddlePosition;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageMapPosition Setup(int _entityId, Vector2i _mapMiddlePosition)
	{
		entityId = _entityId;
		mapMiddlePosition = _mapMiddlePosition;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		mapMiddlePosition = StreamUtils.ReadVector2i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		StreamUtils.Write(_bw, mapMiddlePosition);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && ValidEntityIdForSender(entityId))
		{
			EntityPlayer entityPlayer = _world.GetEntity(entityId) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.ChunkObserver.mapDatabase != null)
			{
				entityPlayer.ChunkObserver.mapDatabase.SetClientMapMiddlePosition(mapMiddlePosition);
			}
		}
	}

	public override int GetLength()
	{
		return 16;
	}
}
