using UnityEngine.Scripting;

[Preserve]
public class ObjectiveModifierSupplyBox : BaseObjectiveModifier
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultContainer = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string expectedItemClassID = "questItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string expectedQuestItemID = "";

	public static string PropExpectedItemClassID = "item_class";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropDefaultContainer = "container";

	public override void AddHooks()
	{
		QuestEventManager.Current.ContainerOpened += Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += Current_ContainerClosed;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.ContainerOpened -= Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= Current_ContainerClosed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		int num = GetItemCount();
		if (num < itemCount && tileEntity.blockValue.Block.GetBlockName() == defaultContainer && !tileEntity.HasItem(expectedItem))
		{
			tileEntity.AddItem(new ItemStack(expectedItem, itemCount - num));
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

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropExpectedItemClassID))
		{
			expectedItemClassID = properties.Values[PropExpectedItemClassID];
		}
		if (properties.Values.ContainsKey(PropQuestItemID))
		{
			expectedQuestItemID = properties.Values[PropQuestItemID];
		}
		if (properties.Values.ContainsKey(PropItemCount))
		{
			itemCount = StringParsers.ParseSInt32(properties.Values[PropItemCount]);
		}
		if (properties.Values.ContainsKey(PropDefaultContainer))
		{
			defaultContainer = properties.Values[PropDefaultContainer];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupExpectedItem()
	{
		if (base.OwnerObjective.OwnerQuest.QuestCode == 0)
		{
			base.OwnerObjective.OwnerQuest.SetupQuestCode();
		}
		expectedItemClass = ItemClass.GetItemClass(expectedItemClassID);
		expectedItem = new ItemValue(expectedItemClass.Id);
		if (expectedItemClass is ItemClassQuest)
		{
			ushort num = StringParsers.ParseUInt16(expectedQuestItemID);
			expectedItemClass = ItemClassQuest.GetItemQuestById(num);
			expectedItem.Seed = num;
		}
		expectedItem.Meta = base.OwnerObjective.OwnerQuest.QuestCode;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveFetchItems()
	{
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		int num = 1;
		int num2 = 1;
		num2 -= playerInventory.Backpack.DecItem(expectedItem, num2);
		if (num2 > 0)
		{
			playerInventory.Toolbelt.DecItem(expectedItem, num2);
		}
		if (num != num2)
		{
			ItemStack stack = new ItemStack(expectedItem.Clone(), num - num2);
			xui.CollectedItemList.AddRemoveItemQueueEntry(stack);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetItemCount()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		expectedItem.Meta = base.OwnerObjective.OwnerQuest.QuestCode;
		return playerInventory.Backpack.GetItemCount(expectedItem, -1, base.OwnerObjective.OwnerQuest.QuestCode) + playerInventory.Toolbelt.GetItemCount(expectedItem, _bConsiderTexture: false, -1, base.OwnerObjective.OwnerQuest.QuestCode);
	}

	public override BaseObjectiveModifier Clone()
	{
		return new ObjectiveModifierSupplyBox
		{
			expectedItemClassID = expectedItemClassID,
			expectedQuestItemID = expectedQuestItemID,
			itemCount = itemCount,
			defaultContainer = defaultContainer
		};
	}
}
