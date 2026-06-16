using UnityEngine.Scripting;

[Preserve]
public class NetPackageBlockTrigger : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageBlockTrigger Setup(Vector3i _blockPos, BlockValue _blockValue)
	{
		blockPos = _blockPos;
		blockValue = _blockValue;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		blockPos = StreamUtils.ReadVector3i(_br);
		blockValue = new BlockValue(_br.ReadUInt32());
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, blockPos);
		_bw.Write(blockValue.rawData);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && base.Sender.bAttachedToEntity)
		{
			EntityPlayer player = _world.GetEntity(base.Sender.entityId) as EntityPlayer;
			blockValue.Block.HandleTrigger(player, _world, blockPos, blockValue);
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
