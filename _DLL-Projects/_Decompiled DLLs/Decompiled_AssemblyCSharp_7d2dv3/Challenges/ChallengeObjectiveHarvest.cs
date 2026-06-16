using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveHarvest : ChallengeBaseTrackedItemObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedHeldClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string heldItemClassID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlock = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requireHeld;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> blockTag = FastTags<TagGroup.Global>.none;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Harvest;

	public override string DescriptionText => Localization.Get("challengeObjectiveHarvest") + " " + expectedItemClass.GetLocalizedItemName() + ":";

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
		expectedHeldClass = ItemClass.GetItemClass(heldItemClassID);
	}

	public override void HandleOnCreated()
	{
		base.HandleOnCreated();
		CreateRequirements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateRequirements()
	{
		if (ShowRequirements && requireHeld)
		{
			Owner.SetRequirementGroup(new RequirementObjectiveGroupHold(heldItemClassID));
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
		QuestEventManager.Current.HarvestItem += Current_HarvestItem;
		base.HandleAddHooks();
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_HarvestItem(ItemValue held, ItemStack stack, BlockValue bv)
	{
		if (!CheckBaseRequirements() && (held.ItemClass == expectedHeldClass || !requireHeld) && (!bv.isair || !isBlock) && (!isBlock || blockTag.IsEmpty || bv.Block.HasAnyFastTags(blockTag)) && stack.itemValue.type == expectedItem.type)
		{
			if (base.Current + stack.count > MaxCount)
			{
				base.Current = MaxCount;
			}
			else
			{
				base.Current += stack.count;
			}
			CheckObjectiveComplete();
			if (base.Complete && IsTracking && trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
			}
		}
	}

	public override void HandleTrackingStarted()
	{
		base.HandleTrackingStarted();
		if (trackingEntry != null)
		{
			Owner.AddTrackingEntry(trackingEntry);
			trackingEntry.TrackingHelper = Owner.TrackingHandler;
			trackingEntry.AddHooks();
		}
	}

	public override void HandleTrackingEnded()
	{
		base.HandleTrackingEnded();
		if (trackingEntry != null)
		{
			trackingEntry.RemoveHooks();
			Owner.RemoveTrackingEntry(trackingEntry);
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("block_tag"))
		{
			blockTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("block_tag"));
		}
		if (e.HasAttribute("held"))
		{
			heldItemClassID = e.GetAttribute("held");
		}
		if (e.HasAttribute("is_block"))
		{
			isBlock = StringParsers.ParseBool(e.GetAttribute("is_block"));
		}
		if (e.HasAttribute("required_held"))
		{
			requireHeld = StringParsers.ParseBool(e.GetAttribute("required_held"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveHarvest
		{
			itemClassID = itemClassID,
			heldItemClassID = heldItemClassID,
			overrideTrackerIndexName = overrideTrackerIndexName,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass,
			expectedHeldClass = expectedHeldClass,
			requireHeld = requireHeld,
			blockTag = blockTag,
			isBlock = isBlock
		};
	}
}
