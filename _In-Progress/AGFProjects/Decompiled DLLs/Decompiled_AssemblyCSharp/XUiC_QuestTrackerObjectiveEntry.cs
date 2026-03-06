using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTrackerObjectiveEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BaseObjective questObjective;

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseChallengeObjective challengeObjective;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBool;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string optionalKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> objectiveOptionalFormatter = new CachedStringFormatter<string>([PublicizedFrom(EAccessModifier.Internal)] (string _s) => "(" + _s + ") ");

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTrackerObjectiveList Owner { get; set; }

	public BaseObjective QuestObjective
	{
		get
		{
			return questObjective;
		}
		set
		{
			if (questObjective != null)
			{
				questObjective.ValueChanged -= Objective_ValueChanged;
			}
			questObjective = value;
			if (questObjective != null)
			{
				ChallengeObjective = null;
				questObjective.ValueChanged += Objective_ValueChanged;
			}
			isDirty = true;
			isBool = questObjective != null && questObjective.ObjectiveValueType == BaseObjective.ObjectiveValueTypes.Boolean;
			RefreshBindings(_forceAll: true);
		}
	}

	public BaseChallengeObjective ChallengeObjective
	{
		get
		{
			return challengeObjective;
		}
		set
		{
			if (challengeObjective != null)
			{
				challengeObjective.ValueChanged -= Objective_ValueChanged;
			}
			challengeObjective = value;
			if (challengeObjective != null)
			{
				QuestObjective = null;
				challengeObjective.ValueChanged += Objective_ValueChanged;
			}
			isDirty = true;
			isBool = false;
			RefreshBindings(_forceAll: true);
		}
	}

	public static string OptionalKeyword
	{
		get
		{
			if (optionalKeyword == "")
			{
				optionalKeyword = Localization.Get("optional");
			}
			return optionalKeyword;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Objective_ValueChanged()
	{
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = questObjective != null;
		bool flag2 = challengeObjective != null;
		switch (bindingName)
		{
		case "hasobjective":
			value = (flag || flag2).ToString();
			return true;
		case "objectivecompletesprite":
			if (flag && questObjective.OwnerQuest != null && questObjective.OwnerQuest.Active && isBool)
			{
				value = (questObjective.Complete ? Owner.completeIconName : Owner.incompleteIconName);
			}
			else
			{
				value = "";
			}
			return true;
		case "objectivecompletecolor":
			if (flag && questObjective.OwnerQuest != null && questObjective.OwnerQuest.Active)
			{
				switch (questObjective.ObjectiveState)
				{
				case BaseObjective.ObjectiveStates.InProgress:
					value = Owner.incompleteColor;
					break;
				case BaseObjective.ObjectiveStates.Warning:
					value = Owner.warningColor;
					break;
				case BaseObjective.ObjectiveStates.Complete:
					value = Owner.completeColor;
					break;
				case BaseObjective.ObjectiveStates.Failed:
					value = Owner.incompleteColor;
					break;
				}
			}
			else if (flag2)
			{
				value = (challengeObjective.Complete ? Owner.completeColor : Owner.incompleteColor);
			}
			else
			{
				value = "";
			}
			return true;
		case "objectivecompletehexcolor":
			if (flag && questObjective.OwnerQuest != null && questObjective.OwnerQuest.Active)
			{
				value = (questObjective.Complete ? Owner.completeHexColor : Owner.incompleteHexColor);
				switch (questObjective.ObjectiveState)
				{
				case BaseObjective.ObjectiveStates.InProgress:
					value = Owner.incompleteHexColor;
					break;
				case BaseObjective.ObjectiveStates.Warning:
					value = Owner.warningHexColor;
					break;
				case BaseObjective.ObjectiveStates.Complete:
					value = Owner.completeHexColor;
					break;
				case BaseObjective.ObjectiveStates.Failed:
					value = Owner.incompleteHexColor;
					break;
				}
			}
			else if (flag2)
			{
				value = (challengeObjective.Complete ? Owner.completeHexColor : Owner.incompleteHexColor);
			}
			else
			{
				value = "FFFFFF";
			}
			return true;
		case "objectivephasehexcolor":
			if (flag && questObjective.OwnerQuest != null && questObjective.OwnerQuest.Active)
			{
				value = ((questObjective.Phase == questObjective.OwnerQuest.CurrentPhase || questObjective.Phase == 0) ? Owner.activeHexColor : Owner.inactiveHexColor);
			}
			else
			{
				value = "FFFFFF";
			}
			return true;
		case "objectiveshowicon":
			value = (flag ? isBool.ToString() : "false");
			return true;
		case "objectivedescription":
			if (flag)
			{
				value = questObjective.Description;
			}
			else if (flag2)
			{
				value = challengeObjective.DescriptionText;
			}
			else
			{
				value = "";
			}
			return true;
		case "objectiveoptional":
			value = ((!flag) ? "" : (questObjective.Optional ? objectiveOptionalFormatter.Format(OptionalKeyword) : ""));
			return true;
		case "objectivestate":
			if (flag)
			{
				if (isBool)
				{
					value = "";
				}
				else
				{
					value = questObjective.StatusText;
				}
			}
			else if (flag2)
			{
				value = challengeObjective.StatusText;
			}
			else
			{
				value = "";
			}
			return true;
		case "objectivetextwidth":
			if (flag && isBool)
			{
				value = "280";
			}
			else
			{
				value = "300";
			}
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ClearObjective()
	{
		QuestObjective = null;
		ChallengeObjective = null;
	}

	public override void Update(float _dt)
	{
		if (questObjective != null && questObjective.UpdateUI)
		{
			RefreshBindings(isDirty);
			isDirty = false;
		}
		base.Update(_dt);
	}
}
