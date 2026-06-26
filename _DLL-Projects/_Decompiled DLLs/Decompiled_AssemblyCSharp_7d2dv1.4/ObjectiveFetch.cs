using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetch : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool KeepItems;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveFetch_keyword");
		expectedItem = ItemClass.GetItem(ID);
		expectedItemClass = ItemClass.GetItemClass(ID);
		itemCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = $"{currentCount}/{itemCount}";
	}

	public override void AddHooks()
	{
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += Toolbelt_OnToolbeltItemsChangedInternal;
		Refresh();
	}

	public override void RemoveHooks()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (playerInventory != null)
		{
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= Backpack_OnBackpackItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= Toolbelt_OnToolbeltItemsChangedInternal;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_AddItem(ItemStack stack)
	{
		if (!base.Complete && stack.itemValue.type == expectedItem.type)
		{
			if (base.CurrentValue + stack.count > itemCount)
			{
				base.CurrentValue = (byte)itemCount;
			}
			else
			{
				base.CurrentValue += (byte)stack.count;
			}
			Refresh();
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

	public override void Refresh()
	{
		if (!base.Complete)
		{
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
			currentCount = playerInventory.Backpack.GetItemCount(expectedItem);
			currentCount += playerInventory.Toolbelt.GetItemCount(expectedItem);
			if (currentCount > itemCount)
			{
				currentCount = itemCount;
			}
			SetupDisplay();
			if (currentCount != base.CurrentValue)
			{
				base.CurrentValue = (byte)currentCount;
			}
			base.Complete = currentCount >= itemCount && base.OwnerQuest.CheckRequirements();
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override void RemoveObjectives()
	{
		if (!KeepItems)
		{
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
			itemCount = playerInventory.Backpack.DecItem(expectedItem, itemCount);
			if (itemCount > 0)
			{
				playerInventory.Toolbelt.DecItem(expectedItem, itemCount);
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetch objectiveFetch = new ObjectiveFetch();
		CopyValues(objectiveFetch);
		objectiveFetch.KeepItems = KeepItems;
		return objectiveFetch;
	}

	public override string ParseBinding(string bindingName)
	{
		string iD = ID;
		string value = Value;
		if (!(bindingName == "items"))
		{
			if (bindingName == "itemswithcount")
			{
				ItemClass itemClass = ItemClass.GetItemClass(iD);
				int num = Convert.ToInt32(value);
				if (itemClass == null)
				{
					return "INVALID";
				}
				return num + " " + itemClass.GetLocalizedItemName();
			}
			return "";
		}
		ItemClass itemClass2 = ItemClass.GetItemClass(iD);
		if (itemClass2 == null)
		{
			return "INVALID";
		}
		return itemClass2.GetLocalizedItemName();
	}
}
