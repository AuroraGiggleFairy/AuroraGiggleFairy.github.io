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
			num = GetCustomValue(infoEntry, itemStack, useMods: false);
			num2 = GetCustomValue(infoEntry, itemStack, useMods: true);
		}
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

	public static string GetStatItemValueTextWithModColoring(ItemStack itemStack, EntityPlayer player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> tags = (infoEntry.TagsSet ? infoEntry.Tags : (primaryFastTags | physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
		MinEventParams.CachedEventParam.Seed = itemStack.itemValue.Seed;
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
			MinEventParams.CachedEventParam.Seed = itemStack.itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods: false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false);
		}
		else
		{
			num = GetCustomValue(infoEntry, itemStack, useMods: false);
			num2 = GetCustomValue(infoEntry, itemStack, useMods: true);
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (num != num2)
			{
				bool flag = num2 > num;
				return ((infoEntry.NegativePreferred ? (!flag) : flag) ? "[00FF00]" : "[FF0000]") + ShowLocalizedBool(Convert.ToBoolean(num2)) + "%[-]";
			}
			return ShowLocalizedBool(Convert.ToBoolean(num));
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				bool flag2 = num2 - num > 0f;
				return ((infoEntry.NegativePreferred ? (!flag2) : flag2) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num) + "[-])";
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
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
				bool flag3 = num2 - num > 0f;
				bool num3 = (infoEntry.NegativePreferred ? (!flag3) : flag3);
				return string.Concat(str1: (num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "", str0: num3 ? "[00FF00]" : "[FF0000]", str2: num2.ToString(), str3: "%[-]");
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
				bool flag4 = num2 - num > 0f;
				return ((infoEntry.NegativePreferred ? (!flag4) : flag4) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2) + "[-]";
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			bool flag5 = num2 - num > 0f;
			bool num4 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
			return string.Concat(str1: (num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "", str0: num4 ? "[00FF00]" : "[FF0000]", str2: num2.ToString(), str3: "[-]");
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

	public static string GetStatItemValueTextWithCompareInfo(ItemStack itemStack, ItemStack compareStack, EntityPlayer player, DisplayInfoEntry infoEntry, bool flipCompare = false, bool useMods = true)
	{
		FastTags<TagGroup.Global> tags = (infoEntry.TagsSet ? infoEntry.Tags : (primaryFastTags | physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		if (compareStack.IsEmpty() || compareStack == itemStack)
		{
			return GetStatItemValueTextWithModColoring(itemStack, player, infoEntry);
		}
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
			MinEventParams.CachedEventParam.Seed = itemStack.itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods);
			num2 = EffectManager.GetValue(infoEntry.StatType, compareStack.itemValue, 0f, player, null, tags, calcEquipment: false, calcHoldingItem: false, calcProgression: false, calcBuffs: false, calcChallenges: true, 1, useMods);
		}
		else
		{
			num = GetCustomValue(infoEntry, itemStack, useMods);
			num2 = GetCustomValue(infoEntry, compareStack, useMods);
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (!compareStack.IsEmpty() && num != num2)
			{
				bool flag = num2 > num;
				bool flag2 = (infoEntry.NegativePreferred ? (!flag) : flag);
				return ShowLocalizedBool(Convert.ToBoolean(num)) + " (" + (flag2 ? "[00FF00]" : "[FF0000]") + ShowLocalizedBool(Convert.ToBoolean(num2)) + "[-])";
			}
			return ShowLocalizedBool(Convert.ToBoolean(num));
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (!compareStack.IsEmpty() && num != num2)
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
		if (!compareStack.IsEmpty() && (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num))))
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
				return text + num + "% (" + (flag6 ? "[00FF00]" : "[FF0000]") + (flag5 ? "+" : "") + num4 + "%[-])";
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
			return text2 + num + " (" + (flag8 ? "[00FF00]" : "[FF0000]") + (flag7 ? "+" : "") + num5.ToString("0.##") + "[-])";
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			num *= 100f;
			num = Mathf.Floor(num);
			if (infoEntry.ShowInverted)
			{
				num -= 100f;
			}
			return ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0") + "%";
		}
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
		return ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0.##");
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

	public static float GetCustomValue(DisplayInfoEntry entry, ItemStack itemStack, bool useMods)
	{
		switch (entry.CustomName)
		{
		case "ExplosionResistance":
		{
			Block block5 = itemStack.itemValue.ToBlockValue().Block;
			if (block5 != null)
			{
				return block5.GetExplosionResistance();
			}
			break;
		}
		case "FertileLevel":
		{
			Block block10 = itemStack.itemValue.ToBlockValue().Block;
			if (block10 != null)
			{
				return block10.blockMaterial.FertileLevel;
			}
			break;
		}
		case "LightOpacity":
		{
			Block block4 = itemStack.itemValue.ToBlockValue().Block;
			if (block4 != null)
			{
				return block4.lightOpacity;
			}
			break;
		}
		case "Mass":
		{
			Block block11 = itemStack.itemValue.ToBlockValue().Block;
			if (block11 != null)
			{
				return block11.blockMaterial.Mass.Value;
			}
			break;
		}
		case "MaxDamage":
		{
			Block block3 = itemStack.itemValue.ToBlockValue().Block;
			if (block3 != null)
			{
				return block3.MaxDamage;
			}
			break;
		}
		case "RequiredPower":
			if (itemStack.itemValue.ToBlockValue().Block is BlockPowered blockPowered)
			{
				return blockPowered.RequiredPower;
			}
			break;
		case "StabilityGlue":
		{
			Block block9 = itemStack.itemValue.ToBlockValue().Block;
			if (block9 != null)
			{
				return block9.blockMaterial.StabilityGlue;
			}
			break;
		}
		case "StabilitySupport":
		{
			Block block2 = itemStack.itemValue.ToBlockValue().Block;
			if (block2 != null)
			{
				if (!block2.StabilitySupport)
				{
					return 0f;
				}
				return 1f;
			}
			break;
		}
		case "Explosion.RadiusBlocks":
		{
			Block block7 = itemStack.itemValue.ToBlockValue().Block;
			if (block7 != null)
			{
				if (block7 is BlockMine blockMine3)
				{
					return blockMine3.Explosion.BlockRadius;
				}
				if (block7 is BlockCarExplode blockCarExplode3)
				{
					return blockCarExplode3.Explosion.BlockRadius;
				}
				if (block7 is BlockCarExplodeLoot blockCarExplodeLoot3)
				{
					return blockCarExplodeLoot3.Explosion.BlockRadius;
				}
			}
			break;
		}
		case "Explosion.BlockDamage":
		{
			Block block8 = itemStack.itemValue.ToBlockValue().Block;
			if (block8 != null)
			{
				if (block8 is BlockMine blockMine4)
				{
					return blockMine4.Explosion.BlockDamage;
				}
				if (block8 is BlockCarExplode blockCarExplode4)
				{
					return blockCarExplode4.Explosion.BlockDamage;
				}
				if (block8 is BlockCarExplodeLoot blockCarExplodeLoot4)
				{
					return blockCarExplodeLoot4.Explosion.BlockDamage;
				}
			}
			break;
		}
		case "Explosion.RadiusEntities":
		{
			Block block = itemStack.itemValue.ToBlockValue().Block;
			if (block != null)
			{
				if (block is BlockMine blockMine)
				{
					return blockMine.Explosion.EntityRadius;
				}
				if (block is BlockCarExplode blockCarExplode)
				{
					return blockCarExplode.Explosion.EntityRadius;
				}
				if (block is BlockCarExplodeLoot blockCarExplodeLoot)
				{
					return blockCarExplodeLoot.Explosion.EntityRadius;
				}
			}
			break;
		}
		case "Explosion.EntityDamage":
		{
			Block block6 = itemStack.itemValue.ToBlockValue().Block;
			if (block6 != null)
			{
				if (block6 is BlockMine blockMine2)
				{
					return blockMine2.Explosion.EntityDamage;
				}
				if (block6 is BlockCarExplode blockCarExplode2)
				{
					return blockCarExplode2.Explosion.EntityDamage;
				}
				if (block6 is BlockCarExplodeLoot blockCarExplodeLoot2)
				{
					return blockCarExplodeLoot2.Explosion.EntityDamage;
				}
			}
			break;
		}
		default:
		{
			float num = 0f;
			if (itemStack.itemValue.ItemClass != null && itemStack.itemValue.ItemClass.Effects != null && itemStack.itemValue.ItemClass.Effects.EffectGroups != null)
			{
				num = GetCustomDisplayValueForItem(itemStack.itemValue, entry);
				if (useMods)
				{
					for (int i = 0; i < itemStack.itemValue.Modifications.Length; i++)
					{
						if (itemStack.itemValue.Modifications[i] != null && itemStack.itemValue.Modifications[i].ItemClass is ItemClassModifier)
						{
							num += GetCustomDisplayValueForItem(itemStack.itemValue.Modifications[i], entry);
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
		float num = 0f;
		for (int i = 0; i < itemValue.ItemClass.Effects.EffectGroups.Count; i++)
		{
			MinEffectGroup minEffectGroup = itemValue.ItemClass.Effects.EffectGroups[i];
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = itemValue.Seed;
			if (minEffectGroup.EffectDisplayValues.ContainsKey(entry.CustomName) && minEffectGroup.EffectDisplayValues[entry.CustomName].IsValid(MinEventParams.CachedEventParam))
			{
				num += minEffectGroup.EffectDisplayValues[entry.CustomName].GetValue(itemValue.Quality);
			}
			List<MinEventActionBase> triggeredEffects = minEffectGroup.TriggeredEffects;
			if (triggeredEffects == null)
			{
				continue;
			}
			for (int j = 0; j < triggeredEffects.Count; j++)
			{
				MinEventActionBase minEventActionBase = triggeredEffects[j];
				if ((minEventActionBase.EventType != MinEventTypes.onSelfPrimaryActionEnd && minEventActionBase.EventType != MinEventTypes.onSelfSecondaryActionEnd) || !(minEventActionBase is MinEventActionModifyCVar))
				{
					continue;
				}
				bool flag = true;
				for (int k = 0; k < minEventActionBase.Requirements.Count; k++)
				{
					if (!minEventActionBase.Requirements[k].IsValid(MinEventParams.CachedEventParam))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					MinEventActionModifyCVar minEventActionModifyCVar = minEventActionBase as MinEventActionModifyCVar;
					if (minEventActionModifyCVar.cvarName == entry.CustomName && minEventActionModifyCVar.targetType == MinEventActionTargetedBase.TargetTypes.self)
					{
						num += minEventActionModifyCVar.GetValueForDisplay();
					}
				}
			}
		}
		return num;
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
