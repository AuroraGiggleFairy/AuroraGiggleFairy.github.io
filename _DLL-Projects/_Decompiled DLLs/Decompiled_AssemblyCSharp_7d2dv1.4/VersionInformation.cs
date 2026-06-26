using System;
using System.IO;
using System.Text.RegularExpressions;

public class VersionInformation : IComparable<VersionInformation>
{
	public enum EGameReleaseType
	{
		Alpha,
		V
	}

	public enum EVersionComparisonResult
	{
		SameBuild,
		SameMinor,
		NewerMinor,
		OlderMinor,
		DifferentMajor
	}

	public readonly EGameReleaseType ReleaseType;

	public readonly int Major;

	public readonly int Minor;

	public readonly int Build;

	public readonly bool IsValid;

	public readonly int NumericalRepresentation;

	public readonly string ShortString;

	public readonly string LongStringNoBuild;

	public readonly string LongString;

	public readonly string SerializableString;

	public readonly Version Version;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex legacyVersionStringMatcher = new Regex("^\\s*Alpha\\s*(\\d+)(?:\\.(\\d+))?\\s*(?:\\(b(\\d+)\\))?\\s*$");

	public VersionInformation(EGameReleaseType _releaseType, int _major, int _minor, int _build)
	{
		ReleaseType = _releaseType;
		Major = _major;
		Minor = _minor;
		Build = _build;
		NumericalRepresentation = ((Major < 1) ? (-1) : ((((int)ReleaseType * 100 + Major) * 100 + Minor) * 1000 + Build));
		ShortString = ((Major < 1) ? "Unk" : $"{ReleaseType.ToStringCached()[0]}{Major}.{Minor}");
		LongStringNoBuild = ((Major < 1) ? "Unknown" : $"{ReleaseType.ToStringCached()} {Major}.{Minor}");
		LongString = ((Major < 1) ? "Unknown" : $"{ReleaseType.ToStringCached()} {Major}.{Minor} (b{Build})");
		SerializableString = $"{ReleaseType.ToStringCached()}.{Major}.{Minor}.{Build}";
		Version = new Version((int)((ReleaseType >= EGameReleaseType.Alpha) ? ReleaseType : EGameReleaseType.Alpha), (Major >= 0) ? Major : 0, (Minor >= 0) ? Minor : 0, (Build >= 0) ? Build : 0);
		IsValid = Major > 0;
	}

	public int CompareTo(VersionInformation _other)
	{
		int num = ReleaseType.CompareTo(_other.ReleaseType);
		if (num != 0)
		{
			return num;
		}
		int major = Major;
		int num2 = major.CompareTo(_other.Major);
		if (num2 != 0)
		{
			return num2;
		}
		major = Minor;
		int num3 = major.CompareTo(_other.Minor);
		if (num3 != 0)
		{
			return num3;
		}
		major = Build;
		return major.CompareTo(_other.Build);
	}

	public bool EqualsMinor(VersionInformation _other)
	{
		if (ReleaseType == _other.ReleaseType && Major == _other.Major)
		{
			return Minor == _other.Minor;
		}
		return false;
	}

	public bool EqualsMajor(VersionInformation _other)
	{
		if (ReleaseType == _other.ReleaseType)
		{
			return Major == _other.Major;
		}
		return false;
	}

	public EVersionComparisonResult CompareToRunningBuild()
	{
		VersionInformation cVersionInformation = Constants.cVersionInformation;
		if (ReleaseType != cVersionInformation.ReleaseType || Major != cVersionInformation.Major)
		{
			return EVersionComparisonResult.DifferentMajor;
		}
		if (Minor < cVersionInformation.Minor)
		{
			return EVersionComparisonResult.OlderMinor;
		}
		if (Minor > cVersionInformation.Minor)
		{
			return EVersionComparisonResult.NewerMinor;
		}
		if (Build != cVersionInformation.Build)
		{
			return EVersionComparisonResult.SameMinor;
		}
		return EVersionComparisonResult.SameBuild;
	}

	public static bool TryParseSerializedString(string _serializedVersionInformation, out VersionInformation _result)
	{
		_result = null;
		string[] array = _serializedVersionInformation.Split('.');
		if (array.Length != 4)
		{
			return false;
		}
		if (!EnumUtils.TryParse<EGameReleaseType>(array[0], out var _result2))
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(array[1], out var _result3))
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(array[2], out var _result4))
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(array[3], out var _result5))
		{
			return false;
		}
		_result = new VersionInformation(_result2, _result3, _result4, _result5);
		return true;
	}

	public static bool TryParseLegacyString(string _legacyVersionString, out VersionInformation _verInfo)
	{
		Match match = legacyVersionStringMatcher.Match(_legacyVersionString);
		_verInfo = null;
		if (match.Success)
		{
			if (!StringParsers.TryParseSInt32(match.Groups[1].Value, out var _result))
			{
				return false;
			}
			int _result2;
			if (match.Groups[2].Success)
			{
				if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out _result2))
				{
					return false;
				}
			}
			else
			{
				_result2 = 0;
			}
			if (!StringParsers.TryParseSInt32(match.Groups[3].Value, out var _result3))
			{
				return false;
			}
			_verInfo = new VersionInformation(EGameReleaseType.Alpha, _result, _result2, _result3);
			return true;
		}
		return false;
	}

	public void Write(BinaryWriter _writer)
	{
		_writer.Write((byte)ReleaseType);
		_writer.Write(Major);
		_writer.Write(Minor);
		_writer.Write(Build);
	}

	public static VersionInformation Read(BinaryReader _reader)
	{
		byte releaseType = _reader.ReadByte();
		int major = _reader.ReadInt32();
		int minor = _reader.ReadInt32();
		int build = _reader.ReadInt32();
		return new VersionInformation((EGameReleaseType)releaseType, major, minor, build);
	}
}
