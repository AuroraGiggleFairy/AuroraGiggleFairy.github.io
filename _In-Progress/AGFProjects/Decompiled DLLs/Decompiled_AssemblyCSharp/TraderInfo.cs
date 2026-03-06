using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderInfo
{
	public class TraderItem
	{
		public ItemValue itemValue;
	}

	public class TraderItemGroup
	{
		public string name;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public List<TraderItemEntry> items = new List<TraderItemEntry>();
	}

	public class TraderItemEntry
	{
		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float prob;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public TraderItem item;

		public TraderItemGroup group;

		public TraderItemGroup parentGroup;
	}

	public class TierItemGroup
	{
		public int minLevel;

		public int maxLevel;

		public int minCount;

		public int maxCount;

		public List<TraderItemEntry> traderItems = new List<TraderItemEntry>();
	}

	public static TraderInfo[] traderInfoList;

	public static Dictionary<string, TraderItemGroup> traderItemGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float buyMarkup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sellMarkdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMinMod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMaxMod;

	public int Id;

	public float SalesMarkup;

	public int ResetInterval = 1;

	public int ResetIntervalInTicks = 24000;

	public int MaxItems = 50;

	public int minCount;

	public int maxCount;

	public bool AllowBuy = true;

	public bool AllowSell = true;

	public float OverrideBuyMarkup = -1f;

	public float OverrideSellMarkdown = -1f;

	public bool UseOpenHours;

	public ulong OpenTime;

	public ulong CloseTime;

	public ulong WarningTime;

	public bool PlayerOwned;

	public bool Rentable;

	public int RentCost;

	public int RentTimeInDays;

	public List<TierItemGroup> TierItemGroups = new List<TierItemGroup>();

	public List<TraderItemEntry> traderItems = new List<TraderItemEntry>();

	public static float BuyMarkup
	{
		get
		{
			return buyMarkup;
		}
		set
		{
			buyMarkup = value;
		}
	}

	public static float SellMarkdown
	{
		get
		{
			return sellMarkdown;
		}
		set
		{
			sellMarkdown = value;
		}
	}

	public static float QualityMinMod
	{
		get
		{
			return qualityMinMod;
		}
		set
		{
			qualityMinMod = value;
		}
	}

	public static float QualityMaxMod
	{
		get
		{
			return qualityMaxMod;
		}
		set
		{
			qualityMaxMod = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static string CurrencyItem { get; set; }

	public int RentTimeInSeconds => RentTimeInDays * 60 * GamePrefs.GetInt(EnumGamePrefs.DayNightLength);

	public int RentTimeInTicks => RentTimeInDays * 24000;

	public bool IsOpen
	{
		get
		{
			if (!UseOpenHours)
			{
				return true;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (OpenTime < CloseTime)
			{
				if (OpenTime < num)
				{
					return num < CloseTime;
				}
				return false;
			}
			if (num <= OpenTime)
			{
				return num < CloseTime;
			}
			return true;
		}
	}

	public bool ShouldPlayOpenSound
	{
		get
		{
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (num > OpenTime)
			{
				return num < OpenTime + 100;
			}
			return false;
		}
	}

	public bool ShouldPlayCloseSound
	{
		get
		{
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (num > CloseTime)
			{
				return num < CloseTime + 100;
			}
			return false;
		}
	}

	public bool IsWarningTime
	{
		get
		{
			if (!UseOpenHours)
			{
				return false;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (OpenTime < WarningTime)
			{
				if (WarningTime < num)
				{
					return num < WarningTime + 100;
				}
				return false;
			}
			if (WarningTime <= OpenTime)
			{
				return num < WarningTime + 100;
			}
			return true;
		}
	}

	public static void InitStatic()
	{
		traderInfoList = new TraderInfo[256];
		traderItemGroups = new Dictionary<string, TraderItemGroup>();
	}

	public void Init()
	{
		traderInfoList[Id] = this;
	}

	public static void Cleanup()
	{
		traderInfoList = null;
		traderItemGroups = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyRandomDegradation(ref ItemValue _itemValue)
	{
		_itemValue.Meta = ItemClass.GetForId(_itemValue.type).GetInitialMetadata(_itemValue);
		int maxUseTimes = _itemValue.MaxUseTimes;
		if (maxUseTimes != 0)
		{
			float num = GameManager.Instance.World.GetGameRandom().RandomFloat * 0.6f + 0.2f;
			_itemValue.UseTimes = (int)((float)maxUseTimes * num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyQuality(ref ItemValue _itemValue, int minQuality = 1, int maxQuality = 6)
	{
		if (ItemClass.list[_itemValue.type].HasQuality || ItemClass.list[_itemValue.type].HasSubItems)
		{
			_itemValue = new ItemValue(_itemValue.type, Mathf.Clamp(minQuality, 1, maxQuality), maxQuality);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItem(TraderItemEntry template, ItemValue item, int countToSpawn, List<ItemStack> spawnedItems)
	{
		if (countToSpawn < 1 || item.ItemClass == null)
		{
			return;
		}
		ItemClass itemClass = item.ItemClass;
		countToSpawn = Math.Min(countToSpawn, itemClass.Stacknumber.Value);
		int num = (itemClass.IsBlock() ? Block.list[item.type].EconomicBundleSize : itemClass.EconomicBundleSize);
		if (itemClass.EconomicValue == -1f)
		{
			return;
		}
		if (num > 1)
		{
			int num2 = countToSpawn % num;
			if (num2 > 0)
			{
				countToSpawn -= num2;
			}
			if (countToSpawn == 0)
			{
				countToSpawn = num;
			}
		}
		if (itemClass.CanStack())
		{
			int value = ItemClass.GetForId(item.type).Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == item.type)
				{
					if (itemStack.CanStack(countToSpawn))
					{
						itemStack.count += countToSpawn;
						spawnedItems[i] = itemStack;
						return;
					}
					int num3 = value - itemStack.count;
					itemStack.count = value;
					spawnedItems[i] = itemStack;
					countToSpawn -= num3;
				}
			}
		}
		int num4 = template.minQuality;
		int maxQuality = template.maxQuality;
		ItemValue itemValue = item.Clone();
		if (item.HasQuality)
		{
			if (num4 <= -1)
			{
				num4 = 1;
				maxQuality = 6;
			}
			itemValue = ((template == null || template.parentGroup == null || template.parentGroup.modsToInstall.Length == 0) ? new ItemValue(item.type, num4, maxQuality, _bCreateDefaultModItems: true, template.modsToInstall, template.modChance) : new ItemValue(item.type, num4, maxQuality, _bCreateDefaultModItems: true, template.parentGroup.modsToInstall, template.parentGroup.modChance));
		}
		else
		{
			itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
		}
		if (itemValue.ItemClass != null && itemValue.ItemClass.Actions != null && itemValue.ItemClass.Actions.Length != 0 && itemValue.ItemClass.Actions[0] != null)
		{
			itemValue.Meta = 0;
		}
		ItemStack item2 = new ItemStack(itemValue, countToSpawn);
		spawnedItems.Add(item2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int RandomSpawnCount(GameRandom random, int min, int max)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num <= (float)min)
		{
			return min;
		}
		if (num > (float)max)
		{
			num = max;
		}
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnAllItemsFromList(GameRandom random, List<TraderItemEntry> itemSet, List<ItemStack> spawnedItems)
	{
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderItemEntry traderItemEntry = itemSet[i];
			int num = RandomSpawnCount(random, traderItemEntry.minCount, traderItemEntry.maxCount);
			if (traderItemEntry.group != null)
			{
				SpawnItemsFromGroup(random, traderItemEntry.group, num, spawnedItems, traderItemEntry.uniqueOnly);
			}
			else
			{
				SpawnItem(traderItemEntry, traderItemEntry.item.itemValue, num, spawnedItems);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItemsFromGroup(GameRandom random, TraderItemGroup group, int numToSpawn, List<ItemStack> spawnedItems, bool uniqueOnly)
	{
		List<int> usedIndices = null;
		if (group.uniqueOnly || uniqueOnly)
		{
			usedIndices = new List<int>();
		}
		for (int i = 0; i < numToSpawn; i++)
		{
			int numToSpawn2 = RandomSpawnCount(random, group.minCount, group.maxCount);
			SpawnLootItemsFromList(random, group.items, numToSpawn2, spawnedItems, usedIndices);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnLootItemsFromList(GameRandom random, List<TraderItemEntry> itemSet, int numToSpawn, List<ItemStack> spawnedItems, List<int> usedIndices)
	{
		if (numToSpawn < 1)
		{
			if (numToSpawn == -1)
			{
				SpawnAllItemsFromList(random, itemSet, spawnedItems);
			}
			return;
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderItemEntry traderItemEntry = itemSet[i];
			if (usedIndices == null || !usedIndices.Contains(i))
			{
				num += traderItemEntry.prob;
			}
		}
		if (num == 0f)
		{
			return;
		}
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				TraderItemEntry traderItemEntry2 = itemSet[k];
				if (usedIndices != null && usedIndices.Contains(k))
				{
					continue;
				}
				num2 += traderItemEntry2.prob / num;
				if (randomFloat <= num2)
				{
					int num3 = RandomSpawnCount(random, traderItemEntry2.minCount, traderItemEntry2.maxCount);
					usedIndices?.Add(k);
					if (traderItemEntry2.group != null)
					{
						SpawnItemsFromGroup(random, traderItemEntry2.group, num3, spawnedItems, traderItemEntry2.uniqueOnly);
					}
					else
					{
						SpawnItem(traderItemEntry2, traderItemEntry2.item.itemValue, num3, spawnedItems);
					}
					break;
				}
			}
		}
	}

	public List<ItemStack> Spawn(GameRandom random)
	{
		List<ItemStack> list = new List<ItemStack>();
		SpawnLootItemsFromList(random, traderItems, -1, list, null);
		return list;
	}

	public List<ItemStack> SpawnTierGroup(GameRandom random, int tierGroupIndex)
	{
		List<ItemStack> list = new List<ItemStack>();
		TierItemGroups[tierGroupIndex].traderItems.Shuffle();
		int numToSpawn = RandomSpawnCount(random, minCount, maxCount);
		SpawnLootItemsFromList(random, TierItemGroups[tierGroupIndex].traderItems, numToSpawn, list, null);
		return list;
	}
}
