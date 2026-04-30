using System;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;

public class Equipment
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class ArmorGroupInfo
	{
		public int Count;

		public int LowestQuality;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] m_slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsSetFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsChangedFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] preferredItemSlots;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass[] m_cosmeticSlots;

	public List<int> m_unlockedCosmetics = new List<int>();

	public int tempCosmeticSlotIndex = -1;

	public ItemClass tempCosmeticSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive m_entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float insulation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float waterProof;

	public float CurrentLowestDurability = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ArmorGroupInfo> ArmorGroupEquipped = new Dictionary<string, ArmorGroupInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageTypes = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> coreDamageResist = FastTags<TagGroup.Global>.Parse("coredamageresist");

	public static Dictionary<int, string> CosmeticMappingIDString = new Dictionary<int, string>();

	public static Dictionary<string, int> CosmeticMappingStringID = new Dictionary<string, int>();

	public ItemClass[] CosmeticSlots
	{
		get
		{
			return m_cosmeticSlots;
		}
		set
		{
			m_cosmeticSlots = value;
		}
	}

	public event Action OnChanged;

	public event Equipment_CosmeticUnlocked CosmeticUnlocked;

	public Equipment()
	{
		int num = 12;
		m_slots = new ItemValue[num];
		CosmeticSlots = new ItemClass[num];
		preferredItemSlots = new int[num];
	}

	public Equipment(EntityAlive _entity)
		: this()
	{
		m_entity = _entity;
	}

	public ItemClass GetCosmeticSlot(int index, bool useTemporary)
	{
		if (useTemporary && tempCosmeticSlotIndex == index)
		{
			return tempCosmeticSlot;
		}
		return CosmeticSlots[index];
	}

	public void SetCosmeticSlot(ItemClassArmor itemClass)
	{
		if (itemClass.EquipSlot < EquipmentSlots.BiomeBadge && HasCosmeticUnlocked(itemClass).isUnlocked)
		{
			int equipSlot = (int)itemClass.EquipSlot;
			CosmeticSlots[equipSlot] = itemClass;
			if ((bool)m_entity && !m_entity.isEntityRemote)
			{
				m_entity.bPlayerStatsChanged = true;
			}
		}
	}

	public void SetCosmeticSlot(int slotID, int id)
	{
		if (id == 0)
		{
			CosmeticSlots[slotID] = null;
			if ((bool)m_entity && !m_entity.isEntityRemote)
			{
				m_entity.bPlayerStatsChanged = true;
			}
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(CosmeticMappingIDString[id]);
		if (itemClass is ItemClassArmor itemClassArmor)
		{
			if (itemClassArmor.EquipSlot < EquipmentSlots.BiomeBadge)
			{
				CosmeticSlots[(int)itemClassArmor.EquipSlot] = itemClass;
				if ((bool)m_entity && !m_entity.isEntityRemote)
				{
					m_entity.bPlayerStatsChanged = true;
				}
			}
		}
		else
		{
			CosmeticSlots[slotID] = itemClass;
			if ((bool)m_entity && !m_entity.isEntityRemote)
			{
				m_entity.bPlayerStatsChanged = true;
			}
		}
	}

	public int[] GetCosmeticIDs()
	{
		int num = 12;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			if (m_cosmeticSlots[i] == null)
			{
				array[i] = 0;
			}
			else
			{
				array[i] = CosmeticMappingStringID[m_cosmeticSlots[i].GetItemName()];
			}
		}
		return array;
	}

	public void ClearCosmeticSlots()
	{
		for (int i = 0; i < m_cosmeticSlots.Length; i++)
		{
			m_cosmeticSlots[i] = null;
		}
		if ((bool)m_entity && !m_entity.isEntityRemote)
		{
			m_entity.bPlayerStatsChanged = true;
		}
	}

	public void UnlockCosmeticItem(ItemClass itemClass)
	{
		if (itemClass == null)
		{
			return;
		}
		string itemName = itemClass.GetItemName();
		if (!CosmeticMappingStringID.ContainsKey(itemName))
		{
			return;
		}
		int item = CosmeticMappingStringID[itemName];
		if (!m_unlockedCosmetics.Contains(item))
		{
			m_unlockedCosmetics.Add(item);
			if (this.CosmeticUnlocked != null)
			{
				this.CosmeticUnlocked(itemName);
			}
		}
	}

	public void ModifyValue(ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags, bool _useDurability = false)
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null && !itemValue.Equals(_originalItemValue) && itemValue.ItemClass != null)
			{
				itemValue.ModifyValue(m_entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags, _useMods: true, _useDurability);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags)
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null && !itemValue.Equals(_originalItemValue) && itemValue.ItemClass != null)
			{
				itemValue.GetModifiedValueData(_modValueSources, _sourceType, m_entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags);
			}
		}
	}

	public (bool isUnlocked, EntitlementSetEnum set) HasCosmeticUnlocked(ItemClass itemClass)
	{
		if (itemClass == null)
		{
			return (isUnlocked: false, set: EntitlementSetEnum.None);
		}
		string itemName = itemClass.GetItemName();
		if (!CosmeticMappingStringID.ContainsKey(itemName))
		{
			return (isUnlocked: false, set: EntitlementSetEnum.None);
		}
		EntitlementSetEnum entitlementSetEnum = EntitlementSetEnum.None;
		if (itemClass.SDCSData != null)
		{
			entitlementSetEnum = EntitlementManager.Instance.GetSetForAsset(itemClass.SDCSData.PrefabName);
			if (entitlementSetEnum != EntitlementSetEnum.None && EntitlementManager.Instance.HasEntitlement(entitlementSetEnum))
			{
				return (isUnlocked: true, set: entitlementSetEnum);
			}
		}
		return (isUnlocked: m_unlockedCosmetics.Contains(CosmeticMappingStringID[itemClass.GetItemName()]), set: entitlementSetEnum);
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null && itemValue.ItemClass != null)
			{
				itemValue.FireEvent(_eventType, _params);
			}
		}
	}

	public void DropItems()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null)
			{
				DropItemOnGround(itemValue);
				SetSlotItem(i, null);
			}
		}
		updateInsulation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInsulation()
	{
		waterProof = 0f;
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null)
			{
				waterProof += itemValue.ItemClass.WaterProof;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DropItemOnGround(ItemValue _itemValue)
	{
		m_entity.world.GetGameManager().ItemDropServer(new ItemStack(_itemValue, 1), m_entity.GetPosition(), new Vector3(0.5f, 0f, 0.5f), m_entity.belongsPlayerId);
	}

	public float GetTotalInsulation()
	{
		return insulation;
	}

	public float GetTotalWaterproof()
	{
		return waterProof;
	}

	public int GetSlotCount()
	{
		return m_slots.Length;
	}

	public ItemValue[] GetItems()
	{
		return m_slots;
	}

	public ItemValue GetSlotItem(int index)
	{
		return m_slots[index];
	}

	public ItemValue GetSlotItemOrNone(int index)
	{
		ItemValue itemValue = m_slots[index];
		if (itemValue == null)
		{
			return ItemValue.None;
		}
		return itemValue;
	}

	public bool HasAnyItems()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (m_slots[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public void SetTempCosmeticSlot(int index, ItemClass itemClass)
	{
		tempCosmeticSlotIndex = index;
		tempCosmeticSlot = itemClass;
	}

	public void ClearTempCosmeticSlot()
	{
		tempCosmeticSlotIndex = -1;
		tempCosmeticSlot = null;
	}

	public void ApplyTempCosmeticSlot()
	{
		if (tempCosmeticSlotIndex != -1)
		{
			m_cosmeticSlots[tempCosmeticSlotIndex] = tempCosmeticSlot;
			if ((bool)m_entity && !m_entity.isEntityRemote)
			{
				m_entity.bPlayerStatsChanged = true;
			}
		}
	}

	public void CalcDamage(ref int entityDamageTaken, ref int armorDamageTaken, FastTags<TagGroup.Global> damageTypeTag, EntityAlive attacker, ItemValue attackingItem)
	{
		armorDamageTaken = entityDamageTaken;
		if (damageTypeTag.Test_AnySet(physicalDamageTypes))
		{
			if (entityDamageTaken > 0)
			{
				float num = GetTotalPhysicalArmorRating(attacker, attackingItem) / 100f;
				armorDamageTaken = Utils.FastMax((num > 0f) ? 1 : 0, Utils.FastRoundToInt((float)entityDamageTaken * num));
				entityDamageTaken -= armorDamageTaken;
			}
		}
		else
		{
			entityDamageTaken = Utils.FastRoundToInt(Utils.FastMax(0f, (float)entityDamageTaken * (1f - EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, m_entity, null, damageTypeTag) / 100f)));
			armorDamageTaken = Utils.FastRoundToInt(Utils.FastMax(0, armorDamageTaken - entityDamageTaken));
		}
	}

	public float GetTotalPhysicalArmorRating(EntityAlive attacker, ItemValue attackingItem)
	{
		FastTags<TagGroup.Global> tags = coreDamageResist;
		if (attackingItem != null)
		{
			tags |= attackingItem.ItemClassOrMissing.ItemTags;
		}
		float value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, m_entity, null, tags, calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, 1, useMods: true, _useDurability: true);
		return EffectManager.GetValue(PassiveEffects.TargetArmor, attackingItem, value, attacker);
	}

	public List<ItemValue> GetArmor()
	{
		List<ItemValue> list = new List<ItemValue>();
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null)
			{
				float _originalValue = 0f;
				float _perc_value = 1f;
				itemValue.ModifyValue(m_entity, null, PassiveEffects.PhysicalDamageResist, ref _originalValue, ref _perc_value, FastTags<TagGroup.Global>.all);
				if (_originalValue != 0f)
				{
					list.Add(itemValue);
				}
			}
		}
		return list;
	}

	public bool CheckBreakUseItems()
	{
		bool result = false;
		CurrentLowestDurability = 1f;
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue == null)
			{
				continue;
			}
			ItemClass forId = ItemClass.GetForId(itemValue.type);
			float percentUsesLeft = itemValue.PercentUsesLeft;
			if (percentUsesLeft < CurrentLowestDurability)
			{
				CurrentLowestDurability = percentUsesLeft;
			}
			if (forId != null && itemValue.MaxUseTimes > 0 && forId.MaxUseTimesBreaksAfter.Value && itemValue.UseTimes > (float)itemValue.MaxUseTimes)
			{
				SetSlotItem(i, null);
				if (m_entity != null && forId.Properties.Values.ContainsKey(ItemClass.PropSoundDestroy))
				{
					Manager.BroadcastPlay(m_entity, forId.Properties.Values[ItemClass.PropSoundDestroy]);
				}
				result = true;
			}
		}
		return result;
	}

	public void SetSlotItem(int index, ItemValue value, bool isLocal = true)
	{
		if (value != null && value.IsEmpty())
		{
			value = null;
		}
		ItemValue itemValue = m_slots[index];
		if (value == null && itemValue == null)
		{
			return;
		}
		m_entity.IsEquipping = true;
		bool flag = false;
		if (itemValue != null && itemValue.Equals(value))
		{
			m_slots[index] = value;
			m_entity.MinEventContext.ItemValue = value;
			value.FireEvent(MinEventTypes.onSelfEquipStart, m_entity.MinEventContext);
		}
		else
		{
			flag = true;
			if (itemValue != null)
			{
				if (itemValue.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && itemValue.Activated != 0)
				{
					m_entity.MinEventContext.ItemValue = itemValue;
					itemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, m_entity.MinEventContext);
					itemValue.Activated = 0;
				}
				for (int i = 0; i < itemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = itemValue.Modifications[i];
					if (itemValue2 != null)
					{
						ItemClass itemClass = itemValue2.ItemClass;
						if (itemClass != null && itemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && itemValue2.Activated != 0)
						{
							m_entity.MinEventContext.ItemValue = itemValue2;
							itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, m_entity.MinEventContext);
							itemValue2.Activated = 0;
						}
					}
				}
				m_entity.MinEventContext.ItemValue = itemValue;
				itemValue.FireEvent(MinEventTypes.onSelfEquipStop, m_entity.MinEventContext);
			}
			preferredItemSlots[index] = value?.type ?? 0;
			m_slots[index] = value;
			slotsSetFlags |= 1 << index;
			slotsChangedFlags |= 1 << index;
		}
		if (flag)
		{
			if ((bool)m_entity && !m_entity.isEntityRemote)
			{
				m_entity.bPlayerStatsChanged = true;
			}
			ResetArmorGroups();
			if (this.OnChanged != null)
			{
				this.OnChanged();
			}
		}
		m_entity.IsEquipping = false;
	}

	public void SetSlotItemRaw(int index, ItemValue _iv)
	{
		if (_iv != null && _iv.IsEmpty())
		{
			_iv = null;
		}
		m_slots[index] = _iv;
	}

	public void FireEventsForSlots(MinEventTypes _event, int _flags = -1)
	{
		m_entity.MinEventContext.Self = m_entity;
		for (int i = 0; i < m_slots.Length; i++)
		{
			if ((_flags & (1 << i)) > 0)
			{
				ItemValue itemValue = m_slots[i];
				if (itemValue != null && itemValue.ItemClass != null)
				{
					m_entity.MinEventContext.ItemValue = itemValue;
					itemValue.FireEvent(_event, m_entity.MinEventContext);
				}
			}
		}
	}

	public void FireEventsForSetSlots()
	{
		FireEventsForSlots(MinEventTypes.onSelfEquipStart, slotsSetFlags);
		slotsSetFlags = 0;
	}

	public void FireEventsForChangedSlots()
	{
		if (slotsChangedFlags == 0)
		{
			return;
		}
		m_entity.IsEquipping = true;
		for (int i = 0; i < m_slots.Length; i++)
		{
			if ((slotsChangedFlags & (1 << i)) <= 0)
			{
				continue;
			}
			ItemValue itemValue = m_slots[i];
			if (itemValue == null)
			{
				continue;
			}
			ItemClass itemClass = itemValue.ItemClass;
			if (itemClass == null)
			{
				continue;
			}
			m_entity.MinEventContext.Self = m_entity;
			m_entity.MinEventContext.ItemValue = itemValue;
			itemValue.FireEvent(MinEventTypes.onSelfEquipChanged, m_entity.MinEventContext);
			if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
			{
				if (itemValue.Activated == 0)
				{
					itemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, m_entity.MinEventContext);
				}
				else
				{
					itemValue.FireEvent(MinEventTypes.onSelfItemActivate, m_entity.MinEventContext);
				}
			}
			for (int j = 0; j < itemValue.Modifications.Length; j++)
			{
				ItemValue itemValue2 = itemValue.Modifications[j];
				if (itemValue2 != null && itemValue2.ItemClass != null && itemValue2.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
				{
					m_entity.MinEventContext.ItemValue = itemValue2;
					if (itemValue2.Activated == 0)
					{
						itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, m_entity.MinEventContext);
					}
					else
					{
						itemValue2.FireEvent(MinEventTypes.onSelfItemActivate, m_entity.MinEventContext);
					}
				}
			}
		}
		slotsChangedFlags = 0;
		m_entity.IsEquipping = false;
	}

	public void Update()
	{
		if (!(Time.time - lastUpdateTime >= 1f))
		{
			return;
		}
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null)
			{
				m_entity.MinEventContext.ItemValue = itemValue;
				itemValue.FireEvent(MinEventTypes.onSelfEquipUpdate, m_entity.MinEventContext);
			}
		}
		lastUpdateTime = Time.time;
	}

	public void SetPreferredItemSlot(int _slot, ItemValue _itemValue)
	{
		if (_slot >= 0 && _slot < preferredItemSlots.Length)
		{
			preferredItemSlots[_slot] = _itemValue.type;
		}
	}

	public int PreferredItemSlot(ItemValue _itemValue)
	{
		for (int i = 0; i < preferredItemSlots.Length; i++)
		{
			if (_itemValue.type == preferredItemSlots[i])
			{
				return i;
			}
		}
		return -1;
	}

	public int PreferredItemSlot(ItemStack _itemStack)
	{
		for (int i = 0; i < preferredItemSlots.Length; i++)
		{
			if (_itemStack.itemValue.type == preferredItemSlots[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)4);
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue.Write(m_slots[i], writer);
		}
		for (int j = 0; j < m_cosmeticSlots.Length; j++)
		{
			if (m_cosmeticSlots[j] == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(CosmeticMappingStringID[m_cosmeticSlots[j].GetItemName()]);
			}
		}
		writer.Write(m_unlockedCosmetics.Count);
		for (int k = 0; k < m_unlockedCosmetics.Count; k++)
		{
			writer.Write(m_unlockedCosmetics[k]);
		}
	}

	public static Equipment Read(BinaryReader reader)
	{
		int num = reader.ReadByte();
		Equipment equipment = new Equipment();
		int num2 = equipment.m_slots.Length;
		if (num <= 2)
		{
			num2 = 5;
		}
		else if (num <= 3)
		{
			num2 = 8;
		}
		for (int i = 0; i < num2; i++)
		{
			equipment.m_slots[i] = ItemValue.ReadOrNull(reader);
		}
		if (num >= 2)
		{
			for (int j = 0; j < num2; j++)
			{
				int num3 = reader.ReadInt32();
				if (num3 == 0)
				{
					equipment.m_cosmeticSlots[j] = null;
				}
				else
				{
					equipment.m_cosmeticSlots[j] = ItemClass.GetItemClass(CosmeticMappingIDString[num3]);
				}
			}
			equipment.m_unlockedCosmetics.Clear();
			int num4 = reader.ReadInt32();
			for (int k = 0; k < num4; k++)
			{
				equipment.m_unlockedCosmetics.Add(reader.ReadInt32());
			}
		}
		return equipment;
	}

	public virtual bool ReturnItem(ItemStack _itemStack, bool isLocal = true)
	{
		int num = PreferredItemSlot(_itemStack);
		if (num < 0 || num >= m_slots.Length)
		{
			return false;
		}
		if (m_slots[num] == null)
		{
			SetSlotItem(num, _itemStack.itemValue, isLocal);
			return true;
		}
		return false;
	}

	public void Apply(Equipment eq, bool isLocal = true)
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			SetSlotItem(i, eq.m_slots[i], isLocal);
		}
		for (int j = 0; j < m_cosmeticSlots.Length; j++)
		{
			m_cosmeticSlots[j] = eq.m_cosmeticSlots[j];
		}
		m_unlockedCosmetics.Clear();
		for (int k = 0; k < eq.m_unlockedCosmetics.Count; k++)
		{
			m_unlockedCosmetics.Add(eq.m_unlockedCosmetics[k]);
		}
		FireEventsForSetSlots();
		if (m_entity.emodel is EModelSDCS eModelSDCS)
		{
			eModelSDCS.UpdateEquipment();
		}
		FireEventsForChangedSlots();
	}

	public void InitializeEquipmentTransforms()
	{
		FireEventsForSlots(MinEventTypes.onSelfEquipStop);
		FireEventsForSlots(MinEventTypes.onSelfEquipStart);
		slotsSetFlags = 0;
		slotsChangedFlags = -1;
		FireEventsForChangedSlots();
	}

	public Equipment Clone()
	{
		Equipment equipment = new Equipment();
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (m_slots[i] != null)
			{
				equipment.m_slots[i] = m_slots[i].Clone();
			}
		}
		for (int j = 0; j < m_cosmeticSlots.Length; j++)
		{
			_ = m_cosmeticSlots[j];
			equipment.m_cosmeticSlots[j] = m_cosmeticSlots[j];
		}
		for (int k = 0; k < m_unlockedCosmetics.Count; k++)
		{
			if (equipment.m_unlockedCosmetics == null)
			{
				equipment.m_unlockedCosmetics = new List<int>();
			}
			equipment.m_unlockedCosmetics.Add(m_unlockedCosmetics[k]);
		}
		return equipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddArmorGroup(string armorGroup, int quality)
	{
		if (ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			ArmorGroupInfo armorGroupInfo = ArmorGroupEquipped[armorGroup];
			armorGroupInfo.Count++;
			if (armorGroupInfo.LowestQuality > quality)
			{
				armorGroupInfo.LowestQuality = quality;
			}
		}
		else
		{
			ArmorGroupEquipped[armorGroup] = new ArmorGroupInfo
			{
				Count = 1,
				LowestQuality = quality
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetArmorGroups()
	{
		ArmorGroupEquipped.Clear();
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue itemValue = m_slots[i];
			if (itemValue != null && itemValue.ItemClass is ItemClassArmor itemClassArmor)
			{
				for (int j = 0; j < itemClassArmor.ArmorGroup.Length; j++)
				{
					AddArmorGroup(itemClassArmor.ArmorGroup[j], itemValue.Quality);
				}
			}
		}
	}

	public int GetArmorGroupCount(string armorGroup)
	{
		if (ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			return ArmorGroupEquipped[armorGroup].Count;
		}
		return 0;
	}

	public int GetArmorGroupLowestQuality(string armorGroup)
	{
		if (ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			return ArmorGroupEquipped[armorGroup].LowestQuality;
		}
		return 0;
	}

	public static void AddCosmeticMapping(int ID, string name)
	{
		CosmeticMappingIDString.Add(ID, name);
		CosmeticMappingStringID.Add(name, ID);
	}

	public static void SetupCosmeticMapping()
	{
		CosmeticMappingIDString.Clear();
		CosmeticMappingStringID.Clear();
		AddCosmeticMapping(-1, "missingItem");
		AddCosmeticMapping(1, "armorPrimitiveHelmet");
		AddCosmeticMapping(2, "armorPrimitiveOutfit");
		AddCosmeticMapping(3, "armorPrimitiveGloves");
		AddCosmeticMapping(4, "armorPrimitiveBoots");
		AddCosmeticMapping(5, "armorLumberjackHelmet");
		AddCosmeticMapping(6, "armorLumberjackOutfit");
		AddCosmeticMapping(7, "armorLumberjackGloves");
		AddCosmeticMapping(8, "armorLumberjackBoots");
		AddCosmeticMapping(9, "armorPreacherHelmet");
		AddCosmeticMapping(10, "armorPreacherOutfit");
		AddCosmeticMapping(11, "armorPreacherGloves");
		AddCosmeticMapping(12, "armorPreacherBoots");
		AddCosmeticMapping(13, "armorRogueHelmet");
		AddCosmeticMapping(14, "armorRogueOutfit");
		AddCosmeticMapping(15, "armorRogueGloves");
		AddCosmeticMapping(16, "armorRogueBoots");
		AddCosmeticMapping(17, "armorAthleticHelmet");
		AddCosmeticMapping(18, "armorAthleticOutfit");
		AddCosmeticMapping(19, "armorAthleticGloves");
		AddCosmeticMapping(20, "armorAthleticBoots");
		AddCosmeticMapping(21, "armorEnforcerHelmet");
		AddCosmeticMapping(22, "armorEnforcerOutfit");
		AddCosmeticMapping(23, "armorEnforcerGloves");
		AddCosmeticMapping(24, "armorEnforcerBoots");
		AddCosmeticMapping(25, "armorFarmerHelmet");
		AddCosmeticMapping(26, "armorFarmerOutfit");
		AddCosmeticMapping(27, "armorFarmerGloves");
		AddCosmeticMapping(28, "armorFarmerBoots");
		AddCosmeticMapping(29, "armorBikerHelmet");
		AddCosmeticMapping(30, "armorBikerOutfit");
		AddCosmeticMapping(31, "armorBikerGloves");
		AddCosmeticMapping(32, "armorBikerBoots");
		AddCosmeticMapping(33, "armorScavengerHelmet");
		AddCosmeticMapping(34, "armorScavengerOutfit");
		AddCosmeticMapping(35, "armorScavengerGloves");
		AddCosmeticMapping(36, "armorScavengerBoots");
		AddCosmeticMapping(37, "armorRangerHelmet");
		AddCosmeticMapping(38, "armorRangerOutfit");
		AddCosmeticMapping(39, "armorRangerGloves");
		AddCosmeticMapping(40, "armorRangerBoots");
		AddCosmeticMapping(41, "armorCommandoHelmet");
		AddCosmeticMapping(42, "armorCommandoOutfit");
		AddCosmeticMapping(43, "armorCommandoGloves");
		AddCosmeticMapping(44, "armorCommandoBoots");
		AddCosmeticMapping(45, "armorAssassinHelmet");
		AddCosmeticMapping(46, "armorAssassinOutfit");
		AddCosmeticMapping(47, "armorAssassinGloves");
		AddCosmeticMapping(48, "armorAssassinBoots");
		AddCosmeticMapping(49, "armorMinerHelmet");
		AddCosmeticMapping(50, "armorMinerOutfit");
		AddCosmeticMapping(51, "armorMinerGloves");
		AddCosmeticMapping(52, "armorMinerBoots");
		AddCosmeticMapping(53, "armorNomadHelmet");
		AddCosmeticMapping(54, "armorNomadOutfit");
		AddCosmeticMapping(55, "armorNomadGloves");
		AddCosmeticMapping(56, "armorNomadBoots");
		AddCosmeticMapping(57, "armorNerdHelmet");
		AddCosmeticMapping(58, "armorNerdOutfit");
		AddCosmeticMapping(59, "armorNerdGloves");
		AddCosmeticMapping(60, "armorNerdBoots");
		AddCosmeticMapping(61, "armorRaiderHelmet");
		AddCosmeticMapping(62, "armorRaiderOutfit");
		AddCosmeticMapping(63, "armorRaiderGloves");
		AddCosmeticMapping(64, "armorRaiderBoots");
		AddCosmeticMapping(65, "armorDesertHelmet");
		AddCosmeticMapping(66, "armorDesertOutfit");
		AddCosmeticMapping(67, "armorDesertGloves");
		AddCosmeticMapping(68, "armorDesertBoots");
		AddCosmeticMapping(69, "armorHoarderHelmet");
		AddCosmeticMapping(70, "armorHoarderOutfit");
		AddCosmeticMapping(71, "armorHoarderGloves");
		AddCosmeticMapping(72, "armorHoarderBoots");
		AddCosmeticMapping(73, "armorMarauderHelmet");
		AddCosmeticMapping(74, "armorMarauderOutfit");
		AddCosmeticMapping(75, "armorMarauderGloves");
		AddCosmeticMapping(76, "armorMarauderBoots");
		AddCosmeticMapping(77, "armorCrimsonWarlordHelmet");
		AddCosmeticMapping(78, "armorCrimsonWarlordOutfit");
		AddCosmeticMapping(79, "armorCrimsonWarlordGloves");
		AddCosmeticMapping(80, "armorCrimsonWarlordBoots");
		AddCosmeticMapping(81, "armorSamuraiHelmet");
		AddCosmeticMapping(82, "armorSamuraiOutfit");
		AddCosmeticMapping(83, "armorSamuraiGloves");
		AddCosmeticMapping(84, "armorSamuraiBoots");
		AddCosmeticMapping(85, "armorPimpHatBlueHelmet");
		AddCosmeticMapping(86, "armorPimpHatPurpleHelmet");
		AddCosmeticMapping(87, "armorWatcherHelmet");
		AddCosmeticMapping(88, "armorWatcherOutfit");
		AddCosmeticMapping(89, "armorWatcherGloves");
		AddCosmeticMapping(90, "armorWatcherBoots");
		AddCosmeticMapping(91, "armorCrackABookHelmet");
		AddCosmeticMapping(92, "armorCrackABookOutfit");
		AddCosmeticMapping(93, "armorMoPowerHelmet");
		AddCosmeticMapping(94, "armorMoPowerOutfit");
		AddCosmeticMapping(95, "armorPassNGasHelmet");
		AddCosmeticMapping(96, "armorPassNGasOutfit");
		AddCosmeticMapping(97, "armorPopNPillsHelmet");
		AddCosmeticMapping(98, "armorPopNPillsOutfit");
		AddCosmeticMapping(99, "armorSavageCountryHelmet");
		AddCosmeticMapping(100, "armorSavageCountryOutfit");
		AddCosmeticMapping(101, "armorShamwayHelmet");
		AddCosmeticMapping(102, "armorShamwayOutfit");
		AddCosmeticMapping(103, "armorShotgunMessiahHelmet");
		AddCosmeticMapping(104, "armorShotgunMessiahOutfit");
		AddCosmeticMapping(105, "armorWorkingStiffsHelmet");
		AddCosmeticMapping(106, "armorWorkingStiffsOutfit");
		AddCosmeticMapping(107, "armorPirateHelmet");
		AddCosmeticMapping(108, "armorPirateOutfit");
		AddCosmeticMapping(109, "armorPirateGloves");
		AddCosmeticMapping(110, "armorPirateBoots");
		AddCosmeticMapping(111, "armorHellreaverHelmet");
		AddCosmeticMapping(112, "armorHellreaverOutfit");
		AddCosmeticMapping(113, "armorHellreaverGloves");
		AddCosmeticMapping(114, "armorHellreaverBoots");
		AddCosmeticMapping(115, "armorMonsterMaskHelmet");
		AddCosmeticMapping(116, "armorVampireMaskHelmet");
		AddCosmeticMapping(117, "armorWerewolfMaskHelmet");
		AddCosmeticMapping(118, "armorZombieMaskHelmet");
		AddCosmeticMapping(119, "armorInspectorMaskHelmet");
		AddCosmeticMapping(120, "armorNobleThiefMaskHelmet");
		AddCosmeticMapping(121, "armorPennyPincherMaskHelmet");
		AddCosmeticMapping(122, "armorTheHatterMaskHelmet");
		AddCosmeticMapping(123, "armorClassicSurvivorHelmet");
		AddCosmeticMapping(124, "armorClassicSurvivorOutfit");
		AddCosmeticMapping(125, "armorClassicSurvivorBoots");
		AddCosmeticMapping(126, "armorSantaHatHelmet");
		AddCosmeticMapping(127, "armorButcherHelmet");
		AddCosmeticMapping(128, "armorButcherOutfit");
		AddCosmeticMapping(129, "armorButcherGloves");
		AddCosmeticMapping(130, "armorButcherBoots");
		AddCosmeticMapping(131, "armorElfHatHelmet");
		AddCosmeticMapping(132, "armorReindeerHatHelmet");
		AddCosmeticMapping(133, "armorSnowmanHatHelmet");
		AddCosmeticMapping(134, "armorTreeHatHelmet");
	}
}
