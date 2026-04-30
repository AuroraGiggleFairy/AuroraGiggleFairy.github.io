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

	[PublicizedFrom(EAccessModifier.Private)]
	public string wearName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> armorTags;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Wear;

	public override string DescriptionText
	{
		get
		{
			if (expectedItemClass == null)
			{
				return Localization.Get("challengeObjectiveWear") + " " + wearName + ":";
			}
			return Localization.Get("challengeObjectiveWear") + " " + expectedItemClass.GetLocalizedItemName() + ":";
		}
	}

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
		if (!CheckBaseRequirements() && (armorTags.IsEmpty || armorTags.Test_AnySet(itemValue.ItemClass.ItemTags)) && (!(itemClassID != "") || expectedItem.type == itemValue.type))
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
		if (e.HasAttribute("tags"))
		{
			armorTags = FastTags<TagGroup.Global>.Parse(e.GetAttribute("tags"));
		}
		if (e.HasAttribute("wear_name_key"))
		{
			wearName = Localization.Get(e.GetAttribute("wear_name_key"));
		}
		else if (e.HasAttribute("wear_name"))
		{
			wearName = e.GetAttribute("wear_name");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveWear
		{
			itemClassID = itemClassID,
			armorTags = armorTags,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass,
			wearName = wearName
		};
	}
}
