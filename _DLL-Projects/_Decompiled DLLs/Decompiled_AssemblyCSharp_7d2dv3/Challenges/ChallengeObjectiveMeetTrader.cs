using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveMeetTrader : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string TraderName = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.MeetTrader;

	public override string DescriptionText
	{
		get
		{
			if (string.IsNullOrEmpty(TraderName))
			{
				return Localization.Get("challengeObjectiveMeetAnyTrader");
			}
			return Localization.Get("challengeObjectiveMeet") + " " + Localization.Get(TraderName) + ":";
		}
	}

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.NPCMeet += Current_NPCMeet;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.NPCMeet -= Current_NPCMeet;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_NPCMeet(EntityNPC npc)
	{
		if (!CheckBaseRequirements() && (TraderName == "" || npc.EntityName == TraderName))
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
		if (e.HasAttribute("trader_name"))
		{
			TraderName = e.GetAttribute("trader_name");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveMeetTrader
		{
			TraderName = TraderName
		};
	}
}
