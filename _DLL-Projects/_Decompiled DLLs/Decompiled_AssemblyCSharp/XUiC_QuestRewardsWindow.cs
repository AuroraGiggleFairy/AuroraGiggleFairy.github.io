using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestRewardsWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestRewardList rewardList;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int, int, int> daytimeFormatter = new CachedStringFormatter<string, int, int, int>([PublicizedFrom(EAccessModifier.Internal)] (string _status, int _day, int _hour, int _min) => string.Format("{0} {1} {2}, {3:00}:{4:00}", _status, Localization.Get("xuiDay"), _day, _hour, _min));

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardNumberFormat = "[DECEA3]{0}[-] {1}";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardNumberBonusFormat = "[DECEA3]{0}[-] {1} ([DECEA3]{2}[-] {3})";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardItemFormat = "[DECEA3]{0}[-] {1}";

	[PublicizedFrom(EAccessModifier.Private)]
	public string rewardItemBonusFormat = "[DECEA3]{0}[-] {1} ([DECEA3]{2}[-] {3})";

	public Quest CurrentQuest
	{
		get
		{
			return currentQuest;
		}
		set
		{
			currentQuest = value;
			questClass = ((currentQuest != null) ? QuestClass.GetQuest(currentQuest.ID) : null);
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		rewardList = GetChildByType<XUiC_QuestRewardList>();
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
		case "showquest":
			value = (currentQuest != null).ToString();
			return true;
		case "showempty":
			value = (currentQuest == null).ToString();
			return true;
		case "finishtime":
			value = "";
			if (currentQuest != null && currentQuest.FinishTime != 0)
			{
				(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(currentQuest.FinishTime);
				int item = tuple.Days;
				int item2 = tuple.Hours;
				int item3 = tuple.Minutes;
				value = daytimeFormatter.Format((currentQuest.CurrentState == Quest.QuestState.Completed) ? Localization.Get("completed") : Localization.Get("failed"), item, item2, item3);
			}
			return true;
		case "commontitle":
			value = ((currentQuest != null) ? (Localization.Get("xuiRewards") + ": ") : "");
			return true;
		case "commonrewards":
			value = "";
			if (currentQuest != null)
			{
				string questRewards = XUiM_Quest.GetQuestRewards(currentQuest, base.xui.playerUI.entityPlayer, isChosen: false, rewardItemFormat, rewardItemBonusFormat, rewardNumberFormat, rewardNumberBonusFormat);
				if (questRewards == "")
				{
					value = Localization.Get("none");
				}
				else
				{
					value = questRewards;
				}
			}
			return true;
		case "chosentitle":
			if (currentQuest == null)
			{
				value = "";
			}
			if (XUiM_Quest.HasQuestRewards(currentQuest, base.xui.playerUI.entityPlayer, isChosen: true))
			{
				float value2 = EffectManager.GetValue(PassiveEffects.QuestRewardChoiceCount, null, 1f, base.xui.playerUI.entityPlayer);
				value = ((value2 == 1f) ? Localization.Get("xuiChooseOne") : Localization.Get("xuiChooseTwo"));
			}
			else
			{
				value = "";
			}
			return true;
		case "chosenrewards":
			value = "";
			if (currentQuest != null)
			{
				string questRewards2 = XUiM_Quest.GetQuestRewards(currentQuest, base.xui.playerUI.entityPlayer, isChosen: true, rewardItemFormat, rewardItemBonusFormat, rewardNumberFormat, rewardNumberBonusFormat);
				if (questRewards2 == "")
				{
					value = Localization.Get("none");
				}
				else
				{
					value = questRewards2;
				}
			}
			return true;
		case "chaintitle":
			value = ((currentQuest != null) ? (Localization.Get("RewardTypeChainQuest") + ": ") : "");
			return true;
		case "chainrewards":
			value = "";
			if (currentQuest != null)
			{
				string chainQuestRewards = XUiM_Quest.GetChainQuestRewards(currentQuest, base.xui.playerUI.entityPlayer, rewardItemFormat, rewardItemBonusFormat, rewardNumberFormat, rewardNumberBonusFormat);
				if (chainQuestRewards == "")
				{
					value = Localization.Get("none");
				}
				else
				{
					value = chainQuestRewards;
				}
			}
			return true;
		default:
			return false;
		}
	}

	public void SetQuest(XUiC_QuestEntry questEntry)
	{
		entry = questEntry;
		if (entry != null)
		{
			CurrentQuest = entry.Quest;
		}
		else
		{
			CurrentQuest = null;
		}
	}
}
