using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCreateWebUser : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string registrationPagePath = "app/createuser";

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "createwebuser" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Create a web dashboard user account";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_senderInfo.NetworkConnection != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be executed from the in-game console.");
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_senderInfo.IsLocalGame)
			{
				string token = createToken(GamePrefs.GetString(EnumGamePrefs.PlayerName), PlatformManager.NativePlatform.User.PlatformUserId, PlatformManager.CrossplatformPlatform?.User?.PlatformUserId);
				string url = createRegistrationPageUrl(token, _isLocalOnListenServer: true);
				openUserRegistrationPage(url);
			}
			else
			{
				string token2 = createToken(_senderInfo.RemoteClientInfo.playerName, _senderInfo.RemoteClientInfo.PlatformId, _senderInfo.RemoteClientInfo.CrossplatformId);
				string s = createRegistrationPageUrl(token2);
				_senderInfo.RemoteClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(GetCommands()[0] + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(s)), _bExecute: true));
			}
		}
		else if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Missing URL in server reply");
		}
		else
		{
			string url2 = Encoding.UTF8.GetString(Convert.FromBase64String(_params[0]));
			openUserRegistrationPage(url2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openUserRegistrationPage(string _url)
	{
		GameManager.Instance.SetConsoleWindowVisible(_b: false);
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(LocalPlayerUI.GetUIForPrimaryPlayer().xui, _url, _modal: true, Utils.OpenSystemBrowser, null, Localization.Get("xuiOpenUserCreationConfirmationText"));
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Requested browser for user creation at " + _url);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string createRegistrationPageUrl(string _token, bool _isLocalOnListenServer = false)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.WebDashboardPort);
		string text;
		if (_isLocalOnListenServer)
		{
			text = $"http://localhost:{num}/";
		}
		else
		{
			string text2 = GamePrefs.GetString(EnumGamePrefs.WebDashboardUrl);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text2 + ((text2[text2.Length - 1] == '/') ? "" : "/");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Server does not specify an explicit WebDashboardUrl, using game server's public IP");
				text = $"http://{SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.IP)}:{num}/";
			}
		}
		return text + "app/createuser?token=" + _token;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string createToken(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
	{
		return "-requires WebDashboard code-";
	}
}
