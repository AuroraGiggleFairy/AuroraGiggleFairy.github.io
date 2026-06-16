using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ItemValue : IEquatable<ItemValue>
{
	public struct Stat(PassiveEffects _type, int _base, int _added)
	{
		public PassiveEffects type = _type;

		public bool isBoosted = _added != 0;

		public short value = (short)(_base + _added);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int EmptySaveVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte MaxModifications = byte.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ItemValue[] emptyItemValueArray = new ItemValue[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> noPreinstallCosmeticItemTags = FastTags<TagGroup.Global>.Parse("weapon,tool,armor");

	public int type;

	public byte Activated;

	public byte SelectedAmmoTypeIndex;

	public float UseTimes;

	public int Meta;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TypedMetadataValue> Metadata;

	public ushort Quality;

	public Stat[] Stats;

	public ItemValue[] Modifications;

	public ItemValue[] CosmeticMods;

	public ushort Seed;

	public TextureFullArray TextureFullArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDurabilityMetaName = "DurabilityModifier";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime baseDate = new DateTime(2013, 10, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsItem = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsStats = 2;

	public static ItemValue None => new ItemValue(0);

	public int MaxUseTimes => ModMaxUseTimes(MaxUseTimesBase, this);

	public int MaxUseTimesUI => MaxUseTimesBase;

	public int MaxUseTimesBase
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (int)EffectManager.GetValue(PassiveEffects.DegradationMax, this, 0f, null, null, ItemClass?.ItemTags ?? FastTags<TagGroup.Global>.none);
		}
	}

	public float MaxDurabilityModifier
	{
		get
		{
			if (!TryGetMetadata("DurabilityModifier", out float value))
			{
				return 1f;
			}
			return value;
		}
		set
		{
			if (value == 1f)
			{
				RemoveMetaData("DurabilityModifier");
			}
			else
			{
				SetMetadata("DurabilityModifier", value);
			}
		}
	}

	public float PercentUsesLeft
	{
		get
		{
			int maxUseTimes = MaxUseTimes;
			if (maxUseTimes > 0)
			{
				return 1f - Utils.FastClamp01(UseTimes / (float)maxUseTimes);
			}
			return 1f;
		}
	}

	public bool HasQuality
	{
		get
		{
			ItemClass itemClass = ItemClass;
			if (itemClass != null)
			{
				if (!itemClass.HasQuality)
				{
					return itemClass is ItemClassModifier;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasModSlots => Modifications.Length != 0;

	public bool IsMod
	{
		get
		{
			ItemClass itemClass = ItemClass;
			if (itemClass != null)
			{
				return itemClass is ItemClassModifier;
			}
			return false;
		}
	}

	public bool IsShapeHelperBlock
	{
		get
		{
			if (ItemClass is ItemClassBlock itemClassBlock)
			{
				return itemClassBlock.GetBlock().SelectAlternates;
			}
			return false;
		}
	}

	public ItemClass ItemClass
	{
		get
		{
			if (type >= 0 && ItemClass.list != null && type < ItemClass.list.Length)
			{
				ItemClass itemClass = ItemClass.list[type];
				if (itemClass is ItemClassQuest)
				{
					return ItemClassQuest.GetItemQuestById(Seed);
				}
				return itemClass;
			}
			return null;
		}
	}

	public ItemClass ItemClassOrMissing
	{
		get
		{
			ItemClass itemClass = ItemClass;
			if (itemClass != null)
			{
				return itemClass;
			}
			return ItemClass.MissingItem;
		}
	}

	public static int ModMaxUseTimes(int _value, ItemValue _iv)
	{
		if (_value <= 0)
		{
			return _value;
		}
		if (!_iv.TryGetMetadata("DurabilityModifier", out float value))
		{
			return _value;
		}
		_value = Mathf.RoundToInt((float)_value * value);
		_value = Mathf.Max(_value, 1);
		return _value;
	}

	public bool TryGetMetadata(string key, out int value)
	{
		if (!TryGetMetadata(key, out var value2, TypedMetadataValue.TypeTag.Integer))
		{
			value = 0;
			return false;
		}
		value = (int)value2;
		return true;
	}

	public bool TryGetMetadata(string key, out float value)
	{
		if (!TryGetMetadata(key, out var value2, TypedMetadataValue.TypeTag.Float))
		{
			value = 0f;
			return false;
		}
		value = (float)value2;
		return true;
	}

	public bool TryGetMetadata(string key, out string value)
	{
		if (!TryGetMetadata(key, out var value2, TypedMetadataValue.TypeTag.String))
		{
			value = null;
			return false;
		}
		value = (string)value2;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMetadata(string key, out object value, TypedMetadataValue.TypeTag typeTag = TypedMetadataValue.TypeTag.None)
	{
		if (Metadata == null)
		{
			value = null;
			return false;
		}
		if (!Metadata.TryGetValue(key, out var value2))
		{
			value = null;
			return false;
		}
		if (typeTag != TypedMetadataValue.TypeTag.None && value2.GetTypeTag() != typeTag)
		{
			value = null;
			return false;
		}
		value = value2.GetValue();
		return true;
	}

	public bool HasMetadata(string key, TypedMetadataValue.TypeTag typeTag = TypedMetadataValue.TypeTag.None)
	{
		if (Metadata == null)
		{
			return false;
		}
		if (typeTag == TypedMetadataValue.TypeTag.None)
		{
			return Metadata.ContainsKey(key);
		}
		if (!Metadata.TryGetValue(key, out var value))
		{
			return false;
		}
		return value.GetTypeTag() == typeTag;
	}

	public object GetMetadata(string key)
	{
		if (Metadata == null)
		{
			return false;
		}
		if (Metadata.TryGetValue(key, out var value))
		{
			return value.GetValue();
		}
		return null;
	}

	public void SetMetadata(string key, int value)
	{
		SetMetadata(key, value, TypedMetadataValue.TypeTag.Integer);
	}

	public void SetMetadata(string key, float value)
	{
		SetMetadata(key, value, TypedMetadataValue.TypeTag.Float);
	}

	public void SetMetadata(string key, string value)
	{
		SetMetadata(key, value, TypedMetadataValue.TypeTag.String);
	}

	public void SetMetadata(string key, object value, string typeTag)
	{
		SetMetadata(key, value, TypedMetadataValue.StringToTag(typeTag));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMetadata(string key, object value, TypedMetadataValue.TypeTag typeTag)
	{
		if (Metadata == null)
		{
			Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		TypedMetadataValue result;
		if (Metadata.TryGetValue(key, out var value2))
		{
			if (!value2.SetValue(value))
			{
				Log.Warning($"Can not update Metadata value '{key}' on ItemValue, type of value '{value}' ({value.GetType().Name}) does not match existing TypeTag ({value2.GetTypeTag()}). From: {StackTraceUtility.ExtractStackTrace()}");
			}
		}
		else if (TypedMetadataValue.TryCreate(value, typeTag, out result))
		{
			Metadata.Add(key, result);
		}
		else
		{
			Log.Warning($"Can not set Metadata key '{key}' on ItemValue, type of value '{value}' ({value.GetType().Name}) does not match TypeTag ({typeTag}). From: {StackTraceUtility.ExtractStackTrace()}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMetadata(string key, TypedMetadataValue tmv)
	{
		if (Metadata == null)
		{
			Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		if (Metadata.TryGetValue(key, out var value))
		{
			object value2 = tmv.GetValue();
			if (!value.SetValue(value2))
			{
				Log.Warning($"Can not update Metadata value '{key}' on ItemValue, type of value '{value2}' ({value2.GetType().Name}) does not match existing TypeTag ({value.GetTypeTag()}). From: {StackTraceUtility.ExtractStackTrace()}");
			}
		}
		else
		{
			Metadata.Add(key, tmv);
		}
	}

	public bool RemoveMetaData(string name)
	{
		bool result = false;
		if (Metadata != null)
		{
			result = Metadata.Remove(name);
		}
		return result;
	}

	public ItemValue()
	{
		Modifications = emptyItemValueArray;
		CosmeticMods = emptyItemValueArray;
	}

	public ItemValue(int _type, bool _bCreateDefaultParts = false)
		: this(_type, 1, 6, _bCreateDefaultParts)
	{
	}

	public ItemValue(int _type, int minQuality, int maxQuality, bool _bCreateDefaultModItems = false, string[] modsToInstall = null, float modInstallDescendingChance = 1f)
	{
		type = _type;
		Modifications = emptyItemValueArray;
		CosmeticMods = emptyItemValueArray;
		if (type == 0)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		Seed = (ushort)((utcNow - baseDate).Seconds + utcNow.Millisecond + type);
		if (!ThreadManager.IsMainThread())
		{
			return;
		}
		ItemClass itemClass = ItemClass;
		if (itemClass == null)
		{
			return;
		}
		GameRandom gameRandom = null;
		if (itemClass.HasQuality)
		{
			if (minQuality == maxQuality)
			{
				Quality = (ushort)minQuality;
			}
			else
			{
				gameRandom = GameRandomManager.Instance.CreateGameRandom(Seed);
				Quality = (ushort)gameRandom.RandomRange(minQuality, maxQuality + 1);
			}
		}
		if (itemClass is ItemClassModifier)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		if (itemClass.Stacknumber.Value > 1)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		Modifications = new ItemValue[CalcModSlotCount()];
		CosmeticMods = new ItemValue[itemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
		if (_bCreateDefaultModItems)
		{
			if (gameRandom == null)
			{
				gameRandom = GameRandomManager.Instance.CreateGameRandom(Seed);
			}
			createDefaultModItems(itemClass, gameRandom, modsToInstall, modInstallDescendingChance);
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public void Clear()
	{
		type = 0;
		UseTimes = 0f;
		Quality = 0;
		Meta = 0;
		Seed = 0;
		Stats = null;
		Modifications = new ItemValue[0];
		CosmeticMods = new ItemValue[0];
		Metadata = null;
		SelectedAmmoTypeIndex = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CalcModSlotCount()
	{
		return Utils.FastMin(255, (int)EffectManager.GetValue(PassiveEffects.ModSlots, this, Utils.FastMax(0, Quality - 1)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createDefaultModItems(ItemClass ic, GameRandom random, string[] modsToInstall, float modInstallDescendingChance)
	{
		FastTags<TagGroup.Global> none = FastTags<TagGroup.Global>.none;
		bool flag = false;
		bool flag2 = false;
		if (modsToInstall != null && modsToInstall.Length != 0)
		{
			float num = modInstallDescendingChance;
			if (!ic.ItemTags.IsEmpty)
			{
				int num2 = 0;
				for (int i = 0; i < modsToInstall.Length; i++)
				{
					ItemClassModifier itemClassModifier = ItemClass.GetItemClass(modsToInstall[i], _caseInsensitive: true) as ItemClassModifier;
					if (itemClassModifier == null)
					{
						itemClassModifier = ItemClassModifier.GetDesiredItemModWithAnyTags(ic.ItemTags, none, FastTags<TagGroup.Global>.Parse(modsToInstall[i]), random);
					}
					if (itemClassModifier == null)
					{
						continue;
					}
					if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
					{
						flag = true;
						if (!flag2 && !(random.RandomFloat > modInstallDescendingChance))
						{
							CosmeticMods[0] = new ItemValue(itemClassModifier.Id);
							none |= itemClassModifier.ItemTags;
							flag2 = true;
							Log.Warning("ItemValue createDefaultModItems cosmetic {0}", CosmeticMods[0]);
						}
					}
					else if (num2 < Modifications.Length && !(random.RandomFloat > num))
					{
						Modifications[num2] = new ItemValue(itemClassModifier.Id);
						none |= itemClassModifier.ItemTags;
						num2++;
						num *= 0.5f;
					}
				}
				for (int j = num2; j < Modifications.Length; j++)
				{
					Modifications[j] = None;
				}
			}
		}
		if (flag || ic.HasAnyTags(noPreinstallCosmeticItemTags))
		{
			return;
		}
		for (int k = 0; k < CosmeticMods.Length; k++)
		{
			ItemClassModifier cosmeticItemMod = ItemClassModifier.GetCosmeticItemMod(ic.ItemTags, none, random);
			if (cosmeticItemMod != null)
			{
				CosmeticMods[k] = new ItemValue(cosmeticItemMod.Id);
				none |= cosmeticItemMod.ItemTags;
			}
			else
			{
				CosmeticMods[k] = None;
			}
		}
	}

	public void ModifyValue(EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags, bool _useMods = true, bool _useDurability = false)
	{
		if (_originalItemValue != null && _originalItemValue.Equals(this))
		{
			return;
		}
		int seed = MinEventParams.CachedEventParam.Seed;
		if (_entity != null)
		{
			seed = _entity.MinEventContext.Seed;
		}
		ItemClass itemClass = ItemClass;
		if (itemClass != null)
		{
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass itemClass2 = ItemClass.GetItemClass(magazineItemNames[SelectedAmmoTypeIndex]);
					if (itemClass2 != null && itemClass2.Effects != null)
					{
						itemClass2.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, 0f, _tags);
					}
				}
			}
			if (itemClass.Effects != null)
			{
				ItemValue itemValue = MinEventParams.CachedEventParam.ItemValue;
				ItemValue itemValue2 = ((_entity != null) ? _entity.MinEventContext.ItemValue : null);
				MinEventParams.CachedEventParam.Seed = (int)Seed + (int)((Seed != 0) ? _passiveEffect : PassiveEffects.None);
				MinEventParams.CachedEventParam.ItemValue = this;
				if (_entity != null)
				{
					_entity.MinEventContext.Seed = MinEventParams.CachedEventParam.Seed;
					_entity.MinEventContext.ItemValue = this;
				}
				float num = _originalValue;
				itemClass.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, (int)Quality, _tags);
				if (_useDurability)
				{
					switch (_passiveEffect)
					{
					case PassiveEffects.PhysicalDamageResist:
						if (PercentUsesLeft < 0.5f)
						{
							float num3 = _originalValue - num;
							_originalValue = num + num3 * PercentUsesLeft * 2f;
						}
						break;
					case PassiveEffects.ElementalDamageResist:
						if (PercentUsesLeft < 0.5f)
						{
							float num4 = _originalValue - num;
							_originalValue = num + num4 * PercentUsesLeft * 2f;
						}
						break;
					case PassiveEffects.BuffResistance:
						if (PercentUsesLeft < 0.5f)
						{
							float num2 = _originalValue - num;
							_originalValue = num + num2 * PercentUsesLeft * 2f;
						}
						break;
					}
				}
				MinEventParams.CachedEventParam.ItemValue = itemValue;
				if (_entity != null)
				{
					_entity.MinEventContext.ItemValue = itemValue2;
				}
			}
		}
		if (Stats != null)
		{
			StatModifyValue(_passiveEffect, ref _originalValue);
		}
		if (_useMods)
		{
			for (int i = 0; i < CosmeticMods.Length; i++)
			{
				if (CosmeticMods[i] != null && CosmeticMods[i].ItemClass is ItemClassModifier)
				{
					CosmeticMods[i].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
			for (int j = 0; j < Modifications.Length; j++)
			{
				if (Modifications[j] != null && Modifications[j].ItemClass is ItemClassModifier)
				{
					Modifications[j].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
		}
		MinEventParams.CachedEventParam.Seed = seed;
		if (_entity != null)
		{
			_entity.MinEventContext.Seed = seed;
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (_originalItemValue != null && _originalItemValue.Equals(this))
		{
			return;
		}
		ItemClass itemClass = ItemClass;
		if (itemClass != null)
		{
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[SelectedAmmoTypeIndex]).GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Ammo, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
			if (itemClass.Effects != null)
			{
				itemClass.Effects.GetModifiedValueData(_modValueSources, _sourceType, _entity, _passiveEffect, ref _originalValue, ref _perc_value, (int)Quality, _tags);
			}
		}
		for (int i = 0; i < CosmeticMods.Length; i++)
		{
			if (CosmeticMods[i] != null && CosmeticMods[i].ItemClass is ItemClassModifier)
			{
				CosmeticMods[i].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.CosmeticMod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
			}
		}
		for (int j = 0; j < Modifications.Length; j++)
		{
			if (Modifications[j] != null && Modifications[j].ItemClass is ItemClassModifier)
			{
				Modifications[j].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Mod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		ItemClass itemClass = ItemClass;
		if (itemClass != null)
		{
			if (itemClass is ItemClassModifier && itemClass.Effects != null)
			{
				itemClass.Effects.FireEvent(_eventType, _eventParms);
				return;
			}
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[SelectedAmmoTypeIndex]).FireEvent(_eventType, _eventParms);
				}
			}
			itemClass.FireEvent(_eventType, _eventParms);
		}
		if (!HasQuality)
		{
			return;
		}
		for (int i = 0; i < Modifications.Length; i++)
		{
			if (Modifications[i] != null)
			{
				Modifications[i].FireEvent(_eventType, _eventParms);
			}
		}
		for (int j = 0; j < CosmeticMods.Length; j++)
		{
			if (CosmeticMods[j] != null)
			{
				CosmeticMods[j].FireEvent(_eventType, _eventParms);
			}
		}
	}

	public ItemValue Clone()
	{
		ItemValue itemValue = new ItemValue(type);
		itemValue.Meta = Meta;
		itemValue.UseTimes = UseTimes;
		itemValue.Quality = Quality;
		itemValue.SelectedAmmoTypeIndex = SelectedAmmoTypeIndex;
		if (Stats != null)
		{
			itemValue.Stats = new Stat[Stats.Length];
			Array.Copy(Stats, itemValue.Stats, Stats.Length);
		}
		CloneModsTo(itemValue);
		if (Metadata != null)
		{
			itemValue.Metadata = new Dictionary<string, TypedMetadataValue>();
			foreach (KeyValuePair<string, TypedMetadataValue> item in Metadata)
			{
				itemValue.Metadata.Add(item.Key, item.Value?.Clone());
			}
		}
		CloneCosmeticModsTo(itemValue);
		itemValue.Activated = Activated;
		if (itemValue.type == 0)
		{
			Seed = 0;
		}
		itemValue.Seed = Seed;
		itemValue.TextureFullArray = TextureFullArray;
		return itemValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloneModsTo(ItemValue _iv)
	{
		int num = Modifications.Length;
		_iv.Modifications = new ItemValue[num];
		for (int i = 0; i < num; i++)
		{
			_iv.Modifications[i] = Modifications[i]?.Clone();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloneCosmeticModsTo(ItemValue _iv)
	{
		int num = CosmeticMods.Length;
		_iv.CosmeticMods = new ItemValue[num];
		for (int i = 0; i < num; i++)
		{
			_iv.CosmeticMods[i] = CosmeticMods[i]?.Clone();
		}
	}

	public bool IsEmpty()
	{
		return type == 0;
	}

	public BlockValue ToBlockValue(bool _allowAlternates = false)
	{
		if (type < Block.ItemsStartHere)
		{
			BlockValue result = new BlockValue
			{
				type = type
			};
			if (_allowAlternates && result.Block.SelectAlternates)
			{
				return result.Block.GetAltBlockValue(Meta);
			}
			return result;
		}
		return BlockValue.Air;
	}

	public void MergeBest(ItemValue _iv)
	{
		int maxUseTimes = MaxUseTimes;
		int maxUseTimes2 = _iv.MaxUseTimes;
		float num = (float)maxUseTimes - UseTimes;
		float num2 = (float)maxUseTimes2 - _iv.UseTimes;
		if (ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.CombineOnly)
		{
			int num3 = Utils.FastMax(maxUseTimes, maxUseTimes2);
			float num4 = num + num2;
			UseTimes = Utils.FastMax(0f, (float)num3 - num4);
		}
		else
		{
			bool flag = false;
			if (_iv.Quality == Quality)
			{
				flag = num2 > num;
				if (flag)
				{
					MaxDurabilityModifier = _iv.MaxDurabilityModifier;
				}
			}
			else if (_iv.Quality > Quality)
			{
				flag = true;
			}
			if (flag)
			{
				UseTimes = Utils.FastMax(0f, (float)maxUseTimes2 - num2);
			}
		}
		if (_iv.Quality > Quality)
		{
			Quality = _iv.Quality;
			MaxDurabilityModifier = _iv.MaxDurabilityModifier;
		}
		MergeBestStats(_iv);
		if (_iv.HasMods())
		{
			_iv.CloneModsTo(this);
		}
		if (_iv.HasCosmetics())
		{
			_iv.CloneCosmeticModsTo(this);
		}
		int newSize = CalcModSlotCount();
		Array.Resize(ref Modifications, newSize);
	}

	public void InitStats(int _count)
	{
		Stats = new Stat[_count];
	}

	public void ClearStats()
	{
		Stats = null;
	}

	public void RemoveUnusedStats()
	{
		if (Stats == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < Stats.Length; i++)
		{
			if (Stats[i].value != 0)
			{
				num++;
			}
		}
		if (num == 0)
		{
			ClearStats();
		}
		else
		{
			if (num >= Stats.Length)
			{
				return;
			}
			Stat[] stats = Stats;
			Stats = new Stat[num];
			int num2 = 0;
			for (int j = 0; j < stats.Length; j++)
			{
				if (stats[j].value != 0)
				{
					Stats[num2++] = stats[j];
				}
			}
		}
	}

	public bool HasStats()
	{
		return Stats != null;
	}

	public bool HasAnyBoostedStats()
	{
		if (Stats != null)
		{
			for (int i = 0; i < Stats.Length; i++)
			{
				if (Stats[i].isBoosted)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetStatPercent(PassiveEffects _type, bool _onlyBoosted)
	{
		float _originalValue = 1f;
		if (Stats != null)
		{
			StatModifyValue(_type, ref _originalValue, _onlyBoosted);
		}
		return _originalValue;
	}

	public void SetStat(int _index, PassiveEffects _type, int _base, int _added)
	{
		Stats[_index] = new Stat(_type, _base, _added);
	}

	public void MergeBestStats(ItemValue _iv)
	{
		if (_iv.Stats == null)
		{
			return;
		}
		if (Stats == null)
		{
			Stats = new Stat[_iv.Stats.Length];
			Array.Copy(_iv.Stats, Stats, _iv.Stats.Length);
			return;
		}
		List<Stat> list = new List<Stat>(Stats);
		for (int i = 0; i < _iv.Stats.Length; i++)
		{
			Stat item = _iv.Stats[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				Stat value = list[j];
				if (item.type == value.type)
				{
					bool flag2 = IsStatLowerBetter(value.type);
					if (item.value != value.value && item.value > value.value != flag2)
					{
						value.value = item.value;
						value.isBoosted = item.isBoosted;
						list[j] = value;
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		Stats = list.ToArray();
	}

	public static bool IsStatLowerBetter(PassiveEffects _type)
	{
		if (_type != PassiveEffects.StaminaLoss)
		{
			return _type == PassiveEffects.TargetArmor;
		}
		return true;
	}

	public void StatModifyValue(PassiveEffects _passiveEffect, ref float _originalValue, bool onlyBoosted = false)
	{
		for (int i = 0; i < Stats.Length; i++)
		{
			Stat stat = Stats[i];
			if (stat.type == _passiveEffect)
			{
				int num = ((!onlyBoosted || stat.isBoosted) ? stat.value : 0);
				float num2 = 1f + (float)num * 0.005f;
				_originalValue *= num2;
				break;
			}
		}
	}

	public void ReadOld(BinaryReader _br)
	{
	}

	public static ItemValue ReadOrNull(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			return null;
		}
		ItemValue itemValue = new ItemValue();
		itemValue.ReadData(_br, b);
		return itemValue;
	}

	public void Read(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			type = 0;
		}
		else
		{
			ReadData(_br, b);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadData(BinaryReader _br, int version)
	{
		int num = 0;
		if (version >= 8)
		{
			num = _br.ReadByte();
		}
		type = _br.ReadUInt16();
		if ((num & 1) > 0)
		{
			type += Block.ItemsStartHere;
		}
		if (version < 8 && type >= 32768)
		{
			type += 32768;
		}
		ItemClass itemClass = ItemClass;
		if (version > 5)
		{
			UseTimes = _br.ReadSingle();
		}
		else
		{
			UseTimes = (int)_br.ReadUInt16();
		}
		Quality = _br.ReadUInt16();
		if (itemClass != null && itemClass.QualityMin > 0)
		{
			Quality = (byte)Utils.FastMax(Quality, itemClass.QualityMin);
		}
		Meta = _br.ReadUInt16();
		if (Meta >= 65535)
		{
			Meta = -1;
		}
		if (version > 6)
		{
			int num2 = _br.ReadByte();
			for (int i = 0; i < num2; i++)
			{
				string text = _br.ReadString();
				TypedMetadataValue tmv = TypedMetadataValue.Read(_br);
				if (EntityPlayerLocal.PermaDegrationOn || text != "DurabilityModifier")
				{
					SetMetadata(text, tmv);
				}
			}
		}
		if ((num & 2) > 0)
		{
			int num3 = _br.ReadByte();
			Stats = new Stat[num3];
			for (int j = 0; j < num3; j++)
			{
				PassiveEffects passiveEffects = (PassiveEffects)_br.ReadByte();
				int num4 = _br.ReadInt16();
				int added = _br.ReadInt16();
				Stats[j] = new Stat(passiveEffects, num4, added);
			}
			RemoveUnusedStats();
		}
		if ((version > 4 || HasQuality) && !(itemClass is ItemClassModifier))
		{
			byte b = _br.ReadByte();
			Modifications = new ItemValue[b];
			if (b != 0)
			{
				for (int k = 0; k < b; k++)
				{
					if (_br.ReadBoolean())
					{
						Modifications[k] = new ItemValue();
						Modifications[k].Read(_br);
					}
					else
					{
						Modifications[k] = None;
					}
				}
			}
			b = _br.ReadByte();
			CosmeticMods = new ItemValue[b];
			if (b != 0)
			{
				for (int l = 0; l < b; l++)
				{
					if (_br.ReadBoolean())
					{
						CosmeticMods[l] = new ItemValue();
						CosmeticMods[l].Read(_br);
					}
					else
					{
						CosmeticMods[l] = None;
					}
				}
			}
		}
		if (version > 1)
		{
			Activated = _br.ReadByte();
		}
		if (version > 2)
		{
			SelectedAmmoTypeIndex = _br.ReadByte();
		}
		if (version > 3)
		{
			Seed = _br.ReadUInt16();
			if (type == 0)
			{
				Seed = 0;
			}
		}
		if (version > 8)
		{
			if (_br.ReadBoolean())
			{
				TextureFullArray.Read(_br);
			}
			else
			{
				TextureFullArray.Fill(0L);
			}
		}
	}

	public static void Write(ItemValue _iv, BinaryWriter _bw)
	{
		if (_iv == null)
		{
			_bw.Write((byte)0);
		}
		else
		{
			_iv.Write(_bw);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		if (IsEmpty())
		{
			_bw.Write((byte)0);
			return;
		}
		_bw.Write((byte)9);
		int num = type;
		byte b = 0;
		if (type >= Block.ItemsStartHere)
		{
			b = 1;
			num -= Block.ItemsStartHere;
		}
		if (Stats != null)
		{
			b |= 2;
		}
		_bw.Write(b);
		_bw.Write((ushort)num);
		_bw.Write(UseTimes);
		_bw.Write(Quality);
		_bw.Write((ushort)Meta);
		int num2 = ((Metadata != null) ? Metadata.Count : 0);
		_bw.Write((byte)num2);
		if (Metadata != null)
		{
			foreach (string key in Metadata.Keys)
			{
				if (Metadata[key]?.GetValue() != null)
				{
					_bw.Write(key);
					TypedMetadataValue.Write(Metadata[key], _bw);
				}
			}
		}
		if (Stats != null)
		{
			int num3 = Stats.Length;
			_bw.Write((byte)num3);
			for (int i = 0; i < num3; i++)
			{
				Stat stat = Stats[i];
				_bw.Write((byte)stat.type);
				short value = (short)((!stat.isBoosted) ? stat.value : 0);
				short value2 = (short)(stat.isBoosted ? stat.value : 0);
				_bw.Write(value);
				_bw.Write(value2);
			}
		}
		if (!(ItemClass is ItemClassModifier))
		{
			_bw.Write((byte)Modifications.Length);
			for (int j = 0; j < Modifications.Length; j++)
			{
				bool flag = Modifications[j] != null && !Modifications[j].IsEmpty();
				_bw.Write(flag);
				if (flag)
				{
					Modifications[j].Write(_bw);
				}
			}
			_bw.Write((byte)CosmeticMods.Length);
			for (int k = 0; k < CosmeticMods.Length; k++)
			{
				bool flag2 = CosmeticMods[k] != null && !CosmeticMods[k].IsEmpty();
				_bw.Write(flag2);
				if (flag2)
				{
					CosmeticMods[k].Write(_bw);
				}
			}
		}
		_bw.Write(Activated);
		_bw.Write(SelectedAmmoTypeIndex);
		if (type == 0)
		{
			Seed = 0;
		}
		_bw.Write(Seed);
		if (TextureFullArray.IsDefault)
		{
			_bw.Write(value: false);
		}
		else
		{
			_bw.Write(value: true);
			TextureFullArray.Write(_bw);
		}
		ItemClass itemClass = ItemClass.list[type];
		if (itemClass == null)
		{
			if (type != 0)
			{
				Log.Error("No ItemClass entry for type " + type);
			}
		}
		else
		{
			((!itemClass.IsBlock()) ? ItemClass.nameIdMapping : Block.nameIdMapping)?.AddMapping(type, itemClass.Name);
		}
	}

	public bool Equals(ItemValue _other)
	{
		if (_other == null)
		{
			return false;
		}
		if (_other.type == type && _other.UseTimes == UseTimes && _other.Meta == Meta && _other.Seed == Seed && _other.Quality == Quality && _other.SelectedAmmoTypeIndex == SelectedAmmoTypeIndex && _other.Activated == Activated && Equals(_other.Metadata, Metadata) && Equals(_other.Stats, Stats) && Equals(_other.CosmeticMods, CosmeticMods))
		{
			return Equals(_other.Modifications, Modifications);
		}
		return false;
	}

	public override bool Equals(object _other)
	{
		if (!(_other is ItemValue))
		{
			return false;
		}
		return Equals((ItemValue)_other);
	}

	public bool EqualsExceptUseTimesAndAmmo(ItemValue _other)
	{
		if (_other == null)
		{
			return false;
		}
		bool flag = ItemClass != null && ItemClass.Actions != null && ItemClass.Actions.Length != 0 && ItemClass.Actions[0] is ItemActionRanged;
		if (_other.type == type && (flag || _other.Meta == Meta) && _other.Seed == Seed && _other.Quality == Quality && _other.SelectedAmmoTypeIndex == SelectedAmmoTypeIndex && _other.Activated == Activated && Equals(_other.Metadata, Metadata) && Equals(_other.Stats, Stats) && Equals(_other.CosmeticMods, CosmeticMods))
		{
			return Equals(_other.Modifications, Modifications);
		}
		return false;
	}

	public bool EqualsForMerging(ItemValue _other)
	{
		if (_other == null)
		{
			return false;
		}
		bool flag = ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.CombineOnly;
		if (_other.type == type && (!flag || UseTimes + _other.UseTimes == 0f || ((float)MaxUseTimes == UseTimes && (float)_other.MaxUseTimes == _other.UseTimes)))
		{
			return Equals(_other.Stats, Stats);
		}
		return false;
	}

	public static bool Equals(ItemValue[] _a, ItemValue[] _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		if (_a.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != null || _b[i] != null)
			{
				if (_a[i] == null || _b[i] == null)
				{
					return false;
				}
				if (!_a[i].Equals(_b[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool Equals(Stat[] _a, Stat[] _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		if (_a.Length == 0)
		{
			return true;
		}
		return Enumerable.SequenceEqual(_a, _b);
	}

	public static bool Equals(object[] _a, object[] _b)
	{
		if (!(_a is ItemValue[]) || !(_b is ItemValue[]))
		{
			return false;
		}
		return Equals((ItemValue[])_a, (ItemValue[])_b);
	}

	public static bool Equals(Dictionary<string, TypedMetadataValue> _a, Dictionary<string, TypedMetadataValue> _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Count != _b.Count)
		{
			return false;
		}
		if (_a.Count == 0)
		{
			return true;
		}
		foreach (string key in _a.Keys)
		{
			if (!_b.ContainsKey(key))
			{
				return false;
			}
			if (!_a[key].Equals(_b[key]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return type;
	}

	public int GetItemId()
	{
		return type - Block.ItemsStartHere;
	}

	public int GetItemOrBlockId()
	{
		if (type < Block.ItemsStartHere)
		{
			return type;
		}
		return type - Block.ItemsStartHere;
	}

	public override string ToString()
	{
		return ((type >= Block.ItemsStartHere) ? ("item=" + (type - Block.ItemsStartHere)) : ("block=" + type)) + " m=" + Meta + " ut=" + UseTimes;
	}

	public string GetPropertyOverride(string _propertyName, string _originalValue)
	{
		if (Modifications.Length == 0 && CosmeticMods.Length == 0)
		{
			return _originalValue;
		}
		string _value = "";
		string itemName = ItemClass.GetItemName();
		for (int i = 0; i < Modifications.Length; i++)
		{
			ItemValue itemValue = Modifications[i];
			if (itemValue != null && itemValue.ItemClass is ItemClassModifier itemClassModifier && itemClassModifier.GetPropertyOverride(_propertyName, itemName, ref _value))
			{
				return _value;
			}
		}
		_value = "";
		for (int j = 0; j < CosmeticMods.Length; j++)
		{
			ItemValue itemValue2 = CosmeticMods[j];
			if (itemValue2 != null && itemValue2.ItemClass is ItemClassModifier itemClassModifier2 && itemClassModifier2.GetPropertyOverride(_propertyName, itemName, ref _value))
			{
				return _value;
			}
		}
		return _originalValue;
	}

	public bool HasCosmetics()
	{
		if (CosmeticMods != null)
		{
			for (int i = 0; i < CosmeticMods.Length; i++)
			{
				ItemValue itemValue = CosmeticMods[i];
				if (itemValue != null && !itemValue.IsEmpty())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMods()
	{
		if (Modifications != null)
		{
			for (int i = 0; i < Modifications.Length; i++)
			{
				ItemValue itemValue = Modifications[i];
				if (itemValue != null && !itemValue.IsEmpty())
				{
					return true;
				}
			}
		}
		return false;
	}
}
