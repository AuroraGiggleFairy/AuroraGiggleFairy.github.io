using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderInfo
{
	public enum TraderHourPresets
	{
		Default,
		MorningOnly,
		MidDayOnly,
		EveningOnly,
		NightOnly,
		OnlyClosedOnBM,
		AlwaysOpen
	}

	public enum TraderDaysPresets
	{
		Default,
		Everyday,
		EveryOtherDay,
		EveryThreeDays,
		EveryFourDays,
		EveryFiveDays
	}

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

	public class TraderDisplayInfo
	{
		public ulong OpenTime;

		public ulong CloseTime;

		public string OpenTimeText;

		public string CloseTimeText;

		public bool IsOpen;

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public TraderInfo info;

		public void Refresh()
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			if (info == null)
			{
				info = traderInfoList[1];
				if (info == null)
				{
					return;
				}
				OpenTime = info.GetOpenTime();
				CloseTime = info.GetCloseTime();
				OpenTimeText = GameUtils.WorldTimeToHourMinutesString(OpenTime);
				CloseTimeText = GameUtils.WorldTimeToHourMinutesString(CloseTime);
			}
			if (Time.time - lastUpdate > 3f)
			{
				ulong num = GameManager.Instance.World.worldTime % 24000;
				if (OpenTime < CloseTime)
				{
					IsOpen = OpenTime < num && num < CloseTime;
				}
				else
				{
					IsOpen = num > OpenTime || num < CloseTime;
				}
				lastUpdate = Time.time;
			}
		}

		public string GetTimeTitle()
		{
			if (!IsOpen)
			{
				return "Opens at";
			}
			return "Closes at";
		}

		public string GetTimeText()
		{
			if (!IsOpen)
			{
				return OpenTimeText;
			}
			return CloseTimeText;
		}
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

	public static bool TraderDialog = true;

	public static int GlobalResetInterval = -1;

	public static int GlobalResetIntervalInTicks = -1;

	public static int VendingResetInterval = -1;

	public static int VendingResetIntervalInTicks = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetInterval = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetIntervalInTicks = 24000;

	public int MaxItems = 50;

	public int minCount;

	public int maxCount;

	public bool AllowBuy = true;

	public bool AllowSell = true;

	public bool IsVendingMachine;

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

	public static int TraderMaxTier = 6;

	public static int TraderBuyLimit = 3;

	public static float TraderItemAbundance = 1f;

	public static float VendingItemAbundance = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClosedBM;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TraderHourPresets traderHoursPreset;

	public static TraderDaysPresets TraderDayPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalOpenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalCloseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalWarningTime;

	public List<TierItemGroup> TierItemGroups = new List<TierItemGroup>();

	public List<TraderItemEntry> traderItems = new List<TraderItemEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTime = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TraderDisplayInfo traderDisplayInfo = null;

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

	public int ResetInterval
	{
		get
		{
			if (IsVendingMachine)
			{
				if (VendingResetInterval != -1)
				{
					return VendingResetInterval;
				}
			}
			else if (GlobalResetInterval != -1)
			{
				return GlobalResetInterval;
			}
			return resetInterval;
		}
		set
		{
			resetInterval = value;
		}
	}

	public int ResetIntervalInTicks
	{
		get
		{
			if (IsVendingMachine)
			{
				if (VendingResetIntervalInTicks != -1)
				{
					return VendingResetIntervalInTicks;
				}
			}
			else if (GlobalResetIntervalInTicks != -1)
			{
				return GlobalResetIntervalInTicks;
			}
			return resetIntervalInTicks;
		}
		set
		{
			resetIntervalInTicks = value;
		}
	}

	public static TraderHourPresets TraderHoursPreset
	{
		get
		{
			return traderHoursPreset;
		}
		set
		{
			traderHoursPreset = value;
			if (traderHoursPreset != TraderHourPresets.Default)
			{
				switch (traderHoursPreset)
				{
				case TraderHourPresets.MorningOnly:
					GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 4, 1);
					GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 11, 50);
					break;
				case TraderHourPresets.MidDayOnly:
					GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 10, 1);
					GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 17, 50);
					break;
				case TraderHourPresets.EveningOnly:
					GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 16, 1);
					GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 21, 50);
					break;
				case TraderHourPresets.NightOnly:
					GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 22, 1);
					GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 3, 50);
					break;
				case TraderHourPresets.OnlyClosedOnBM:
				{
					(int duskHour, int dawnHour) tuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
					int item = tuple.duskHour;
					int item2 = tuple.dawnHour;
					GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, item2, 0);
					GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, item - 1, 50);
					break;
				}
				}
				GlobalWarningTime = GlobalCloseTime - 300;
			}
		}
	}

	public int RentTimeInSeconds => RentTimeInDays * 60 * GamePrefs.GetInt(EnumGamePrefs.DayNightLength);

	public int RentTimeInTicks => RentTimeInDays * 24000;

	public bool IsOpen
	{
		get
		{
			if (traderHoursPreset == TraderHourPresets.OnlyClosedOnBM)
			{
				int num = GameStats.GetInt(EnumGameStats.BloodMoonDay);
				int num2 = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
				if (num2 < num || num2 > num + 1)
				{
					return true;
				}
			}
			if (!UseOpenHours || traderHoursPreset == TraderHourPresets.AlwaysOpen || World.SandboxUseTraderArea != TraderAreaStates.Default)
			{
				return true;
			}
			ulong num3 = GameManager.Instance.World.worldTime % 24000;
			ulong openTime = GetOpenTime();
			ulong closeTime = GetCloseTime();
			if (openTime < closeTime)
			{
				if (openTime < num3)
				{
					return num3 < closeTime;
				}
				return false;
			}
			if (num3 <= openTime)
			{
				return num3 < closeTime;
			}
			return true;
		}
	}

	public bool IsTraderActivitiesOpen
	{
		get
		{
			if (traderHoursPreset == TraderHourPresets.OnlyClosedOnBM)
			{
				return !GameManager.Instance.World.isEventBloodMoon;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000;
			ulong openTime = GetOpenTime();
			ulong closeTime = GetCloseTime();
			if (openTime < closeTime)
			{
				if (openTime < num)
				{
					return num < closeTime;
				}
				return false;
			}
			if (num <= openTime)
			{
				return num < closeTime;
			}
			return true;
		}
	}

	public bool ShouldPlayOpenSound
	{
		get
		{
			if (traderHoursPreset == TraderHourPresets.OnlyClosedOnBM)
			{
				bool isEventBloodMoon = GameManager.Instance.World.isEventBloodMoon;
				if (lastClosedBM != isEventBloodMoon)
				{
					if (!lastClosedBM)
					{
						lastClosedBM = isEventBloodMoon;
						return false;
					}
					lastClosedBM = isEventBloodMoon;
				}
			}
			ulong openTime = GetOpenTime();
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (num > openTime)
			{
				return num < openTime + 100;
			}
			return false;
		}
	}

	public bool ShouldPlayCloseSound
	{
		get
		{
			if (traderHoursPreset == TraderHourPresets.OnlyClosedOnBM && GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) != GameStats.GetInt(EnumGameStats.BloodMoonDay))
			{
				return false;
			}
			ulong closeTime = GetCloseTime();
			ulong num = GameManager.Instance.World.worldTime % 24000;
			if (num > closeTime)
			{
				return num < closeTime + 100;
			}
			return false;
		}
	}

	public bool IsWarningTime
	{
		get
		{
			if (traderHoursPreset == TraderHourPresets.OnlyClosedOnBM && GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) != GameStats.GetInt(EnumGameStats.BloodMoonDay))
			{
				return false;
			}
			if (!UseOpenHours || traderHoursPreset == TraderHourPresets.AlwaysOpen || World.SandboxUseTraderArea != TraderAreaStates.Default)
			{
				return false;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000;
			ulong openTime = GetOpenTime();
			ulong warningTime = GetWarningTime();
			if (openTime < warningTime)
			{
				if (warningTime < num)
				{
					return num < warningTime + 100;
				}
				return false;
			}
			if (warningTime <= openTime)
			{
				return num < warningTime + 100;
			}
			return true;
		}
	}

	public ulong GetOpenTime()
	{
		if (traderHoursPreset != TraderHourPresets.Default)
		{
			return GlobalOpenTime;
		}
		return OpenTime;
	}

	public ulong GetCloseTime()
	{
		if (traderHoursPreset != TraderHourPresets.Default)
		{
			return GlobalCloseTime;
		}
		return CloseTime;
	}

	public ulong GetWarningTime()
	{
		if (traderHoursPreset != TraderHourPresets.Default)
		{
			return GlobalWarningTime;
		}
		return WarningTime;
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

	public static string GetNextTraderTime()
	{
		if (traderDisplayInfo == null)
		{
			if (traderInfoList.Length == 0)
			{
				return "";
			}
			traderDisplayInfo = new TraderDisplayInfo();
		}
		traderDisplayInfo.Refresh();
		return traderDisplayInfo.GetTimeText();
	}

	public static string GetNextTraderText()
	{
		if (traderDisplayInfo == null)
		{
			if (traderInfoList.Length == 0)
			{
				return "";
			}
			traderDisplayInfo = new TraderDisplayInfo();
		}
		traderDisplayInfo.Refresh();
		return traderDisplayInfo.GetTimeTitle();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItem(GameRandom random, TraderItemEntry template, ItemValue item, int countToSpawn, List<ItemStack> spawnedItems)
	{
		if (countToSpawn < 1)
		{
			return;
		}
		ItemClass itemClass = item.ItemClass;
		if (itemClass == null || (item.HasQuality && TraderMaxTier == 0))
		{
			return;
		}
		ItemClass itemClass2 = ItemClass.HandleSandboxTechType(itemClass);
		if (itemClass2 != itemClass)
		{
			if (itemClass2 == null)
			{
				return;
			}
			item = ItemClass.GetItem(itemClass2.Name);
			itemClass = itemClass2;
		}
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
		int num5 = template.maxQuality;
		ItemValue itemValue;
		if (item.HasQuality)
		{
			if (num4 <= -1)
			{
				num4 = 1;
				num5 = 6;
			}
			if (num5 > TraderMaxTier)
			{
				num5 = TraderMaxTier;
			}
			if (num5 < num4)
			{
				return;
			}
			itemValue = ((template == null || template.parentGroup == null || template.parentGroup.modsToInstall.Length == 0) ? new ItemValue(item.type, num4, num5, _bCreateDefaultModItems: true, template.modsToInstall, template.modChance) : new ItemValue(item.type, num4, num5, _bCreateDefaultModItems: true, template.parentGroup.modsToInstall, template.parentGroup.modChance));
		}
		else
		{
			itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
		}
		if (itemValue.ItemClass != null && itemValue.ItemClass.Actions != null && itemValue.ItemClass.Actions.Length != 0 && itemValue.ItemClass.Actions[0] != null)
		{
			itemValue.Meta = 0;
		}
		itemClass.AddGSStats(itemValue, -1, random);
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
		float num = (IsVendingMachine ? VendingItemAbundance : TraderItemAbundance);
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderItemEntry traderItemEntry = itemSet[i];
			int num2 = RandomSpawnCount(random, (int)((float)traderItemEntry.minCount * num), (int)((float)traderItemEntry.maxCount * num));
			if (traderItemEntry.group != null)
			{
				SpawnItemsFromGroup(random, traderItemEntry.group, num2, spawnedItems, traderItemEntry.uniqueOnly);
			}
			else
			{
				SpawnItem(random, traderItemEntry, traderItemEntry.item.itemValue, num2, spawnedItems);
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
						SpawnItem(random, traderItemEntry2, traderItemEntry2.item.itemValue, num3, spawnedItems);
					}
					break;
				}
			}
		}
	}
}
