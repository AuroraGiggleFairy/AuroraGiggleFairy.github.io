public class XUiM_PlayerDefense : XUiModel
{
	public static string GetBashing(EntityPlayer _player)
	{
		return GetDefenseFromPlayer(_player, EnumDamageTypes.Bashing) + "%";
	}

	public static string GetPiercing(EntityPlayer _player)
	{
		return GetDefenseFromPlayer(_player, EnumDamageTypes.Piercing) + "%";
	}

	public static string GetRadiation(EntityPlayer _player)
	{
		return GetDefenseFromPlayer(_player, EnumDamageTypes.Radiation) + "%";
	}

	public static string GetWaterproof(EntityPlayer _player)
	{
		return $"{(int)(_player.equipment.GetTotalWaterproof() * 100f)}%";
	}

	public static string GetFireproof(EntityPlayer _player)
	{
		return GetDefenseFromPlayer(_player, EnumDamageTypes.Heat) + "%";
	}

	public static string GetElectrical(EntityPlayer _player)
	{
		return GetDefenseFromPlayer(_player, EnumDamageTypes.Electrical) + "%";
	}

	public static string GetInsulation(EntityPlayer _player)
	{
		return ValueDisplayFormatters.TemperatureRelative(_player.equipment.GetTotalInsulation(), 0);
	}

	public static string GetWeight(EntityPlayer _player)
	{
		float num = 0f;
		int slotCount = _player.equipment.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			ItemClass itemClass = _player.equipment.GetSlotItem(i)?.ItemClass;
			if (itemClass != null)
			{
				num += itemClass.Encumbrance * 100f;
			}
		}
		return num.ToCultureInvariantString() + "%";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromPlayer(EntityPlayer _player, EnumDamageTypes _armorType)
	{
		float value = 0f;
		if (_armorType != EnumDamageTypes.None && _armorType < EnumDamageTypes.Disease)
		{
			if (_armorType <= EnumDamageTypes.Crushing)
			{
				value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, _player);
			}
			else if (_armorType <= EnumDamageTypes.Electrical)
			{
				value = EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, _player, null, FastTags<TagGroup.Global>.Parse(_armorType.ToStringCached()));
			}
		}
		return value.ToCultureInvariantString("00");
	}

	public static string GetBashing(ItemValue _itemValue, XUi _xui)
	{
		return GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Bashing, _xui) + "%";
	}

	public static string GetPiercing(ItemValue _itemValue, XUi _xui)
	{
		return GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Piercing, _xui) + "%";
	}

	public static string GetRadiation(ItemValue _itemValue, XUi _xui)
	{
		return GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Radiation, _xui) + "%";
	}

	public static string GetWaterproof(ItemValue _itemValue)
	{
		float num = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			num = itemClass.WaterProof;
		}
		return $"{(int)(num * 100f)}%";
	}

	public static string GetFireproof(ItemValue _itemValue, XUi _xui)
	{
		return GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Heat, _xui) + "%";
	}

	public static string GetElectrical(ItemValue _itemValue, XUi _xui)
	{
		return GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Electrical, _xui) + "%";
	}

	public static string GetInsulation(ItemValue _itemValue)
	{
		float fahrenheit = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			fahrenheit = itemClass.Insulation;
		}
		return ValueDisplayFormatters.TemperatureRelative(fahrenheit, 0);
	}

	public static string GetWeight(ItemValue _itemValue)
	{
		float value = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			value = itemClass.Encumbrance * 100f;
		}
		return value.ToCultureInvariantString() + "%";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromItemValue(ItemValue _itemValue, EnumDamageTypes _damageType, XUi _xui)
	{
		return (EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, _itemValue, 0f, _xui.playerUI.entityPlayer, null, FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached())) * 100f).ToCultureInvariantString("00");
	}
}
