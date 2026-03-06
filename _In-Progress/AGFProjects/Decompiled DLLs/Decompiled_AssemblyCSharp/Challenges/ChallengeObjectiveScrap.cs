using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveScrap : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public string itemClassID = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Scrap;

	public override string DescriptionText
	{
		get
		{
			string text = ((expectedItemClass != null) ? expectedItemClass.GetLocalizedItemName() : Localization.Get("xuiItems"));
			return Localization.Get("challengeObjectiveScrap") + " " + text + ":";
		}
	}

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.ScrapItem += Current_ScrapItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ScrapItem(ItemStack stack)
	{
		if (!CheckBaseRequirements() && (expectedItemClass == null || stack.itemValue.type == expectedItem.type))
		{
			base.Current += stack.count;
			CheckObjectiveComplete();
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.ScrapItem -= Current_ScrapItem;
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
		return new ChallengeObjectiveScrap
		{
			itemClassID = itemClassID,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass
		};
	}
}
