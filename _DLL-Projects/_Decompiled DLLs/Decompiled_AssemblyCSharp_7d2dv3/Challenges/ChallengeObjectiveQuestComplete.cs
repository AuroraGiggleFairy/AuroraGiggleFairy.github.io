using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveQuestComplete : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string questTagText;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTag = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tier;

	[PublicizedFrom(EAccessModifier.Private)]
	public string questText = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.QuestComplete;

	public override string DescriptionText
	{
		get
		{
			if (questText == "")
			{
				questText = Localization.Get("challengeTargetAnyQuest");
			}
			return questText + " " + Localization.Get("challengeObjectiveQuestCompleted") + ":";
		}
	}

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.QuestComplete += Current_QuestComplete;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.QuestComplete -= Current_QuestComplete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_QuestComplete(FastTags<TagGroup.Global> questTags, QuestClass questClass)
	{
		if ((questTag.IsEmpty || questTags.Test_AnySet(questTag)) && !CheckBaseRequirements())
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
		if (e.HasAttribute("quest_tag"))
		{
			questTagText = e.GetAttribute("quest_tag");
			questTag = FastTags<TagGroup.Global>.Parse(questTagText);
		}
		else
		{
			questTag = FastTags<TagGroup.Global>.none;
		}
		if (e.HasAttribute("quest_text_key"))
		{
			questText = Localization.Get(e.GetAttribute("quest_text_key"));
		}
		if (e.HasAttribute("tier"))
		{
			tier = StringParsers.ParseSInt32(e.GetAttribute("tier"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveQuestComplete
		{
			questTag = questTag,
			questText = questText,
			tier = tier
		};
	}
}
