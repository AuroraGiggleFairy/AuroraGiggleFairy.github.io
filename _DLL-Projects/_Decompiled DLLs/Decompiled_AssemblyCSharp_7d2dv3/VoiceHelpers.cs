using System;
using System.Diagnostics;
using Platform;

public static class VoiceHelpers
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool? snapWithoutMicPermission;

	public static bool VoiceAllowed => PermissionsManager.IsCommunicationAllowed();

	public static bool PlatformVoiceEnabled
	{
		get
		{
			if (GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled))
			{
				return VoiceAllowed;
			}
			return false;
		}
	}

	public static bool InAnyVoiceChat
	{
		get
		{
			if (VoiceAllowed)
			{
				if (DiscordManager.Instance.ActiveVoiceLobby == null)
				{
					return PartyVoice.Instance.InVoice;
				}
				return true;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool pushToTalkButtonValid(LocalPlayerUI _playerUI)
	{
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		bool flag = _playerUI.windowManager.IsInputActive();
		bool flag2 = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen();
		if (!(GameManager.Instance.IsEditMode() && controlKeyPressed) && !flag)
		{
			return !flag2;
		}
		return false;
	}

	public static bool PushToTalkPressed()
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (uIForPrimaryPlayer == null || uIForPrimaryPlayer.playerInput == null)
		{
			return false;
		}
		if (uIForPrimaryPlayer.playerInput.PermanentActions.PushToTalk.IsPressed)
		{
			return pushToTalkButtonValid(uIForPrimaryPlayer);
		}
		return false;
	}

	public static bool PushToTalkWasPressed()
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (uIForPrimaryPlayer == null || uIForPrimaryPlayer.playerInput == null)
		{
			return false;
		}
		if (uIForPrimaryPlayer.playerInput.PermanentActions.PushToTalk.WasPressed)
		{
			return pushToTalkButtonValid(uIForPrimaryPlayer);
		}
		return false;
	}

	public static bool LocalPlayerTalking()
	{
		if (!PartyVoice.Instance.SendingVoice)
		{
			if (DiscordManager.Instance.IsReady)
			{
				return DiscordManager.Instance.ActiveVoiceLobby?.VoiceCall.IsSpeaking ?? false;
			}
			return false;
		}
		return true;
	}

	public static IPartyVoice.EVoiceMemberState GetPlayerVoiceState(EntityPlayer _player, bool _partyOnly = false)
	{
		if (_player == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		IPartyVoice.EVoiceMemberState playerDiscordVoiceState = GetPlayerDiscordVoiceState(_player, _partyOnly);
		IPartyVoice.EVoiceMemberState eVoiceMemberState;
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled))
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_player.entityId);
			eVoiceMemberState = ((playerDataFromEntityID != null) ? PartyVoice.Instance.GetVoiceMemberState(playerDataFromEntityID.PrimaryId) : IPartyVoice.EVoiceMemberState.Disabled);
		}
		else
		{
			eVoiceMemberState = IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (playerDiscordVoiceState < eVoiceMemberState)
		{
			return eVoiceMemberState;
		}
		return playerDiscordVoiceState;
	}

	public static IPartyVoice.EVoiceMemberState GetPlayerDiscordVoiceState(EntityPlayer _player, bool _partyOnly = false)
	{
		if (_player == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (!DiscordManager.Instance.IsReady)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
		if (activeVoiceLobby == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (!activeVoiceLobby.IsInVoice)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (_partyOnly && activeVoiceLobby.LobbyType != DiscordManager.ELobbyType.Party)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (!DiscordManager.Instance.TryGetUserFromEntity(_player, out var _user) || !_user.InCurrentVoice)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		return _user.VoiceState;
	}

	public static bool IsSnapWithoutMicPermission()
	{
		if (snapWithoutMicPermission.HasValue)
		{
			return snapWithoutMicPermission.Value;
		}
		if (GameIO.IsRunningAsSnap() == null)
		{
			snapWithoutMicPermission = false;
			return false;
		}
		snapWithoutMicPermission = !SnapHasMicPermission();
		return snapWithoutMicPermission.Value;
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool SnapHasMicPermission()
		{
			ProcessStartInfo processStartInfo = ((!GameIO.IsRunningInSteamRuntime()) ? new ProcessStartInfo
			{
				FileName = "/usr/bin/snapctl",
				Arguments = "is-connected audio-record"
			} : new ProcessStartInfo
			{
				FileName = "/usr/bin/steam-runtime-launch-client",
				Arguments = " --alongside-steam -- /usr/bin/snapctl is-connected audio-record"
			});
			processStartInfo.CreateNoWindow = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;
			try
			{
				Process process = Process.Start(processStartInfo);
				if (process == null)
				{
					Log.Out("Snap microphone permission check: Could not run snapctl");
					return false;
				}
				string text = process.StandardError.ReadToEnd();
				process.WaitForExit(150);
				if (!process.HasExited)
				{
					process.Kill();
					Log.Error("Snap microphone permission check: snapctl did not terminate");
					return false;
				}
				bool flag = process.ExitCode == 0;
				Log.Out($"Snap microphone permission check done: Has permission: {flag}");
				if (text.Length > 5)
				{
					Log.Out("ErrOut: " + text);
				}
				return flag;
			}
			catch (Exception ex)
			{
				Log.Error("Snap microphone permission check failed with exception: " + ex.Message);
				return false;
			}
		}
	}
}
