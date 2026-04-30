using UnityEngine.Scripting;

[Preserve]
public class BlockDeadgrass : Block
{
	public override void Init()
	{
		base.Init();
		IsDecoration = true;
	}

	public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		if (_blockPosThatChanged.x == _myBlockPos.x && _blockPosThatChanged.z == _myBlockPos.z && _blockPosThatChanged.y == _myBlockPos.y - 1 && !_newNeighborBlockValue.Block.shape.IsSolidCube)
		{
			world.SetBlockRPC(_myBlockPos, BlockValue.Air);
		}
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.meta = (byte)_rnd.RandomRange(16);
		return _blockValue;
	}

	public override void OnBlockPlaceBefore(WorldBase _world, ref BlockPlacement.Result _bpResult, EntityAlive _ea, GameRandom _rnd)
	{
		_bpResult.blockValue.meta = (byte)_rnd.RandomRange(16);
	}
}
