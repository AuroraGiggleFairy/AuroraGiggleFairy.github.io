using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBaseFetchContainer : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultContainer = "";

	public string questItemClassID = "questItem";

	public static string PropQuestItemClass = "quest_item";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropDefaultContainer = "default_container";

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	public override ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return ObjectiveValueTypes.Number;
			}
			return ObjectiveValueTypes.Boolean;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveFetchItems()
	{
		if (base.CurrentValue == 3)
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
				ItemStack stack = new ItemStack(expectedItem.Clone(), num - num2);
				xui.CollectedItemList.AddRemoveItemQueueEntry(stack);
			}
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.fetchTag);
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveFetchContainer_keyword");
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		SetupQuestTag();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupExpectedItem()
	{
		if (base.OwnerQuest.QuestCode == 0)
		{
			base.OwnerQuest.SetupQuestCode();
		}
		expectedItemClass = ItemClass.GetItemClass(questItemClassID);
		expectedItem = new ItemValue(expectedItemClass.Id);
		if (expectedItemClass is ItemClassQuest)
		{
			ushort num = StringParsers.ParseUInt16(ID);
			expectedItemClass = ItemClassQuest.GetItemQuestById(num);
			expectedItem.Seed = num;
		}
		expectedItem.Meta = base.OwnerQuest.QuestCode;
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		RemoveFetchItems();
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		RemoveFetchItems();
	}

	public override void ResetObjective()
	{
		RemoveFetchItems();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetItemCount(int _expectedMeta = -2)
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (_expectedMeta == -2)
		{
			_expectedMeta = base.OwnerQuest.QuestCode;
		}
		expectedItem.Meta = _expectedMeta;
		return playerInventory.Backpack.GetItemCount(expectedItem, -1, _expectedMeta) + playerInventory.Toolbelt.GetItemCount(expectedItem, _bConsiderTexture: false, -1, _expectedMeta);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		((ObjectiveBaseFetchContainer)objective).questItemClassID = questItemClassID;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropQuestItemClass))
		{
			questItemClassID = properties.Values[PropQuestItemClass];
		}
		if (properties.Values.ContainsKey(PropQuestItemID))
		{
			ID = properties.Values[PropQuestItemID];
		}
		if (properties.Values.ContainsKey(PropItemCount))
		{
			Value = properties.Values[PropItemCount];
		}
		if (properties.Values.ContainsKey(PropDefaultContainer))
		{
			defaultContainer = properties.Values[PropDefaultContainer];
		}
	}
}
