using System.IO;

public class PlayerProfile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMale = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string raceName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int variantNumber = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string archetype = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string eyeColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string hairName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string hairColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string mustacheName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string chopsName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string beardName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int version = 5;

	public bool IsMale
	{
		get
		{
			return isMale;
		}
		set
		{
			isMale = value;
		}
	}

	public string RaceName
	{
		get
		{
			return raceName;
		}
		set
		{
			raceName = value;
		}
	}

	public string EyeColor
	{
		get
		{
			return eyeColor;
		}
		set
		{
			eyeColor = value;
		}
	}

	public string HairName
	{
		get
		{
			return hairName;
		}
		set
		{
			hairName = value;
		}
	}

	public string HairColor
	{
		get
		{
			return hairColor;
		}
		set
		{
			hairColor = value;
		}
	}

	public string MustacheName
	{
		get
		{
			return mustacheName;
		}
		set
		{
			mustacheName = value;
		}
	}

	public string ChopsName
	{
		get
		{
			return chopsName;
		}
		set
		{
			chopsName = value;
		}
	}

	public string BeardName
	{
		get
		{
			return beardName;
		}
		set
		{
			beardName = value;
		}
	}

	public int VariantNumber
	{
		get
		{
			return variantNumber;
		}
		set
		{
			variantNumber = value;
		}
	}

	public string ProfileArchetype
	{
		get
		{
			if (archetype == null || archetype == string.Empty)
			{
				if (isMale)
				{
					archetype = "BaseMale";
				}
				else
				{
					archetype = "BaseFemale";
				}
			}
			return archetype;
		}
		set
		{
			archetype = value;
		}
	}

	public string EntityClassName
	{
		get
		{
			if (!isMale)
			{
				return "playerFemale";
			}
			return "playerMale";
		}
	}

	public Archetype CreateTempArchetype()
	{
		if (archetype != "BaseMale" && archetype != "BaseFemale")
		{
			return Archetype.GetArchetype(archetype);
		}
		return new Archetype(archetype, isMale, _canCustomize: true)
		{
			Race = raceName,
			Variant = variantNumber,
			Hair = hairName,
			HairColor = hairColor,
			MustacheName = mustacheName,
			ChopsName = chopsName,
			BeardName = beardName,
			EyeColorName = EyeColor
		};
	}

	public PlayerProfile()
	{
		raceName = "white";
		isMale = true;
		variantNumber = 1;
	}

	public PlayerProfile Clone()
	{
		return new PlayerProfile
		{
			raceName = raceName,
			isMale = isMale,
			variantNumber = variantNumber,
			archetype = archetype,
			hairName = hairName,
			hairColor = hairColor,
			mustacheName = mustacheName,
			chopsName = chopsName,
			beardName = beardName,
			EyeColor = EyeColor
		};
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(5);
		writer.Write(archetype);
		writer.Write(isMale);
		writer.Write(raceName);
		writer.Write((byte)variantNumber);
		writer.Write(hairName ?? "");
		writer.Write(hairColor ?? "");
		writer.Write(mustacheName ?? "");
		writer.Write(chopsName ?? "");
		writer.Write(beardName ?? "");
		writer.Write(eyeColor ?? "Blue01");
	}

	public static PlayerProfile Read(BinaryReader reader)
	{
		PlayerProfile playerProfile = new PlayerProfile();
		int num = reader.ReadInt32();
		playerProfile.archetype = reader.ReadString();
		playerProfile.IsMale = reader.ReadBoolean();
		playerProfile.RaceName = reader.ReadString();
		playerProfile.VariantNumber = reader.ReadByte();
		if (num > 1)
		{
			playerProfile.HairName = reader.ReadString();
		}
		if (num > 2)
		{
			playerProfile.HairColor = reader.ReadString();
		}
		if (num > 3)
		{
			playerProfile.MustacheName = reader.ReadString();
			playerProfile.ChopsName = reader.ReadString();
			playerProfile.BeardName = reader.ReadString();
		}
		if (num > 4)
		{
			playerProfile.EyeColor = reader.ReadString();
		}
		return playerProfile;
	}

	public static PlayerProfile LoadLocalProfile()
	{
		return LoadProfile(ProfileSDF.CurrentProfileName());
	}

	public static PlayerProfile LoadProfile(string _profileName)
	{
		return new PlayerProfile
		{
			IsMale = ProfileSDF.GetIsMale(_profileName),
			RaceName = ProfileSDF.GetRaceName(_profileName),
			VariantNumber = ProfileSDF.GetVariantNumber(_profileName),
			ProfileArchetype = ProfileSDF.GetArchetype(_profileName),
			HairName = ProfileSDF.GetHairName(_profileName),
			HairColor = ProfileSDF.GetHairColorName(_profileName),
			MustacheName = ProfileSDF.GetMustacheName(_profileName),
			ChopsName = ProfileSDF.GetChopsName(_profileName),
			BeardName = ProfileSDF.GetBeardName(_profileName),
			EyeColor = ProfileSDF.GetEyeColorName(_profileName)
		};
	}
}
