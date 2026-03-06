using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlaceholderMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct PlaceholderTarget(BlockValue _block, float _prob, string _biome, bool _randomRotation)
	{
		public readonly BlockValue block = _block;

		public float prob = _prob;

		public readonly string biome = _biome;

		public readonly bool randomRotation = _randomRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct QuestPlaceholderEntry
	{
		public FastTags<TagGroup.Global> QuestTag;

		public List<PlaceholderTarget> PlaceholderList;
	}

	public static BlockPlaceholderMap Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BlockValue, List<PlaceholderTarget>> placeholders = new Dictionary<BlockValue, List<PlaceholderTarget>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BlockValue, List<QuestPlaceholderEntry>> questResetPlaceholders = new Dictionary<BlockValue, List<QuestPlaceholderEntry>>();

	public static void InitStatic()
	{
		Instance = new BlockPlaceholderMap();
	}

	public static void Cleanup()
	{
		if (Instance != null)
		{
			Instance.Clear();
		}
	}

	public void AddPlaceholder(BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation)
	{
		addPlaceholderInternal(placeholders, _placeholderBlockValue, _targetValue, _targetProb, _biome, _randomRotation);
	}

	public void AddQuestResetPlaceholder(BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation, FastTags<TagGroup.Global> questTags)
	{
		if (!questResetPlaceholders.ContainsKey(_placeholderBlockValue))
		{
			questResetPlaceholders.Add(_placeholderBlockValue, new List<QuestPlaceholderEntry>());
		}
		for (int i = 0; i < questResetPlaceholders[_placeholderBlockValue].Count; i++)
		{
			QuestPlaceholderEntry questPlaceholderEntry = questResetPlaceholders[_placeholderBlockValue][i];
			if (questPlaceholderEntry.QuestTag.Test_AnySet(questTags))
			{
				questPlaceholderEntry.PlaceholderList.Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
				return;
			}
		}
		QuestPlaceholderEntry item = default(QuestPlaceholderEntry);
		item.QuestTag = questTags;
		item.PlaceholderList = new List<PlaceholderTarget>();
		item.PlaceholderList.Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
		questResetPlaceholders[_placeholderBlockValue].Add(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addPlaceholderInternal(Dictionary<BlockValue, List<PlaceholderTarget>> _map, BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation)
	{
		if (!_map.ContainsKey(_placeholderBlockValue))
		{
			_map.Add(_placeholderBlockValue, new List<PlaceholderTarget>());
		}
		_map[_placeholderBlockValue].Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation));
	}

	public bool IsReplaceableBlockType(BlockValue _blockValue)
	{
		if (!_blockValue.isair)
		{
			return placeholders.ContainsKey(_blockValue);
		}
		return false;
	}

	public void AdjustProbs(BlockValue _blockValue)
	{
		if (placeholders.TryGetValue(_blockValue, out var value))
		{
			float num = 0f;
			for (int i = 0; i < value.Count; i++)
			{
				num = Utils.FastMax(num, value[i].prob);
			}
			float num2 = 1f / num;
			for (int j = 0; j < value.Count; j++)
			{
				PlaceholderTarget value2 = value[j];
				value2.prob *= num2;
				value[j] = value2;
			}
		}
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, int _blockX, int _blockZ, bool _useAlternate = false)
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_blockX, 0, _blockZ);
		return Replace(_blockValue, _random, chunk, _blockX, 0, _blockZ, FastTags<TagGroup.Global>.none, _useAlternate);
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, Chunk _chunk, int _blockX, int _blockY, int _blockZ, FastTags<TagGroup.Global> questTags, bool useAlternate = false, bool allowRandomRotation = true)
	{
		if (!placeholders.ContainsKey(_blockValue))
		{
			return _blockValue;
		}
		List<PlaceholderTarget> list = placeholders[_blockValue];
		bool ischild = _blockValue.ischild;
		Vector3i parent = _blockValue.parent;
		BlockValue result = _blockValue;
		string text = null;
		GameRandom gameRandom = _random;
		if (gameRandom == null)
		{
			Vector3i vector3i = _chunk.GetWorldPos() + new Vector3i(_blockX, _blockY, _blockZ);
			if (ischild)
			{
				vector3i += parent;
			}
			gameRandom = Utils.RandomFromSeedOnPos(vector3i.x, vector3i.y, vector3i.z, GameManager.Instance.World.Seed);
		}
		if (useAlternate && questResetPlaceholders.ContainsKey(_blockValue))
		{
			List<QuestPlaceholderEntry> list2 = questResetPlaceholders[_blockValue];
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].QuestTag.Test_AnySet(questTags))
				{
					list = list2[i].PlaceholderList;
					break;
				}
			}
		}
		while (true)
		{
			int index = gameRandom.RandomRange(list.Count);
			PlaceholderTarget placeholderTarget = list[index];
			if (placeholderTarget.biome != null)
			{
				if (text == null)
				{
					byte biomeId = _chunk.GetBiomeId(World.toBlockXZ(_blockX), World.toBlockXZ(_blockZ));
					text = GameManager.Instance.World.Biomes.GetBiome(biomeId).m_sBiomeName;
					bool flag = false;
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].biome == null || list[j].biome.EqualsCaseInsensitive(text))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				if (!placeholderTarget.biome.EqualsCaseInsensitive(text))
				{
					continue;
				}
			}
			if (!(placeholderTarget.prob >= 1f) && !(gameRandom.RandomFloat < placeholderTarget.prob))
			{
				continue;
			}
			result.type = placeholderTarget.block.type;
			if (allowRandomRotation && placeholderTarget.randomRotation)
			{
				byte b;
				if (result.Block.shape.Has45DegreeRotations)
				{
					b = (byte)gameRandom.RandomRange(8);
					if (b > 3)
					{
						b += 20;
					}
				}
				else
				{
					b = (byte)gameRandom.RandomRange(4);
				}
				result.rotation = b;
			}
			else
			{
				result.rotation = _blockValue.rotation;
			}
			break;
		}
		if (result.Equals(_blockValue))
		{
			result = BlockValue.Air;
		}
		if (_random == null)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}
		if (ischild)
		{
			result.ischild = true;
			result.parent = parent;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Clear()
	{
		questResetPlaceholders.Clear();
		placeholders.Clear();
	}
}
