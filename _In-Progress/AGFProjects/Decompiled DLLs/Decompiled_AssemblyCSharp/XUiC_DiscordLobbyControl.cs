using Discord.Sdk;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLobbyControl : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.ELobbyType lobbyType;

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.LobbyInfo lobby;

	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.LobbyStateChanged += onLobbyStateChanged;
		DiscordManager.Instance.CallStatusChanged += onCallStatusChanged;
		if (GetChildById("btnJoinVoice") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += btnJoinPressed;
		}
		if (GetChildById("btnLeaveVoice") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += btnLeavePressed;
		}
		if (GetChildById("btnJoinVoice")?.ViewComponent is XUiV_Button xUiV_Button)
		{
			xUiV_Button.Controller.OnPress += btnJoinPressed;
		}
		if (GetChildById("btnLeaveVoice")?.ViewComponent is XUiV_Button xUiV_Button2)
		{
			xUiV_Button2.Controller.OnPress += btnLeavePressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLobbyStateChanged(DiscordManager.LobbyInfo _lobby, bool _isReady, bool _isJoined)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCallStatusChanged(DiscordManager.CallInfo _call, Call.Status _callStatus)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnJoinPressed(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.JoinLobbyVoice(lobbyType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnLeavePressed(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.LeaveLobbyVoice();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "lobby_ready":
			_value = lobby.IsReady.ToString();
			return true;
		case "lobby_joined":
			_value = lobby.IsJoined.ToString();
			return true;
		case "voice_status":
			_value = lobby.VoiceCall.Status.ToString();
			return true;
		case "in_other_voice":
		{
			DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
			_value = (activeVoiceLobby != null && activeVoiceLobby != lobby).ToString();
			return true;
		}
		case "any_lobby_unstable_state":
			_value = DiscordManager.Instance.AnyLobbyInUnstableVoiceState.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "lobby_type")
		{
			lobbyType = EnumUtils.Parse<DiscordManager.ELobbyType>(_value, _ignoreCase: true);
			lobby = DiscordManager.Instance.GetLobby(lobbyType);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}
}
