using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Platform;

public class PermissionsManager
{
	[Flags]
	public enum PermissionSources
	{
		Platform = 1,
		GamePrefs = 2,
		LaunchPrefs = 4,
		DebugMask = 8,
		TitleStorage = 0x10,
		All = 0x1F
	}

	public static EUserPerms DebugPermissionsMask = EUserPerms.All;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TitleStorageOverridesManager.TSOverrides tsOverrides = default(TitleStorageOverridesManager.TSOverrides);

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool resolvingPermissions;

	public static EUserPerms GetPermissions(PermissionSources _sources = PermissionSources.All)
	{
		EUserPerms eUserPerms = EUserPerms.All;
		if (_sources.HasFlag(PermissionSources.Platform) && !GameManager.IsDedicatedServer)
		{
			eUserPerms &= PlatformManager.MultiPlatform.User.Permissions;
		}
		if (_sources.HasFlag(PermissionSources.GamePrefs))
		{
			if (eUserPerms.HasFlag(EUserPerms.Communication) && !GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication))
			{
				eUserPerms &= ~EUserPerms.Communication;
			}
			if (eUserPerms.HasFlag(EUserPerms.Crossplay) && !GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay))
			{
				eUserPerms &= ~EUserPerms.Crossplay;
			}
		}
		if (_sources.HasFlag(PermissionSources.LaunchPrefs) && eUserPerms.HasFlag(EUserPerms.Crossplay) && !LaunchPrefs.AllowCrossplay.Value)
		{
			eUserPerms &= ~EUserPerms.Crossplay;
		}
		if (_sources.HasFlag(PermissionSources.DebugMask))
		{
			eUserPerms &= DebugPermissionsMask;
		}
		if (_sources.HasFlag(PermissionSources.TitleStorage) && eUserPerms.HasFlag(EUserPerms.Crossplay) && !tsOverrides.Crossplay)
		{
			eUserPerms &= ~EUserPerms.Crossplay;
		}
		return eUserPerms;
	}

	public static IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4})", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
		yield return null;
		if (_cancellationToken?.IsCancelled() ?? false)
		{
			yield break;
		}
		bool needsWait = resolvingPermissions;
		if (needsWait)
		{
			Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Waiting on existing resolve...", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
			while (resolvingPermissions)
			{
				yield return null;
				if (_cancellationToken?.IsCancelled() ?? false)
				{
					yield break;
				}
			}
		}
		bool tsFetchComplete;
		try
		{
			resolvingPermissions = true;
			if (needsWait)
			{
				Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Finished waiting. Executing resolve.", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
			}
			tsFetchComplete = false;
			if (_perms.HasCrossplay())
			{
				Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Fetching Title Storage Overrides...", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
				TitleStorageOverridesManager.Instance.FetchFromSource(FetchComplete);
			}
			else
			{
				tsFetchComplete = true;
			}
			if (!GameManager.IsDedicatedServer)
			{
				yield return PlatformManager.MultiPlatform.User.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
				if (_cancellationToken?.IsCancelled() ?? false)
				{
					resolvingPermissions = false;
					yield break;
				}
			}
			while (!tsFetchComplete && !(_cancellationToken?.IsCancelled() ?? false))
			{
				yield return null;
			}
		}
		finally
		{
			resolvingPermissions = false;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void FetchComplete(TitleStorageOverridesManager.TSOverrides _overrides)
		{
			Log.Out(string.Format("[PermissionsManager] {0}({1}: [{2}], {3}: {4}) Fetched Title Storage Overrides!", "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt));
			tsFetchComplete = true;
			tsOverrides = _overrides;
		}
	}

	public static string GetPermissionDenyReason(EUserPerms _perms, PermissionSources _sources = PermissionSources.All)
	{
		if (_sources.HasFlag(PermissionSources.GamePrefs))
		{
			if (_perms.HasFlag(EUserPerms.Communication) && !GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication))
			{
				return Localization.Get("permissionsMissing_communication");
			}
			if (_perms.HasFlag(EUserPerms.Crossplay) && !GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay))
			{
				return Localization.Get("permissionsMissing_crossplay");
			}
		}
		if (_sources.HasFlag(PermissionSources.LaunchPrefs) && _perms.HasFlag(EUserPerms.Crossplay) && !LaunchPrefs.AllowCrossplay.Value)
		{
			return Localization.Get("auth_noCrossplay");
		}
		if (_sources.HasFlag(PermissionSources.TitleStorage) && _perms.HasFlag(EUserPerms.Crossplay) && !tsOverrides.Crossplay)
		{
			return Localization.Get("auth_noCrossplayOverridden");
		}
		if (_sources.HasFlag(PermissionSources.Platform) && !GameManager.IsDedicatedServer)
		{
			string permissionDenyReason = PlatformManager.MultiPlatform.User.GetPermissionDenyReason(_perms);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				return permissionDenyReason;
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAllowed(EUserPerms _checkPerms)
	{
		return GetPermissions().HasFlag(_checkPerms);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMultiplayerAllowed()
	{
		return GetPermissions().HasMultiplayer();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCommunicationAllowed()
	{
		return GetPermissions().HasCommunication();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCrossplayAllowed()
	{
		return GetPermissions().HasCrossplay();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanHostMultiplayer()
	{
		EUserPerms permissions = GetPermissions();
		if (permissions.HasMultiplayer())
		{
			return permissions.HasHostMultiplayer();
		}
		return false;
	}
}
