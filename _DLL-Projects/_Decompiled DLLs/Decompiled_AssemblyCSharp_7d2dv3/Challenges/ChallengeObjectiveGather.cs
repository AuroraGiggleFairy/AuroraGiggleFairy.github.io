using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveGather : ChallengeBaseTrackedItemObjective
{
	public BaseRequirementObjectiveGroup Parent;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Gather;

	public override string DescriptionText => Localization.Get("challengeObjectiveGather") + " " + expectedItemClass.GetLocalizedItemName();

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
	}

	public override void HandleAddHooks()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal -= ItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= ItemsChangedInternal;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += ItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += ItemsChangedInternal;
		player.DragAndDropItemChanged -= ItemsChangedInternal;
		player.DragAndDropItemChanged += ItemsChangedInternal;
		base.HandleAddHooks();
		ItemsChangedInternal();
		if (IsRequirement && trackingEntry != null)
		{
			Owner.AddTrackingEntry(trackingEntry);
			trackingEntry.TrackingHelper = Owner.TrackingHandler;
			trackingEntry.AddHooks();
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

	public override bool CheckObjectiveComplete(bool handleComplete = true)
	{
		if (CheckForNeededItem())
		{
			base.Complete = true;
			base.Current = MaxCount;
			if (handleComplete)
			{
				Owner.HandleComplete();
			}
			return true;
		}
		base.Complete = false;
		return base.CheckObjectiveComplete(handleComplete);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemsChangedInternal()
	{
		if (CheckBaseRequirements())
		{
			return;
		}
		if (CheckObjectiveComplete())
		{
			if (IsTracking && trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
			}
			if (IsRequirement)
			{
				Parent.CheckPrerequisites();
			}
		}
		else if (IsTracking && trackingEntry != null)
		{
			trackingEntry.AddHooks();
		}
	}

	public override void UpdateStatus()
	{
		base.UpdateStatus();
		if (base.Complete)
		{
			if (trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
			}
		}
		else if (trackingEntry != null)
		{
			trackingEntry.AddHooks();
		}
	}

	public override void HandleRemoveHooks()
	{
		EntityPlayerLocal player = Owner.Owner.Player;
		if (!(player == null))
		{
			LocalPlayerUI.GetUIForPlayer(player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= ItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= ItemsChangedInternal;
			player.DragAndDropItemChanged -= ItemsChangedInternal;
			if (IsRequirement && trackingEntry != null)
			{
				trackingEntry.RemoveHooks();
				Owner.RemoveTrackingEntry(trackingEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleUpdatingCurrent()
	{
		base.HandleUpdatingCurrent();
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui.PlayerInventory;
		int itemCount = playerInventory.Backpack.GetItemCount(expectedItem);
		itemCount += playerInventory.Toolbelt.GetItemCount(expectedItem);
		if (itemCount > MaxCount)
		{
			itemCount = MaxCount;
		}
		if (current != itemCount)
		{
			base.Current = itemCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForNeededItem()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player).xui.PlayerInventory;
		return playerInventory.Backpack.GetItemCount(expectedItem) + playerInventory.Toolbelt.GetItemCount(expectedItem) >= MaxCount;
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveGather
		{
			itemClassID = itemClassID,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass,
			trackingEntry = trackingEntry
		};
	}
}
