using UnityEngine.Scripting;

[Preserve]
public class BlockTallgrass : BlockPlant
{
	public BlockTallgrass()
	{
		IsRandomlyTick = false;
	}

	public override bool CheckPlantAlive(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!CanPlantStay(_world, _clrIdx, _blockPos, _blockValue))
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
			return false;
		}
		return true;
	}

	public override void OnBlockPlaceBefore(WorldBase _world, ref BlockPlacement.Result _bpResult, EntityAlive _ea, GameRandom _rnd)
	{
		_bpResult.blockValue.meta2and1 = CalcMeta(_rnd);
		_bpResult.blockValue.rotation = (byte)_rnd.RandomRange(32);
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.meta2and1 = CalcMeta(_rnd);
		_blockValue.rotation = (byte)_rnd.RandomRange(32);
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte CalcMeta(GameRandom _rnd)
	{
		return (byte)(_rnd.RandomRange(6) | (_rnd.RandomRange(256) & -8));
	}
}
