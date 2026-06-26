using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchAnyContainer : ObjectiveBaseFetchContainer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveFetch_keyword");
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		itemCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = $"{currentCount}/{itemCount}";
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.fetchTag);
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
	}

	public override void AddHooks()
	{
		base.CurrentValue = 0;
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += Toolbelt_OnToolbeltItemsChangedInternal;
		QuestEventManager.Current.ContainerOpened += Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += Current_ContainerClosed;
	}

	public override void RemoveObjectives()
	{
		QuestEventManager.Current.ContainerOpened -= Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= Current_ContainerClosed;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (playerInventory != null)
		{
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= Backpack_OnBackpackItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= Toolbelt_OnToolbeltItemsChangedInternal;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Backpack_OnBackpackItemsChangedInternal()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (!base.Complete && uIForPlayer.xui.PlayerInventory != null)
		{
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Toolbelt_OnToolbeltItemsChangedInternal()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (!base.Complete && uIForPlayer.xui.PlayerInventory != null)
		{
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (GetItemCount() < itemCount && tileEntity.blockValue.Block.GetBlockName() == defaultContainer && !tileEntity.HasItem(expectedItem))
		{
			tileEntity.AddItem(new ItemStack(expectedItem, itemCount));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (tileEntity.blockValue.Block.GetBlockName() == defaultContainer)
		{
			tileEntity.RemoveItem(expectedItem);
			tileEntity.SetModified();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		currentCount = GetItemCount();
		if (currentCount != 0)
		{
			SetupDisplay();
			base.CurrentValue = 3;
			base.Complete = base.OwnerQuest.CheckRequirements();
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
				RemoveHooks();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchAnyContainer objectiveFetchAnyContainer = new ObjectiveFetchAnyContainer();
		CopyValues(objectiveFetchAnyContainer);
		return objectiveFetchAnyContainer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		((ObjectiveFetchAnyContainer)objective).defaultContainer = defaultContainer;
	}
}
