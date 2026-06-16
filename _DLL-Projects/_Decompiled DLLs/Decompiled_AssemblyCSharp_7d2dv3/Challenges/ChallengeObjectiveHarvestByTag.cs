using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveHarvestByTag : ChallengeBaseTrackedItemObjective
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

	[PublicizedFrom(EAccessModifier.Private)]
	public string harvestTag = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> harvestTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	public string targetName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideNavObject = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Harvest;

	public override string DescriptionText => Localization.Get("challengeObjectiveHarvest") + " " + Localization.Get(targetName) + ":";

	public override void Init()
	{
		harvestTags = FastTags<TagGroup.Global>.Parse(harvestTag);
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
		if (overrideTrackerIndexName != null && overrideTrackerIndexName != null && trackingEntry == null && !disableTracking)
		{
			trackingEntry = new TrackingEntry
			{
				TrackedItem = expectedItemClass,
				Owner = this,
				blockIndexName = overrideTrackerIndexName,
				navObjectName = ((overrideNavObject != "") ? overrideNavObject : "quest_resource"),
				trackDistance = trackDistance
			};
			trackingEntry.TrackingHelper = Owner.GetTrackingHelper();
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_HarvestItem(ItemValue held, ItemStack stack, BlockValue bv)
	{
		if (!CheckBaseRequirements() && (held.ItemClass == expectedHeldClass || !requireHeld) && (!bv.isair || !isBlock) && (!isBlock || blockTag.IsEmpty || bv.Block.HasAnyFastTags(blockTag)) && stack.itemValue.ItemClass.ItemTags.Test_AnySet(harvestTags))
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
		if (e.HasAttribute("harvest_tags"))
		{
			harvestTag = e.GetAttribute("harvest_tags");
		}
		if (e.HasAttribute("target_name_key"))
		{
			targetName = Localization.Get(e.GetAttribute("target_name_key"));
		}
		else if (e.HasAttribute("target_name"))
		{
			targetName = e.GetAttribute("target_name");
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
		if (e.HasAttribute("override_nav_object"))
		{
			overrideNavObject = e.GetAttribute("override_nav_object");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveHarvestByTag
		{
			itemClassID = itemClassID,
			heldItemClassID = heldItemClassID,
			overrideTrackerIndexName = overrideTrackerIndexName,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass,
			expectedHeldClass = expectedHeldClass,
			requireHeld = requireHeld,
			blockTag = blockTag,
			isBlock = isBlock,
			harvestTag = harvestTag,
			harvestTags = harvestTags,
			targetName = targetName,
			overrideNavObject = overrideNavObject
		};
	}
}
