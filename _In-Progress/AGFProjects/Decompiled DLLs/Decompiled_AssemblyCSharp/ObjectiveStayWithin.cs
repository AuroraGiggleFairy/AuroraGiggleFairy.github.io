using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveStayWithin : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayDistance = "0 km";

	public static string PropRadius = "radius";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSetup;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Distance;

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (currentDistance < maxDistance)
				{
					return ValueDisplayFormatters.Distance(currentDistance) + "/" + displayDistance;
				}
				base.ObjectiveState = ObjectiveStates.Failed;
				return Localization.Get("failed");
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
			{
				return displayDistance;
			}
			if (base.ObjectiveState == ObjectiveStates.Failed)
			{
				return Localization.Get("failed");
			}
			return Localization.Get("completed");
		}
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveStayWithin_keyword");
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		if (base.OwnerQuest == base.OwnerQuest.OwnerJournal.ActiveQuest || base.OwnerQuest.OwnerJournal.ActiveQuest == null)
		{
			QuestEventManager.Current.QuestBounds = default(Rect);
		}
	}

	public override void Refresh()
	{
		SetupDisplay();
		if (base.ObjectiveState != ObjectiveStates.NotStarted || base.OwnerQuest.CurrentState != Quest.QuestState.InProgress)
		{
			base.Complete = base.OwnerQuest.CurrentState != Quest.QuestState.Failed;
		}
	}

	public override void Read(BinaryReader _br)
	{
	}

	public override void Write(BinaryWriter _bw)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveStayWithin obj = (ObjectiveStayWithin)objective;
		obj.maxDistance = maxDistance;
		obj.displayDistance = displayDistance;
	}

	public override BaseObjective Clone()
	{
		ObjectiveStayWithin objectiveStayWithin = new ObjectiveStayWithin();
		CopyValues(objectiveStayWithin);
		return objectiveStayWithin;
	}

	public override void Update(float updateTime)
	{
		Vector3 position = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		Vector3 pos = base.OwnerQuest.Position;
		if (!positionSetup)
		{
			if (base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.Location))
			{
				base.OwnerQuest.Position = pos;
				QuestEventManager.Current.QuestBounds = new Rect(pos.x, pos.z, maxDistance, maxDistance);
				positionSetup = true;
			}
			else if (base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition))
			{
				base.OwnerQuest.Position = pos;
				QuestEventManager.Current.QuestBounds = new Rect(pos.x, pos.z, maxDistance, maxDistance);
				positionSetup = true;
			}
		}
		position.y = 0f;
		pos.y = 0f;
		currentDistance = (position - pos).magnitude;
		float num = currentDistance / maxDistance;
		if (num > 1f)
		{
			base.Complete = false;
			base.ObjectiveState = ObjectiveStates.Failed;
			base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
		}
		else if (num > 0.75f)
		{
			base.ObjectiveState = ObjectiveStates.Warning;
		}
		else
		{
			base.ObjectiveState = ObjectiveStates.Complete;
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropRadius))
		{
			maxDistance = StringParsers.ParseFloat(properties.Values[PropRadius]);
			displayDistance = ValueDisplayFormatters.Distance(maxDistance);
		}
	}
}
