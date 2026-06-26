using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool voiceActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDead;

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "partyvisible":
			value = ((player != null) ? (player.Party != null && !playerDead).ToString() : "false");
			return true;
		case "isleader":
			value = ((player != null) ? (player.Party != null && player.Party.Leader == player).ToString() : "false");
			return true;
		case "voicevisible":
			value = GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled).ToString();
			return true;
		case "voiceactive":
			value = voiceActive.ToString();
			return true;
		default:
			return false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		player = base.xui.playerUI.entityPlayer;
		RefreshBindings(_forceAll: true);
		player.PartyJoined += Player_PartyChanged;
		player.PartyChanged += Player_PartyChanged;
		player.PartyLeave += Player_PartyChanged;
		playerDead = base.xui.playerUI.entityPlayer.IsDead();
	}

	public override void OnClose()
	{
		base.OnClose();
		player.PartyJoined -= Player_PartyChanged;
		player.PartyChanged -= Player_PartyChanged;
		player.PartyLeave -= Player_PartyChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.time > updateTime && player != null)
		{
			updateTime = Time.time + 1f;
			bool flag = player.IsDead();
			if (flag != playerDead)
			{
				playerDead = flag;
				RefreshBindings(_forceAll: true);
			}
		}
		updateVoiceState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Player_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceState()
	{
		if (!GameManager.IsDedicatedServer && GameStats.GetInt(EnumGameStats.GameState) == 1 && !(player == null))
		{
			bool controlKeyPressed = InputUtils.ControlKeyPressed;
			bool flag = player.PlayerUI.playerInput.PermanentActions.PushToTalk.IsPressed && !(GameManager.Instance.IsEditMode() && controlKeyPressed) && GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled) && !player.PlayerUI.windowManager.IsInputActive();
			if (flag != voiceActive)
			{
				voiceActive = flag;
				RefreshBindings();
			}
		}
	}
}
