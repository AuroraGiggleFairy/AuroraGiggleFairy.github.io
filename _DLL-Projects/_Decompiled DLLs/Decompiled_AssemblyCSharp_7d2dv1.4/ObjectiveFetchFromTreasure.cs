using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchFromTreasure : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string altContainerName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i containerPos = Vector3i.zero;

	public static string questItemClassID = "questItem";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropBlock = "block";

	public static string PropAltBlock = "alt_block";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveFetchItems()
	{
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
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
			xui.CollectedItemList.RemoveItemStack(new ItemStack(expectedItem.Clone(), num - num2));
		}
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveFetchContainer_keyword");
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupExpectedItem()
	{
		if (base.OwnerQuest.QuestCode == 0)
		{
			base.OwnerQuest.SetupQuestCode();
		}
		expectedItemClass = ItemClass.GetItemClass(questItemClassID);
		int id = expectedItemClass.Id;
		ushort num = StringParsers.ParseUInt16(ID);
		expectedItemClass = ItemClassQuest.GetItemQuestById(num);
		expectedItem = new ItemValue(id);
		expectedItem.Seed = num;
		expectedItem.Meta = base.OwnerQuest.QuestCode;
		itemCount = 1;
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = "";
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		RemoveFetchItems();
	}

	public override void HandlePhaseCompleted()
	{
		base.HandlePhaseCompleted();
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		RemoveFetchItems();
	}

	public override void AddHooks()
	{
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += Toolbelt_OnToolbeltItemsChangedInternal;
		QuestEventManager.Current.ContainerOpened += Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += Current_ContainerClosed;
		QuestEventManager.Current.BlockChange += Current_BlockChange;
		Refresh();
	}

	public override void RemoveObjectives()
	{
		QuestEventManager.Current.ContainerOpened -= Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= Current_ContainerClosed;
		QuestEventManager.Current.BlockChange -= Current_BlockChange;
	}

	public override void RemoveHooks()
	{
		base.OwnerQuest.RemoveMapObject();
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
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.TreasurePoint);
		if ((float)containerLocation.x == pos.x && (float)containerLocation.z == pos.z && GetItemCount() < 1 && GameManager.Instance.World.GetBlock(containerLocation).Block.GetBlockName() == containerName && lootTE != null && !lootTE.HasItem(expectedItem))
		{
			hasOpened = true;
			lootTE.AddItem(new ItemStack(expectedItem, 1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (!(GameManager.Instance.World.GetBlock(containerLocation).Block.GetBlockName() == containerName) || lootTE == null)
		{
			return;
		}
		lootTE.RemoveItem(expectedItem);
		lootTE.SetModified();
		if (base.Complete)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockChange(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (base.Complete || !hasOpened)
		{
			return;
		}
		base.OwnerQuest.GetPositionData(out var pos, Quest.PositionDataTypes.TreasurePoint);
		containerPos = new Vector3i(pos);
		if (!(blockPos != containerPos) && GameManager.Instance.World.GetChunkFromWorldPos(blockPos) is Chunk chunk && chunk.IsDisplayed)
		{
			string blockName = blockNew.GetBlockName();
			if (blockName != containerName && blockName != altContainerName)
			{
				base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetItemCount()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		expectedItem.Meta = base.OwnerQuest.QuestCode;
		return playerInventory.Backpack.GetItemCount(expectedItem, -1, base.OwnerQuest.QuestCode) + playerInventory.Toolbelt.GetItemCount(expectedItem, _bConsiderTexture: false, -1, base.OwnerQuest.QuestCode);
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			currentCount = GetItemCount();
			if (currentCount > 1)
			{
				currentCount = 1;
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
				RemoveHooks();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchFromTreasure objectiveFetchFromTreasure = new ObjectiveFetchFromTreasure();
		CopyValues(objectiveFetchFromTreasure);
		return objectiveFetchFromTreasure;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveFetchFromTreasure obj = (ObjectiveFetchFromTreasure)objective;
		obj.containerName = containerName;
		obj.hasOpened = hasOpened;
		obj.containerName = containerName;
		obj.altContainerName = altContainerName;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropQuestItemID))
		{
			ID = properties.Values[PropQuestItemID];
		}
		if (properties.Values.ContainsKey(PropItemCount))
		{
			Value = properties.Values[PropItemCount];
		}
		if (properties.Values.ContainsKey(PropBlock))
		{
			containerName = properties.Values[PropBlock];
		}
		if (properties.Values.ContainsKey(PropAltBlock))
		{
			altContainerName = properties.Values[PropAltBlock];
		}
	}
}
