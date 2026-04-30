using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveHold : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public string itemClassID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] itemClassList;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Hold;

	public override string DescriptionText => Localization.Get("challengeObjectiveHold") + " " + Localization.Get(itemClassList[0]) + ":";

	public override void Init()
	{
		itemClassList = itemClassID.Split(',');
		expectedItemClass = ItemClass.GetItemClass(itemClassList[0]);
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.HoldItem -= Current_HoldItem;
		QuestEventManager.Current.HoldItem += Current_HoldItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_HoldItem(ItemValue itemValue)
	{
		if (itemValue.ItemClass != null && itemClassList.ContainsCaseInsensitive(itemValue.ItemClass.Name))
		{
			base.Current = MaxCount;
		}
		else
		{
			base.Current = 0;
		}
		CheckObjectiveComplete();
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.HoldItem -= Current_HoldItem;
	}

	public override bool HandleCheckStatus()
	{
		ItemClass holdingItem = Owner.Owner.Player.inventory.holdingItem;
		if (holdingItem != null)
		{
			base.Current = (itemClassList.ContainsCaseInsensitive(holdingItem.Name) ? MaxCount : 0);
		}
		base.Complete = CheckObjectiveComplete(handleComplete: false);
		return base.Complete;
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
		return new ChallengeObjectiveHold
		{
			itemClassID = itemClassID,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass
		};
	}
}
