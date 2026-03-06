using SDF;
using UnityEngine;

public static class ProfileSDF
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static SdfFile profileSDF;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PROFILE_NAMES = "profileNames";

	[PublicizedFrom(EAccessModifier.Private)]
	static ProfileSDF()
	{
		profileSDF = new SdfFile();
		profileSDF.Open(GameIO.GetSaveGameRootDir() + "/sdcs_profiles.sdf");
	}

	public static string CurrentProfileName()
	{
		return profileSDF.GetString("selectedProfile") ?? "";
	}

	public static void Save()
	{
		profileSDF.SaveAndKeepOpen();
	}

	public static bool ProfileExists(string _profileName)
	{
		string text = profileSDF.GetString("profileNames");
		if (text == null)
		{
			return false;
		}
		if (!text.Contains(","))
		{
			return text == _profileName;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == _profileName)
			{
				return true;
			}
		}
		return false;
	}

	public static void DeleteProfile(string _profileName)
	{
		if (!ProfileExists(_profileName))
		{
			return;
		}
		string[] keys = profileSDF.GetKeys();
		foreach (string text in keys)
		{
			if (text.StartsWith(_profileName + "."))
			{
				profileSDF.Remove(text);
				Save();
			}
		}
		removeProfileName(_profileName);
		Save();
	}

	public static void SaveArchetype(string _archetype, bool _isMale)
	{
		if (!ProfileExists(_archetype))
		{
			addProfileName(_archetype);
			setSex(_archetype, _isMale);
			setArchetype(_archetype, _archetype);
		}
		Save();
	}

	public static void SaveProfile(string _profileName, string _archetype, bool _isMale, string _raceName, int _variantNumber, string _eyeColorName, string _hairName, string _hairColor, string _mustacheName, string _chopsName, string _beardName)
	{
		if (!ProfileExists(_profileName))
		{
			addProfileName(_profileName);
		}
		if (_archetype == "")
		{
			_archetype = ((!_isMale) ? "BaseFemale" : "BaseMale");
		}
		setSex(_profileName, _isMale);
		setRace(_profileName, _raceName);
		setArchetype(_profileName, _archetype);
		setVariant(_profileName, _variantNumber);
		setEyeColor(_profileName, _eyeColorName);
		setHairName(_profileName, _hairName);
		setHairColor(_profileName, _hairColor);
		setMustacheName(_profileName, _mustacheName);
		setChopsName(_profileName, _chopsName);
		setBeardName(_profileName, _beardName);
		SetSelectedProfile(_profileName);
		Save();
	}

	public static Archetype CreateTempArchetype(string _profileName)
	{
		Archetype archetype = new Archetype(_profileName, GetIsMale(_profileName), _canCustomize: true);
		archetype.Race = GetRaceName(_profileName);
		archetype.Variant = GetVariantNumber(_profileName);
		archetype.Hair = GetHairName(_profileName);
		archetype.HairColor = GetHairColorName(_profileName);
		archetype.MustacheName = GetMustacheName(_profileName);
		archetype.ChopsName = GetChopsName(_profileName);
		archetype.BeardName = GetBeardName(_profileName);
		archetype.EyeColorName = GetEyeColorName(_profileName);
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "body",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "body"
		});
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "hands",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "hands"
		});
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "feet",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "feet"
		});
		return archetype;
	}

	public static void SetSelectedProfile(string _profileName)
	{
		profileSDF.Set("selectedProfile", _profileName);
		if (GetIsMale(_profileName))
		{
			GamePrefs.Set(EnumGamePrefs.OptionsPlayerModel, "playerMale");
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.OptionsPlayerModel, "playerFemale");
		}
	}

	public static void addProfileName(string _profileName)
	{
		string text = profileSDF.GetString("profileNames");
		if (text == null || text.Length == 0)
		{
			profileSDF.Set("profileNames", _profileName, isBinary: false);
		}
		else
		{
			profileSDF.Set("profileNames", profileSDF.GetString("profileNames") + "," + _profileName, isBinary: false);
		}
	}

	public static void removeProfileName(string _profileName)
	{
		string text = profileSDF.GetString("profileNames");
		if (text == null || text.Length == 0)
		{
			profileSDF.Set("profileNames", "", isBinary: false);
			return;
		}
		string text2 = "";
		bool flag = true;
		string[] array = text.Split(',');
		foreach (string text3 in array)
		{
			if (text3 != _profileName)
			{
				if (!flag)
				{
					text2 += ",";
				}
				text2 += text3;
				flag = false;
			}
		}
		profileSDF.Set("profileNames", text2, isBinary: false);
	}

	public static void setSex(string _profileName, bool _isMale)
	{
		profileSDF.Set(_profileName + ".isMale", _isMale);
	}

	public static void setRace(string _profileName, string _raceName)
	{
		profileSDF.Set(_profileName + ".race", _raceName);
	}

	public static void setVariant(string _profileName, int _variantNumber)
	{
		profileSDF.Set(_profileName + ".variant", _variantNumber);
	}

	public static void setColor(string _profileName, string _colorName, Color _color)
	{
		profileSDF.Set(_profileName + "." + _colorName + ".r", _color.r);
		profileSDF.Set(_profileName + "." + _colorName + ".g", _color.g);
		profileSDF.Set(_profileName + "." + _colorName + ".b", _color.b);
		profileSDF.Set(_profileName + "." + _colorName + ".a", _color.a);
	}

	public static void setEyeColor(string _profileName, string _eyeColorName)
	{
		profileSDF.Set(_profileName + ".eyeColor", _eyeColorName);
	}

	public static void setHairName(string _profileName, string _hairName)
	{
		profileSDF.Set(_profileName + ".hairName", _hairName);
	}

	public static void setHairColor(string _profileName, string _hairColorName)
	{
		profileSDF.Set(_profileName + ".hairColor", _hairColorName);
	}

	public static void setMustacheName(string _profileName, string _mustacheName)
	{
		profileSDF.Set(_profileName + ".mustacheName", _mustacheName);
	}

	public static void setChopsName(string _profileName, string _chopsName)
	{
		profileSDF.Set(_profileName + ".chopsName", _chopsName);
	}

	public static void setBeardName(string _profileName, string _beardName)
	{
		profileSDF.Set(_profileName + ".beardName", _beardName);
	}

	public static void setEyebrowName(string _profileName, string _eyebrowName)
	{
		profileSDF.Set(_profileName + ".eyebrowName", _eyebrowName);
	}

	public static void setArchetype(string _profileName, string _archetype)
	{
		profileSDF.Set(_profileName + ".archetype", _archetype);
	}

	public static Color GetSkinColor(string _profileName)
	{
		float r = profileSDF.GetFloat(_profileName + ".skin.r").GetValueOrDefault();
		float g = profileSDF.GetFloat(_profileName + ".skin.g").GetValueOrDefault();
		float b = profileSDF.GetFloat(_profileName + ".skin.b").GetValueOrDefault();
		float a = profileSDF.GetFloat(_profileName + ".skin.a").GetValueOrDefault();
		return new Color(r, g, b, a);
	}

	public static Color GetEyebrowColor(string _profileName)
	{
		float r = profileSDF.GetFloat(_profileName + ".eyebrow.r").GetValueOrDefault();
		float g = profileSDF.GetFloat(_profileName + ".eyebrow.g").GetValueOrDefault();
		float b = profileSDF.GetFloat(_profileName + ".eyebrow.b").GetValueOrDefault();
		float a = profileSDF.GetFloat(_profileName + ".eyebrow.a").GetValueOrDefault();
		return new Color(r, g, b, a);
	}

	public static bool GetIsMale(string _profileName)
	{
		return profileSDF.GetBool(_profileName + ".isMale") == true;
	}

	public static string GetRaceName(string _profileName)
	{
		string text = profileSDF.GetString(_profileName + ".race");
		if (text == null)
		{
			return "White";
		}
		return text;
	}

	public static int GetVariantNumber(string _profileName)
	{
		return profileSDF.GetInt(_profileName + ".variant").GetValueOrDefault();
	}

	public static string GetArchetype(string _profileName)
	{
		string text = profileSDF.GetString(_profileName + ".archetype");
		if (text == null)
		{
			if (!GetIsMale(_profileName))
			{
				return "BaseFemale";
			}
			return "BaseMale";
		}
		return text;
	}

	public static float GetBodyDna(string _profileName, string _bodyPartName)
	{
		return Mathf.Clamp01(profileSDF.GetFloat(_profileName + ".bodyData." + _bodyPartName).GetValueOrDefault());
	}

	public static string GetEyeColorName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".eyeColor");
	}

	public static string GetHairName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".hairName");
	}

	public static string GetHairColorName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".hairColor");
	}

	public static string GetMustacheName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".mustacheName");
	}

	public static string GetChopsName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".chopsName");
	}

	public static string GetBeardName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".beardName");
	}

	public static string GetEyebrowName(string _profileName)
	{
		return profileSDF.GetString(_profileName + ".eyebrowName");
	}

	public static string[] GetProfiles()
	{
		string text = profileSDF.GetString("profileNames");
		if (text == null)
		{
			return new string[0];
		}
		if (text.Contains(","))
		{
			return profileSDF.GetString("profileNames").Split(',');
		}
		return new string[1] { text };
	}
}
