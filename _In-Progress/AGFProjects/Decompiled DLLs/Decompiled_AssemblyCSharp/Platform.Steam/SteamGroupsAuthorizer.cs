using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

namespace Platform.Steam;

[Preserve]
public class SteamGroupsAuthorizer : AuthorizerAbs
{
	public override int Order => 470;

	public override string AuthorizerName => "SteamGroups";

	public override string StateLocalizationKey => "authstate_steamgroups";

	public override EPlatformIdentifier PlatformRestriction => EPlatformIdentifier.Steam;

	public override bool AuthorizerActive
	{
		get
		{
			if (GameManager.Instance.adminTools != null)
			{
				return PlatformManager.InstanceForPlatformIdentifier(EPlatformIdentifier.Steam) != null;
			}
			return false;
		}
	}

	public override void ServerStart()
	{
		base.ServerStart();
		PlatformManager.NativePlatform.AuthenticationServer?.StartServerSteamGroups(groupStatusCallback);
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		Dictionary<string, AdminWhitelist.WhitelistGroup> groups = GameManager.Instance.adminTools.Whitelist.GetGroups();
		Dictionary<string, AdminUsers.GroupPermission> groups2 = GameManager.Instance.adminTools.Users.GetGroups();
		if (groups.Count == 0 && groups2.Count == 0)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		EPlatformIdentifier platformIdentifier = _clientInfo.PlatformId.PlatformIdentifier;
		IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(platformIdentifier);
		if (platform == null)
		{
			string customReason = platformIdentifier.ToStringCached();
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.UnsupportedPlatform, 0, default(DateTime), customReason));
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		groups.CopyKeysTo(hashSet);
		groups2.CopyKeysTo(hashSet);
		_clientInfo.groupMembershipsWaiting = hashSet.Count;
		foreach (string item in hashSet)
		{
			if (!platform.AuthenticationServer.RequestUserInGroupStatus(_clientInfo, item))
			{
				Interlocked.Decrement(ref _clientInfo.groupMembershipsWaiting);
			}
		}
		if (_clientInfo.groupMembershipsWaiting == 0)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		return (EAuthorizerSyncResult.WaitAsync, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void groupStatusCallback(ClientInfo _clientInfo, ulong _groupId, bool _member, bool _officer)
	{
		int num = Interlocked.Decrement(ref _clientInfo.groupMembershipsWaiting);
		if (_member)
		{
			_clientInfo.groupMemberships[_groupId.ToString()] = ((!_officer) ? 1 : 2);
		}
		if (num == 0)
		{
			authResponsesHandler.AuthorizationAccepted(this, _clientInfo);
		}
	}
}
