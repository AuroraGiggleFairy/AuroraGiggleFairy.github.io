using System;
using System.Reflection;

public static class GameCompat
{
	public const int MinSupportedMajor = 3;
	public const int MinSupportedMinor = 0;

	private static readonly object InitLock = new object();
	private static bool initialized;

	public static int Major { get; private set; }
	public static int Minor { get; private set; }
	public static int Build { get; private set; }
	public static string DisplayString { get; private set; }
	public static bool IsSupported { get; private set; }
	public static bool HasLegacyItemInfoTintFormatter { get; private set; }
	public static bool HasRenamedItemInfoTintFormatter { get; private set; }
	public static bool HasSharedItemTypeIconHelper { get; private set; }

	public static void Initialize()
	{
		lock (InitLock)
		{
			if (initialized)
			{
				return;
			}

			initialized = true;
			ReadGameVersion();
			DetectCapabilities();
			IsSupported = Major > MinSupportedMajor || (Major == MinSupportedMajor && Minor >= MinSupportedMinor);
			Console.WriteLine(
				"CosmeticLockIcon: game " + DisplayString
				+ "; supportedRange=V" + MinSupportedMajor + "." + MinSupportedMinor + "+"
				+ "; isSupported=" + IsSupported
				+ "; itemInfoTint=" + DescribeItemInfoTintLayout());
		}
	}

	public static bool IsAtLeast(int major, int minor)
	{
		if (Major != major)
		{
			return Major > major;
		}

		return Minor >= minor;
	}

	private static string DescribeItemInfoTintLayout()
	{
		if (HasLegacyItemInfoTintFormatter)
		{
			return "altitemtypeiconcolorFormatter";
		}

		if (HasRenamedItemInfoTintFormatter)
		{
			return "iconTypeIconTintFormatter";
		}

		return "local-fallback";
	}

	private static void ReadGameVersion()
	{
		Major = 0;
		Minor = 0;
		Build = 0;
		DisplayString = "Unknown";

		try
		{
			Type constantsType = Type.GetType("Constants, Assembly-CSharp") ?? typeof(Constants);
			object versionInfo = GetStaticMemberValue(constantsType, "cVersionInformation");
			if (versionInfo != null)
			{
				object major = GetInstanceMemberValue(versionInfo, "Major");
				object minor = GetInstanceMemberValue(versionInfo, "Minor");
				object build = GetInstanceMemberValue(versionInfo, "Build");
				object longString = GetInstanceMemberValue(versionInfo, "LongString");
				if (major is int)
				{
					Major = (int)major;
				}
				if (minor is int)
				{
					Minor = (int)minor;
				}
				if (build is int)
				{
					Build = (int)build;
				}
				string text = longString as string;
				if (!string.IsNullOrEmpty(text))
				{
					DisplayString = text;
					return;
				}
			}

			object majorConst = GetStaticMemberValue(constantsType, "cVersionMajor");
			object minorConst = GetStaticMemberValue(constantsType, "cVersionMinor");
			object buildConst = GetStaticMemberValue(constantsType, "cVersionBuild");
			if (majorConst is int)
			{
				Major = (int)majorConst;
			}
			if (minorConst is int)
			{
				Minor = (int)minorConst;
			}
			if (buildConst is int)
			{
				Build = (int)buildConst;
			}
			if (Major > 0)
			{
				DisplayString = "V " + Major + "." + Minor + " (b" + Build + ")";
			}
		}
		catch (Exception ex)
		{
			DisplayString = "Unknown (" + ex.GetType().Name + ")";
		}
	}

	private static void DetectCapabilities()
	{
		Type itemInfoType = Type.GetType("XUiC_ItemInfoWindow, Assembly-CSharp");
		HasLegacyItemInfoTintFormatter = HasInstanceField(itemInfoType, "altitemtypeiconcolorFormatter");
		HasRenamedItemInfoTintFormatter = HasInstanceField(itemInfoType, "iconTypeIconTintFormatter");
		HasSharedItemTypeIconHelper = Type.GetType("XUiBindingHelper, Assembly-CSharp") != null;
	}

	private static bool HasInstanceField(Type type, string fieldName)
	{
		if (type == null || string.IsNullOrEmpty(fieldName))
		{
			return false;
		}

		FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		return field != null;
	}

	private static object GetStaticMemberValue(Type type, string memberName)
	{
		if (type == null || string.IsNullOrEmpty(memberName))
		{
			return null;
		}

		const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo field = type.GetField(memberName, flags);
		if (field != null)
		{
			return field.GetValue(null);
		}

		PropertyInfo prop = type.GetProperty(memberName, flags);
		if (prop != null && prop.GetIndexParameters().Length == 0)
		{
			return prop.GetValue(null, null);
		}

		return null;
	}

	private static object GetInstanceMemberValue(object instance, string memberName)
	{
		if (instance == null || string.IsNullOrEmpty(memberName))
		{
			return null;
		}

		Type type = instance.GetType();
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo field = type.GetField(memberName, flags);
		if (field != null)
		{
			return field.GetValue(instance);
		}

		PropertyInfo prop = type.GetProperty(memberName, flags);
		if (prop != null && prop.GetIndexParameters().Length == 0)
		{
			return prop.GetValue(instance, null);
		}

		return null;
	}
}
