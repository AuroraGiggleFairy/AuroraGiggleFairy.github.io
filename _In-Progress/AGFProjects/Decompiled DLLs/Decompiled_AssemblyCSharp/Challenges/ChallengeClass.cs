using System.Collections.Generic;
using System.Xml.Linq;
using Platform;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeClass
{
	public enum UINavTypes
	{
		None,
		Crafting,
		TwitchActions
	}

	public static Dictionary<string, ChallengeClass> s_Challenges = new CaseInsensitiveStringDictionary<ChallengeClass>();

	public string Name;

	public string Title;

	public string Icon;

	public ChallengeGroup ChallengeGroup;

	public string ShortDescription;

	public string Description;

	public string PreReqChallengeHint;

	public string ChallengeHint;

	public string RewardEvent;

	public string RewardText = "";

	public string TagName = string.Empty;

	public FastTags<TagGroup.Global> Tags = FastTags<TagGroup.Global>.none;

	public List<BaseChallengeObjective> ObjectiveList = new List<BaseChallengeObjective>();

	public ChallengeClass NextChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextIndex = 0;

	public bool RedeemAlways;

	public bool NeedsConstantUIUpdate;

	public MinEffectController Effects;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int OrderIndex
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool HasNavType => GetNavType() != UINavTypes.None;

	public ChallengeClass(string name)
	{
		Name = name;
		OrderIndex = nextIndex++;
	}

	public static ChallengeClass NewClass(string id)
	{
		if (s_Challenges.ContainsKey(id))
		{
			return null;
		}
		ChallengeClass challengeClass = new ChallengeClass(id.ToLower());
		s_Challenges[id] = challengeClass;
		return challengeClass;
	}

	public static void Cleanup()
	{
		s_Challenges.Clear();
	}

	public static void InitChallenges()
	{
		foreach (string key in s_Challenges.Keys)
		{
			s_Challenges[key].Init();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			ObjectiveList[i].BaseInit();
			if (ObjectiveList[i].NeedsConstantUIUpdate)
			{
				NeedsConstantUIUpdate = true;
			}
		}
	}

	public bool HasEventsOrPassives()
	{
		return Effects != null;
	}

	public void ModifyValue(EntityAlive _ea, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (Effects != null)
		{
			Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, 0f, _tags);
		}
	}

	public static ChallengeClass GetChallenge(string name)
	{
		if (s_Challenges.ContainsKey(name))
		{
			return s_Challenges[name];
		}
		return null;
	}

	public Challenge CreateChallenge(ChallengeJournal ownerJournal)
	{
		Challenge challenge = new Challenge();
		challenge.ChallengeClass = this;
		challenge.Owner = ownerJournal;
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			BaseChallengeObjective baseChallengeObjective = ObjectiveList[i];
			BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
			baseChallengeObjective2.Owner = challenge;
			baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
			baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
			baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
			baseChallengeObjective2.Biome = baseChallengeObjective.Biome;
			baseChallengeObjective2.HandleOnCreated();
			challenge.ObjectiveList.Add(baseChallengeObjective2);
		}
		return challenge;
	}

	public string GetNextChallengeName()
	{
		if (NextChallenge != null)
		{
			return NextChallenge.Name;
		}
		return "";
	}

	public void AddObjective(BaseChallengeObjective objective)
	{
		ObjectiveList.Add(objective);
		objective.OwnerClass = this;
	}

	public bool ResetObjectives(Challenge challenge)
	{
		int count = challenge.ObjectiveList.Count;
		if (count > ObjectiveList.Count)
		{
			challenge.ObjectiveList.RemoveRange(ObjectiveList.Count, count - ObjectiveList.Count);
		}
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			BaseChallengeObjective baseChallengeObjective = ObjectiveList[i];
			if (i < count && baseChallengeObjective.GetType() != challenge.ObjectiveList[i].GetType())
			{
				return false;
			}
			BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
			baseChallengeObjective2.Owner = challenge;
			baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
			baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
			baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
			baseChallengeObjective2.Biome = baseChallengeObjective.Biome;
			baseChallengeObjective2.HandleOnCreated();
			if (i < count)
			{
				BaseChallengeObjective obj = challenge.ObjectiveList[i];
				baseChallengeObjective2.CopyValues(obj, baseChallengeObjective);
				challenge.ObjectiveList[i] = baseChallengeObjective2;
				baseChallengeObjective2.Current = Utils.FastMin(baseChallengeObjective2.Current, baseChallengeObjective2.MaxCount);
			}
			else
			{
				challenge.ObjectiveList.Add(baseChallengeObjective2);
			}
			if (!challenge.IsActive)
			{
				baseChallengeObjective2.Current = baseChallengeObjective2.MaxCount;
				baseChallengeObjective2.Complete = true;
			}
		}
		return true;
	}

	public string GetHint(bool isPreReq)
	{
		if (ChallengeHint == null)
		{
			return "";
		}
		if (isPreReq)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				string key = PreReqChallengeHint + "_alt";
				if (Localization.Exists(key))
				{
					return Localization.Get(key);
				}
			}
			return Localization.Get(PreReqChallengeHint);
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			string key2 = ChallengeHint + "_alt";
			if (Localization.Exists(key2))
			{
				return Localization.Get(key2);
			}
		}
		return Localization.Get(ChallengeHint);
	}

	public string GetDescription()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			string key = Description + "_alt";
			if (Localization.Exists(key))
			{
				return Localization.Get(key);
			}
		}
		return Localization.Get(Description);
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (Effects != null)
		{
			Effects.FireEvent(_eventType, _params);
		}
	}

	public void ParseElement(XElement e)
	{
		if (e.HasAttribute("icon"))
		{
			Icon = e.GetAttribute("icon");
		}
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
		if (e.HasAttribute("group"))
		{
			(ChallengeGroup = ChallengeGroup.s_ChallengeGroups[e.GetAttribute("group")]).AddChallenge(this);
		}
		if (e.HasAttribute("prerequisite_hint"))
		{
			PreReqChallengeHint = e.GetAttribute("prerequisite_hint");
		}
		if (e.HasAttribute("hint"))
		{
			ChallengeHint = e.GetAttribute("hint");
		}
		if (e.HasAttribute("short_description_key"))
		{
			ShortDescription = Localization.Get(e.GetAttribute("short_description_key"));
		}
		else if (e.HasAttribute("short_description"))
		{
			ShortDescription = e.GetAttribute("short_description");
		}
		if (e.HasAttribute("description_key"))
		{
			Description = e.GetAttribute("description_key");
		}
		else if (e.HasAttribute("description"))
		{
			Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("reward_event"))
		{
			RewardEvent = e.GetAttribute("reward_event");
		}
		else
		{
			RewardEvent = ChallengesFromXml.DefaultRewardEvent;
		}
		if (e.HasAttribute("reward_text_key"))
		{
			RewardText = Localization.Get(e.GetAttribute("reward_text_key"));
		}
		else if (e.HasAttribute("reward_text"))
		{
			RewardText = e.GetAttribute("reward_text");
		}
		else
		{
			RewardText = ChallengesFromXml.DefaultRewardText;
		}
		if (e.HasAttribute("tags"))
		{
			TagName = e.GetAttribute("tags");
			Tags = FastTags<TagGroup.Global>.Parse(TagName);
		}
		if (e.HasAttribute("redeem_always"))
		{
			RedeemAlways = StringParsers.ParseBool(e.GetAttribute("redeem_always"));
		}
	}

	public UINavTypes GetNavType()
	{
		for (int i = 0; i < ObjectiveList.Count; i++)
		{
			BaseChallengeObjective baseChallengeObjective = ObjectiveList[i];
			if (baseChallengeObjective.NavType != UINavTypes.None)
			{
				return baseChallengeObjective.NavType;
			}
		}
		return UINavTypes.None;
	}
}
