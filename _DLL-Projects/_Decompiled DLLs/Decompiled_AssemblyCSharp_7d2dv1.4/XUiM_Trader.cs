using UnityEngine;

public class XUiM_Trader : XUiModel
{
	public TraderData Trader;

	public EntityNPC TraderEntity;

	public XUiC_TraderWindowGroup TraderWindowGroup;

	public TileEntityTrader TraderTileEntity;

	public static int GetBuyPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null, int index = -1)
	{
		float num = 0f;
		int num2 = 1;
		bool flag = false;
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
		if (_xui.Trader.Trader == null)
		{
			num4 = TraderInfo.BuyMarkup;
		}
		else if (_xui.Trader.Trader.TraderInfo.Rentable || _xui.Trader.Trader.TraderInfo.PlayerOwned)
		{
			num4 = 1f + (float)_xui.Trader.Trader.GetMarkupByIndex(index) * 0.2f;
			flag = true;
		}
		else
		{
			flag = _xui.Trader.Trader.TraderInfo.OverrideBuyMarkup != -1f;
			num4 = (flag ? _xui.Trader.Trader.TraderInfo.OverrideBuyMarkup : TraderInfo.BuyMarkup);
		}
		if (itemValue.HasQuality)
		{
			num3 = num * num4;
			num3 *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f);
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
		return (int)(num3 * (float)(count / num2));
	}

	public static int GetSellPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null)
	{
		bool flag = false;
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
		if (_xui.Trader.Trader == null)
		{
			num2 = TraderInfo.SellMarkdown;
		}
		else
		{
			flag = _xui.Trader.Trader.TraderInfo.OverrideSellMarkdown != -1f;
			num2 = (flag ? _xui.Trader.Trader.TraderInfo.OverrideSellMarkdown : TraderInfo.SellMarkdown);
		}
		if (itemValue.HasQuality)
		{
			num = economicValue * num2;
			num *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)(int)itemValue.Quality - 1f) / 5f);
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
		return (int)(num * (float)(count / economicBundleSize));
	}
}
