using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float ForcedRefreshTime = 1f;

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
			_value = ((xui.playerUI.entityPlayer != null) ? (xui.playerUI.entityPlayer.Party != null && !playerDead).ToString() : "false");
			return true;
		case "isleader":
			_value = ((xui.playerUI.entityPlayer != null) ? (xui.playerUI.entityPlayer.Party != null && xui.playerUI.entityPlayer.Party.Leader == xui.playerUI.entityPlayer).ToString() : "false");
			return true;
		case "voicevisible":
			_value = VoiceHelpers.InAnyVoiceChat.ToString();
			return true;
		case "voiceactive":
			_value = voiceActive.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.entityPlayer.PartyJoined += Player_PartyChanged;
		xui.playerUI.entityPlayer.PartyChanged += Player_PartyChanged;
		xui.playerUI.entityPlayer.PartyLeave += Player_PartyChanged;
		playerDead = xui.playerUI.entityPlayer.IsDead();
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.entityPlayer.PartyJoined -= Player_PartyChanged;
		xui.playerUI.entityPlayer.PartyChanged -= Player_PartyChanged;
		xui.playerUI.entityPlayer.PartyLeave -= Player_PartyChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!(xui.playerUI.entityPlayer == null))
		{
			float time = Time.time;
			bool num = time > updateTime;
			bool flag = xui.playerUI.entityPlayer.IsDead();
			bool flag2 = flag != playerDead;
			playerDead = flag;
			if (num || flag2 || updateVoiceState())
			{
				updateTime = time + 1f;
				RefreshBindings();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Player_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		RefreshBindings();
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
		if (xui.playerUI.entityPlayer == null)
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
