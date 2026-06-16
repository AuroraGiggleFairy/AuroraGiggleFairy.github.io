using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveTwitch : BaseChallengeObjective
{
	public TwitchObjectiveTypes TwitchObjectiveType;

	public string Param = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Twitch;

	public override string DescriptionText => TwitchObjectiveType switch
	{
		TwitchObjectiveTypes.Enabled => Localization.Get("challengeObjectiveTwitchEnabled"), 
		TwitchObjectiveTypes.EnableExtras => Localization.Get("challengeObjectiveTwitchEnableExtras"), 
		TwitchObjectiveTypes.HelperReward => Localization.Get("challengeObjectiveTwitchHelperRewards"), 
		TwitchObjectiveTypes.ChannelPointRedeems => Localization.Get("challengeObjectiveTwitchChannelPointRedeems"), 
		TwitchObjectiveTypes.VoteComplete => Localization.Get("challengeObjectiveTwitchVotesCompleted"), 
		TwitchObjectiveTypes.PimpPot => Localization.Get("challengeObjectiveTwitchPimpPotRewarded"), 
		TwitchObjectiveTypes.BitPot => Localization.Get("challengeObjectiveTwitchBitPotRewarded"), 
		TwitchObjectiveTypes.DefeatBossHorde => Localization.Get("challengeObjectiveTwitchBossHordesDefeated"), 
		TwitchObjectiveTypes.GoodAction => Localization.Get("challengeObjectiveTwitchGoodActions"), 
		TwitchObjectiveTypes.BadAction => Localization.Get("challengeObjectiveTwitchBadActions"), 
		_ => "", 
	};

	public override ChallengeClass.UINavTypes NavType
	{
		get
		{
			if (TwitchObjectiveType == TwitchObjectiveTypes.EnableExtras)
			{
				return ChallengeClass.UINavTypes.TwitchActions;
			}
			return ChallengeClass.UINavTypes.None;
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.TwitchEventReceive += Current_TwitchEventReceive;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.TwitchEventReceive -= Current_TwitchEventReceive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_TwitchEventReceive(TwitchObjectiveTypes action, string param)
	{
		if (!CheckBaseRequirements())
		{
			if (action == TwitchObjectiveType && (Param == "" || Param.EqualsCaseInsensitive(param)))
			{
				base.Current++;
			}
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("objective_type"))
		{
			TwitchObjectiveType = (TwitchObjectiveTypes)Enum.Parse(typeof(TwitchObjectiveTypes), e.GetAttribute("objective_type"), ignoreCase: true);
		}
		if (e.HasAttribute("objective_param"))
		{
			Param = e.GetAttribute("objective_param");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveTwitch
		{
			TwitchObjectiveType = TwitchObjectiveType,
			Param = Param
		};
	}
}
