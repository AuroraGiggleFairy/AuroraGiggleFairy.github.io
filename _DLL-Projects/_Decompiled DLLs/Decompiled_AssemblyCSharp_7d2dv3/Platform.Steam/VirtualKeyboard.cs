using System;
using System.Collections;
using Steamworks;

namespace Platform.Steam;

public class VirtualKeyboard : IVirtualKeyboard
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GamepadTextInputDismissed_t> m_TextInputDismissed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<bool, string> onTextReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public string textBefore;

	public void Init(IPlatform _owner)
	{
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!GameManager.IsDedicatedServer)
			{
				if (SteamUtils.IsSteamInBigPictureMode())
				{
					if (m_TextInputDismissed == null)
					{
						m_TextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(GamePadTextInputDismissed_Callback);
					}
				}
				else
				{
					Log.Out("Not running in Big Picture Mode, no on-screen keyboard available");
				}
			}
		};
	}

	public string Open(string _title, string _defaultText, Action<bool, string> _onTextReceived, UIInput.InputType _mode = UIInput.InputType.Standard, bool _multiLine = false, uint singleLineLength = 200u)
	{
		if (onTextReceived != null)
		{
			Log.Warning("The virtual keyboard was already opened and has not closed yet.");
			return null;
		}
		if (_onTextReceived == null)
		{
			throw new ArgumentException("The callback function must not be null");
		}
		textBefore = _defaultText;
		onTextReceived = _onTextReceived;
		if (SteamUtils.ShowGamepadTextInput((_mode == UIInput.InputType.Password) ? EGamepadTextInputMode.k_EGamepadTextInputModePassword : EGamepadTextInputMode.k_EGamepadTextInputModeNormal, _multiLine ? EGamepadTextInputLineMode.k_EGamepadTextInputLineModeMultipleLines : EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, _title, _multiLine ? 500u : singleLineLength, _defaultText))
		{
			return null;
		}
		Log.Out("Opening OnScreen keyboard failed, probably not running in Steam Big Picture Mode");
		onTextReceived(arg1: false, _defaultText);
		onTextReceived = null;
		return Localization.Get("ttSteamBPM");
	}

	public void Destroy()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GamePadTextInputDismissed_Callback(GamepadTextInputDismissed_t _result)
	{
		Action<bool, string> action = onTextReceived;
		onTextReceived = null;
		string pchText;
		bool enteredGamepadTextInput = SteamUtils.GetEnteredGamepadTextInput(out pchText, 500u);
		Log.Out("OnScreen keyboard result: ok={0}, submitted={1}, text={2}", enteredGamepadTextInput, _result.m_bSubmitted, pchText);
		action?.Invoke(_result.m_bSubmitted, _result.m_bSubmitted ? pchText : (textBefore ?? ""));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator debugOut()
	{
		int i = 0;
		while (i < 100)
		{
			Log.Out("POST: Enabled={3}, Is={0}, Down={1}, Up={2}", PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.IsPressed, PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasPressed, PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasReleased, PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.Enabled);
			i++;
			yield return null;
		}
	}
}
