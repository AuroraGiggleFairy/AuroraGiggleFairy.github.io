using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

public static class ArmorIconUIHarmonyPatches
{
	private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private static readonly object CacheLock = new object();
	private static readonly Dictionary<string, bool> MagnitudeCapabilityCache = new Dictionary<string, bool>(StringComparer.Ordinal);
	private static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>(StringComparer.Ordinal);
	private static readonly HashSet<string> MissingMemberCache = new HashSet<string>(StringComparer.Ordinal);

	private static readonly string[] MagnitudePropertyKeys = new string[]
	{
		"Magnitude",
		"magnitude",
		"HasMagnitude",
		"UseMagnitude",
		"DisplayMagnitude",
		"MagnitudeMin",
		"MagnitudeMax"
	};

	public static bool TryGetCosmeticArmorIcon(ItemClass itemClass, EntityPlayer player, string bindingName, out string icon, ItemValue itemValue = null)
	{
		icon = null;
		ItemClassArmor itemClassArmor = itemClass as ItemClassArmor;
		if (itemClassArmor == null)
		{
			return false;
		}
		if (!itemClassArmor.IsCosmetic || player == null)
		{
			return false;
		}
		if (HasMagnitudeIndicator(itemClassArmor, itemValue))
		{
			return false;
		}
		if (bindingName == "itemtypeicon" || bindingName == "altitemtypeicon")
		{
			bool flag = IsCosmeticUnlocked(player, itemClassArmor);
			icon = (flag ? itemClassArmor.AltItemTypeIcon : itemClassArmor.ItemTypeIcon);
			return !string.IsNullOrEmpty(icon);
		}
		return false;
	}

	public static bool IsCosmeticUnlocked(EntityPlayer player, ItemClassArmor itemClassArmor)
	{
		if (player == null || player.equipment == null || itemClassArmor == null)
		{
			return false;
		}

		object unlockState;
		try
		{
			unlockState = player.equipment.HasCosmeticUnlocked(itemClassArmor);
		}
		catch
		{
			return false;
		}

		return GetUnlockedFlag(unlockState);
	}

	public static bool HasMagnitudeIndicator(ItemClass itemClass, ItemValue itemValue = null)
	{
		if (!(itemClass is ItemClassArmor))
		{
			return false;
		}

		if (HasCachedMagnitudeCapability(itemClass))
		{
			return true;
		}

		if (itemValue != null)
		{
			return HasMagnitudeMetadata(itemValue);
		}

		return false;
	}

	private static bool HasCachedMagnitudeCapability(ItemClass itemClass)
	{
		if (itemClass == null)
		{
			return false;
		}

		string cacheKey = !string.IsNullOrEmpty(itemClass.Name)
			? itemClass.Name
			: itemClass.GetType().FullName;

		lock (CacheLock)
		{
			bool cached;
			if (MagnitudeCapabilityCache.TryGetValue(cacheKey, out cached))
			{
				return cached;
			}
		}

		bool hasCapability = HasMagnitudeProperty(itemClass) || HasMagnitudeMember(itemClass) || HasStarBasedIcon(itemClass);

		lock (CacheLock)
		{
			MagnitudeCapabilityCache[cacheKey] = hasCapability;
		}

		return hasCapability;
	}

