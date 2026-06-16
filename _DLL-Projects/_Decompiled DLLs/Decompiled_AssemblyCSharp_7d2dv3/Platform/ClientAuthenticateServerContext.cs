namespace Platform;

public sealed class ClientAuthenticateServerContext
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ClientAuthenticateServerSuccessDelegate success;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ClientAuthenticateServerDisconnectDelegate disconnect;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo GameServerInfo { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs PlatformUserId { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs CrossplatformUserId { get; }

	public ClientAuthenticateServerContext(GameServerInfo _gameServerInfo, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossplatformUserId, ClientAuthenticateServerSuccessDelegate _success, ClientAuthenticateServerDisconnectDelegate _disconnect)
	{
		GameServerInfo = _gameServerInfo;
		PlatformUserId = _platformUserId;
		CrossplatformUserId = _crossplatformUserId;
		success = _success;
		disconnect = _disconnect;
	}

	public void Success()
	{
		success?.Invoke();
	}

	public void DisconnectNoCrossplay()
	{
		string reason = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay) ?? Localization.Get("auth_noCrossplay");
		disconnect?.Invoke(reason);
	}

	public void DisconnectNoCrossplay(EPlayGroup otherPlayGroup)
	{
		disconnect?.Invoke(string.Format(Localization.Get("auth_noCrossplayBetween"), EPlayGroupExtensions.Current, otherPlayGroup));
	}
}
