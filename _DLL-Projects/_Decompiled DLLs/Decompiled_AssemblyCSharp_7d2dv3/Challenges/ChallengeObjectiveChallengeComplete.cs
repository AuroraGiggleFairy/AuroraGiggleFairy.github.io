using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveChallengeComplete : BaseChallengeObjective
{
	public string ChallengeName = "";

	public bool IsGroup;

	public bool IsRedeemed;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.ChallengeComplete;

	public override string DescriptionText
	{
		get
		{
			string text = Localization.Get("challengeTargetAnyChallenge");
			if (ChallengeName != "")
			{
				if (IsGroup)
				{
					ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[ChallengeName];
					if (challengeGroup != null)
					{
						text = challengeGroup.Title;
					}
				}
				else
				{
					ChallengeClass challenge = ChallengeClass.GetChallenge(ChallengeName);
					if (challenge != null)
					{
						text = challenge.Title;
					}
				}
			}
			if (IsRedeemed)
			{
				return Localization.Get("challengeObjectiveRedeem") + " [DECEA3]" + text + "[-]:";
			}
			return Localization.Get("challengeObjectiveComplete") + " [DECEA3]" + text + "[-]:";
		}
	}

	public override void BaseInit()
	{
		base.BaseInit();
		UpdateMax();
	}

	public override void HandleOnCreated()
	{
		base.HandleOnCreated();
		CreateRequirements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMax()
	{
		if (IsGroup)
		{
			ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[ChallengeName];
			MaxCount = challengeGroup.ChallengeClasses.Count;
			if (OwnerClass.ChallengeGroup == challengeGroup)
			{
				MaxCount--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateRequirements()
	{
		if (ShowRequirements)
		{
			Owner.SetRequirementGroup(new RequirementObjectiveGroupWindowOpen("Challenges"));
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.ChallengeComplete += Current_ChallengeComplete;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.ChallengeComplete -= Current_ChallengeComplete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ChallengeComplete(ChallengeClass _challenge, bool _isRedeemed)
	{
		if (IsGroup)
		{
			base.Current = 0;
			List<ChallengeClass> challengeClasses = ChallengeGroup.s_ChallengeGroups[ChallengeName].ChallengeClasses;
			bool flag = false;
			for (int i = 0; i < challengeClasses.Count; i++)
			{
				Challenge challenge = Owner.Owner.ChallengeDictionary[challengeClasses[i].Name];
				flag = false;
				if (challenge.ChallengeState == Challenge.ChallengeStates.Active)
				{
					if (challenge != Owner)
					{
						continue;
					}
					flag = true;
				}
				if (!flag && (!IsRedeemed || challenge.ChallengeState == Challenge.ChallengeStates.Redeemed) && (IsRedeemed || challenge.ChallengeState == Challenge.ChallengeStates.Completed))
				{
					base.Current++;
				}
			}
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
		else if ((string.IsNullOrEmpty(ChallengeName) || string.Compare(_challenge.Name, ChallengeName, ignoreCase: true) == 0) && _isRedeemed == IsRedeemed)
		{
			base.Current++;
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
		if (e.HasAttribute("challenge"))
		{
			ChallengeName = e.GetAttribute("challenge");
		}
		if (e.HasAttribute("is_group"))
		{
			IsGroup = StringParsers.ParseBool(e.GetAttribute("is_group"));
			if (IsGroup)
			{
				MaxCount = -1;
			}
		}
		if (e.HasAttribute("is_redeemed"))
		{
			IsRedeemed = StringParsers.ParseBool(e.GetAttribute("is_redeemed"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveChallengeComplete
		{
			ChallengeName = ChallengeName,
			IsRedeemed = IsRedeemed,
			IsGroup = IsGroup
		};
	}
}
