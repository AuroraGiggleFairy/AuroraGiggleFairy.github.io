using System.Collections.Generic;
using UnityEngine;

public static class EffectManager
{
	public class ModifierValuesAndSources
	{
		public enum ValueSourceType
		{
			None,
			Self,
			Held,
			Worn,
			Attribute,
			Skill,
			Perk,
			Mod,
			CosmeticMod,
			Fault,
			Buff,
			Progression,
			Base,
			Ammo,
			ModBonus
		}

		public enum ValueTypes
		{
			None,
			BaseValue,
			PercentValue
		}

		public enum ModTypes
		{
			None,
			Base,
			Percentage
		}

		public PassiveEffects PassiveEffectName;

		public MinEffectController.SourceParentType ParentType;

		public ValueSourceType ValueSource;

		public ValueTypes ValueType;

		public FastTags<TagGroup.Global> Tags;

		public object Source;

		public float Value;

		public PassiveEffect.ValueModifierTypes ModifierType;

		public int ModItemSource;
	}

	public static FastEnumIntEqualityComparer<PassiveEffects> PassiveEffectsComparer = new FastEnumIntEqualityComparer<PassiveEffects>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int slotsQueriedFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int slotsQueriedForEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ItemStack[] slotsCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cInfoStringBaseValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cInfoStringPercValue;

