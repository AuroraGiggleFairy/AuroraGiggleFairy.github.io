using System;
using System.Collections.Generic;

public class NavObjectClass
{
	public enum RequirementTypes
	{
		None,
		CVar,
		QuestBounds,
		Tracking,
		NoTag,
		InParty,
		IsAlly,
		IsPlayer,
		IsVehicleOwner,
		IsOwner,
		NoActiveQuests,
		MinimumTreasureRadius,
		IsTwitchSpawnedSelf,
		IsTwitchSpawnedOther
	}

	public static List<NavObjectClass> NavObjectClassList = new List<NavObjectClass>();

	public DynamicProperties Properties = new DynamicProperties();

	public string NavObjectClassName = "";

	public RequirementTypes RequirementType;

	public string RequirementName = "";

	public bool UseOverrideIcon;

	public string Tag;

	public NavObjectMapSettings MapSettings;

	public NavObjectCompassSettings CompassSettings;

	public NavObjectScreenSettings OnScreenSettings;

	public NavObjectMapSettings InactiveMapSettings;

	public NavObjectCompassSettings InactiveCompassSettings;

	public NavObjectScreenSettings InactiveOnScreenSettings;

	public static void Reset()
	{
		NavObjectClassList.Clear();
	}

	public bool IsOnMap(bool isActive)
	{
		return GetMapSettings(isActive) != null;
	}

	public bool IsOnCompass(bool isActive)
	{
		return GetCompassSettings(isActive) != null;
	}

	public bool IsOnScreen(bool isActive)
	{
		return GetOnScreenSettings(isActive) != null;
	}

	public NavObjectMapSettings GetMapSettings(bool isActive)
	{
		if (isActive)
		{
			return MapSettings;
		}
		return InactiveMapSettings;
	}

	public NavObjectCompassSettings GetCompassSettings(bool isActive)
	{
		if (isActive)
		{
			return CompassSettings;
		}
		return InactiveCompassSettings;
	}

	public NavObjectScreenSettings GetOnScreenSettings(bool isActive)
	{
		if (isActive)
		{
			return OnScreenSettings;
		}
		return InactiveOnScreenSettings;
	}

	public NavObjectClass(string name)
	{
		NavObjectClassName = name;
	}

	public static NavObjectClass GetNavObjectClass(string className)
	{
		for (int i = 0; i < NavObjectClassList.Count; i++)
		{
			if (NavObjectClassList[i].NavObjectClassName == className)
			{
				return NavObjectClassList[i];
			}
		}
		return null;
	}

	public void Init()
	{
		if (Properties.Values.ContainsKey("requirement_type"))
		{
			if (!Enum.TryParse<RequirementTypes>(Properties.Values["requirement_type"], out RequirementType))
			{
				RequirementType = RequirementTypes.None;
			}
			if (RequirementType != RequirementTypes.None && Properties.Values.ContainsKey("requirement_name"))
			{
				RequirementName = Properties.Values["requirement_name"];
			}
		}
		if (Properties.Values.ContainsKey("tag"))
		{
			Tag = Properties.Values["tag"];
		}
		if (Properties.Values.ContainsKey("use_override_icon"))
		{
			UseOverrideIcon = StringParsers.ParseBool(Properties.Values["use_override_icon"]);
		}
	}
}
