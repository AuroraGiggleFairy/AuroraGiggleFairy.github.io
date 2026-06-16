using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlantGrowing : BlockPlant
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropPlantGrowing = "PlantGrowing";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropNext = "Next";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropGrowthRate = "GrowthRate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropGrowthDeviation = "GrowthDeviation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropFertileLevel = "FertileLevel";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropGrowOnTop = "GrowOnTop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropIsGrowOnTopEnabled = "IsGrowOnTopEnabled";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropLightLevelStay = "LightLevelStay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropLightLevelGrow = "LightLevelGrow";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropIsRandom = "IsRandom";

	[PublicizedFrom(EAccessModifier.Protected)]
	public const string PropGrowIfAnythinOnTop = "GrowIfAnythinOnTop";

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

	public static float CropGrowthModifier = 1f;

	public BlockPlantGrowing()
	{
		fertileLevel = 5;
	}

	public override void LateInit()
	{
		base.LateInit();
		if (!base.Properties.Classes.ContainsKey("PlantGrowing"))
		{
			return;
		}
		DynamicProperties dynamicProperties = base.Properties.Classes["PlantGrowing"];
		if (dynamicProperties.Values.ContainsKey("Next"))
		{
			nextPlant = ItemClass.GetItem(dynamicProperties.Values["Next"]).ToBlockValue();
			if (nextPlant.Equals(BlockValue.Air))
			{
				throw new Exception("Block with name '" + dynamicProperties.Values["Next"] + "' not found!");
			}
		}
		growOnTop = BlockValue.Air;
		if (dynamicProperties.Values.ContainsKey("IsGrowOnTopEnabled") && StringParsers.ParseBool(dynamicProperties.Values["IsGrowOnTopEnabled"]))
		{
			bGrowOnTopEnabled = true;
			if (dynamicProperties.Values.ContainsKey("GrowOnTop"))
			{
				growOnTop = ItemClass.GetItem(dynamicProperties.Values["GrowOnTop"]).ToBlockValue();
				if (growOnTop.Equals(BlockValue.Air))
				{
					throw new Exception("Block with name '" + dynamicProperties.Values["GrowOnTop"] + "' not found!");
				}
			}
		}
		if (dynamicProperties.Values.ContainsKey("GrowthRate"))
		{
			growthRate = StringParsers.ParseFloat(dynamicProperties.Values["GrowthRate"]);
		}
		if (dynamicProperties.Values.ContainsKey("GrowthDeviation"))
		{
			growthDeviation = StringParsers.ParseFloat(dynamicProperties.Values["GrowthDeviation"]);
		}
		if (dynamicProperties.Values.ContainsKey("FertileLevel"))
		{
			fertileLevel = int.Parse(dynamicProperties.Values["FertileLevel"]);
		}
		if (dynamicProperties.Values.ContainsKey("LightLevelStay"))
		{
			lightLevelStay = int.Parse(dynamicProperties.Values["LightLevelStay"]);
		}
		if (dynamicProperties.Values.ContainsKey("LightLevelGrow"))
		{
			lightLevelGrow = int.Parse(dynamicProperties.Values["LightLevelGrow"]);
		}
		if (dynamicProperties.Values.ContainsKey("GrowIfAnythinOnTop"))
		{
			isPlantGrowingIfAnythingOnTop = StringParsers.ParseBool(dynamicProperties.Values["GrowIfAnythinOnTop"]);
		}
		if (dynamicProperties.Values.ContainsKey("IsRandom"))
		{
			isPlantGrowingRandom = StringParsers.ParseBool(dynamicProperties.Values["IsRandom"]);
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

	public override bool CanPlaceBlockAt(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (!base.CanPlaceBlockAt(_world, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache != null)
		{
			byte light = chunkCache.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN);
			if (light < lightLevelStay || light < lightLevelGrow)
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanGrowOn(WorldBase _world, Vector3i _blockPos, BlockValue _blockValueOfPlant)
	{
		if (fertileLevel == 0)
		{
			return true;
		}
		return _world.GetBlock(_blockPos).Block.blockMaterial.FertileLevel >= fertileLevel;
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
				addScheduledTick(_world, _blockPos);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addScheduledTick(WorldBase _world, Vector3i _blockPos)
	{
		if (!isPlantGrowingRandom)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, blockID, GetTickRate());
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
		_world.GetWBT().AddScheduledBlockUpdate(_blockPos, blockID, (ulong)num5);
	}

	public override ulong GetTickRate()
	{
		return (ulong)(growthRate * CropGrowthModifier * 20f * 60f);
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (nextPlant.isair)
		{
			return false;
		}
		if (!CheckPlantAlive(_world, _blockPos, _blockValue))
		{
			return true;
		}
		if (_bRandomTick)
		{
			addScheduledTick(_world, _blockPos);
			return true;
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return true;
		}
		Vector3i blockPos = _blockPos + Vector3i.up;
		if (chunkCache.GetLight(blockPos, Chunk.LIGHT_TYPE.SUN) < lightLevelGrow)
		{
			addScheduledTick(_world, _blockPos);
			return true;
		}
		BlockValue block = _world.GetBlock(_blockPos + Vector3i.up);
		if (!isPlantGrowingIfAnythingOnTop && !block.isair)
		{
			return true;
		}
		if (nextPlant.Block is BlockPlant blockPlant && !blockPlant.CanGrowOn(_world, _blockPos + Vector3i.down, nextPlant))
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
		if (isPlantGrowingRandom || _ticksIfLoaded <= GetTickRate() || !_blockValue.Block.UpdateTick(_world, _blockPos, _blockValue, _bRandomTick: false, _ticksIfLoaded - GetTickRate(), _rnd))
		{
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		if (!growOnTop.isair && _blockPos.y + 1 < 255 && block.isair)
		{
			_blockValue.type = growOnTop.type;
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _blockPos, _blockValue, _rnd);
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			if (isPlantGrowingRandom || _ticksIfLoaded <= GetTickRate() || !block2.UpdateTick(_world, _blockPos + Vector3i.up, _blockValue, _bRandomTick: false, _ticksIfLoaded - GetTickRate(), _rnd))
			{
				_world.SetBlockRPC(_blockPos + Vector3i.up, _blockValue);
			}
		}
		return true;
	}

	public BlockValue ForceNextGrowStage(World _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = _world.GetBlock(_blockPos + Vector3i.up);
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
			_blockValue = _blockValue.Block.OnBlockPlaced(_world, _blockPos, _blockValue, _world.GetGameRandom());
			Block block2 = _blockValue.Block;
			if (_blockValue.damage >= block2.blockMaterial.MaxDamage)
			{
				_blockValue.damage = block2.blockMaterial.MaxDamage - 1;
			}
			_world.SetBlockRPC(_blockPos + Vector3i.up, _blockValue);
		}
		return _blockValue;
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (!_world.IsRemote())
		{
			_world.GetWBT().InvalidateScheduledBlockUpdate(_blockPos, blockID);
		}
	}
}
