using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordWindow : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("btnLoginAccount") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.AuthManager.LoginDiscordUser();
			};
		}
		if (GetChildById("btnLoginProvisional") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.AuthManager.LoginProvisionalAccount();
			};
		}
		if (GetChildById("btnDisconnect") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.AuthManager.Disconnect();
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		RefreshBindingsSelfAndChildren();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		DiscordManager instance = DiscordManager.Instance;
		switch (_bindingName)
		{
		case "status":
			_value = instance.Status.ToStringCached();
			return true;
		case "supports_full_accounts":
			_value = DiscordManager.SupportsFullAccounts.ToString();
			return true;
		case "supports_provisional_accounts":
			_value = DiscordManager.SupportsProvisionalAccounts.ToString();
			return true;
		case "is_ready":
			_value = instance.IsReady.ToString();
			return true;
		case "displayname":
			_value = instance.LocalUser?.DisplayName ?? "";
			return true;
		case "discorddisplayname":
			_value = instance.LocalUser?.DiscordDisplayName ?? "";
			return true;
		case "discordusername":
			_value = instance.LocalUser?.DiscordUserName ?? "";
			return true;
		case "userid":
		{
			DiscordManager.DiscordUser localUser = instance.LocalUser;
			object obj;
			if (localUser == null)
			{
				obj = null;
			}
			else
			{
				ulong iD = localUser.ID;
				obj = iD.ToString();
			}
			if (obj == null)
			{
				obj = "";
			}
			_value = (string)obj;
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
