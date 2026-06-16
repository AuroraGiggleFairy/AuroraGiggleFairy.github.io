using System;
using System.Collections.Generic;
using Platform;

public class EntitlementManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntitlementManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<IEntitlementValidator> entitlementValidators = new List<IEntitlementValidator>();

	public static EntitlementManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new EntitlementManager();
				if (PlatformManager.MultiPlatform.EntitlementValidators != null)
				{
					PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _) =>
					{
						lock (lockObj)
						{
							_instance.entitlementValidators = PlatformManager.MultiPlatform.EntitlementValidators;
						}
					};
				}
			}
			return _instance;
		}
	}

	public bool HasEntitlement(object _addressableKey)
	{
		EntitlementSetEnum entitlementSet = GetEntitlementSet(_addressableKey);
		return HasEntitlement(entitlementSet);
	}

	public bool HasEntitlement(EntitlementSetEnum _set)
	{
		var (flag, result) = CheckOverride(_set);
		if (flag)
		{
			return result;
		}
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		lock (lockObj)
		{
			foreach (IEntitlementValidator entitlementValidator in entitlementValidators)
			{
				if (entitlementValidator.HasEntitlement(_set))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAvailableOnPlatform(object _addressableKey)
	{
		EntitlementSetEnum setForAsset = GetSetForAsset(_addressableKey);
		return IsAvailableOnPlatform(setForAsset);
	}

	public bool IsAvailableOnPlatform(EntitlementSetEnum _set)
	{
		var (flag, result) = CheckOverride(_set);
		if (flag)
		{
			return result;
		}
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		lock (lockObj)
		{
			foreach (IEntitlementValidator entitlementValidator in entitlementValidators)
			{
				if (entitlementValidator.IsAvailableOnPlatform(_set))
				{
					return true;
				}
			}
		}
		return false;
	}

	public EntitlementSetEnum GetSetForAsset(object addressableKey)
	{
		if (!(addressableKey is string text))
		{
			if (EntitlementAddressablesMaps.AddressablesKeyMap.TryGetValue(addressableKey, out var value))
			{
				return value;
			}
			return EntitlementSetEnum.None;
		}
		if (string.IsNullOrEmpty(text))
		{
			return EntitlementSetEnum.None;
		}
		StringSpan key = text.ToLowerInvariant();
		key = key.Trim();
		if (key.Length >= 2 && key[0] == '@' && key[1] == ':')
		{
			key = key.Slice(2);
		}
		while (key.Length > 0)
		{
			if (EntitlementAddressablesMaps.AddressablesStringMap.TryGetValue(key, out var value2))
			{
				return value2;
			}
			int num = key.LastIndexOf('/');
			if (num == -1)
			{
				break;
			}
			key = key.Slice(0, num);
		}
		return EntitlementSetEnum.None;
	}

	public bool IsEntitlementPurchasable(object _addressableKey)
	{
		EntitlementSetEnum entitlementSet = GetEntitlementSet(_addressableKey);
		return IsEntitlementPurchasable(entitlementSet);
	}

	public bool IsEntitlementPurchasable(EntitlementSetEnum _set)
	{
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		lock (lockObj)
		{
			foreach (IEntitlementValidator entitlementValidator in entitlementValidators)
			{
				if (entitlementValidator.IsEntitlementPurchasable(_set))
				{
					return true;
				}
			}
		}
		return false;
	}

	public EntitlementSetEnum GetEntitlementSet(object addressableKey)
	{
		if (!(addressableKey is string text))
		{
			if (EntitlementAddressablesMaps.AddressablesKeyMap.TryGetValue(addressableKey, out var value))
			{
				return value;
			}
			return EntitlementSetEnum.None;
		}
		if (string.IsNullOrEmpty(text))
		{
			return EntitlementSetEnum.None;
		}
		StringSpan key = text.ToLowerInvariant();
		key = key.Trim();
		if (key.Length >= 2 && key[0] == '@' && key[1] == ':')
		{
			key = key.Slice(2);
		}
		while (key.Length > 0)
		{
			if (EntitlementAddressablesMaps.AddressablesStringMap.TryGetValue(key, out var value2))
			{
				return value2;
			}
			int num = key.LastIndexOf('/');
			if (num == -1)
			{
				break;
			}
			key = key.Slice(0, num);
		}
		return EntitlementSetEnum.None;
	}

	public void OpenStore(EntitlementSetEnum _set, Action<EntitlementSetEnum> _onPurchased)
	{
		if (_set == EntitlementSetEnum.None)
		{
			return;
		}
		lock (lockObj)
		{
			using IEnumerator<IEntitlementValidator> enumerator = entitlementValidators.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.OpenStore(_set, _onPurchased))
			{
			}
		}
	}

	public (bool hasOverride, bool overrideValue) CheckOverride(EntitlementSetEnum _set)
	{
		return (hasOverride: false, overrideValue: false);
	}
}
