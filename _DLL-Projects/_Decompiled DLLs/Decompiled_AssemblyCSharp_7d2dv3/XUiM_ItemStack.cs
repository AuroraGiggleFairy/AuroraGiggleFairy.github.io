using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class XUiM_ItemStack : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> primaryFastTags = FastTags<TagGroup.Global>.Parse("primary");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageFastTags = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedTrue = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedFalse = "";

	public static bool HasItemStats(ItemStack itemStack)
	{
		if (itemStack.itemValue.ItemClass == null)
		{
			return false;
		}
		if (itemStack.itemValue.ItemClass.IsBlock())
		{
			return Block.list[itemStack.itemValue.type].DisplayType != "";
		}
		return itemStack.itemValue.ItemClass.DisplayType != "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BuffActionStrings(ItemAction itemAction, List<string> stringList)
	{
		if (itemAction.BuffActions == null || itemAction.BuffActions.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemAction.BuffActions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(itemAction.BuffActions[i]);
			if (buff != null && !string.IsNullOrEmpty(buff.Name))
			{
				stringList.Add(StringFormatHandler(Localization.Get("lblEffect"), $"{buff.Name}"));
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStat(string _title, float _value)
	{
		if (_value > 0f)
		{
			return $"{_title}: [00ff00]+{_value.ToCultureInvariantString()}[-]";
		}
		if (_value < 0f)
		{
			return $"{_title}: [ff0000]{_value.ToCultureInvariantString()}[-]";
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStatPercentage(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return $"{title}: [REPLACE_COLOR]{value}[-]\n";
	}

	public static string GetStatItemValueTextWithModInfo(ItemStack itemStack, EntityPlayer player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> tags = (infoEntry.TagsSet ? infoEntry.Tags : (primaryFastTags | physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
		MinEventParams.CachedEventParam.Seed = itemStack.itemValue.Seed;
		if (infoEntry.CustomName == "")
		{
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods: false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false);
		}
		else
		{
			num = GetCustomValue(infoEntry, itemStack.itemValue, useMods: false);
			num2 = GetCustomValue(infoEntry, itemStack.itemValue, useMods: true);
		}
		degradationMaxMod(infoEntry.StatType, itemStack.itemValue, player, tags, _useMods: false, ref num);
		degradationMaxMod(infoEntry.StatType, itemStack.itemValue, player, tags, _useMods: true, ref num2);
		if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num)))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				float num3 = num2 - num;
				bool flag = num3 > 0f;
				bool flag2 = (infoEntry.NegativePreferred ? (!flag) : flag);
				string text = ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
				return text + num2 + "% (" + (flag2 ? "[00FF00]" : "[FF0000]") + (flag ? "+" : "") + num3 + "%[-])";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
				num = Mathf.Floor(num);
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				float num4 = num2 - num;
				bool flag3 = num4 > 0f;
				bool flag4 = (infoEntry.NegativePreferred ? (!flag3) : flag3);
				return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2) + " (" + (flag4 ? "[00FF00]" : "[FF0000]") + (flag3 ? "+" : "") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num4) + "[-])";
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			float num5 = num2 - num;
			bool flag5 = num5 > 0f;
			bool flag6 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
			string text2 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
			return text2 + num2 + " (" + (flag6 ? "[00FF00]" : "[FF0000]") + (flag5 ? "+" : "") + num5.ToString("0.##") + "[-])";
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			num2 *= 100f;
			num2 = Mathf.Floor(num2);
			if (infoEntry.ShowInverted)
			{
				num2 -= 100f;
			}
			return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0") + "%";
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
		{
			num2 *= 10f;
			num2 = Mathf.Floor(num2);
			num2 /= 10f;
		}
		else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
		{
			num2 *= 100f;
			num2 = Mathf.Floor(num2);
			num2 /= 100f;
		}
		else
		{
			num2 = Mathf.Floor(num2);
		}
		if (infoEntry.ShowInverted)
		{
			num2 -= 1f;
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2);
		}
		return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0.##");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void degradationMaxMod(PassiveEffects _statType, ItemValue _itemValue, EntityPlayer _player, FastTags<TagGroup.Global> _tags, bool _useMods, ref float _value)
	{
		if (_statType == PassiveEffects.DegradationMax)
		{
			_value = EffectManager.GetValue(PassiveEffects.DegradationMax, _itemValue, 0f, _player, null, _tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, _useMods);
			_value = ItemValue.ModMaxUseTimes((int)_value, _itemValue);
		}
	}

	public static string GetStatItemValueTextWithModColoring(ItemValue itemValue, EntityPlayerLocal player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> tags = (infoEntry.TagsSet ? infoEntry.Tags : (primaryFastTags | physicalDamageFastTags));
		MinEventParams.CachedEventParam.ItemValue = itemValue;
		MinEventParams.CachedEventParam.Seed = itemValue.Seed;
		float _value;
		float _value2;
		if (infoEntry.CustomName == "")
		{
			_value = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods: false);
			_value2 = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false);
		}
		else
		{
			_value = GetCustomValue(infoEntry, itemValue, useMods: false);
			_value2 = GetCustomValue(infoEntry, itemValue, useMods: true);
		}
		string text = "";
		degradationMaxMod(infoEntry.StatType, itemValue, player, tags, _useMods: false, ref _value);
		degradationMaxMod(infoEntry.StatType, itemValue, player, tags, _useMods: true, ref _value2);
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (_value != _value2)
			{
				bool flag = _value2 > _value;
				return ((infoEntry.NegativePreferred ? (!flag) : flag) ? "[00FF00]" : "[FF0000]") + ShowLocalizedBool(Convert.ToBoolean(_value2)) + "%[-]";
			}
			return ShowLocalizedBool(Convert.ToBoolean(_value));
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (_value != _value2)
			{
				if (infoEntry.ShowInverted)
				{
					_value -= 1f;
					_value2 -= 1f;
				}
				bool flag2 = _value2 - _value > 0f;
				return ((infoEntry.NegativePreferred ? (!flag2) : flag2) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(_value) + "[-])";
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(_value);
		}
		if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(_value2 * 100f) != Mathf.Floor(_value * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(_value2) != Mathf.Floor(_value)))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				_value = Mathf.Floor(_value * 100f);
				_value2 = Mathf.Floor(_value2 * 100f);
				if (infoEntry.ShowInverted)
				{
					_value -= 100f;
					_value2 -= 100f;
				}
				bool flag3 = _value2 - _value > 0f;
				bool num = (infoEntry.NegativePreferred ? (!flag3) : flag3);
				text = string.Concat(str1: (_value2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "", str0: num ? "[00FF00]" : "[FF0000]", str2: _value2.ToString(), str3: "%[-]");
			}
			else
			{
				if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
				{
					_value2 = Mathf.Floor(_value2 * 10f) / 10f;
					_value = Mathf.Floor(_value * 10f) / 10f;
				}
				else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
				{
					_value2 = Mathf.Floor(_value2 * 100f) / 100f;
					_value = Mathf.Floor(_value * 100f) / 100f;
				}
				else
				{
					_value2 = Mathf.Floor(_value2);
					_value = Mathf.Floor(_value);
				}
				if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
				{
					bool flag4 = _value2 - _value > 0f;
					text = ((infoEntry.NegativePreferred ? (!flag4) : flag4) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(_value2) + "[-]";
				}
				else
				{
					if (infoEntry.ShowInverted)
					{
						_value -= 1f;
						_value2 -= 1f;
					}
					bool flag5 = _value2 - _value > 0f;
					bool num2 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
					text = string.Concat(str1: (_value > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "", str0: num2 ? "[00FF00]" : "[FF0000]", str2: _value2.ToString(), str3: "[-]");
				}
			}
		}
		else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			_value2 = Mathf.Floor(_value2 * 100f);
			if (infoEntry.ShowInverted)
			{
				_value2 -= 100f;
			}
			text = ((_value2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + _value2.ToString("0") + "%";
		}
		else
		{
			_value2 = ((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1) ? (Mathf.Floor(_value2 * 10f) / 10f) : ((infoEntry.DisplayType != DisplayInfoEntry.DisplayTypes.Decimal2) ? Mathf.Floor(_value2) : (Mathf.Floor(_value2 * 100f) / 100f)));
			if (infoEntry.ShowInverted)
			{
				_value2 -= 1f;
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				text = XUiM_PlayerBuffs.GetCVarValueAsTimeString(_value2);
			}
			else
			{
				text = _value2.ToString("0.##");
				if (_value2 > 0f && infoEntry.DisplayLeadingPlus)
				{
					text = "+" + text;
				}
			}
		}
		if (itemValue.GetStatPercent(infoEntry.StatType, _onlyBoosted: true) != 1f)
		{
			text = "[sp=ui_stat]" + text;
		}
		if (InputUtils.ShiftKeyPressed || LocalPlayerUI.GetUIForPrimaryPlayer().playerInput.GUIActions.Inspect.IsPressed || player.PlayerUI.windowManager.IsWindowOpen("combine"))
		{
			float statPercent = itemValue.GetStatPercent(infoEntry.StatType, _onlyBoosted: false);
			if (statPercent != 1f)
			{
				text += $" [00f0f0]({(statPercent - 1f) * 100f:0.#}%)";
			}
		}
		return text;
	}

	public static string GetStatItemValueTextWithCompareInfo(ItemValue itemValue, ItemValue compareValue, EntityPlayerLocal player, DisplayInfoEntry infoEntry, bool flipCompare = false, bool useMods = true)
	{
		FastTags<TagGroup.Global> tags = (infoEntry.TagsSet ? infoEntry.Tags : (primaryFastTags | physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		if (compareValue.IsEmpty() || compareValue.Equals(itemValue))
		{
			return GetStatItemValueTextWithModColoring(itemValue, player, infoEntry);
		}
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods);
			num2 = EffectManager.GetValue(infoEntry.StatType, compareValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods);
		}
		else
		{
			num = GetCustomValue(infoEntry, itemValue, useMods);
			num2 = GetCustomValue(infoEntry, compareValue, useMods);
		}
		degradationMaxMod(infoEntry.StatType, itemValue, player, tags, useMods, ref num);
		degradationMaxMod(infoEntry.StatType, compareValue, player, tags, useMods, ref num2);
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (!compareValue.IsEmpty() && num != num2)
			{
				bool flag = num2 > num;
				bool flag2 = (infoEntry.NegativePreferred ? (!flag) : flag);
				return ShowLocalizedBool(Convert.ToBoolean(num)) + " (" + (flag2 ? "[00FF00]" : "[FF0000]") + ShowLocalizedBool(Convert.ToBoolean(num2)) + "[-])";
			}
			return ShowLocalizedBool(Convert.ToBoolean(num));
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (!compareValue.IsEmpty() && num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				float num3 = num2 - num;
				if (flipCompare)
				{
					num3 = num - num2;
				}
				bool flag3 = num3 > 0f;
				bool flag4 = (infoEntry.NegativePreferred ? (!flag3) : flag3);
				return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num) + " (" + (flag4 ? "[00FF00]" : "[FF0000]") + (flag3 ? "+" : "-") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(Mathf.Abs(num3)) + "[-])";
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
		string empty = string.Empty;
		if (!compareValue.IsEmpty() && (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num))))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				float num4 = num2 - num;
				if (flipCompare)
				{
					num4 = num - num2;
				}
				bool flag5 = num4 > 0f;
				bool flag6 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
				string text = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
				empty = text + num + "% (" + (flag6 ? "[00FF00]" : "[FF0000]") + (flag5 ? "+" : "") + num4 + "%[-])";
			}
			else
			{
				if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
				{
					num2 *= 10f;
					num2 = Mathf.Floor(num2);
					num2 /= 10f;
					num *= 10f;
					num = Mathf.Floor(num);
					num /= 10f;
				}
				else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
				{
					num2 *= 100f;
					num2 = Mathf.Floor(num2);
					num2 /= 100f;
					num *= 100f;
					num = Mathf.Floor(num);
					num /= 100f;
				}
				else
				{
					num2 = Mathf.Floor(num2);
					num = Mathf.Floor(num);
				}
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				float num5 = num2 - num;
				if (flipCompare)
				{
					num5 = num - num2;
				}
				bool flag7 = num5 > 0f;
				bool flag8 = (infoEntry.NegativePreferred ? (!flag7) : flag7);
				string text2 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
				empty = text2 + num + " (" + (flag8 ? "[00FF00]" : "[FF0000]") + (flag7 ? "+" : "") + num5.ToString("0.##") + "[-])";
			}
		}
		else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			num *= 100f;
			num = Mathf.Floor(num);
			if (infoEntry.ShowInverted)
			{
				num -= 100f;
			}
			empty = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0") + "%";
		}
		else
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num = Mathf.Floor(num);
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
			}
			empty = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0.##");
		}
		bool num6 = ItemValue.IsStatLowerBetter(infoEntry.StatType);
		float statPercent = itemValue.GetStatPercent(infoEntry.StatType, _onlyBoosted: false);
		statPercent = compareValue.GetStatPercent(infoEntry.StatType, _onlyBoosted: false) - statPercent;
		if (!num6)
		{
			if (statPercent > 0f)
			{
				empty += $" [00f0f0](+{statPercent * 100f:0.#}%)";
			}
		}
		else if (statPercent < 0f)
		{
			empty += $" [00f0f0]({statPercent * 100f:0.#}%)";
		}
		return empty;
	}

	public static string ShowLocalizedBool(bool value)
	{
		if (localizedTrue == "")
		{
			localizedTrue = Localization.Get("statTrue");
			localizedFalse = Localization.Get("statFalse");
		}
		if (!value)
		{
			return localizedFalse;
		}
		return localizedTrue;
	}

	public static bool CanCompare(ItemClass item1, ItemClass item2)
	{
		if (item1 == null || item2 == null)
		{
			return false;
		}
		string displayType = item1.DisplayType;
		string displayType2 = item2.DisplayType;
		if (item1.IsBlock())
		{
			displayType = Block.list[item1.Id].DisplayType;
		}
		if (item2.IsBlock())
		{
			displayType2 = Block.list[item2.Id].DisplayType;
		}
		ItemDisplayEntry displayStatsForTag = UIDisplayInfoManager.Current.GetDisplayStatsForTag(displayType);
		ItemDisplayEntry displayStatsForTag2 = UIDisplayInfoManager.Current.GetDisplayStatsForTag(displayType2);
		if (displayStatsForTag != null && displayStatsForTag2 != null)
		{
			return displayStatsForTag.DisplayGroup == displayStatsForTag2.DisplayGroup;
		}
		return false;
	}

	public static float GetCustomValue(DisplayInfoEntry entry, ItemValue itemValue, bool useMods)
	{
		Block block = itemValue?.ToBlockValue().Block;
		if (block != null && block.SelectAlternates)
		{
			block = block.GetAltBlockValue(itemValue.Meta).Block ?? block;
		}
		switch (entry.CustomName)
		{
		case "ExplosionResistance":
			if (block != null)
			{
				return block.GetExplosionResistance();
			}
			break;
		case "FertileLevel":
			if (block != null)
			{
				return block.blockMaterial.FertileLevel;
			}
			break;
		case "LightOpacity":
			if (block != null)
			{
				return block.lightOpacity;
			}
			break;
		case "Mass":
			if (block != null)
			{
				return block.blockMaterial.Mass.Value;
			}
			break;
		case "MaxDamage":
			if (block != null)
			{
				return block.MaxDamage;
			}
			break;
		case "RequiredPower":
			if (block is BlockPowered blockPowered)
			{
				return blockPowered.RequiredPower;
			}
			break;
		case "StabilityGlue":
			if (block != null)
			{
				return block.blockMaterial.StabilityGlue;
			}
			break;
		case "StabilitySupport":
			if (block != null)
			{
				if (!block.StabilitySupport)
				{
					return 0f;
				}
				return 1f;
			}
			break;
		case "Explosion.RadiusBlocks":
			if (block != null)
			{
				if (block is BlockMine blockMine)
				{
					return blockMine.Explosion.BlockRadius;
				}
				if (block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out var featureData))
				{
					return featureData.Props.GetFloat("RadiusBlocks");
				}
			}
			break;
		case "Explosion.BlockDamage":
			if (block != null)
			{
				if (block is BlockMine blockMine4)
				{
					return blockMine4.Explosion.BlockDamage;
				}
				if (block is BlockCompositeTileEntity blockCompositeTileEntity4 && blockCompositeTileEntity4.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out var featureData4))
				{
					return featureData4.Props.GetFloat("BlockDamage");
				}
			}
			break;
		case "Explosion.RadiusEntities":
			if (block != null)
			{
				if (block is BlockMine blockMine2)
				{
					return blockMine2.Explosion.EntityRadius;
				}
				if (block is BlockCompositeTileEntity blockCompositeTileEntity2 && blockCompositeTileEntity2.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out var featureData2))
				{
					return featureData2.Props.GetFloat("RadiusEntities");
				}
			}
			break;
		case "Explosion.EntityDamage":
			if (block != null)
			{
				if (block is BlockMine blockMine3)
				{
					return blockMine3.Explosion.EntityDamage;
				}
				if (block is BlockCompositeTileEntity blockCompositeTileEntity3 && blockCompositeTileEntity3.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out var featureData3))
				{
					return featureData3.Props.GetFloat("EntityDamage");
				}
			}
			break;
		default:
		{
			float num = 0f;
			if (itemValue.ItemClass != null && itemValue.ItemClass.Effects != null && itemValue.ItemClass.Effects.EffectGroups != null)
			{
				num = GetCustomDisplayValueForItem(itemValue, entry);
				if (useMods)
				{
					for (int i = 0; i < itemValue.Modifications.Length; i++)
					{
						if (itemValue.Modifications[i] != null && itemValue.Modifications[i].ItemClass is ItemClassModifier)
						{
							num += GetCustomDisplayValueForItem(itemValue.Modifications[i], entry);
						}
					}
				}
			}
			return num;
		}
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetCustomDisplayValueForItem(ItemValue itemValue, DisplayInfoEntry entry)
	{
		float newValue = 0f;
		List<MinEffectGroup> list = itemValue.ItemClass.Effects?.EffectGroups;
		if (list == null)
		{
			return newValue;
		}
		for (int i = 0; i < list.Count; i++)
		{
			MinEffectGroup minEffectGroup = list[i];
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = itemValue.Seed;
			if (minEffectGroup.EffectDisplayValues.ContainsKey(entry.CustomName) && minEffectGroup.EffectDisplayValues[entry.CustomName].IsValid(MinEventParams.CachedEventParam))
			{
				newValue += minEffectGroup.EffectDisplayValues[entry.CustomName].GetValue(itemValue.Quality);
			}
			foreach (MinEventActionBase triggeredEffect in minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfPrimaryActionEnd))
			{
				AddValueForDisplayIfValid(triggeredEffect);
			}
			foreach (MinEventActionBase triggeredEffect2 in minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfSecondaryActionEnd))
			{
				AddValueForDisplayIfValid(triggeredEffect2);
			}
		}
		return newValue;
		[PublicizedFrom(EAccessModifier.Internal)]
		void AddValueForDisplayIfValid(MinEventActionBase actionBase)
		{
			if (actionBase is MinEventActionModifyCVar && actionBase.CanExecute(actionBase.EventType, MinEventParams.CachedEventParam))
			{
				MinEventActionModifyCVar minEventActionModifyCVar = actionBase as MinEventActionModifyCVar;
				if (minEventActionModifyCVar.cvarName == entry.CustomName && minEventActionModifyCVar.targetType == MinEventActionTargetedBase.TargetTypes.self)
				{
					newValue += minEventActionModifyCVar.GetValueForDisplay();
				}
			}
		}
	}

	public static bool CheckKnown(EntityPlayerLocal player, ItemClass itemClass, ItemValue itemValue = null)
	{
		string unlocks = itemClass.Unlocks;
		bool flag = false;
		if (unlocks != "")
		{
			if (player.GetCVar(unlocks) == 1f)
			{
				flag = true;
			}
			if (!flag)
			{
				ProgressionValue progressionValue = player.Progression.GetProgressionValue(unlocks);
				if (progressionValue != null)
				{
					if (progressionValue.ProgressionClass.IsCrafting)
					{
						if (progressionValue.Level == progressionValue.ProgressionClass.MaxLevel)
						{
							flag = true;
						}
					}
					else if (progressionValue.Level == 1)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Recipe recipe = CraftingManager.GetRecipe(unlocks);
				if (recipe != null && !recipe.scrapable && !recipe.wildcardForgeCategory && recipe.IsUnlocked(player))
				{
					flag = true;
				}
			}
		}
		return flag;
	}
}
