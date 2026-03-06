using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.XGamingRuntime;

namespace Platform.XBL;

[PublicizedFrom(EAccessModifier.Internal)]
public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex msInviteUriMatcher = new Regex("^ms-xbl-(\\w+):\\/\\/(\\w+)\\/?\\?(.*)$", RegexOptions.Compiled);

	public void Init(IPlatform _owner)
	{
		PlatformManager.NativePlatform.User.UserLoggedIn += [PublicizedFrom(EAccessModifier.Private)] (IPlatform _platform) =>
		{
			XblHelpers.Succeeded(SDK.XGameInviteRegisterForEvent(InviteReceivedCallback, out var _), "Register for invite event", _logToConsole: true, _printSuccess: true);
		};
	}

	public (string invite, string password) TakePendingInvite()
	{
		string item = pendingInvite;
		pendingInvite = null;
		string item2 = pendingPassword;
		pendingPassword = null;
		return (invite: item, password: item2);
	}

	public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
	{
		yield return InviteManager.HandleSessionIdInvite(_invite, _password, _onFinished);
	}

	public string GetListenerIdentifier()
	{
		return "XBL";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InviteReceivedCallback(IntPtr _, string _inviteuri)
	{
		Log.Out("[XBL] Invite received: '" + _inviteuri + "'");
		string text = ParseInviteUri(_inviteuri);
		if (text == null)
		{
			Log.Error("[XBL] Received invite but could not extract connect information");
		}
		else
		{
			pendingInvite = text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string ParseInviteUri(string _inviteUri)
	{
		Match match = msInviteUriMatcher.Match(_inviteUri);
		if (!match.Success)
		{
			return null;
		}
		string[] array = match.Groups[3].Value.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2[0].EqualsCaseInsensitive("connectionString"))
			{
				return array2[1];
			}
		}
		return null;
	}
}
