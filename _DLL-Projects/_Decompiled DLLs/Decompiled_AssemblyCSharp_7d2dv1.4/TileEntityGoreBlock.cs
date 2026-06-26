using System.Collections;
using UnityEngine;

public class TileEntityGoreBlock : TileEntityLootContainer
{
	public ulong tickTimeToRemove;

	public TileEntityGoreBlock(Chunk _chunk)
		: base(_chunk)
	{
		tickTimeToRemove = GameTimer.Instance.ticks + 60000;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.GoreBlock;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (GameTimer.Instance.ticks > tickTimeToRemove)
		{
			ThreadManager.StartCoroutine(destroyBlockLater(world));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator destroyBlockLater(World world)
	{
		yield return new WaitForEndOfFrame();
		world.SetBlockRPC(ToWorldPos(), BlockValue.Air);
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		tickTimeToRemove = _br.ReadUInt64();
		if (readVersion < 4)
		{
			tickTimeToRemove += 60000uL;
		}
	}

	public override void write(PooledBinaryWriter stream, StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		stream.Write(tickTimeToRemove);
	}
}
