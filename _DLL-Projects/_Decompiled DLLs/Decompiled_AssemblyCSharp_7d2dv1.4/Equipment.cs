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
	public const int CurrentSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] m_slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsSetFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsChangedFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] preferredItemSlots;

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
	public static FastTags<TagGroup.Global> piercingDamage = FastTags<TagGroup.Global>.Parse("piercing");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> slashingDamage = FastTags<TagGroup.Global>.Parse("slashing");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> corrosiveDamage = FastTags<TagGroup.Global>.Parse("corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> coreDamageResist = FastTags<TagGroup.Global>.Parse("coredamageresist");

	public event Action OnChanged;

	public Equipment()
	{
		int num = 5;
		m_slots = new ItemValue[num];
		preferredItemSlots = new int[num];
	}

	public Equipment(EntityAlive _entity)
		: this()
	{
		m_entity = _entity;
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

	public bool IsNaked()
	{
		return !HasAnyItems();
	}

	public void CalcDamage(ref int entityDamageTaken, ref int armorDamageTaken, FastTags<TagGroup.Global> damageTypeTag, EntityAlive attacker, ItemValue attackingItem)
	{
		armorDamageTaken = entityDamageTaken;
		if (damageTypeTag.Test_AnySet(physicalDamageTypes))
		{
			if (entityDamageTaken > 0)
			{
				float totalPhysicalArmorResistPercent = GetTotalPhysicalArmorResistPercent(attacker, attackingItem);
				armorDamageTaken = Utils.FastMax((totalPhysicalArmorResistPercent > 0f) ? 1 : 0, Mathf.RoundToInt((float)entityDamageTaken * totalPhysicalArmorResistPercent));
				entityDamageTaken -= armorDamageTaken;
			}
		}
		else
		{
			entityDamageTaken = Mathf.RoundToInt(Utils.FastMax(0f, (float)entityDamageTaken * (1f - EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, m_entity, null, damageTypeTag) / 100f)));
			armorDamageTaken = Mathf.RoundToInt(Utils.FastMax(0, armorDamageTaken - entityDamageTaken));
		}
	}

	public float GetTotalPhysicalArmorResistPercent(EntityAlive attacker, ItemValue attackingItem)
	{
		return GetTotalPhysicalArmorRating(attacker, attackingItem) / 100f;
	}

	public float GetTotalPhysicalArmorRating(EntityAlive attacker, ItemValue attackingItem)
	{
		float value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, m_entity, null, coreDamageResist, calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, 1, useMods: true, _useDurability: true);
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
			preferredItemSlots[index] = value?.type ?? itemValue.type;
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
			if ((_flags & (1 << i)) <= 0)
			{
				continue;
			}
			ItemValue itemValue = m_slots[i];
			if (itemValue == null || itemValue.ItemClass == null)
			{
				continue;
			}
			m_entity.MinEventContext.ItemValue = itemValue;
			itemValue.FireEvent(_event, m_entity.MinEventContext);
			for (int j = 0; j < itemValue.Modifications.Length; j++)
			{
				ItemValue itemValue2 = itemValue.Modifications[j];
				if (itemValue2 != null && itemValue2.ItemClass != null)
				{
					itemValue2.FireEvent(_event, m_entity.MinEventContext);
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
		writer.Write((byte)1);
		for (int i = 0; i < m_slots.Length; i++)
		{
			ItemValue.Write(m_slots[i], writer);
		}
	}

	public static Equipment Read(BinaryReader reader)
	{
		reader.ReadByte();
		Equipment equipment = new Equipment();
		for (int i = 0; i < equipment.m_slots.Length; i++)
		{
			equipment.m_slots[i] = ItemValue.ReadOrNull(reader);
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
}
