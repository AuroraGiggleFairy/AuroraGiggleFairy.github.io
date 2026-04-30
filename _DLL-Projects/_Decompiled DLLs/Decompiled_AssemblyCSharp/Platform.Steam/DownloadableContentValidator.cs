using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

public class DownloadableContentValidator : IEntitlementValidator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<EntitlementSetEnum, uint> entitlementMap = new Dictionary<EntitlementSetEnum, uint>
	{
		{
			EntitlementSetEnum.MarauderCosmetic,
			3486400u
		},
		{
			EntitlementSetEnum.HoarderCosmetic,
			3314750u
		},
		{
			EntitlementSetEnum.DesertCosmetic,
			3635260u
		},
		{
			EntitlementSetEnum.ClassicSurvivorCosmetic,
			4206290u
		},
		{
			EntitlementSetEnum.PirateCosmetic,
			4234590u
		},
		{
			EntitlementSetEnum.ChristmasCosmetics,
			4234580u
		},
		{
			EntitlementSetEnum.HellreaverCosmetic,
			4234570u
		},
		{
			EntitlementSetEnum.ButcherCosmetic,
			4234560u
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<EntitlementSetEnum> pendingDlcChecks = new HashSet<EntitlementSetEnum>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EntitlementSetEnum, Action<EntitlementSetEnum>> dlcPurchaseCallbacks = new Dictionary<EntitlementSetEnum, Action<EntitlementSetEnum>>();

	public void Init(IPlatform _owner)
	{
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
		return DLCTitleStorageManager.Instance.IsDLCPurchasable(_dlcSet, DLCEnvironmentFlags.Dev | DLCEnvironmentFlags.Cert | DLCEnvironmentFlags.Retail);
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
		if (!XUiC_MainMenu.openedOnce)
		{
			Log.Out("[DownloadableContentValidator] Ignored, game not fully loaded yet");
			return false;
		}
		return SteamApps.BIsDlcInstalled(new AppId_t(entitlementMap[_dlcSet]));
	}

	public bool OpenStore(EntitlementSetEnum _dlcSet, Action<EntitlementSetEnum> _onDlcPurchased)
	{
		if (!entitlementMap.ContainsKey(_dlcSet))
		{
			return false;
		}
		if (!XUiC_MainMenu.openedOnce)
		{
			Log.Out("[DownloadableContentValidator] Ignored, game not fully loaded yet");
			return true;
		}
		SteamFriends.ActivateGameOverlayToStore(new AppId_t(entitlementMap[_dlcSet]), EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
		pendingDlcChecks.Add(_dlcSet);
		dlcPurchaseCallbacks[_dlcSet] = _onDlcPurchased;
		if (pendingDlcChecks.Count == 1)
		{
			ThreadManager.StartCoroutine(CheckDlcPurchases());
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CheckDlcPurchases()
	{
		while (pendingDlcChecks.Count > 0)
		{
			yield return new WaitForSeconds(2f);
			List<EntitlementSetEnum> list = new List<EntitlementSetEnum>();
			foreach (EntitlementSetEnum pendingDlcCheck in pendingDlcChecks)
			{
				if (SteamApps.BIsDlcInstalled(new AppId_t(entitlementMap[pendingDlcCheck])))
				{
					list.Add(pendingDlcCheck);
				}
			}
			foreach (EntitlementSetEnum item in list)
			{
				pendingDlcChecks.Remove(item);
				if (dlcPurchaseCallbacks.ContainsKey(item))
				{
					dlcPurchaseCallbacks[item]?.Invoke(item);
					dlcPurchaseCallbacks.Remove(item);
				}
			}
		}
	}
}
