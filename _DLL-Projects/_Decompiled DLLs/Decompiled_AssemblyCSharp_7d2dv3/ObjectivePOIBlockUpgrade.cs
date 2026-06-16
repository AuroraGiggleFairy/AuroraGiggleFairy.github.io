using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIBlockUpgrade : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount = -1;

	public static string PropBlockName = "block_index";

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
		keyword = Localization.Get("ObjectiveBlockUpgrade_keyword");
		localizedName = ((ID != "" && ID != null) ? Localization.Get(ID) : "Any Block");
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupDisplay()
	{
		base.Description = "TEST";
		StatusText = $"{base.CurrentValue}/{neededCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
		QuestEventManager.Current.BlockUpgrade += Current_BlockUpgrade;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockUpgrade(string blockname, Vector3i blockPos)
	{
		if (!base.Complete)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, NavObjectName);
			Block blockByName = Block.GetBlockByName(blockname);
			if ((ID == null || ID == "" || ID.EqualsCaseInsensitive(blockByName.IndexName)) && base.OwnerQuest.CheckRequirements())
			{
				base.CurrentValue++;
				Refresh();
			}
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
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue >= neededCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
				HandleRemoveHooks();
				QuestEventManager.Current.FinishManagedQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			}
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
		if (base.OwnerQuest.SharedOwnerID != -1)
		{
			return;
		}
		base.CurrentValue = 0;
		List<Vector3i> list = new List<Vector3i>();
		List<bool> list2 = new List<bool>();
		QuestEventManager.Current.SetupRepairForMP(list, list2, GameManager.Instance.World, pos);
		neededCount = list.Count;
		byte b = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list2[i])
			{
				NavObjectManager.Instance.RegisterNavObject(NavObjectName, list[i].ToVector3() + Vector3.one * 0.5f);
			}
			else
			{
				b++;
			}
		}
		base.CurrentValue = b;
		Refresh();
		SetupDisplay();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_WaitingForServer()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Completed()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectivePOIBlockUpgrade objectivePOIBlockUpgrade = new ObjectivePOIBlockUpgrade();
		CopyValues(objectivePOIBlockUpgrade);
		return objectivePOIBlockUpgrade;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBlockName))
		{
			ID = properties.Values[PropBlockName];
		}
	}
}
