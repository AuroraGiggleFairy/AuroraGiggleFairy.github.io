using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestObjectivesWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultReqColor = "DECEA3";

	[PublicizedFrom(EAccessModifier.Private)]
	public string missingReqColor = "FF0000";

	[PublicizedFrom(EAccessModifier.Private)]
	public string questlimitedColor = "FF0000";

	[PublicizedFrom(EAccessModifier.Private)]
	public string questlimitedcompleteColor = "FFFFFF";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestObjectiveList objectiveList;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	public Quest CurrentQuest
	{
		get
		{
			return currentQuest;
		}
		set
		{
			currentQuest = value;
			questClass = ((value != null) ? QuestClass.GetQuest(value.ID) : null);
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		objectiveList = GetChildByType<XUiC_QuestObjectiveList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "questname":
			value = ((currentQuest != null) ? questClass.Name : "");
			return true;
		case "questtitle":
			value = ((currentQuest != null) ? questClass.SubTitle : "");
			return true;
		case "questdifficulty":
			value = ((currentQuest != null) ? questClass.Difficulty : "");
			return true;
		case "showquest":
			value = (currentQuest != null).ToString();
			return true;
		case "showempty":
			value = (currentQuest == null).ToString();
			return true;
		case "showrequirements":
			value = ((currentQuest != null) ? (currentQuest.Requirements.Count > 0).ToString() : "false");
			return true;
		case "questrequirements":
			value = ((currentQuest != null) ? currentQuest.RequirementsString.Replace("DEFAULT_COLOR", defaultReqColor).Replace("MISSING_COLOR", missingReqColor) : "");
			return true;
		case "questrequirementstitle":
			value = Localization.Get("xuiRequirements");
			return true;
		case "tieradd":
			if (currentQuest != null && questClass.AddsToTierComplete && (currentQuest.AddsProgression || (currentQuest.OwnerJournal.CanAddProgression && currentQuest.QuestProgressDay == int.MinValue)))
			{
				if (questClass.DifficultyTier != 0)
				{
					string arg = ((questClass.DifficultyTier > 0) ? "+" : "-") + questClass.DifficultyTier;
					value = string.Format(Localization.Get("xuiQuestTierAdd"), arg);
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "tieraddlimited":
			if (currentQuest != null && questClass.AddsToTierComplete && ((!currentQuest.OwnerJournal.CanAddProgression && currentQuest.QuestProgressDay == int.MinValue) || currentQuest.QuestProgressDay == -1))
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		case "tieraddlimitedcolor":
			if (currentQuest != null && questClass.AddsToTierComplete)
			{
				if (currentQuest.QuestProgressDay == int.MinValue && !currentQuest.OwnerJournal.CanAddProgression)
				{
					value = questlimitedColor;
				}
				else
				{
					value = questlimitedcompleteColor;
				}
			}
			else
			{
				value = "255,255,255,255";
			}
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "default_req_color":
				defaultReqColor = Utils.ColorToHex(StringParsers.ParseColor32(value));
				return true;
			case "missing_req_color":
				missingReqColor = Utils.ColorToHex(StringParsers.ParseColor32(value));
				return true;
			case "quest_limited_color":
				questlimitedColor = value;
				return true;
			case "quest_limited_complete_color":
				questlimitedcompleteColor = value;
				return true;
			}
		}
		return flag;
	}

	public void SetQuest(XUiC_QuestEntry questEntry)
	{
		entry = questEntry;
		if (entry != null)
		{
			CurrentQuest = entry.Quest;
			objectiveList.Quest = entry.Quest;
		}
		else
		{
			CurrentQuest = null;
		}
	}
}
