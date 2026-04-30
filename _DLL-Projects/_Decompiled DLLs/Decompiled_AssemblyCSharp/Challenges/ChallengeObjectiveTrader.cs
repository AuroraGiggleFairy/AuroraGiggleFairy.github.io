using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveTrader : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool BuyItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string TraderName = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Trader;

	public override string DescriptionText
	{
		get
		{
			if (string.IsNullOrEmpty(TraderName))
			{
				if (!BuyItems)
				{
					return Localization.Get("challengeObjectiveSellItems");
				}
				return Localization.Get("challengeObjectiveBuyItems");
			}
			if (!BuyItems)
			{
				return string.Format(Localization.Get("challengeObjectiveSellItemsTo"), Localization.Get(TraderName));
			}
			return string.Format(Localization.Get("challengeObjectiveBuyItemsFrom"), Localization.Get(TraderName));
		}
	}

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		if (BuyItems)
		{
			QuestEventManager.Current.BuyItems += Current_BuyItems;
		}
		else
		{
			QuestEventManager.Current.SellItems += Current_SellItems;
		}
	}

	public override void HandleRemoveHooks()
	{
		if (BuyItems)
		{
			QuestEventManager.Current.BuyItems -= Current_BuyItems;
		}
		else
		{
			QuestEventManager.Current.SellItems -= Current_SellItems;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BuyItems(string traderName, int itemCounts)
	{
		if (!CheckBaseRequirements() && (TraderName == "" || traderName == TraderName))
		{
			base.Current += itemCounts;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SellItems(string traderName, int itemCounts)
	{
		if (!CheckBaseRequirements() && (TraderName == "" || traderName == TraderName))
		{
			base.Current += itemCounts;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("is_buy"))
		{
			BuyItems = StringParsers.ParseBool(e.GetAttribute("is_buy"));
		}
		if (e.HasAttribute("trader_name"))
		{
			TraderName = e.GetAttribute("trader_name");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveTrader
		{
			BuyItems = BuyItems,
			TraderName = TraderName
		};
	}
}
