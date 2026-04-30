using Discord.Sdk;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordVoiceControls : XUiController
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.LobbyStateChanged += onLobbyStateChanged;
		DiscordManager.Instance.CallChanged += onCallChanged;
		DiscordManager.Instance.CallStatusChanged += onCallStatusChanged;
		DiscordManager.Instance.SelfMuteStateChanged += onSelfMuteStateChanged;
		DiscordManager.Instance.VoiceStateChanged += onVoiceStateChanged;
		if (GetChildById("btnMuteMic")?.ViewComponent is XUiV_Button xUiV_Button)
		{
			xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.Mute = !DiscordManager.Instance.Mute;
			};
		}
		if (GetChildById("btnMuteOutput")?.ViewComponent is XUiV_Button xUiV_Button2)
		{
			xUiV_Button2.Controller.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.Deaf = !DiscordManager.Instance.Deaf;
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onVoiceStateChanged(bool _self, ulong _userId)
	{
		if (_self)
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onSelfMuteStateChanged(bool _selfMute, bool _selfDeaf)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLobbyStateChanged(DiscordManager.LobbyInfo _lobby, bool _isReady, bool _isJoined)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCallChanged(DiscordManager.CallInfo _newCall)
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCallStatusChanged(DiscordManager.CallInfo _call, Call.Status _callStatus)
	{
		IsDirty = true;
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
		DiscordManager instance = DiscordManager.Instance;
		switch (_bindingName)
		{
		case "in_voice":
			_value = (instance.ActiveVoiceLobby != null).ToString();
			return true;
		case "current_voice_lobby":
			_value = instance.ActiveVoiceLobby?.LobbyType.ToStringCached() ?? "None";
			return true;
		case "voice_muted":
			_value = instance.Mute.ToString();
			return true;
		case "output_muted":
			_value = instance.Deaf.ToString();
			return true;
		case "is_speaking":
			_value = instance.ActiveVoiceLobby?.VoiceCall.IsSpeaking.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
