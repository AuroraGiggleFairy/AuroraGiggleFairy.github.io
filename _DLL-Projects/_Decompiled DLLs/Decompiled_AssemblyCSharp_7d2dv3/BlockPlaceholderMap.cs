using System;
using System.Collections.Generic;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlaceholderMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct PlaceholderTarget
	{
		public readonly BlockValue blockValue;

		public readonly float prob;

		public readonly string biomeName;

		public readonly bool isRandomRotation;

		public readonly global::SandboxOptions.SandboxOptions sandboxOption;

		public readonly bool invertSandbox;

		public float Prob
		{
			get
			{
				if (sandboxOption != global::SandboxOptions.SandboxOptions.Max && SandboxOptionManager.GetOptionType(sandboxOption) == BaseSandboxOption.OptionTypes.Float)
				{
					if (invertSandbox)
					{
						return 1f - SandboxOptionManager.GetFloat(sandboxOption);
					}
					return SandboxOptionManager.GetFloat(sandboxOption);
				}
				return prob;
			}
		}

		public PlaceholderTarget(BlockValue _blockValue, float _prob, string _biomeName, bool _isRandomRotation, string _sandboxOption)
		{
			blockValue = _blockValue;
			prob = _prob;
			biomeName = _biomeName;
			isRandomRotation = _isRandomRotation;
			invertSandbox = false;
			sandboxOption = global::SandboxOptions.SandboxOptions.Max;
			if (_sandboxOption != "")
			{
				if (_sandboxOption.StartsWith('!'))
				{
					invertSandbox = true;
					_sandboxOption = _sandboxOption.Remove(0, 1);
				}
				sandboxOption = Enum.Parse<global::SandboxOptions.SandboxOptions>(_sandboxOption);
			}
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Clear()
	{
		questResetPlaceholders.Clear();
		placeholders.Clear();
	}

	public void AddPlaceholder(BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation, string _sandboxOption)
	{
		addPlaceholderInternal(placeholders, _placeholderBlockValue, _targetValue, _targetProb, _biome, _randomRotation, _sandboxOption);
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
				questPlaceholderEntry.PlaceholderList.Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation, ""));
				return;
			}
		}
		QuestPlaceholderEntry item = default(QuestPlaceholderEntry);
		item.QuestTag = questTags;
		item.PlaceholderList = new List<PlaceholderTarget>();
		item.PlaceholderList.Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation, ""));
		questResetPlaceholders[_placeholderBlockValue].Add(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addPlaceholderInternal(Dictionary<BlockValue, List<PlaceholderTarget>> _map, BlockValue _placeholderBlockValue, BlockValue _targetValue, float _targetProb, string _biome, bool _randomRotation, string _sandboxOption)
	{
		if (!_map.ContainsKey(_placeholderBlockValue))
		{
			_map.Add(_placeholderBlockValue, new List<PlaceholderTarget>());
		}
		_map[_placeholderBlockValue].Add(new PlaceholderTarget(_targetValue, _targetProb, _biome, _randomRotation, _sandboxOption));
	}

	public void AdjustData(BlockValue _blockValue)
	{
		if (placeholders.TryGetValue(_blockValue, out var value))
		{
			value.Sort([PublicizedFrom(EAccessModifier.Internal)] (PlaceholderTarget a, PlaceholderTarget b) =>
			{
				float prob = b.prob;
				return prob.CompareTo(a.prob);
			});
		}
	}

	public bool IsReplaceableBlockType(BlockValue _blockValue)
	{
		if (!_blockValue.isair)
		{
			return placeholders.ContainsKey(_blockValue);
		}
		return false;
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, int _blockX, int _blockZ, bool _useAlternate = false)
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_blockX, _blockZ);
		return Replace(_blockValue, _random, chunk, _blockX, 0, _blockZ, FastTags<TagGroup.Global>.none, _useAlternate);
	}

	public BlockValue Replace(BlockValue _blockValue, GameRandom _random, Chunk _chunk, int _blockX, int _blockY, int _blockZ, FastTags<TagGroup.Global> questTags, bool useAlternate = false, bool allowRandomRotation = true)
	{
		if (!placeholders.TryGetValue(_blockValue, out var value))
		{
			return _blockValue;
		}
		bool ischild = _blockValue.ischild;
		Vector3i parent = _blockValue.parent;
		BlockValue result = _blockValue;
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
			List<QuestPlaceholderEntry> list = questResetPlaceholders[_blockValue];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].QuestTag.Test_AnySet(questTags))
				{
					value = list[i].PlaceholderList;
					break;
				}
			}
		}
		Span<int> span = stackalloc int[value.Count];
		int num = 0;
		float num2 = 0f;
		string text = null;
		for (int j = 0; j < value.Count; j++)
		{
			PlaceholderTarget placeholderTarget = value[j];
			if (placeholderTarget.biomeName != null)
			{
				if (text == null)
				{
					byte biomeId = _chunk.GetBiomeId(World.toBlockXZ(_blockX), World.toBlockXZ(_blockZ));
					text = GameManager.Instance.World.Biomes.GetBiome(biomeId).m_sBiomeName;
				}
				if (!placeholderTarget.biomeName.EqualsCaseInsensitive(text))
				{
					continue;
				}
			}
			if (placeholderTarget.sandboxOption == global::SandboxOptions.SandboxOptions.Max || SandboxOptionManager.GetOptionType(placeholderTarget.sandboxOption) != BaseSandboxOption.OptionTypes.Bool || SandboxOptionManager.GetBool(placeholderTarget.sandboxOption) != placeholderTarget.invertSandbox)
			{
				span[num] = j;
				num++;
				num2 += placeholderTarget.Prob;
			}
		}
		if (num > 0 && num2 > 0f)
		{
			num--;
			int index = span[num];
			if (num > 0)
			{
				float num3 = gameRandom.RandomFloat * num2;
				for (int k = 0; k < num; k++)
				{
					int num4 = span[k];
					float prob = value[num4].Prob;
					if (num3 < prob)
					{
						index = num4;
						break;
					}
					num3 -= prob;
				}
			}
			result.type = value[index].blockValue.type;
			if (allowRandomRotation && value[index].isRandomRotation)
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

	public BlockValue Replace(BlockValueRef _bvRef, BlockValue _blockValue, GameRandom _random, bool _useAlternate = false)
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(_bvRef);
		return Replace(_bvRef, _blockValue, _random, chunk, FastTags<TagGroup.Global>.none, _useAlternate);
	}

	public BlockValue Replace(BlockValueRef _bvRef, BlockValue _blockValue, GameRandom _random, Chunk _chunk, FastTags<TagGroup.Global> questTags, bool useAlternate = false, bool allowRandomRotation = true)
	{
		return _bvRef.Type switch
		{
			BlockValueRefType.None => BlockValue.Air, 
			BlockValueRefType.Block => Replace(_blockValue, _random, _chunk, _bvRef.BlockPosition.x, _bvRef.BlockPosition.y, _bvRef.BlockPosition.z, questTags, useAlternate, allowRandomRotation), 
			BlockValueRefType.Prop => BlockValue.Air, 
			_ => throw new ArgumentOutOfRangeException("Type"), 
		};
	}
}
