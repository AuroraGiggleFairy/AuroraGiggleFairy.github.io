using UnityEngine.Scripting;

[Preserve]
public class NetPackageLootContainerDropContent : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldBlockType = -1;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageLootContainerDropContent Setup(Vector3i _worldPos, int _lootEntityId)
	{
		worldPos = _worldPos;
		lootEntityId = _lootEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		worldPos = StreamUtils.ReadVector3i(_br);
		lootEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, worldPos);
		_bw.Write(lootEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().DropContentOfLootContainerServer(_world.GetBlock(worldPos), worldPos, lootEntityId);
	}

	public override int GetLength()
	{
		return 16;
	}
}
