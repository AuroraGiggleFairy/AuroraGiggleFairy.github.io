using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchFromContainer : ObjectiveBaseFetchContainer
{
	public enum FetchModeTypes
	{
		Standard,
		Hidden
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	public string nearbyKeyword = "";

	public static string PropFetchMode = "fetch_mode";

	public FetchModeTypes FetchMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i lootContainerPos;

	public override ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return ObjectiveValueTypes.Distance;
			}
			return ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			if (base.ObjectiveState != ObjectiveStates.Failed)
			{
				return base.CurrentValue != 3;
			}
			return false;
		}
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (FetchMode != FetchModeTypes.Standard && !(distance >= 10f))
				{
					return nearbyKeyword;
				}
				return ValueDisplayFormatters.Distance(distance);
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
			{
				return "";
			}
			if (base.ObjectiveState == ObjectiveStates.Failed)
			{
				return Localization.Get("failed");
			}
			return Localization.Get("completed");
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag((FetchMode == FetchModeTypes.Standard) ? QuestEventManager.fetchTag : FastTags<TagGroup.Global>.Parse("hidden_cache"));
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveFetchContainer_keyword");
		if (expectedItemClass == null)
		{
			SetupExpectedItem();
		}
		base.OwnerQuest.AddQuestTag((FetchMode == FetchModeTypes.Standard) ? QuestEventManager.fetchTag : FastTags<TagGroup.Global>.Parse("hidden_cache"));
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, expectedItemClass.GetLocalizedItemName());
		StatusText = "";
		nearbyKeyword = Localization.Get("ObjectiveNearby_keyword");
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		base.OwnerQuest.RemovePositionData((FetchMode == FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		base.OwnerQuest.RemoveMapObject();
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

	public override void SetPosition(Quest.PositionDataTypes dataType, Vector3i position)
	{
		if (base.Phase == base.OwnerQuest.CurrentPhase && ((FetchMode == FetchModeTypes.Standard) ? 5 : 6) == (int)dataType)
		{
			FinalizePoint(position);
		}
	}

	public override void ResetObjective()
	{
		base.ResetObjective();
		Quest.PositionDataTypes dataType = ((FetchMode == FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		base.OwnerQuest.RemovePositionData(dataType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		Quest.PositionDataTypes dataType = ((FetchMode == FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		if (base.OwnerQuest.GetPositionData(out position, dataType))
		{
			base.OwnerQuest.HandleMapObject(dataType, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				QuestEventManager.Current.SetupFetchForMP(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, pos, FetchMode, base.OwnerQuest.GetSharedWithIDList());
			}
			base.CurrentValue = 2;
		}
		else
		{
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, pos, FetchMode, base.OwnerQuest.GetSharedWithIDList()));
			}
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(Vector3i containerPos)
	{
		Quest.PositionDataTypes dataType = ((FetchMode == FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		position = containerPos.ToVector3();
		base.OwnerQuest.SetPositionData(dataType, position);
		lootContainerPos = containerPos;
		base.OwnerQuest.HandleMapObject(dataType, NavObjectName);
		base.CurrentValue = 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (GetItemCount() < 1 && containerLocation == lootContainerPos && lootTE != null && !lootTE.HasItem(expectedItem))
		{
			lootTE.AddItem(new ItemStack(expectedItem, 1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (containerLocation == lootContainerPos && lootTE != null)
		{
			lootTE.RemoveItem(expectedItem);
			lootTE.SetModified();
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
				base.OwnerQuest.RemovePositionData((FetchMode == FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
				base.OwnerQuest.RemoveMapObject();
				base.OwnerQuest.RefreshQuestCompletion();
				RemoveHooks();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchFromContainer objectiveFetchFromContainer = new ObjectiveFetchFromContainer();
		CopyValues(objectiveFetchFromContainer);
		return objectiveFetchFromContainer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveFetchFromContainer obj = (ObjectiveFetchFromContainer)objective;
		obj.FetchMode = FetchMode;
		obj.defaultContainer = defaultContainer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		GetPosition();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		EntityPlayerLocal ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		position.y = 0f;
		Vector3 a = ownerPlayer.position;
		a.y = 0f;
		distance = Vector3.Distance(a, position);
		if (world == null)
		{
			world = GameManager.Instance.World;
		}
		if (distance < 5f)
		{
			BlockValue block = world.GetBlock(lootContainerPos);
			if (block.Block.IndexName == null || !block.Block.IndexName.EqualsCaseInsensitive("fetchcontainer"))
			{
				world.SetBlockRPC(lootContainerPos, Block.GetBlockValue(defaultContainer));
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropFetchMode))
		{
			FetchMode = EnumUtils.Parse<FetchModeTypes>(properties.Values[PropFetchMode]);
		}
	}
}
