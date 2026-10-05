using System;
using System.Reflection;

namespace SortingCart
{
	internal static class GameVersion
	{
		private static bool initialized;

		internal static int Major { get; private set; }
		internal static int Minor { get; private set; }
		internal static string DisplayString { get; private set; } = "Unknown";
		internal static bool UseV33 { get; private set; }

		internal static void Initialize()
		{
			if (initialized)
			{
				return;
			}

			initialized = true;
			Read();
			bool legacyLoot = Type.GetType("ITileEntityLootable, Assembly-CSharp") != null;
			if (Major > 0)
			{
				UseV33 = Major > 3 || (Major == 3 && Minor >= 30);
			}
			else
			{
				UseV33 = !legacyLoot;
			}

			Log.Info("game " + DisplayString + "; path=" + (UseV33 ? "3.3+" : "3.0-3.2"));
		}

		private static void Read()
		{
			try
			{
				Type constantsType = Type.GetType("Constants, Assembly-CSharp");
				if (constantsType == null)
				{
					return;
				}

				object versionInfo = GetStatic(constantsType, "cVersionInformation");
				if (versionInfo != null)
				{
					Major = AsInt(GetInstance(versionInfo, "Major"));
					Minor = AsInt(GetInstance(versionInfo, "Minor"));
					string text = GetInstance(versionInfo, "LongString") as string;
					if (!string.IsNullOrEmpty(text))
					{
						DisplayString = text;
						return;
					}
				}

				Major = AsInt(GetStatic(constantsType, "cVersionMajor"));
				Minor = AsInt(GetStatic(constantsType, "cVersionMinor"));
				int build = AsInt(GetStatic(constantsType, "cVersionBuild"));
				if (Major > 0)
				{
					DisplayString = "V " + Major + "." + Minor + "." + build;
				}
			}
			catch (Exception ex)
			{
				Log.Error("version read failed", ex);
			}
		}

		private static object GetStatic(Type type, string name)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return property.GetValue(null);
			}

			FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			return field?.GetValue(null);
		}

		private static object GetInstance(object instance, string name)
		{
			Type type = instance.GetType();
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return property.GetValue(instance);
			}

			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return field?.GetValue(instance);
		}

		private static int AsInt(object value)
		{
			return value is int number ? number : 0;
		}
	}
}
