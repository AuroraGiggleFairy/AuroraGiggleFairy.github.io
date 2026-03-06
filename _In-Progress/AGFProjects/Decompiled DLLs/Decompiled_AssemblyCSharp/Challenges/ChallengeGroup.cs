using System.Collections.Generic;
using System.Xml.Linq;
using UniLinq;

namespace Challenges;

public class ChallengeGroup
{
	public class ChallengeCount
	{
		public FastTags<TagGroup.Global> Tags;

		public int Count;
	}

	public static Dictionary<string, ChallengeGroup> s_ChallengeGroups = new CaseInsensitiveStringDictionary<ChallengeGroup>();

	public string Name;

	public string Title;

	public bool IsComplete;

	public string RewardEvent;

	public string RewardText;

	public string ObjectiveText;

	public bool IsRandom;

	public int ActiveChallengeCount = 10;

	public int DayReset = -1;

	public bool LinkChallenges;

	public string Category;

	public string HiddenBy = "";

	public bool UIDirty;

	public List<ChallengeCount> ChallengeCounts;

	public List<ChallengeClass> ChallengeClasses = new List<ChallengeClass>();

	public MinEffectController Effects;

	public ChallengeGroup(string name)
	{
		Name = name;
	}

	public static ChallengeGroup NewClass(string id)
	{
		if (s_ChallengeGroups.ContainsKey(id))
		{
			return null;
		}
		ChallengeGroup challengeGroup = new ChallengeGroup(id.ToLower());
		s_ChallengeGroups[id] = challengeGroup;
		return challengeGroup;
	}

	public static ChallengeGroup GetGroup(string id)
	{
		if (!s_ChallengeGroups.ContainsKey(id))
		{
			return null;
		}
		return s_ChallengeGroups[id];
	}

	public void AddChallengeCount(string tag, int count)
	{
		if (ChallengeCounts == null)
		{
			ChallengeCounts = new List<ChallengeCount>();
		}
		ChallengeCounts.Add(new ChallengeCount
		{
			Tags = FastTags<TagGroup.Global>.Parse(tag),
			Count = count
		});
	}

	public void AddChallenge(ChallengeClass challenge)
	{
		ChallengeClasses.Add(challenge);
	}

	public void ParseElement(XElement e)
	{
		if (e.HasAttribute("title_key"))
		{
			Title = Localization.Get(e.GetAttribute("title_key"));
		}
		else if (e.HasAttribute("title"))
		{
			Title = e.GetAttribute("title");
		}
		else
		{
			Title = Name;
		}
		if (e.HasAttribute("category"))
		{
			Category = e.GetAttribute("category");
		}
		if (e.HasAttribute("reward_event"))
		{
			RewardEvent = e.GetAttribute("reward_event");
		}
		if (e.HasAttribute("reward_text_key"))
		{
			RewardText = Localization.Get(e.GetAttribute("reward_text_key"));
		}
		else if (e.HasAttribute("reward_text"))
		{
			RewardText = e.GetAttribute("reward_text");
		}
		if (e.HasAttribute("objective_text_key"))
		{
			ObjectiveText = Localization.Get(e.GetAttribute("objective_text_key"));
		}
		else if (e.HasAttribute("objective_text"))
		{
			ObjectiveText = e.GetAttribute("objective_text");
		}
		if (e.HasAttribute("active_challenge_count"))
		{
			ActiveChallengeCount = StringParsers.ParseSInt32(e.GetAttribute("active_challenge_count"));
		}
		if (e.HasAttribute("day_reset"))
		{
			DayReset = StringParsers.ParseSInt32(e.GetAttribute("day_reset"));
		}
		if (e.HasAttribute("is_random"))
		{
			IsRandom = StringParsers.ParseBool(e.GetAttribute("is_random"));
		}
		if (e.HasAttribute("link_challenges"))
		{
			LinkChallenges = StringParsers.ParseBool(e.GetAttribute("link_challenges"));
		}
		if (e.HasAttribute("hidden_by"))
		{
			HiddenBy = e.GetAttribute("hidden_by");
		}
	}

	public bool IsVisible(EntityPlayer player)
	{
		if (HiddenBy == "")
		{
			return true;
		}
		if (s_ChallengeGroups.ContainsKey(HiddenBy))
		{
			return s_ChallengeGroups[HiddenBy].IsComplete;
		}
		return ChallengeCategory.s_ChallengeCategories[Category].CanShow(player);
	}

	public bool HasEventsOrPassives()
	{
		return Effects != null;
	}

	public void ModifyValue(EntityAlive _ea, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (Effects != null && IsComplete)
		{
			Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, 0f, _tags);
		}
	}

	public List<ChallengeClass> GetChallengeClassesForCreate()
	{
		List<ChallengeClass> list = new List<ChallengeClass>();
		for (int i = 0; i < ChallengeClasses.Count; i++)
		{
			list.Add(ChallengeClasses[i]);
		}
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		for (int j = 0; j < list.Count * 2; j++)
		{
			int index = gameRandom.RandomRange(list.Count);
			int index2 = gameRandom.RandomRange(list.Count);
			ChallengeClass value = list[index];
			list[index] = list[index2];
			list[index2] = value;
		}
		if (ChallengeCounts != null)
		{
			for (int k = 0; k < ChallengeCounts.Count; k++)
			{
				ChallengeCount challengeCount = ChallengeCounts[k];
				int num = challengeCount.Count;
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					if (list[num2].Tags.Test_AnySet(challengeCount.Tags))
					{
						if (num == 0)
						{
							list.RemoveAt(num2);
						}
						else
						{
							num--;
						}
					}
				}
			}
			list = list.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass c) => c.TagName).ToList();
		}
		return list;
	}
}
