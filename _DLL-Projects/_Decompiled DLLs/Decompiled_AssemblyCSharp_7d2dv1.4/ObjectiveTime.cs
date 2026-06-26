using System.IO;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTime : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum OverrideTypes
	{
		None,
		VoteTime
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public OverrideTypes overrideType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float overrideOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstRun = true;

	public static string PropTime = "time";

	public static string PropOverrideType = "override_type";

	public static string PropOverrideOffset = "override_offset";

	[PublicizedFrom(EAccessModifier.Private)]
	public int dayLengthInSeconds;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Time;

	public override bool UpdateUI => true;

	public override bool ShowInQuestLog => false;

	public override string StatusText
	{
		get
		{
			if (currentTime > 0f)
			{
				return XUiM_PlayerBuffs.GetTimeString(currentTime);
			}
			if (base.Optional)
			{
				base.ObjectiveState = ObjectiveStates.Failed;
				return Localization.Get("failed");
			}
			base.ObjectiveState = ObjectiveStates.Complete;
			return Localization.Get("completed");
		}
	}

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveTime_keyword");
		dayLengthInSeconds = GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60;
	}

	public override void SetupDisplay()
	{
		base.Description = $"{keyword}:";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	public override void Refresh()
	{
		SetupDisplay();
		if (base.Optional)
		{
			base.Complete = currentTime > 0f;
		}
		else
		{
			base.Complete = currentTime <= 0f;
		}
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override void Read(BinaryReader _br)
	{
		currentTime = (int)_br.ReadUInt16();
		currentValue = 1;
	}

	public override void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)currentTime);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveTime obj = (ObjectiveTime)objective;
		obj.currentTime = currentTime;
		obj.overrideType = overrideType;
		obj.overrideOffset = overrideOffset;
	}

	public override BaseObjective Clone()
	{
		ObjectiveTime objectiveTime = new ObjectiveTime();
		CopyValues(objectiveTime);
		return objectiveTime;
	}

	public override void Update(float updateTime)
	{
		if (firstRun)
		{
			if (overrideType == OverrideTypes.VoteTime && TwitchManager.HasInstance && TwitchManager.Current.IsVoting)
			{
				currentTime = TwitchManager.Current.VotingManager.VoteTimeRemaining + overrideOffset;
				firstRun = false;
				return;
			}
			if (Value.EqualsCaseInsensitive("day"))
			{
				currentTime = dayLengthInSeconds;
			}
			else
			{
				currentTime = StringParsers.ParseFloat(Value);
			}
			firstRun = false;
		}
		currentTime -= updateTime;
		if (currentTime < 0f)
		{
			Refresh();
			HandleRemoveHooks();
		}
	}

	public override void HandleFailed()
	{
		currentTime = 0f;
		base.Complete = false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref Value);
		properties.ParseEnum(PropOverrideType, ref overrideType);
		properties.ParseFloat(PropOverrideOffset, ref overrideOffset);
	}
}
