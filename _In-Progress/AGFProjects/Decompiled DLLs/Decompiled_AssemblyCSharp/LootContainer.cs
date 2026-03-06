using System;
using System.Collections.Generic;
using UnityEngine;

public class LootContainer
{
	public enum DestroyOnClose
	{
		False,
		True,
		Empty
	}

	public class LootItem
	{
		public ItemValue itemValue;
	}

	public class LootGroup
	{
		public string name;

		public string lootQualityTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public float minLevel;

		public float maxLevel;

		public string[] modsToInstall;

		public float modChance = 1f;

		public readonly List<LootEntry> items = new List<LootEntry>();
	}

	public class LootEntry
	{
		public string lootProbTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float minLevel;

		public float maxLevel;

		public float prob;

		public bool forceProb;

		public string[] modsToInstall;

		public float modChance = 1f;

		public float lootstageCountMod;

		public LootItem item;

		public LootGroup group;

		public LootGroup parentGroup;

		public FastTags<TagGroup.Global> tags;

		public string[] buffsToAdd;

		public bool randomDurability = true;

		public List<BaseLootEntryRequirement> Requirements;

		public bool HasRequirements(EntityPlayer player)
		{
			if (Requirements != null)
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					if (!Requirements[i].CheckRequirement(player))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public class LootProbabilityTemplate
	{
		public string name;

		public readonly List<LootEntry> templates = new List<LootEntry>();
	}

	public class LootQualityTemplate
	{
		public string name;

		public readonly List<LootGroup> templates = new List<LootGroup>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, LootContainer> lootContainers = new CaseInsensitiveStringDictionary<LootContainer>();

	public static readonly Dictionary<string, LootGroup> lootGroups = new Dictionary<string, LootGroup>();

	public static readonly Dictionary<string, LootQualityTemplate> lootQualityTemplates = new Dictionary<string, LootQualityTemplate>();

	public static readonly Dictionary<string, LootProbabilityTemplate> lootProbTemplates = new Dictionary<string, LootProbabilityTemplate>();

	public string Name;

	public string soundOpen;

	public string soundClose;

	public Vector2i size;

	public float openTime;

	public int minCount;

	public int maxCount;

	public DestroyOnClose destroyOnClose;

	public string lootQualityTemplate;

	public List<string> BuffActions;

	public string OnOpenEvent = "";

	public bool ignoreLootAbundance;

	public bool useUnmodifiedLootstage;

	public bool UniqueItems;

	public bool IgnoreLootProb;

	public readonly List<LootEntry> itemsToSpawn = new List<LootEntry>();

	public static Dictionary<EntityPlayer, string[]> OverrideItems = new Dictionary<EntityPlayer, string[]>();

	public static void InitStatic()
	{
		Cleanup();
	}

	public void Init()
	{
		lootContainers[Name] = this;
	}

	public static void Cleanup()
	{
		lootContainers.Clear();
		lootGroups.Clear();
		lootQualityTemplates.Clear();
		lootProbTemplates.Clear();
	}

	public static bool IsLoaded()
	{
		return lootContainers.Count > 0;
	}

	public static LootContainer GetLootContainer(string _name, bool _errorOnMiss = true)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return null;
		}
		if (lootContainers.TryGetValue(_name, out var value))
		{
			return value;
		}
		if (_errorOnMiss)
		{
			Log.Error("LootContainer '" + _name + "' unknown");
		}
		return null;
	}

