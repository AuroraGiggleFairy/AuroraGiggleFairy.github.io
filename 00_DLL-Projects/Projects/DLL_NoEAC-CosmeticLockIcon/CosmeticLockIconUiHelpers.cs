using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class CosmeticLockIconUiHelpers
{
	private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private static readonly object CacheLock = new object();
	private static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>(StringComparer.Ordinal);
	private static readonly HashSet<string> MissingMemberCache = new HashSet<string>(StringComparer.Ordinal);
	private static readonly Dictionary<Type, MethodInfo> FormatMethodCache = new Dictionary<Type, MethodInfo>();
	private static readonly string[] TintFormatterFieldNames = new string[]
	{
		"altitemtypeiconcolorFormatter",
		"iconTypeIconTintFormatter"
	};

	private static string BuildCacheKey(Type type, string memberName)
	{
		string typeName = type != null ? type.FullName : string.Empty;
		return typeName + "|" + memberName;
	}

	private static MemberInfo ResolveMember(Type type, string memberName)
	{
		if (type == null || string.IsNullOrEmpty(memberName))
		{
			return null;
		}

		string cacheKey = BuildCacheKey(type, memberName);
		lock (CacheLock)
		{
			MemberInfo cached;
			if (MemberCache.TryGetValue(cacheKey, out cached))
			{
				return cached;
			}

			if (MissingMemberCache.Contains(cacheKey))
			{
				return null;
			}
		}

		MemberInfo foundMember = null;
		PropertyInfo prop = type.GetProperty(memberName, InstanceFlags);
		if (prop != null && prop.GetIndexParameters().Length == 0)
		{
			foundMember = prop;
		}
		else
		{
			FieldInfo field = type.GetField(memberName, InstanceFlags);
			if (field != null)
			{
				foundMember = field;
			}
		}

		lock (CacheLock)
		{
			if (foundMember != null)
			{
				MemberCache[cacheKey] = foundMember;
			}
			else
			{
				MissingMemberCache.Add(cacheKey);
			}
		}

		return foundMember;
	}

	private static object GetMemberValue(object instance, params string[] memberNames)
	{
		if (instance == null || memberNames == null || memberNames.Length == 0)
		{
			return null;
		}

		Type type = instance.GetType();
		while (type != null)
		{
			for (int i = 0; i < memberNames.Length; i++)
			{
				string memberName = memberNames[i];
				if (string.IsNullOrEmpty(memberName))
				{
					continue;
				}

				MemberInfo member = ResolveMember(type, memberName);
				if (member == null)
				{
					continue;
				}

				try
				{
					PropertyInfo prop = member as PropertyInfo;
					if (prop != null)
					{
						object propValue = prop.GetValue(instance, null);
						if (propValue != null)
						{
							return propValue;
						}
						continue;
					}

					FieldInfo field = member as FieldInfo;
					if (field != null)
					{
						object fieldValue = field.GetValue(instance);
						if (fieldValue != null)
						{
							return fieldValue;
						}
					}
				}
				catch
				{
					return null;
				}
			}

			type = type.BaseType;
		}

		return null;
	}

	public static string FormatXuiRgbaColor(Color32 color)
	{
		return color.r + "," + color.g + "," + color.b + "," + color.a;
	}

	public static string FormatXuiRgbaColor(object controller, Color32 color)
	{
		GameCompat.Initialize();
		string formatted;
		if (TryFormatWithControllerFormatter(controller, color, out formatted))
		{
			return formatted;
		}

		return FormatXuiRgbaColor(color);
	}

	public static EntityPlayerLocal GetEntityPlayerLocal(object controller)
	{
		if (controller == null)
		{
			return null;
		}

		XUiController xuiController = controller as XUiController;
		if (xuiController != null)
		{
			try
			{
				EntityPlayerLocal fastPlayer = TryGetPlayerFromXuiController(xuiController);
				if (fastPlayer != null)
				{
					return fastPlayer;
				}
			}
			catch
			{
			}
		}

		EntityPlayerLocal localPlayer = GetMemberValue(controller, "localPlayer", "LocalPlayer") as EntityPlayerLocal;
		if (localPlayer != null)
		{
			return localPlayer;
		}

		object xui = GetMemberValue(controller, "xui", "XUi", "_xui");
		if (xui == null)
		{
			return null;
		}

		object playerUI = GetMemberValue(xui, "playerUI", "PlayerUI", "_playerUI");
		if (playerUI == null)
		{
			return null;
		}

		return GetMemberValue(playerUI, "entityPlayer", "_entityPlayer", "localPlayer", "LocalPlayer") as EntityPlayerLocal;
	}

	private static EntityPlayerLocal TryGetPlayerFromXuiController(XUiController controller)
	{
		if (controller == null || controller.xui == null || controller.xui.playerUI == null)
		{
			return null;
		}

		return controller.xui.playerUI.entityPlayer;
	}

	private static bool TryFormatWithControllerFormatter(object controller, Color32 color, out string formatted)
	{
		formatted = null;
		if (controller == null)
		{
			return false;
		}

		object formatter = GetMemberValue(controller, TintFormatterFieldNames);
		if (formatter == null)
		{
			return false;
		}

		MethodInfo formatMethod = ResolveFormatMethod(formatter.GetType());
		if (formatMethod == null)
		{
			return false;
		}

		try
		{
			formatted = formatMethod.Invoke(formatter, new object[] { color }) as string;
			return !string.IsNullOrEmpty(formatted);
		}
		catch
		{
			formatted = null;
			return false;
		}
	}

	private static MethodInfo ResolveFormatMethod(Type formatterType)
	{
		if (formatterType == null)
		{
			return null;
		}

		lock (CacheLock)
		{
			MethodInfo cached;
			if (FormatMethodCache.TryGetValue(formatterType, out cached))
			{
				return cached;
			}

			MethodInfo[] methods = formatterType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			MethodInfo found = null;
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo method = methods[i];
				if (method.Name != "Format" || method.ReturnType != typeof(string))
				{
					continue;
				}

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length == 1 && (parameters[0].ParameterType == typeof(Color32) || parameters[0].ParameterType == typeof(Color)))
				{
					found = method;
					break;
				}
			}

			FormatMethodCache[formatterType] = found;
			return found;
		}
	}
}