	public static float GetValue(PassiveEffects _passiveEffect, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), bool calcEquipment = true, bool calcHoldingItem = true, bool calcProgression = true, bool calcBuffs = true, bool calcChallenges = true, int craftingTier = 1, bool useMods = true, bool _useDurability = false)
	{
		float _perc_value = 1f;
		if (_entity != null)
		{
			MinEventParams.CopyTo(_entity.MinEventContext, MinEventParams.CachedEventParam);
		}
		if (_originalItemValue != null)
		{
			if (_entity != null && _entity.MinEventContext.ItemValue == null)
			{
				_entity.MinEventContext.ItemValue = _originalItemValue;
			}
			MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			if (_originalItemValue.type != 0 && tags.IsEmpty)
			{
				ItemClass itemClass = _originalItemValue.ItemClass;
				if (itemClass != null)
				{
					tags = itemClass.ItemTags;
				}
			}
		}
		if (_entity == null)
		{
			_recipe?.ModifyValue(_passiveEffect, ref _originalValue, ref _perc_value, tags, craftingTier);
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
		}
		else
		{
			if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
			{
				return _originalValue;
			}
			if (EntityClass.list.TryGetValue(_entity.entityClass, out var _value) && _value.Effects != null)
			{
				_value.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, 0f, tags);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags, useMods);
			}
			else
			{
				EntityVehicle entityVehicle = _entity as EntityVehicle;
				if (entityVehicle != null)
				{
					entityVehicle.GetVehicle()?.GetUpdatedItemValue().ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags);
				}
				else if (calcHoldingItem && _entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue && !_entity.inventory.holdingItemItemValue.IsMod)
				{
					_entity.inventory.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, tags);
				}
			}
			if (calcEquipment && _entity.equipment != null)
			{
				_entity.equipment.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, tags, _useDurability);
			}
			if (_originalItemValue != null)
			{
				if (_entity != null)
				{
					_entity.MinEventContext.ItemValue = _originalItemValue;
				}
				MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			}
			if (calcProgression && _entity.Progression != null)
			{
				_entity.Progression.ModifyValue(_passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
			if (calcChallenges && _entity.challengeJournal != null)
			{
				_entity.challengeJournal.ModifyValue(_passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
			_recipe?.ModifyValue(_passiveEffect, ref _originalValue, ref _perc_value, tags, craftingTier);
			EntityPlayerLocal entityPlayerLocal = _entity as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				if (slotsCached == null || _entity.entityId != slotsQueriedForEntity || slotsQueriedFrame != Time.frameCount)
				{
					LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
					slotsCached = ((uIForPlayer.xui.currentWorkstationToolGrid != null) ? uIForPlayer.xui.currentWorkstationToolGrid.GetSlots() : null);
					slotsQueriedFrame = Time.frameCount;
					slotsQueriedForEntity = _entity.entityId;
				}
				if (slotsCached != null)
				{
					for (int i = 0; i < slotsCached.Length; i++)
					{
						if (!slotsCached[i].IsEmpty())
						{
							slotsCached[i].itemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags);
						}
					}
				}
			}
			if (calcBuffs && _entity.Buffs != null)
			{
				_entity.Buffs.ModifyValue(_passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && useMods && _originalItemValue.ItemClass.Effects != null)
		{
			for (int j = 0; j < _originalItemValue.Modifications.Length; j++)
			{
				if (_originalItemValue.Modifications[j] != null && _originalItemValue.Modifications[j].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.ModifyValue(_entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref _perc_value, (int)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached()));
				}
			}
		}
		return _originalValue * _perc_value;
	}

	public static float GetItemValue(PassiveEffects _passiveEffect, ItemValue _originalItemValue, float _originalValue = 0f)
	{
		float _perc_value = 1f;
		if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
		{
			MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			_originalItemValue.ModifyValue(null, null, _passiveEffect, ref _originalValue, ref _perc_value, _originalItemValue.ItemClass.ItemTags);
		}
		return _originalValue * _perc_value;
	}

	public static float GetDisplayValues(PassiveEffects _passiveEffect, out float baseValueChange, out float percValueMultiplier, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), int craftingTier = 1)
	{
		float num = _originalValue;
		baseValueChange = 0f;
		percValueMultiplier = 1f;
		if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return _originalValue;
		}
		if (_entity == null)
		{
			_recipe?.ModifyValue(_passiveEffect, ref baseValueChange, ref percValueMultiplier, tags, craftingTier);
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
		}
		else
		{
			if (EntityClass.list.ContainsKey(_entity.entityClass) && EntityClass.list[_entity.entityClass].Effects != null)
			{
				EntityClass.list[_entity.entityClass].Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref percValueMultiplier, 0f, tags);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
			else
			{
				if (_entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue)
				{
					_entity.inventory.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
				}
				if (_entity.equipment != null)
				{
					_entity.equipment.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
				}
			}
			if (_entity.Progression != null)
			{
				_entity.Progression.ModifyValue(_passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
			_recipe?.ModifyValue(_passiveEffect, ref baseValueChange, ref percValueMultiplier, tags, craftingTier);
			if (_entity.Buffs != null)
			{
				_entity.Buffs.ModifyValue(_passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && _originalItemValue.ItemClass.Effects != null)
		{
			for (int i = 0; i < _originalItemValue.Modifications.Length; i++)
			{
				if (_originalItemValue.Modifications[i] != null && _originalItemValue.Modifications[i].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.ModifyValue(_entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref percValueMultiplier, (int)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached()));
				}
			}
		}
		baseValueChange = _originalValue - num;
		return _originalValue * percValueMultiplier;
	}

	public static string GetInfoString(PassiveEffects _gAttribute, ItemValue _itemValue, EntityAlive _ea = null, float modAmount = 0f)
	{
		return string.Format("{0}: {1}\n", _gAttribute.ToStringCached(), (modAmount + GetValue(_gAttribute, _itemValue, 0f, _ea)).ToCultureInvariantString("0.0"));
	}

	public static string GetColoredInfoString(PassiveEffects _passiveEffect, ItemValue _itemValue, EntityAlive _ea = null)
	{
		GetDisplayValues(_passiveEffect, out cInfoStringBaseValue, out cInfoStringPercValue, _itemValue, 0f, _ea);
		return string.Format("{0}: [REPLACE_COLOR]{1}*{2}[-]\n", _passiveEffect.ToStringCached(), cInfoStringBaseValue.ToCultureInvariantString("0.0"), cInfoStringPercValue.ToCultureInvariantString("0.0"));
	}

	public static List<ModifierValuesAndSources> GetValuesAndSources(PassiveEffects _passiveEffect, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), bool calcEquipment = true, bool calcHoldingItem = true)
	{
		float _perc_value = 1f;
		List<ModifierValuesAndSources> list = new List<ModifierValuesAndSources>();
		if (_entity == null)
		{
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Self, _entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
		}
		else
		{
			if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
			{
				return list;
			}
			if (EntityClass.list.ContainsKey(_entity.entityClass) && EntityClass.list[_entity.entityClass].Effects != null)
			{
				EntityClass.list[_entity.entityClass].Effects.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Base, _entity, _passiveEffect, ref _originalValue, ref _perc_value, 0f, tags);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Self, _entity, null, _passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
			else
			{
				if (calcHoldingItem && _entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue && !_entity.inventory.holdingItemItemValue.IsMod)
				{
					_entity.inventory.holdingItemItemValue.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Held, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, tags);
				}
				if (calcEquipment && _entity.equipment != null)
				{
					_entity.equipment.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Worn, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, tags);
				}
			}
			if (_entity.Progression != null)
			{
				_entity.Progression.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Progression, _passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
			if (_entity.Buffs != null)
			{
				_entity.Buffs.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.Buff, _passiveEffect, ref _originalValue, ref _perc_value, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && _originalItemValue.ItemClass.Effects != null)
		{
			for (int i = 0; i < _originalItemValue.Modifications.Length; i++)
			{
				if (_originalItemValue.Modifications[i] != null && _originalItemValue.Modifications[i].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.GetModifiedValueData(list, ModifierValuesAndSources.ValueSourceType.ModBonus, _entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref _perc_value, (int)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached()));
				}
			}
		}
		return list;
	}
}