	private static bool HasMagnitudeProperty(ItemClass itemClass)
	{
		if (itemClass == null || itemClass.Properties == null)
		{
			return false;
		}

		IDictionary values = itemClass.Properties.Values as IDictionary;
		if (values == null)
		{
			return false;
		}

		for (int i = 0; i < MagnitudePropertyKeys.Length; i++)
		{
			string key = MagnitudePropertyKeys[i];
			if (values.Contains(key) && IsTruthyMagnitudeValue(values[key]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool HasMagnitudeMember(ItemClass itemClass)
	{
		object memberValue;
		if (TryGetMemberValue(itemClass, "HasMagnitude", out memberValue) && IsTruthyMagnitudeValue(memberValue))
		{
			return true;
		}
		if (TryGetMemberValue(itemClass, "Magnitude", out memberValue) && IsTruthyMagnitudeValue(memberValue))
		{
			return true;
		}
		if (TryGetMemberValue(itemClass, "MagnitudeMin", out memberValue) && IsTruthyMagnitudeValue(memberValue))
		{
			return true;
		}
		if (TryGetMemberValue(itemClass, "MagnitudeMax", out memberValue) && IsTruthyMagnitudeValue(memberValue))
		{
			return true;
		}
		return false;
	}

	private static bool HasMagnitudeMetadata(ItemValue itemValue)
	{
		if (itemValue == null)
		{
			return false;
		}

		int magnitudeInt;
		float magnitudeFloat;

		if ((itemValue.TryGetMetadata("Magnitude", out magnitudeInt) && magnitudeInt != 0)
			|| (itemValue.TryGetMetadata("magnitude", out magnitudeInt) && magnitudeInt != 0))
		{
			return true;
		}

		if ((itemValue.TryGetMetadata("Magnitude", out magnitudeFloat) && Math.Abs(magnitudeFloat) > 0.0001f)
			|| (itemValue.TryGetMetadata("magnitude", out magnitudeFloat) && Math.Abs(magnitudeFloat) > 0.0001f))
		{
			return true;
		}

		return itemValue.Meta > 0;
	}

	private static bool HasStarBasedIcon(ItemClass itemClass)
	{
		return ContainsMagnitudeToken(itemClass.ItemTypeIcon) || ContainsMagnitudeToken(itemClass.AltItemTypeIcon);
	}

	private static bool ContainsMagnitudeToken(string iconName)
	{
		if (string.IsNullOrEmpty(iconName))
		{
			return false;
		}

		return iconName.IndexOf("star", StringComparison.OrdinalIgnoreCase) >= 0
			|| iconName.IndexOf("magnitude", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private static bool IsTruthyMagnitudeValue(object rawValue)
	{
		if (rawValue == null)
		{
			return false;
		}

		if (rawValue is bool)
		{
			return (bool)rawValue;
		}

		if (rawValue is int)
		{
			return (int)rawValue != 0;
		}

		if (rawValue is float)
		{
			return Math.Abs((float)rawValue) > 0.0001f;
		}

		if (rawValue is double)
		{
			return Math.Abs((double)rawValue) > 0.0001;
		}

		if (rawValue is decimal)
		{
			return Math.Abs((decimal)rawValue) > 0.0001m;
		}

		string text = rawValue as string ?? rawValue.ToString();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}

		text = text.Trim();
		if (text.Length == 0)
		{
			return false;
		}

		bool boolValue;
		if (bool.TryParse(text, out boolValue))
		{
			return boolValue;
		}

		double numberValue;
		if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out numberValue))
		{
			return Math.Abs(numberValue) > 0.0001;
		}

		return true;
	}

	private static bool GetUnlockedFlag(object unlockState)
	{
		if (unlockState == null)
		{
			return false;
		}

		if (unlockState is bool)
		{
			return (bool)unlockState;
		}

		object memberValue;
		if (TryGetMemberValue(unlockState, "isUnlocked", out memberValue) && memberValue is bool)
		{
			return (bool)memberValue;
		}
		if (TryGetMemberValue(unlockState, "Item1", out memberValue) && memberValue is bool)
		{
			return (bool)memberValue;
		}

		return false;
	}

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

	private static bool TryGetMemberValue(object instance, string memberName, out object value)
	{
		value = null;
		if (instance == null || string.IsNullOrEmpty(memberName))
		{
			return false;
		}

		Type type = instance.GetType();
		while (type != null)
		{
			MemberInfo member = ResolveMember(type, memberName);
			if (member != null)
			{
				try
				{
					PropertyInfo prop = member as PropertyInfo;
					if (prop != null)
					{
						value = prop.GetValue(instance, null);
						return true;
					}

					FieldInfo field = member as FieldInfo;
					if (field != null)
					{
						value = field.GetValue(instance);
						return true;
					}
				}
				catch
				{
					value = null;
					return false;
				}
			}

			type = type.BaseType;
		}

		return false;
	}
}
