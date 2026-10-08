using System;
using System.Reflection;

namespace ExpandedInteractionPrompts
{
	internal static class GameVersion
	{
		private static bool initialized;

		internal static bool UseV33 { get; private set; }

		internal static string DisplayString { get; private set; } = "Unknown";

		internal static void Initialize()
		{
			if (initialized)
			{
				return;
			}

			initialized = true;
			int major = 0;
			int minor = 0;
			try
			{
				Type constantsType = Type.GetType("Constants, Assembly-CSharp");
				if (constantsType != null)
				{
					object versionInfo = GetStatic(constantsType, "cVersionInformation");
					if (versionInfo != null)
					{
						major = AsInt(GetInstance(versionInfo, "Major"));
						minor = AsInt(GetInstance(versionInfo, "Minor"));
						string text = GetInstance(versionInfo, "LongString") as string;
						if (!string.IsNullOrEmpty(text))
						{
							DisplayString = text;
						}
					}

					if (major == 0)
					{
						major = AsInt(GetStatic(constantsType, "cVersionMajor"));
						minor = AsInt(GetStatic(constantsType, "cVersionMinor"));
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ExpandedInteractionPrompts: version read failed: " + ex.Message);
			}

			// 3.2 ItemStack.count is a field. 3.3 exposes count as a property.
			bool countIsProperty = typeof(ItemStack).GetProperty("count") != null;
			UseV33 = major > 0 ? major > 3 || (major == 3 && minor >= 30) : countIsProperty;
			Console.WriteLine("ExpandedInteractionPrompts: game " + DisplayString + "; path=" + (UseV33 ? "3.3+" : "pre-3.3"));
		}

		private static object GetStatic(Type type, string name)
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = type.GetProperty(name, flags);
			if (property != null)
			{
				return property.GetValue(null);
			}

			return type.GetField(name, flags)?.GetValue(null);
		}

		private static object GetInstance(object instance, string name)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type type = instance.GetType();
			PropertyInfo property = type.GetProperty(name, flags);
			if (property != null)
			{
				return property.GetValue(instance);
			}

			return type.GetField(name, flags)?.GetValue(instance);
		}

		private static int AsInt(object value)
		{
			return value is int number ? number : 0;
		}
	}
}
