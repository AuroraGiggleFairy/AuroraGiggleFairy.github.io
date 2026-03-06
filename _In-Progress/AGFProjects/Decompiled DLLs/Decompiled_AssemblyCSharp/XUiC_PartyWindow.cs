using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float ForcedRefreshTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool voiceActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDead;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "partyvisible":
			_value = ((player != null) ? (player.Party != null && !playerDead).ToString() : "false");
			return true;
		case "isleader":
			_value = ((player != null) ? (player.Party != null && player.Party.Leader == player).ToString() : "false");
			return true;
		case "voicevisible":
			_value = VoiceHelpers.InAnyVoiceChat.ToString();
			return true;
		case "voiceactive":
			_value = voiceActive.ToString();
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
		if (!(player == null))
		{
			float time = Time.time;
			bool num = time > updateTime;
			bool flag = player.IsDead();
			bool flag2 = flag != playerDead;
			playerDead = flag;
			if (num || flag2 || updateVoiceState())
			{
				updateTime = time + 1f;
				RefreshBindings(_forceAll: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Player_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateVoiceState()
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		bool flag = VoiceHelpers.LocalPlayerTalking();
		if (flag == voiceActive)
		{
			return false;
		}
		voiceActive = flag;
		return true;
	}
}
