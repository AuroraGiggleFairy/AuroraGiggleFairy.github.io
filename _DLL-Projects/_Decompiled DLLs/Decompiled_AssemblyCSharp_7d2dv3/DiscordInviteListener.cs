using System;
using System.Collections;
using Platform;

public class DiscordInviteListener : IJoinSessionGameInviteListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static DiscordInviteListener listenerInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public (string session, string password)? pendingActivityInvite;

	public static DiscordInviteListener ListenerInstance => listenerInstance ?? (listenerInstance = new DiscordInviteListener());

	public void Init(IPlatform _owner)
	{
	}

	public void SetPendingInvite(string _sessionId, string _password)
	{
		pendingActivityInvite = (_sessionId, _password);
	}

	public (string invite, string password) TakePendingInvite()
	{
		if (!pendingActivityInvite.HasValue)
		{
			return (invite: null, password: null);
		}
		(string session, string password) value = pendingActivityInvite.Value;
		pendingActivityInvite = null;
		return value;
	}

	public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
	{
		yield return InviteManager.HandleSessionIdInvite(_invite, _password, _onFinished);
	}

	public string GetListenerIdentifier()
	{
		return "DCD";
	}
}
