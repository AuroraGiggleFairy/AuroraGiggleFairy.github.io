using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestObjectiveEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string notstartedIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeColor = "0,255,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteColor = "255,0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string warningColor = "255,255,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string inactiveColor = "160,160,160,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string originalColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseObjective objective;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string optionalKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> objectiveOptionalFormatter = new CachedStringFormatter<string>([PublicizedFrom(EAccessModifier.Internal)] (string _s) => "(" + _s + ") ");

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestObjectiveList Owner { get; set; }

	public BaseObjective Objective
	{
		get
		{
			return objective;
		}
		set
		{
			if (objective != null)
			{
				objective.ValueChanged -= Objective_ValueChanged;
			}
			objective = value;
			if (objective != null)
			{
				objective.ValueChanged += Objective_ValueChanged;
			}
			isDirty = true;
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

	public void SetIsTracker()
	{
		isTracker = true;
		completeColor = Utils.ColorToHex(StringParsers.ParseColor(completeColor));
		incompleteColor = Utils.ColorToHex(StringParsers.ParseColor(incompleteColor));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = objective != null;
		switch (bindingName)
		{
		case "hasobjective":
			value = flag.ToString();
			return true;
		case "objectivecompletesprite":
			if (objective != null)
			{
				Quest ownerQuest = objective.OwnerQuest;
				if (ownerQuest.CurrentState == Quest.QuestState.Completed)
				{
					value = completeIconName;
					return true;
				}
				if (ownerQuest.CurrentState == Quest.QuestState.NotStarted || objective.ObjectiveState == BaseObjective.ObjectiveStates.NotStarted)
				{
					value = notstartedIconName;
				}
				else
				{
					value = (objective.Complete ? completeIconName : incompleteIconName);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "objectivecompletecolor":
			if (objective != null)
			{
				if (objective.OwnerQuest.CurrentState == Quest.QuestState.Completed)
				{
					value = completeColor;
					return true;
				}
				switch (objective.ObjectiveState)
				{
				case BaseObjective.ObjectiveStates.NotStarted:
					value = originalColor;
					break;
				case BaseObjective.ObjectiveStates.InProgress:
					value = incompleteColor;
					break;
				case BaseObjective.ObjectiveStates.Warning:
					value = warningColor;
					break;
				case BaseObjective.ObjectiveStates.Complete:
					value = completeColor;
					break;
				case BaseObjective.ObjectiveStates.Failed:
					value = incompleteColor;
					break;
				}
			}
			else
			{
				value = completeColor;
			}
			return true;
		case "objectivephasecolor":
			if (objective != null)
			{
				if (objective.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					value = originalColor;
				}
				else
				{
					value = (((objective.Phase == 0 || objective.Phase == objective.OwnerQuest.CurrentPhase) && objective.OwnerQuest.CurrentState == Quest.QuestState.InProgress) ? originalColor : inactiveColor);
				}
			}
			else
			{
				value = "FFFFFF";
			}
			return true;
		case "objectivephasehexcolor":
			if (objective != null)
			{
				if (objective.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					value = Owner.activeHexColor;
				}
				else
				{
					value = ((objective.Phase == objective.OwnerQuest.CurrentPhase) ? Owner.activeHexColor : Owner.inactiveHexColor);
				}
			}
			else
			{
				value = "FFFFFF";
			}
			return true;
		case "objectivedescription":
			value = (flag ? objective.Description : "");
			return true;
		case "objectivestate":
			value = (flag ? objective.StatusText : "");
			return true;
		case "objectiveoptional":
			value = ((!flag) ? "" : (objective.Optional ? objectiveOptionalFormatter.Format(OptionalKeyword) : ""));
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		isDirty = true;
	}

	public override void Update(float _dt)
	{
		if (objective != null && objective.OwnerQuest.CurrentState == Quest.QuestState.InProgress && objective.UpdateUI && Time.time > updateTime)
		{
			updateTime = Time.time + 0.1f;
			RefreshBindings(isDirty);
			isDirty = false;
		}
		base.Update(_dt);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "notstarted_icon":
			notstartedIconName = value;
			return true;
		case "complete_icon":
			completeIconName = value;
			return true;
		case "incomplete_icon":
			incompleteIconName = value;
			return true;
		case "complete_color":
			completeColor = value;
			return true;
		case "incomplete_color":
			incompleteColor = value;
			return true;
		case "warning_color":
			warningColor = value;
			return true;
		case "inactive_color":
			inactiveColor = value;
			return true;
		case "original_color":
			originalColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
