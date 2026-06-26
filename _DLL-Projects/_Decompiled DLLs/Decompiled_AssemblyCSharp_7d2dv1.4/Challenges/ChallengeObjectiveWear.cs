using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveWear : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public string itemClassID = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Wear;

	public override string DescriptionText => Localization.Get("challengeObjectiveWear") + " " + expectedItemClass.GetLocalizedItemName() + ":";

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.WearItem -= Current_WearItem;
		XUi xui = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui;
		_ = xui.PlayerInventory;
		if (xui.PlayerEquipment.IsWearing(expectedItem))
		{
			base.Current = MaxCount;
			CheckObjectiveComplete();
		}
		else
		{
			QuestEventManager.Current.WearItem += Current_WearItem;
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.WearItem -= Current_WearItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WearItem(ItemValue itemValue)
	{
		if (itemValue.type == expectedItem.type)
		{
			base.Current = MaxCount;
			CheckObjectiveComplete();
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("item"))
		{
			itemClassID = e.GetAttribute("item");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveWear
		{
			itemClassID = itemClassID,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass
		};
	}
}
