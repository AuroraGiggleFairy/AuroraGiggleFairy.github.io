using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlantGrowing : BlockPlant
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingNextPlant = "PlantGrowing.Next";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowthRate = "PlantGrowing.GrowthRate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowthDeviation = "PlantGrowing.GrowthDeviation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingFertileLevel = "PlantGrowing.FertileLevel";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowOnTop = "PlantGrowing.GrowOnTop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingIsGrowOnTopEnabled = "PlantGrowing.IsGrowOnTopEnabled";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingLightLevelStay = "PlantGrowing.LightLevelStay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingLightLevelGrow = "PlantGrowing.LightLevelGrow";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingIsRandom = "PlantGrowing.IsRandom";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingGrowIfAnythinOnTop = "PlantGrowing.GrowIfAnythinOnTop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue nextPlant;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue growOnTop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bGrowOnTopEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float growthRate;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float growthDeviation = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lightLevelGrow = 8;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlantGrowingRandom = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlantGrowingIfAnythingOnTop = true;

	public BlockPlantGrowing()
	{
		fertileLevel = 5;
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(PropGrowingNextPlant))
		{
			nextPlant = ItemClass.GetItem(base.Properties.Values[PropGrowingNextPlant]).ToBlockValue();
			if (nextPlant.Equals(BlockValue.Air))
			{
				throw new Exception("Block with name '" + base.Properties.Values[PropGrowingNextPlant] + "' not found!");
			}
		}
		growOnTop = BlockValue.Air;
		if (base.Properties.Values.ContainsKey(PropGrowingIsGrowOnTopEnabled) && StringParsers.ParseBool(base.Properties.Values[PropGrowingIsGrowOnTopEnabled]))
		{
			bGrowOnTopEnabled = true;
			if (base.Properties.Values.ContainsKey(PropGrowingGrowOnTop))
			{
				growOnTop = ItemClass.GetItem(base.Properties.Values[PropGrowingGrowOnTop]).ToBlockValue();
				if (growOnTop.Equals(BlockValue.Air))
				{
					throw new Exception("Block with name '" + base.Properties.Values[PropGrowingGrowOnTop] + "' not found!");
				}
			}
		}
		if (base.Properties.Values.ContainsKey(PropGrowingGrowthRate))
		{
			growthRate = StringParsers.ParseFloat(base.Properties.Values[PropGrowingGrowthRate]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingGrowthDeviation))
		{
			growthDeviation = StringParsers.ParseFloat(base.Properties.Values[PropGrowingGrowthDeviation]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingFertileLevel))
		{
			fertileLevel = int.Parse(base.Properties.Values[PropGrowingFertileLevel]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingLightLevelStay))
		{
			lightLevelStay = int.Parse(base.Properties.Values[PropGrowingLightLevelStay]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingLightLevelGrow))
		{
			lightLevelGrow = int.Parse(base.Properties.Values[PropGrowingLightLevelGrow]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingGrowIfAnythinOnTop))
		{
			isPlantGrowingIfAnythingOnTop = StringParsers.ParseBool(base.Properties.Values[PropGrowingGrowIfAnythinOnTop]);
		}
		if (base.Properties.Values.ContainsKey(PropGrowingIsRandom))
		{
			isPlantGrowingRandom = StringParsers.ParseBool(base.Properties.Values[PropGrowingIsRandom]);
		}
		if (growthRate > 0f)
		{
			BlockTag = BlockTags.GrowablePlant;
			IsRandomlyTick = true;
		}
		else
		{
			IsRandomlyTick = false;
		}
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			byte light = chunkCluster.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN);
			if (light < lightLevelStay || light < lightLevelGrow)
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanGrowOn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValueOfPlant)
	{
		if (fertileLevel == 0)
		{
			return true;
		}
		return _world.GetBlock(_clrIdx, _blockPos).Block.blockMaterial.FertileLevel >= fertileLevel;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_ea is EntityPlayerLocal)
		{
			_ea.Progression.AddLevelExp((int)_result.blockValue.Block.blockMaterial.Experience);
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!nextPlant.isair)
		{
			if (_blockValue.ischild)
			{
				Log.Warning("BlockPlantGrowing OnBlockAdded child at {0}, {1}", _blockPos, _blockValue);
			}
			else if (!_world.IsRemote())
			{
				addScheduledTick(_world, _chunk.ClrIdx, _blockPos);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addScheduledTick(WorldBase _world, int _clrIdx, Vector3i _blockPos)
	{
		if (!isPlantGrowingRandom)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, blockID, GetTickRate());
			return;
		}
		int num = (int)GetTickRate();
		int num2 = (int)((float)num * growthDeviation);
		int num3 = num / 2;
		int max = num + num3;
		GameRandom gameRandom = _world.GetGameRandom();
		int num4;
		int num5;
		do
		{
			float randomGaussian = gameRandom.RandomGaussian;
			num4 = Mathf.RoundToInt((float)num + (float)num2 * randomGaussian);
			num5 = Utils.FastClamp(num4, num3, max);
		}
		while (num5 != num4);
		_world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, blockID, (ulong)num5);
	}

	public override ulong GetTickRate()
	{
		return (ulong)(growthRate * 20f * 60f);
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (nextPlant.isair)
		{
			return false;
		}
		if (!CheckPlantAlive(_world, _clrIdx, _blockPos, _blockValue))
		{
			return true;
		}
		if (_bRandomTick)
		{
			addScheduledTick(_world, _clrIdx, _blockPos);
			return true;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return true;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		if (chunkCluster.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN) < lightLevelGrow)
		{
			addScheduledTick(_world, _clrIdx, _blockPos);
			return true;
		}
		BlockValue block = _world.GetBlock(_clrIdx, _blockPos + Vector3i.up);
		if (!isPlantGrowingIfAnythingOnTop && !block.isair)
		{
			return true;
		}
		if (nextPlant.Block is BlockPlant blockPlant && !blockPlant.CanGrowOn(_world, _clrIdx, _blockPos + Vector3i.down, nextPlant))
		{
			return true;
		}
		_blockValue.type = nextPlant.type;
		BiomeDefinition biome = ((World)_world).GetBiome(_blockPos.x, _blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(_blockValue.type))
		{
			_blockValue.type = biome.Replacements[_blockValue.type];
		}
		BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(_blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
		blockValue.rotation = _blockValue.rotation;
		blockValue.meta = _blockValue.meta;
		blockValue.meta2 = 0;
		_blockValue = blockValue;
		if (bGrowOnTopEnabled)
		{
			_blockValue.meta = (byte)((_blockValue.meta + 1) & 0xF);
		}
		if (isPlantGrowingRandom || _ticksIfLoaded <= GetTickRate() || !_blockValue.Block.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick: false, _ticksIfLoaded - GetTickRate(), _rnd))
		{
			_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
		}
		if (!growOnTop.isair && _blockPos.y + 1 < 255 && block.isair)
		{
			_blockValue.type = growOnTop.type;
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _rnd);
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			if (isPlantGrowingRandom || _ticksIfLoaded <= GetTickRate() || !block2.UpdateTick(_world, _clrIdx, _blockPos + Vector3i.up, _blockValue, _bRandomTick: false, _ticksIfLoaded - GetTickRate(), _rnd))
			{
				_world.SetBlockRPC(_clrIdx, _blockPos + Vector3i.up, _blockValue);
			}
		}
		return true;
	}

	public BlockValue ForceNextGrowStage(World _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = _world.GetBlock(_clrIdx, _blockPos + Vector3i.up);
		if (!isPlantGrowingIfAnythingOnTop && !block.isair)
		{
			return _blockValue;
		}
		_blockValue.type = nextPlant.type;
		BiomeDefinition biome = _world.GetBiome(_blockPos.x, _blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(_blockValue.type))
		{
			_blockValue.type = biome.Replacements[_blockValue.type];
		}
		BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(_blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
		blockValue.rotation = _blockValue.rotation;
		blockValue.meta = _blockValue.meta;
		blockValue.meta2 = 0;
		_blockValue = blockValue;
		if (bGrowOnTopEnabled)
		{
			_blockValue.meta = (byte)((_blockValue.meta + 1) & 0xF);
		}
		if (!growOnTop.isair && _blockPos.y + 1 < 255 && block.isair)
		{
			_blockValue.type = growOnTop.type;
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _world.GetGameRandom());
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			_world.SetBlockRPC(_clrIdx, _blockPos + Vector3i.up, _blockValue);
		}
		return _blockValue;
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (!_world.IsRemote())
		{
			_world.GetWBT().InvalidateScheduledBlockUpdate(_chunk.ClrIdx, _blockPos, blockID);
		}
	}
}
