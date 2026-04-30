using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIBlockActivate : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeEvent = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> activateList;

	public static string PropBlockName = "block_index";

	public static string PropEventComplete = "complete_event";

	[PublicizedFrom(EAccessModifier.Private)]
	public new float updateTime;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveRestorePower_keyword");
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
		if (neededCount == -1)
		{
			StatusText = "";
		}
		else
		{
			StatusText = $"{base.CurrentValue}/{neededCount}";
		}
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockDestroy -= Current_BlockDestroy;
		QuestEventManager.Current.BlockDestroy += Current_BlockDestroy;
		QuestEventManager.Current.BlockActivate -= Current_BlockActivate;
		QuestEventManager.Current.BlockActivate += Current_BlockActivate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockDestroy(Block block, Vector3i blockPos)
	{
		if (activateList != null && activateList.Contains(blockPos))
		{
			base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockname, Vector3i blockPos)
	{
		if (activateList == null || base.Complete)
		{
			return;
		}
		NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, NavObjectName);
		if (activateList.Contains(blockPos))
		{
			Block blockByName = Block.GetBlockByName(blockname);
			if ((ID == null || ID == "" || ID.EqualsCaseInsensitive(blockByName.IndexName)) && base.OwnerQuest.CheckRequirements())
			{
				base.CurrentValue++;
				activateList.Remove(blockPos);
				Refresh();
			}
		}
	}

	public void AddActivatedBlock(Vector3i blockPos)
	{
		NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, NavObjectName);
		base.CurrentValue++;
		activateList.Remove(blockPos);
	}

	public override void RemoveHooks()
	{
		QuestEventManager current = QuestEventManager.Current;
		current.RemoveObjectiveToBeUpdated(this);
		current.BlockActivate -= Current_BlockActivate;
		current.BlockDestroy -= Current_BlockDestroy;
		ClearNavObjects();
		if (base.OwnerQuest != null && base.OwnerQuest.RallyMarkerActivated)
		{
			current.ActiveQuestBlocks.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearNavObjects()
	{
		if (activateList != null)
		{
			for (int i = 0; i < activateList.Count; i++)
			{
				NavObjectManager.Instance.UnRegisterNavObjectByPosition(activateList[i].ToVector3() + Vector3.one * 0.5f, NavObjectName);
			}
			activateList.Clear();
		}
	}

	public override void Refresh()
	{
		if (neededCount == -1)
		{
			return;
		}
		if (base.CurrentValue > neededCount)
		{
			base.CurrentValue = (byte)neededCount;
		}
		if (base.Complete)
		{
			return;
		}
		base.Complete = base.CurrentValue >= neededCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
			HandleRemoveHooks();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.FinishManagedQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
				return;
			}
			Vector3 pos = Vector3.zero;
			base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.FinishManagedQuest, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, pos, base.OwnerQuest.QuestCode));
		}
	}

	public override void Update(float deltaTime)
	{
		if (!(Time.time > updateTime))
		{
			return;
		}
		updateTime = Time.time + 1f;
		if (neededCount != -1)
		{
			return;
		}
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				activateList = new List<Vector3i>();
				QuestEventManager.Current.SetupActivateForMP(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode, completeEvent, activateList, GameManager.Instance.World, pos, ID, base.OwnerQuest.GetSharedWithIDList());
				SetupActivationList(pos, activateList);
			}
		}
		else if (base.OwnerQuest.SharedOwnerID == -1)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode, completeEvent, pos, ID, base.OwnerQuest.GetSharedWithIDList()));
		}
		else if (QuestEventManager.Current.ActiveQuestBlocks != null && QuestEventManager.Current.ActiveQuestBlocks.Count > 0)
		{
			SetupActivationList(pos, QuestEventManager.Current.ActiveQuestBlocks);
		}
	}

	public override bool SetupActivationList(Vector3 prefabPos, List<Vector3i> newActivateList)
	{
		Vector3 pos = Vector3.zero;
		base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition);
		if (pos.x != prefabPos.x && pos.z != prefabPos.z)
		{
			return false;
		}
		base.CurrentValue = 0;
		neededCount = newActivateList.Count;
		byte b = 0;
		for (int i = 0; i < newActivateList.Count; i++)
		{
			NavObjectManager.Instance.RegisterNavObject(NavObjectName, newActivateList[i].ToVector3() + Vector3.one * 0.5f);
		}
		base.CurrentValue = b;
		QuestEventManager.Current.ActiveQuestBlocks = newActivateList;
		activateList = newActivateList;
		Refresh();
		SetupDisplay();
		return true;
	}

	public override BaseObjective Clone()
	{
		ObjectivePOIBlockActivate objectivePOIBlockActivate = new ObjectivePOIBlockActivate();
		CopyValues(objectivePOIBlockActivate);
		objectivePOIBlockActivate.completeEvent = completeEvent;
		objectivePOIBlockActivate.neededCount = neededCount;
		objectivePOIBlockActivate.activateList = activateList;
		return objectivePOIBlockActivate;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBlockName))
		{
			ID = properties.Values[PropBlockName];
		}
		properties.ParseString(PropEventComplete, ref completeEvent);
	}
}
