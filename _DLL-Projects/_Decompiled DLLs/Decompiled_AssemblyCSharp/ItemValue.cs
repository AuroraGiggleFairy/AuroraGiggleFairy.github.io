using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemValue : IEquatable<ItemValue>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int EmptySaveVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte MaxModifications = byte.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ItemValue[] emptyItemValueArray = new ItemValue[0];

	public static ItemValue None = new ItemValue(0);

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

	public ItemValue[] Modifications;

	public ItemValue[] CosmeticMods;

	public ushort Seed;

	public TextureFullArray TextureFullArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime baseDate = new DateTime(2013, 10, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsItem = 1;

	public int MaxUseTimes => (int)EffectManager.GetValue(PassiveEffects.DegradationMax, this, 0f, null, null, (ItemClass != null) ? ItemClass.ItemTags : FastTags<TagGroup.Global>.none);

	public float PercentUsesLeft
	{
		get
		{
			int maxUseTimes = MaxUseTimes;
			if (maxUseTimes > 0)
			{
				return 1f - Mathf.Clamp01(UseTimes / (float)maxUseTimes);
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
			gameRandom = GameRandomManager.Instance.CreateGameRandom(Seed);
			Quality = (ushort)Math.Min(65535, gameRandom.RandomRange(minQuality, maxQuality + 1));
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
		Modifications = new ItemValue[Math.Min(255, (int)EffectManager.GetValue(PassiveEffects.ModSlots, this, Utils.FastMax(0, Quality - 1)))];
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
		Modifications = new ItemValue[0];
		CosmeticMods = new ItemValue[0];
		Metadata = null;
		SelectedAmmoTypeIndex = 0;
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
					Modifications[j] = None.Clone();
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
				CosmeticMods[k] = None.Clone();
			}
		}
	}

	public float GetValue(EntityAlive _entity, PassiveEffects _passiveEffect, FastTags<TagGroup.Global> _tags)
	{
		float _originalValue = 0f;
		float _perc_value = 1f;
		ItemClass itemClass = ItemClass;
		if (itemClass != null)
		{
			if (_entity != null)
			{
				MinEventParams.CopyTo(_entity.MinEventContext, MinEventParams.CachedEventParam);
			}
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[SelectedAmmoTypeIndex]).ModifyValue(_entity, this, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
			if (itemClass.Effects != null)
			{
				int seed = MinEventParams.CachedEventParam.Seed;
				if (_entity != null)
				{
					seed = _entity.MinEventContext.Seed;
				}
				MinEventParams.CachedEventParam.Seed = (int)Seed + (int)((Seed != 0) ? _passiveEffect : PassiveEffects.None);
				if (_entity != null)
				{
					_entity.MinEventContext.Seed = MinEventParams.CachedEventParam.Seed;
				}
				itemClass.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, (int)Quality, _tags);
				MinEventParams.CachedEventParam.Seed = seed;
				if (_entity != null)
				{
					_entity.MinEventContext.Seed = seed;
				}
			}
		}
		return _originalValue * _perc_value;
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
		itemValue.Modifications = new ItemValue[Modifications.Length];
		for (int i = 0; i < Modifications.Length; i++)
		{
			itemValue.Modifications[i] = ((Modifications[i] != null) ? Modifications[i].Clone() : null);
		}
		if (Metadata != null)
		{
			itemValue.Metadata = new Dictionary<string, TypedMetadataValue>();
			foreach (KeyValuePair<string, TypedMetadataValue> item in Metadata)
			{
				itemValue.Metadata.Add(item.Key, item.Value?.Clone());
			}
		}
		itemValue.CosmeticMods = new ItemValue[CosmeticMods.Length];
		for (int j = 0; j < CosmeticMods.Length; j++)
		{
			itemValue.CosmeticMods[j] = ((CosmeticMods[j] != null) ? CosmeticMods[j].Clone() : null);
		}
		itemValue.Activated = Activated;
		if (itemValue.type == 0)
		{
			Seed = 0;
		}
		itemValue.Seed = Seed;
		itemValue.TextureFullArray = TextureFullArray;
		return itemValue;
	}

	public bool IsEmpty()
	{
		return type == 0;
	}

	public BlockValue ToBlockValue()
	{
		if (type < Block.ItemsStartHere)
		{
			return new BlockValue
			{
				type = type
			};
		}
		return BlockValue.Air;
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
		if (version > 5)
		{
			UseTimes = _br.ReadSingle();
		}
		else
		{
			UseTimes = (int)_br.ReadUInt16();
		}
		Quality = _br.ReadUInt16();
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
				string key = _br.ReadString();
				TypedMetadataValue tmv = TypedMetadataValue.Read(_br);
				SetMetadata(key, tmv);
			}
		}
		if ((version > 4 || HasQuality) && !(ItemClass is ItemClassModifier))
		{
			byte b = _br.ReadByte();
			Modifications = new ItemValue[b];
			if (b != 0)
			{
				for (int j = 0; j < b; j++)
				{
					if (_br.ReadBoolean())
					{
						Modifications[j] = new ItemValue();
						Modifications[j].Read(_br);
					}
					else
					{
						Modifications[j] = None.Clone();
					}
				}
			}
			b = _br.ReadByte();
			CosmeticMods = new ItemValue[b];
			if (b != 0)
			{
				for (int k = 0; k < b; k++)
				{
					if (_br.ReadBoolean())
					{
						CosmeticMods[k] = new ItemValue();
						CosmeticMods[k].Read(_br);
					}
					else
					{
						CosmeticMods[k] = None.Clone();
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
		byte value = 0;
		if (type >= Block.ItemsStartHere)
		{
			value = 1;
			num -= Block.ItemsStartHere;
		}
		_bw.Write(value);
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
		if (!(ItemClass is ItemClassModifier))
		{
			_bw.Write((byte)Modifications.Length);
			for (int i = 0; i < Modifications.Length; i++)
			{
				bool flag = Modifications[i] != null && !Modifications[i].IsEmpty();
				_bw.Write(flag);
				if (flag)
				{
					Modifications[i].Write(_bw);
				}
			}
			_bw.Write((byte)CosmeticMods.Length);
			for (int j = 0; j < CosmeticMods.Length; j++)
			{
				bool flag2 = CosmeticMods[j] != null && !CosmeticMods[j].IsEmpty();
				_bw.Write(flag2);
				if (flag2)
				{
					CosmeticMods[j].Write(_bw);
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
		if (_other.type == type && _other.UseTimes == UseTimes && _other.Meta == Meta && _other.Seed == Seed && _other.Quality == Quality && _other.SelectedAmmoTypeIndex == SelectedAmmoTypeIndex && _other.Activated == Activated && Equals(_other.Metadata, Metadata) && Equals(_other.CosmeticMods, CosmeticMods))
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
		if (_other.type == type && (flag || _other.Meta == Meta) && _other.Seed == Seed && _other.Quality == Quality && _other.SelectedAmmoTypeIndex == SelectedAmmoTypeIndex && _other.Activated == Activated && Equals(_other.Metadata, Metadata) && Equals(_other.CosmeticMods, CosmeticMods))
		{
			return Equals(_other.Modifications, Modifications);
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

	public bool HasMods()
	{
		for (int i = 0; i < Modifications.Length; i++)
		{
			ItemValue itemValue = Modifications[i];
			if (itemValue != null && !itemValue.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}
}
