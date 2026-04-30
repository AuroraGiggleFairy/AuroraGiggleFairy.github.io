using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Challenges;

public class ChallengeCategory
{
	public enum ShowTypes
	{
		Normal,
		CVar,
		Twitch,
		BiomeProgression
	}

	public static Dictionary<string, ChallengeCategory> s_ChallengeCategories = new CaseInsensitiveStringDictionary<ChallengeCategory>();

	public string Name;

	public string Icon;

	public string Title;

	public string DisplayKey = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ShowTypes showType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string showValue = "";

	public ChallengeCategory(string name)
	{
		Name = name;
	}

	public bool CanShow(EntityPlayer player)
	{
		if (showType == ShowTypes.Twitch)
		{
			return player.TwitchEnabled;
		}
		if (showType == ShowTypes.CVar)
		{
			return player.Buffs.GetCustomVar(showValue) > 0f;
		}
		if (showType == ShowTypes.BiomeProgression)
		{
			return GameStats.GetBool(EnumGameStats.BiomeProgression);
		}
		return true;
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
		if (e.HasAttribute("icon"))
		{
			Icon = e.GetAttribute("icon");
		}
		if (e.HasAttribute("show_type"))
		{
			showType = (ShowTypes)Enum.Parse(typeof(ShowTypes), e.GetAttribute("show_type"), ignoreCase: true);
		}
		if (e.HasAttribute("show_value"))
		{
			showValue = e.GetAttribute("show_value");
		}
		if (e.HasAttribute("display_key"))
		{
			DisplayKey = e.GetAttribute("display_key");
		}
	}
}
