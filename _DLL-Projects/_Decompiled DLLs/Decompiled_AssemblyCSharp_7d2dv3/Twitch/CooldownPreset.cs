using System;
using System.Collections.Generic;

namespace Twitch;

public class CooldownPreset
{
	public enum CooldownTypes
	{
		Always,
		Fill,
		None
	}

	public static string PropName = "name";

	public static string PropTitle = "title";

	public static string PropTitleKey = "title_key";

	public static string PropCooldownType = "cooldown_type";

	public static string PropIsDefault = "is_default";

	public static string PropStartCooldown = "start_cooldown";

	public static string PropDeathCooldown = "death_cooldown";

	public static string PropBMStartOffset = "bm_start_offset";

	public static string PropBMEndOffset = "bm_end_offset";

	public string Name;

	public bool IsDefault;

	public string Title;

	public CooldownTypes CooldownType = CooldownTypes.Fill;

	public float CooldownFillMax;

	public int NextCooldownTime;

	public int StartCooldownTime;

	public int AfterDeathCooldownTime;

	public int BMStartOffset;

	public int BMEndOffset;

	public List<TwitchCooldownEntry> CooldownMaxEntries = new List<TwitchCooldownEntry>();

	public void AddCooldownMaxEntry(int start, int end, int cooldownMax, int cooldownTime)
	{
		if (CooldownMaxEntries == null)
		{
			CooldownMaxEntries = new List<TwitchCooldownEntry>();
		}
		CooldownMaxEntries.Add(new TwitchCooldownEntry
		{
			StartGameStage = start,
			EndGameStage = end,
			CooldownMax = cooldownMax,
			CooldownTime = cooldownTime
		});
	}

	public void SetupCooldownInfo(int gameStage, EntityPlayerLocal localPlayer)
	{
		if (localPlayer == null)
		{
			return;
		}
		for (int i = 0; i < CooldownMaxEntries.Count; i++)
		{
			if (gameStage < CooldownMaxEntries[i].StartGameStage || (gameStage > CooldownMaxEntries[i].EndGameStage && CooldownMaxEntries[i].EndGameStage != -1))
			{
				continue;
			}
			float num = 1f;
			if (localPlayer.Party != null)
			{
				int num2 = 0;
				for (int j = 0; j < localPlayer.Party.MemberList.Count; j++)
				{
					if (localPlayer.Party.MemberList[j].TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
					{
						num2++;
					}
				}
				num += (float)(num2 - 1) * 0.5f;
			}
			CooldownFillMax = (float)CooldownMaxEntries[i].CooldownMax * num;
			NextCooldownTime = CooldownMaxEntries[i].CooldownTime;
			return;
		}
		CooldownFillMax = 100f;
		NextCooldownTime = 180;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		if (properties.Values.ContainsKey(PropName))
		{
			Name = properties.Values[PropName];
		}
		if (properties.Values.ContainsKey(PropTitle))
		{
			Title = properties.Values[PropTitle];
		}
		if (properties.Values.ContainsKey(PropTitleKey))
		{
			Title = Localization.Get(properties.Values[PropTitleKey]);
		}
		if (properties.Values.ContainsKey(PropCooldownType))
		{
			CooldownType = (CooldownTypes)Enum.Parse(typeof(CooldownTypes), properties.Values[PropCooldownType], ignoreCase: true);
		}
		if (properties.Values.ContainsKey(PropIsDefault))
		{
			IsDefault = StringParsers.ParseBool(properties.Values[PropIsDefault]);
		}
		if (properties.Values.ContainsKey(PropStartCooldown))
		{
			StartCooldownTime = StringParsers.ParseSInt32(properties.Values[PropStartCooldown]);
		}
		else
		{
			StartCooldownTime = 300;
		}
		if (properties.Values.ContainsKey(PropDeathCooldown))
		{
			AfterDeathCooldownTime = StringParsers.ParseSInt32(properties.Values[PropDeathCooldown]);
		}
		else
		{
			AfterDeathCooldownTime = 180;
		}
		if (properties.Values.ContainsKey(PropBMStartOffset))
		{
			BMStartOffset = StringParsers.ParseSInt32(properties.Values[PropBMStartOffset]);
		}
		if (properties.Values.ContainsKey(PropBMEndOffset))
		{
			BMEndOffset = StringParsers.ParseSInt32(properties.Values[PropBMEndOffset]);
		}
	}
}
