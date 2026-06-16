using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestRewardEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string completeIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeColor = "0,255,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteColor = "255,0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseReward reward;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string chainQuestTypeKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string questTypeKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string optionalKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string bonusKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> rewardOptionalFormatter = new CachedStringFormatter<string>([PublicizedFrom(EAccessModifier.Internal)] (string _s) => "(" + _s + ") ");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> rewardTypeFormatter = new CachedStringFormatter<string, string>([PublicizedFrom(EAccessModifier.Internal)] (string _s1, string _s2) => _s1 + " " + _s2);

	public BaseReward Reward
	{
		get
		{
			return reward;
		}
		set
		{
			reward = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ChainQuest { get; set; }

	public static string ChainQuestTypeKeyword
	{
		get
		{
			if (chainQuestTypeKeyword == "")
			{
				chainQuestTypeKeyword = Localization.Get("RewardTypeChainQuest");
			}
			return chainQuestTypeKeyword;
		}
	}

	public static string QuestTypeKeyword
	{
		get
		{
			if (questTypeKeyword == "")
			{
				questTypeKeyword = Localization.Get("RewardTypeQuest");
			}
			return questTypeKeyword;
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

	public static string BonusKeyword
	{
		get
		{
			if (bonusKeyword == "")
			{
				bonusKeyword = Localization.Get("bonus");
			}
			return bonusKeyword;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = reward != null;
		switch (bindingName)
		{
		case "hasreward":
			value = flag.ToString();
			return true;
		case "rewardicon":
			value = (flag ? reward.Icon : "");
			return true;
		case "rewardiconatlas":
			value = (flag ? reward.IconAtlas : "");
			return true;
		case "rewarddescription":
			value = (flag ? reward.Description : "");
			return true;
		case "rewardtype":
			if (flag)
			{
				if (ChainQuest)
				{
					value = ChainQuestTypeKeyword;
				}
				else
				{
					string v = (reward.Optional ? BonusKeyword : Localization.Get(QuestClass.GetQuest(Reward.OwnerQuest.ID).Category));
					value = rewardTypeFormatter.Format(v, QuestTypeKeyword);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "rewardvalue":
			value = (flag ? reward.ValueText : "");
			return true;
		case "rewardoptional":
			value = ((!flag) ? "" : (reward.Optional ? rewardOptionalFormatter.Format(OptionalKeyword) : ""));
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		RefreshBindings();
		IsDirty = false;
		base.Update(_dt);
	}
}
