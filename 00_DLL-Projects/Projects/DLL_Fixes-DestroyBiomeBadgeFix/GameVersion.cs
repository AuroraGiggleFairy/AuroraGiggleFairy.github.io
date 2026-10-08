using System;
using System.Reflection;

namespace AGFProjects.DestroyBiomeBadgeFix
{
	internal static class GameVersion
	{
		private static bool initialized;

		internal static bool UseV33 { get; private set; }

		internal static void Initialize()
		{
			if (initialized)
			{
				return;
			}

			initialized = true;
			int major = 0;
			int minor = 0;
			string display = "Unknown";
			try
			{
				Type constantsType = Type.GetType("Constants, Assembly-CSharp");
				object versionInfo = constantsType != null ? Member(constantsType, null, "cVersionInformation") : null;
				if (versionInfo != null)
				{
					major = AsInt(Member(versionInfo.GetType(), versionInfo, "Major"));
					minor = AsInt(Member(versionInfo.GetType(), versionInfo, "Minor"));
					display = Member(versionInfo.GetType(), versionInfo, "LongString") as string ?? display;
				}

				if (major == 0 && constantsType != null)
				{
					major = AsInt(Member(constantsType, null, "cVersionMajor"));
					minor = AsInt(Member(constantsType, null, "cVersionMinor"));
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("[AGF] DestroyBiomeBadgeFix version read failed: " + ex.Message);
			}

			bool hasGetItems = typeof(Equipment).GetMethod("GetItems", Type.EmptyTypes) != null;
			UseV33 = major > 0 ? major > 3 || (major == 3 && minor >= 30) : !hasGetItems;
			UnityEngine.Debug.Log("[AGF] DestroyBiomeBadgeFix game " + display + "; path=" + (UseV33 ? "3.3+" : "pre-3.3"));
		}

		private static int AsInt(object value)
		{
			return value is int number ? number : 0;
		}

		private static object Member(Type type, object instance, string name)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = type.GetProperty(name, flags);
			if (property != null)
			{
				return property.GetValue(instance, null);
			}

			return type.GetField(name, flags)?.GetValue(instance);
		}
	}
}
