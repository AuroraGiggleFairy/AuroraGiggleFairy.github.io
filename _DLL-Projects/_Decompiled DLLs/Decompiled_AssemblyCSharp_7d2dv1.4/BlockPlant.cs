using UnityEngine.Scripting;

[Preserve]
public class BlockPlant : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int lightLevelStay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fertileLevel = 1;

	public BlockPlant()
	{
		IsRandomlyTick = true;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return CanGrowOn(_world, _clrIdx, _blockPos - Vector3i.up, _blockValue);
		}
		return false;
	}

	public virtual bool CanGrowOn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValueOfPlant)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (fertileLevel != 0)
		{
			return _world.GetBlock(_clrIdx, _blockPos).Block.blockMaterial.FertileLevel >= fertileLevel;
		}
		return true;
	}

	public override void OnNeighborBlockChange(WorldBase _world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		base.OnNeighborBlockChange(_world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		if (!_myBlockValue.ischild)
		{
			CheckPlantAlive(_world, _clrIdx, _myBlockPos, _myBlockValue);
		}
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		CheckPlantAlive(_world, _clrIdx, _blockPos, _blockValue);
		return false;
	}

	public virtual bool CheckPlantAlive(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (!CanPlantStay(_world, _clrIdx, _blockPos, _blockValue))
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
			return false;
		}
		return true;
	}

	public override bool CanPlantStay(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (lightLevelStay == 0 || _world.GetBlockLightValue(_clrIdx, _blockPos) >= lightLevelStay || _world.GetBlockLightValue(_clrIdx, _blockPos + Vector3i.up) >= lightLevelStay || _world.IsOpenSkyAbove(_clrIdx, _blockPos.x, _blockPos.y, _blockPos.z))
		{
			return CanGrowOn(_world, _clrIdx, _blockPos - Vector3i.up, _blockValue);
		}
		return false;
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.rotation = (byte)_rnd.RandomRange(4);
		return _blockValue;
	}
}
