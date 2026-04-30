using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public class DownloadableContentValidator : IEntitlementValidator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<EntitlementSetEnum, string> entitlementMap = new Dictionary<EntitlementSetEnum, string>
	{
		{
			EntitlementSetEnum.MarauderCosmetic,
			"9P0P2QLB276Q"
		},
		{
			EntitlementSetEnum.HoarderCosmetic,
			"9NZ80ZC0SS1S"
		},
		{
			EntitlementSetEnum.DesertCosmetic,
			"9MZV1FWF2CGR"
		},
		{
			EntitlementSetEnum.ClassicSurvivorCosmetic,
			"9MWGC48ZGF76"
		},
		{
			EntitlementSetEnum.PirateCosmetic,
			"9P54R75H9N3Z"
		},
		{
			EntitlementSetEnum.ChristmasCosmetics,
			"9NB7W1P5CQ30"
		},
		{
			EntitlementSetEnum.HellreaverCosmetic,
			"9NJQPF9T4XLD"
		},
		{
			EntitlementSetEnum.ButcherCosmetic,
			"9N63SN1VPKR3"
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<string> ownedEntitlements = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public User user;

	[PublicizedFrom(EAccessModifier.Private)]
	public XStoreContext storeContext;

	[PublicizedFrom(EAccessModifier.Private)]
	public int remainingStoreOperations;

	public void Init(IPlatform _owner)
	{
		PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Internal)] (IPlatform _) =>
		{
			user = (User)_owner.User;
			foreach (EntitlementSetEnum key in entitlementMap.Keys)
			{
				FetchEntitlement(key);
			}
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FetchEntitlement(EntitlementSetEnum _dlcSet, Action<EntitlementSetEnum> _onDlcFetched = null)
	{
		if (_dlcSet != EntitlementSetEnum.None)
		{
			if (!entitlementMap.ContainsKey(_dlcSet))
			{
				Log.Warning($"[XBL] DLC map missing entry for DLC Set {_dlcSet}");
			}
			else if (StartStoreOperation())
			{
				SDK.XStoreAcquireLicenseForDurablesAsync(storeContext, entitlementMap[_dlcSet], licenseAcquired);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void licenseAcquired(int hr, XStoreLicense license)
		{
			CompleteStoreOperation();
			if (license != null && SDK.XStoreIsLicenseValid(license))
			{
				lock (lockObj)
				{
					ownedEntitlements.Add(entitlementMap[_dlcSet]);
				}
				_onDlcFetched?.Invoke(_dlcSet);
			}
		}
	}

	public bool IsAvailableOnPlatform(EntitlementSetEnum _dlcSet)
	{
		if (_dlcSet != EntitlementSetEnum.None)
		{
			return entitlementMap.ContainsKey(_dlcSet);
		}
		return true;
	}

	public bool IsEntitlementPurchasable(EntitlementSetEnum _dlcSet)
	{
		DLCEnvironmentFlags dlcEnvironments = DLCEnvironmentFlags.None;
		string text = ((user != null) ? user.SandboxHelper.SandboxId : null);
		if (text == null)
		{
			Log.Warning(string.Format("[XBL] {0} no sandbox id. Defaulting to {1}", "DLCEnvironmentFlags", DLCEnvironmentFlags.None));
		}
		else
		{
			dlcEnvironments = XblSandboxHelper.SandboxIdToDLCEnvironment(text);
		}
		return DLCTitleStorageManager.Instance.IsDLCPurchasable(_dlcSet, dlcEnvironments);
	}

	public bool HasEntitlement(EntitlementSetEnum _dlcSet)
	{
		if (_dlcSet == EntitlementSetEnum.None)
		{
			return true;
		}
		if (!entitlementMap.ContainsKey(_dlcSet))
		{
			return false;
		}
		lock (lockObj)
		{
			return ownedEntitlements.Contains(entitlementMap[_dlcSet]);
		}
	}

	public bool OpenStore(EntitlementSetEnum _dlcSet, Action<EntitlementSetEnum> _onDlcPurchased)
	{
		if (!entitlementMap.ContainsKey(_dlcSet))
		{
			return false;
		}
		string dlcId = entitlementMap[_dlcSet];
		if (!StartStoreOperation())
		{
			return true;
		}
		SDK.XStoreShowPurchaseUIAsync(storeContext, dlcId, null, null, OnStoreClosed);
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		void CheckLicenseResult(int hresult, XStoreCanAcquireLicenseResult result)
		{
			CompleteStoreOperation();
			if (result.Status == XStoreCanLicenseStatus.Licensable)
			{
				lock (lockObj)
				{
					ownedEntitlements.Add(dlcId);
				}
				ThreadManager.AddSingleTaskMainThread("OnDlcPurchased", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
				{
					_onDlcPurchased?.Invoke(_dlcSet);
				});
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void OnStoreClosed(int hr)
		{
			try
			{
				if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
				{
					lock (lockObj)
					{
						ownedEntitlements.Add(dlcId);
					}
					ThreadManager.AddSingleTaskMainThread("OnDlcPurchased", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
					{
						_onDlcPurchased?.Invoke(_dlcSet);
					});
				}
				else
				{
					StartStoreOperation();
					SDK.XStoreCanAcquireLicenseForStoreIdAsync(storeContext, dlcId, CheckLicenseResult);
				}
			}
			finally
			{
				CompleteStoreOperation();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StartStoreOperation()
	{
		lock (lockObj)
		{
			remainingStoreOperations++;
			if (storeContext == null)
			{
				int num = SDK.XStoreCreateContext(null, out storeContext);
				if (Unity.XGamingRuntime.Interop.HR.FAILED(num))
				{
					remainingStoreOperations--;
					Log.Error($"Failed to create store context with error {num}.");
					return false;
				}
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteStoreOperation()
	{
		lock (lockObj)
		{
			if (remainingStoreOperations > 0)
			{
				remainingStoreOperations--;
			}
			if (remainingStoreOperations == 0 && storeContext != null)
			{
				SDK.XStoreCloseContextHandle(storeContext);
				storeContext = null;
			}
		}
	}
}
