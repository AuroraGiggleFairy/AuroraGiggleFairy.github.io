using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveInteractWithNPC : BaseObjective
{
	public static string PropUseClosest = "use_closest";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useClosest;

	public override bool PlayObjectiveComplete => false;

	public override void SetupObjective()
	{
	}

	public override void SetupDisplay()
	{
		base.Description = Localization.Get("ObjectiveTalkToTrader_keyword");
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.NPCInteract += Current_NPCInteract;
		if (!useClosest)
		{
			return;
		}
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(base.OwnerQuest.Position, new Vector3(50f, 50f, 50f)));
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			if (entitiesInBounds[i] is EntityNPC)
			{
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.QuestGiver, entitiesInBounds[i].position);
				base.OwnerQuest.QuestGiverID = entitiesInBounds[i].entityId;
				base.OwnerQuest.QuestFaction = ((EntityNPC)entitiesInBounds[i]).NPCInfo.QuestFaction;
				base.OwnerQuest.RallyMarkerActivated = true;
				base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.QuestGiver, NavObjectName);
				break;
			}
		}
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.NPCInteract -= Current_NPCInteract;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_NPCInteract(EntityNPC npc)
	{
		if ((!base.OwnerQuest.QuestClass.ReturnToQuestGiver || base.OwnerQuest.QuestGiverID == -1 || base.OwnerQuest.CheckIsQuestGiver(npc.entityId)) && base.OwnerQuest.CheckRequirements())
		{
			if (base.OwnerQuest.QuestFaction == 0)
			{
				base.OwnerQuest.QuestFaction = npc.NPCInfo.QuestFaction;
			}
			base.CurrentValue = 1;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			bool complete = base.CurrentValue == 1;
			base.Complete = complete;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, PlayObjectiveComplete);
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveInteractWithNPC objectiveInteractWithNPC = new ObjectiveInteractWithNPC();
		CopyValues(objectiveInteractWithNPC);
		objectiveInteractWithNPC.useClosest = useClosest;
		return objectiveInteractWithNPC;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropUseClosest))
		{
			useClosest = StringParsers.ParseBool(properties.Values[PropUseClosest]);
		}
	}
}
