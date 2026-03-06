using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveReturnToNPC : ObjectiveRandomGoto
{
	public override bool PlayObjectiveComplete => false;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveReturnToTrader_keyword") + ((base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted) ? "" : ":");
		completeWithinRange = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		icon = "ui_game_symbol_quest";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.QuestGiver))
		{
			base.OwnerQuest.Position = position;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.QuestGiver, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		base.CurrentValue = 3;
		base.Complete = true;
		base.OwnerQuest.RefreshQuestCompletion();
		positionSet = true;
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		HiddenObjective = true;
		base.OwnerQuest.RemoveMapObject();
		return Vector3.zero;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (base.OwnerQuest.QuestClass.AddsToTierComplete)
		{
			base.OwnerQuest.OwnerJournal.HandleQuestCompleteToday(base.OwnerQuest);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveReturnToNPC objectiveReturnToNPC = new ObjectiveReturnToNPC();
		CopyValues(objectiveReturnToNPC);
		return objectiveReturnToNPC;
	}
}
