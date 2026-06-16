using SandboxOptions;
using UnityEngine;

public class XUiM_Trader : XUiModel
{
	public ITrader Trader;

	public XUiC_TraderWindowGroup TraderWindowGroup;

	public TraderData TraderData
	{
		get
		{
			if (Trader == null)
			{
				return null;
			}
			return Trader.TraderData;
		}
	}

	public static int GetBuyPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null, int index = -1)
	{
		float num = 0f;
		int num2 = 1;
		bool flag = false;
		TraderData traderData = _xui.Trader.TraderData;
		TraderInfo traderInfo = traderData?.TraderInfo;
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		if (itemClass.IsBlock())
		{
			num = Block.list[itemValue.type].EconomicValue;
			num2 = Block.list[itemValue.type].EconomicBundleSize;
		}
		else
		{
			num = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, itemClass.EconomicValue, _xui.playerUI.entityPlayer);
			num2 = itemClass.EconomicBundleSize;
		}
		if (num == 0f)
		{
			return 0;
		}
		float num3 = 0f;
		float num4 = 0f;
		if (traderData == null)
		{
			num4 = TraderInfo.BuyMarkup;
		}
		else if (traderInfo.Rentable || traderInfo.PlayerOwned)
		{
			if (index != -1)
			{
				num4 = 1f + (float)traderData.PrimaryInventory[index].Markup * 0.2f;
				flag = true;
			}
		}
		else
		{
			flag = traderInfo.OverrideBuyMarkup != -1f;
			num4 = (flag ? traderInfo.OverrideBuyMarkup : TraderInfo.BuyMarkup);
		}
		if (itemValue.HasQuality)
		{
			num3 = num * num4;
			num3 = ((!(itemClass.TraderQualityMinMod > 0f) && !(itemClass.TraderQualityMaxMod > 0f)) ? (num3 * Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f)) : (num3 * Mathf.Lerp(itemClass.TraderQualityMinMod, itemClass.TraderQualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f)));
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num3 *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num3 += (float)GetBuyPrice(_xui, itemValue2, 1);
				}
			}
		}
		else
		{
			num3 = num * num4;
		}
		if (!flag)
		{
			num3 -= num3 * EffectManager.GetValue(PassiveEffects.BarteringBuying, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags);
		}
		return Mathf.CeilToInt((float)(int)(num3 * (float)(count / num2)) * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.TraderBuyPrices));
	}

	public static int GetSellPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null)
	{
		bool flag = false;
		TraderData traderData = _xui.Trader.TraderData;
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		int economicBundleSize;
		float economicValue;
		if (itemClass.IsBlock())
		{
			Block block = Block.list[itemValue.type];
			economicValue = block.EconomicValue;
			economicValue *= block.EconomicSellScale;
			economicBundleSize = block.EconomicBundleSize;
		}
		else
		{
			economicValue = itemClass.EconomicValue;
			economicValue *= itemClass.EconomicSellScale;
			economicValue = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, economicValue, _xui.playerUI.entityPlayer);
			economicBundleSize = itemClass.EconomicBundleSize;
		}
		if (economicValue == 0f)
		{
			return 0;
		}
		float num = 0f;
		float num2;
		if (traderData == null)
		{
			num2 = TraderInfo.SellMarkdown;
		}
		else
		{
			TraderInfo traderInfo = traderData.TraderInfo;
			flag = traderInfo.OverrideSellMarkdown != -1f;
			num2 = (flag ? traderInfo.OverrideSellMarkdown : TraderInfo.SellMarkdown);
		}
		if (itemValue.HasQuality)
		{
			num = economicValue * num2;
			num = ((!(itemClass.TraderQualityMinMod > 0f) && !(itemClass.TraderQualityMaxMod > 0f)) ? (num * Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f)) : (num * Mathf.Lerp(itemClass.TraderQualityMinMod, itemClass.TraderQualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f)));
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num += (float)GetSellPrice(_xui, itemValue2, 1);
				}
			}
		}
		else
		{
			num = economicValue * num2;
		}
		if (!flag)
		{
			num += num * EffectManager.GetValue(PassiveEffects.BarteringSelling, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags);
		}
		return Mathf.CeilToInt((float)(int)(num * (float)(count / economicBundleSize)) * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.TraderSellPrices));
	}
}