	public static ItemStack GetRewardItem(string lootGroup, float questDifficulty)
	{
		if (!lootGroups.ContainsKey(lootGroup))
		{
			return ItemStack.Empty.Clone();
		}
		List<ItemStack> list = new List<ItemStack>();
		int slotsLeft = 1;
		SpawnItemsFromGroup(GameManager.Instance.lootManager.Random, lootGroups[lootGroup], 1, 1f, list, ref slotsLeft, questDifficulty, 0f, lootGroups[lootGroup].lootQualityTemplate, null, FastTags<TagGroup.Global>.none, uniqueItems: true, ignoreLootProb: true, _forceStacking: false, null);
		if (list.Count == 0)
		{
			return ItemStack.Empty.Clone();
		}
		return list[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnItem(GameRandom random, LootEntry template, ItemValue lootItemValue, int countToSpawn, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool _forceStacking)
	{
		if (lootItemValue.ItemClass == null)
		{
			return false;
		}
		if (player != null)
		{
			countToSpawn = Math.Min((int)EffectManager.GetValue(PassiveEffects.LootQuantity, player.inventory.holdingItemItemValue, countToSpawn, player, null, lootItemValue.ItemClass.ItemTags | containerTags), lootItemValue.ItemClass.Stacknumber.Value);
		}
		if (countToSpawn < 1)
		{
			return false;
		}
		if (lootItemValue.ItemClass.CanStack())
		{
			int value = lootItemValue.ItemClass.Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == lootItemValue.type)
				{
					if (itemStack.CanStack(countToSpawn) || _forceStacking)
					{
						itemStack.count += countToSpawn;
						return true;
					}
					int num = value - itemStack.count;
					itemStack.count = value;
					countToSpawn -= num;
				}
			}
		}
		if (slotsLeft < 1)
		{
			return false;
		}
		int num2 = template.minQuality;
		int maxQuality = template.maxQuality;
		string text = lootQualityTemplate;
		if (string.IsNullOrEmpty(text) || template.parentGroup?.lootQualityTemplate != null)
		{
			text = template.parentGroup?.lootQualityTemplate;
		}
		if (!string.IsNullOrEmpty(text))
		{
			bool flag = false;
			for (int j = 0; j < lootQualityTemplates[text].templates.Count; j++)
			{
				_ = random.RandomFloat;
				LootGroup lootGroup = lootQualityTemplates[text].templates[j];
				num2 = lootGroup.minQuality;
				maxQuality = lootGroup.maxQuality;
				if (!(lootGroup.minLevel <= gameStage) || !(lootGroup.maxLevel >= gameStage))
				{
					continue;
				}
				for (int k = 0; k < lootGroup.items.Count; k++)
				{
					LootEntry lootEntry = lootGroup.items[k];
					if (random.RandomFloat <= lootEntry.prob)
					{
						num2 = lootEntry.minQuality;
						maxQuality = lootEntry.maxQuality;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		string[] modsToInstall = template.modsToInstall;
		float modChance = template.modChance;
		if (template.parentGroup != null && template.parentGroup.modsToInstall.Length != 0)
		{
			modsToInstall = template.parentGroup.modsToInstall;
			modChance = template.parentGroup.modChance;
		}
		ItemValue itemValue;
		if (lootItemValue.HasQuality)
		{
			if (num2 <= -1)
			{
				num2 = 1;
				maxQuality = 6;
			}
			itemValue = new ItemValue(lootItemValue.type, num2, maxQuality, _bCreateDefaultModItems: true, modsToInstall, modChance);
		}
		else
		{
			itemValue = new ItemValue(lootItemValue.type, 1, 6, _bCreateDefaultModItems: true, modsToInstall, modChance);
		}
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] != null)
			{
				itemValue.Meta = 0;
			}
			if (itemValue.MaxUseTimes > 0)
			{
				if (template.randomDurability)
				{
					itemValue.UseTimes = (int)((float)itemValue.MaxUseTimes * random.RandomRange(0.2f, 0.8f));
				}
				else
				{
					itemValue.UseTimes = 0f;
				}
			}
		}
		ItemStack itemStack2 = null;
		if (player != null)
		{
			if (!OverrideItems.ContainsKey(player))
			{
				itemStack2 = new ItemStack(itemValue, countToSpawn);
			}
			else
			{
				string[] array = OverrideItems[player];
				itemStack2 = new ItemStack(ItemClass.GetItem(array[random.RandomRange(array.Length)]), 1);
			}
		}
		else
		{
			itemStack2 = new ItemStack(itemValue, countToSpawn);
		}
		spawnedItems.Add(itemStack2);
		slotsLeft--;
		return true;
	}

	public static int RandomSpawnCount(GameRandom random, int min, int max, float abundance)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num < (float)min)
		{
			num = min;
		}
		if (num > (float)max)
		{
			num = max;
		}
		num *= abundance;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnAllItemsFromList(GameRandom random, List<LootEntry> itemSet, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float playerLevelPercentage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, List<string> _buffsToAdd)
	{
		bool flag = false;
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootEntry lootEntry = itemSet[i];
			bool flag2 = false;
			if (player != null && !lootEntry.HasRequirements(player))
			{
				continue;
			}
			if (!lootEntry.forceProb || random.RandomFloat <= getProbability(player, lootEntry, playerLevelPercentage, ignoreLootProb))
			{
				int num = RandomSpawnCount(random, lootEntry.minCount, lootEntry.maxCount, (lootEntry.group == null) ? abundance : 1f);
				if (lootEntry.group != null)
				{
					if (lootEntry.group.minLevel <= playerLevelPercentage && lootEntry.group.maxLevel >= playerLevelPercentage)
					{
						flag2 = SpawnItemsFromGroup(random, lootEntry.group, num, abundance, spawnedItems, ref slotsLeft, playerLevelPercentage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _buffsToAdd);
					}
				}
				else
				{
					flag2 = SpawnItem(random, lootEntry, lootEntry.item.itemValue, num, spawnedItems, ref slotsLeft, playerLevelPercentage, lootQualityTemplate, player, containerTags, _forceStacking);
				}
			}
			flag = flag || flag2;
			if (flag2 && lootEntry.buffsToAdd != null)
			{
				_buffsToAdd.AddRange(lootEntry.buffsToAdd);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnItemsFromGroup(GameRandom random, LootGroup group, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, List<string> _buffsToAdd)
	{
		bool flag = false;
		for (int i = 0; i < numToSpawn; i++)
		{
			if (slotsLeft <= 0)
			{
				break;
			}
			flag |= SpawnLootItemsFromList(random, group.items, RandomSpawnCount(random, group.minCount, group.maxCount, 1f), abundance, spawnedItems, ref slotsLeft, gameStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _buffsToAdd);
		}
		return flag;
	}

	public static bool SpawnLootItemsFromList(GameRandom random, List<LootEntry> itemSet, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float lootStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, List<string> _buffsToAdd)
	{
		if (numToSpawn < 1)
		{
			if (numToSpawn == -1)
			{
				return SpawnAllItemsFromList(random, itemSet, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _buffsToAdd);
			}
			return false;
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootEntry lootEntry = itemSet[i];
			if (!lootEntry.forceProb)
			{
				num += getProbability(player, lootEntry, lootStage, ignoreLootProb);
			}
		}
		if (num == 0f)
		{
			return false;
		}
		List<int> list = new List<int>();
		bool flag = false;
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				bool flag2 = false;
				LootEntry lootEntry2 = itemSet[k];
				if (list.Contains(k) && (lootEntry2.forceProb || uniqueItems))
				{
					continue;
				}
				float probability = getProbability(player, lootEntry2, lootStage, ignoreLootProb);
				bool flag3;
				if (lootEntry2.forceProb)
				{
					flag3 = random.RandomFloat <= probability;
				}
				else
				{
					num2 += probability / num;
					flag3 = randomFloat <= num2 + rareLootChance;
				}
				if (!flag3)
				{
					continue;
				}
				list.Add(k);
				if (uniqueItems)
				{
					num -= getProbability(player, lootEntry2, lootStage, ignoreLootProb);
				}
				int num3 = RandomSpawnCount(random, lootEntry2.minCount, lootEntry2.maxCount, (lootEntry2.group == null) ? abundance : 1f);
				num3 += Mathf.RoundToInt((float)num3 * (lootEntry2.lootstageCountMod * lootStage));
				if (lootEntry2.group != null)
				{
					if (lootEntry2.group.minLevel <= lootStage && lootEntry2.group.maxLevel >= lootStage)
					{
						flag2 = SpawnItemsFromGroup(random, lootEntry2.group, num3, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _buffsToAdd);
					}
				}
				else
				{
					flag2 = SpawnItem(random, lootEntry2, lootEntry2.item.itemValue, num3, spawnedItems, ref slotsLeft, lootStage, lootQualityTemplate, player, containerTags, _forceStacking);
				}
				flag = flag || flag2;
				if (flag2 && lootEntry2.buffsToAdd != null)
				{
					_buffsToAdd.AddRange(lootEntry2.buffsToAdd);
				}
				break;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float getProbability(EntityPlayer _player, LootEntry _item, float _lootstage, bool _ignoreLootProb)
	{
		if (_player != null && !_item.HasRequirements(_player))
		{
			return 0f;
		}
		if (_item.lootProbTemplate != string.Empty && lootProbTemplates.ContainsKey(_item.lootProbTemplate))
		{
			LootProbabilityTemplate lootProbabilityTemplate = lootProbTemplates[_item.lootProbTemplate];
			for (int i = 0; i < lootProbabilityTemplate.templates.Count; i++)
			{
				LootEntry lootEntry = lootProbabilityTemplate.templates[i];
				if (!(lootEntry.minLevel <= _lootstage) || !(lootEntry.maxLevel >= _lootstage))
				{
					continue;
				}
				if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
				{
					if (_ignoreLootProb)
					{
						return lootEntry.prob;
					}
					return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags);
				}
				if (_item.tags.IsEmpty)
				{
					return lootEntry.prob;
				}
				return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.tags);
			}
		}
		if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
		{
			if (_ignoreLootProb)
			{
				return _item.prob;
			}
			return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags);
		}
		if (_item.tags.IsEmpty)
		{
			return _item.prob;
		}
		return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.tags);
	}

	public void ExecuteBuffActions(int instigatorId, EntityAlive target)
	{
		if (BuffActions != null)
		{
			for (int i = 0; i < BuffActions.Count; i++)
			{
				target.Buffs.AddBuff(BuffActions[i]);
			}
		}
	}

	public IList<ItemStack> Spawn(GameRandom random, int _maxItems, float playerLevelPercentage, float rareLootChance, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb)
	{
		List<ItemStack> list = new List<ItemStack>();
		int numToSpawn = Mathf.Min(RandomSpawnCount(random, minCount, maxCount, 1f), _maxItems);
		float abundance = 1f;
		if (!ignoreLootAbundance)
		{
			abundance = (float)GamePrefs.GetInt(EnumGamePrefs.LootAbundance) * 0.01f;
		}
		List<string> list2 = new List<string>();
		if (SpawnLootItemsFromList(random, itemsToSpawn, numToSpawn, abundance, list, ref _maxItems, playerLevelPercentage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking: false, list2))
		{
			foreach (string item in list2)
			{
				player.Buffs.AddBuff(item);
			}
		}
		return list;
	}
}
