using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float ForcedRefreshTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastStaminaValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite arrowContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealthModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStaminaModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	public string defaultHealthColor = "255,0,0,128";

	public string twitchHealthColor = "100,65,165,128";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite iconSprite1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite iconSprite2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color defaultIconColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 iconSpriteSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color iconBlinkColor = new Color32(byte.MaxValue, 180, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float arrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastArrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPartyVoice.EVoiceMemberState voiceState;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool oldTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool oldSafe;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer.TwitchActionsStates oldTwitchActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt healthcurrentFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> healthcurrentWMaxFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i, int _i2) => $"{_i}/{_i2}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat healthfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> distanceFormatter = new CachedStringFormatter<float>(ValueDisplayFormatters.Distance);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor arrowcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public string deathIcon = "ui_game_symbol_death";

	public string leaderIcon = "server_favorite";

	public string twitchActiveIcon = "ui_game_symbol_twitch_actions";

	public string twitchDisabledIcon = "ui_game_symbol_twitch_action_disabled";

	public string twitchSafeIcon = "ui_game_symbol_brick";

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer Player { get; set; }

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		XUiController[] childrenById = GetChildrenById("BarHealth");
		if (childrenById != null)
		{
			barHealth = new XUiV_Sprite[childrenById.Length];
			for (int i = 0; i < childrenById.Length; i++)
			{
				barHealth[i] = (XUiV_Sprite)childrenById[i].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarHealthModifiedMax");
		if (childrenById != null)
		{
			barHealthModifiedMax = new XUiV_Sprite[childrenById.Length];
			for (int j = 0; j < childrenById.Length; j++)
			{
				barHealthModifiedMax[j] = (XUiV_Sprite)childrenById[j].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarStamina");
		if (childrenById != null)
		{
			barStamina = new XUiV_Sprite[childrenById.Length];
			for (int k = 0; k < childrenById.Length; k++)
			{
				barStamina[k] = (XUiV_Sprite)childrenById[k].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarStaminaModifiedMax");
		if (childrenById != null)
		{
			barStaminaModifiedMax = new XUiV_Sprite[childrenById.Length];
			for (int l = 0; l < childrenById.Length; l++)
			{
				barStaminaModifiedMax[l] = (XUiV_Sprite)childrenById[l].ViewComponent;
			}
		}
		XUiController childById = GetChildById("arrowContent");
		if (childById != null)
		{
			arrowContent = (XUiV_Sprite)childById.ViewComponent;
		}
		XUiController childById2 = GetChildById("icon1");
		if (childById2 != null)
		{
			iconSprite1 = (XUiV_Sprite)childById2.ViewComponent;
			defaultIconColor = iconSprite1.Color;
			iconSpriteSize = new Vector2(iconSprite1.Sprite.width, iconSprite1.Sprite.height);
		}
		childById2 = GetChildById("icon2");
		if (childById2 != null)
		{
			iconSprite2 = (XUiV_Sprite)childById2.ViewComponent;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Player == null || !XUi.IsGameRunning())
		{
			return;
		}
		RefreshFill();
		updateVoiceState();
		if (Time.time > updateTime)
		{
			updateTime = Time.time + 0.5f;
			if (HasChanged() || IsDirty)
			{
				if (IsDirty)
				{
					RefreshBindings(_forceAll: true);
					IsDirty = false;
				}
				else
				{
					RefreshBindings();
				}
			}
		}
		if (Player != null && arrowContent != null)
		{
			arrowRotation = ReturnRotation(base.xui.playerUI.entityPlayer, Player);
			if (lastArrowRotation < 15f && arrowRotation > 345f)
			{
				lastArrowRotation = arrowRotation;
			}
			else if (lastArrowRotation > 345f && arrowRotation < 15f)
			{
				lastArrowRotation = arrowRotation;
			}
			else
			{
				lastArrowRotation = Mathf.Lerp(lastArrowRotation, arrowRotation, _dt * 3f);
			}
			arrowContent.UiTransform.localEulerAngles = new Vector3(0f, 0f, lastArrowRotation - 180f);
		}
		if (!(Player != null))
		{
			return;
		}
		if (Player.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabledEnding)
		{
			float num = Mathf.PingPong(Time.time, 0.5f);
			float num2 = 1f;
			if (num > 0.25f)
			{
				num2 = 1f + num - 0.25f;
			}
			if (Player.Party.Leader == Player)
			{
				iconSprite2.Color = Color.Lerp(defaultIconColor, iconBlinkColor, num * 4f);
				iconSprite2.Sprite.SetDimensions((int)(iconSpriteSize.x * num2), (int)(iconSpriteSize.y * num2));
			}
			else
			{
				iconSprite1.Color = Color.Lerp(defaultIconColor, iconBlinkColor, num * 4f);
				iconSprite1.Sprite.SetDimensions((int)(iconSpriteSize.x * num2), (int)(iconSpriteSize.y * num2));
			}
		}
		else
		{
			iconSprite1.Color = defaultIconColor;
			iconSprite2.Color = defaultIconColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceState()
	{
		IPartyVoice.EVoiceMemberState playerVoiceState = VoiceHelpers.GetPlayerVoiceState(Player, _partyOnly: true);
		if (playerVoiceState != voiceState)
		{
			voiceState = playerVoiceState;
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetPlayer(EntityPlayer player)
	{
		Player = player;
		if (Player == null)
		{
			RefreshBindings(_forceAll: true);
		}
		else
		{
			IsDirty = true;
		}
	}

	public bool HasChanged()
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		float magnitude = (Player.GetPosition() - entityPlayer.GetPosition()).magnitude;
		bool result = oldValue != magnitude || Player.TwitchEnabled != oldTwitch || Player.TwitchSafe != oldSafe || Player.TwitchActionsEnabled != oldTwitchActions;
		oldValue = magnitude;
		distance = magnitude;
		oldTwitch = Player.TwitchEnabled;
		oldSafe = Player.TwitchSafe;
		oldTwitchActions = Player.TwitchActionsEnabled;
		return result;
	}

	public void RefreshFill()
	{
		if (Player == null)
		{
			return;
		}
		float t = Time.deltaTime * 3f;
		if (barHealth != null)
		{
			float valuePercentUI = Player.Stats.Health.ValuePercentUI;
			float fill = Math.Max(lastHealthValue, 0f) * 1.01f;
			lastHealthValue = Mathf.Lerp(lastHealthValue, valuePercentUI, t);
			for (int i = 0; i < barHealth.Length; i++)
			{
				barHealth[i].Fill = fill;
			}
		}
		if (barHealthModifiedMax != null)
		{
			for (int j = 0; j < barHealthModifiedMax.Length; j++)
			{
				barHealthModifiedMax[j].Fill = Player.Stats.Health.ModifiedMax / Player.Stats.Health.Max;
			}
		}
		if (barStamina != null)
		{
			float valuePercentUI2 = Player.Stats.Stamina.ValuePercentUI;
			float fill2 = Math.Max(lastStaminaValue, 0f) * 1.01f;
			lastStaminaValue = Mathf.Lerp(lastStaminaValue, valuePercentUI2, t);
			for (int k = 0; k < barStamina.Length; k++)
			{
				barStamina[k].Fill = fill2;
			}
		}
		if (barStaminaModifiedMax != null)
		{
			for (int l = 0; l < barStaminaModifiedMax.Length; l++)
			{
				barStaminaModifiedMax[l].Fill = Player.Stats.Stamina.ModifiedMax / Player.Stats.Stamina.Max;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "healthcurrent":
			if (Player == null)
			{
				value = "";
				return true;
			}
			value = healthcurrentFormatter.Format(Player.Health);
			return true;
		case "healthcurrentwithmax":
			if (Player == null)
			{
				value = "";
				return true;
			}
			value = healthcurrentWMaxFormatter.Format(Player.Health, Player.GetMaxHealth());
			return true;
		case "healthfill":
		{
			if (Player == null)
			{
				value = "0";
				return true;
			}
			float valuePercentUI = Player.Stats.Health.ValuePercentUI;
			value = healthfillFormatter.Format(valuePercentUI);
			return true;
		}
		case "healthmodifiedmax":
			if (Player == null || base.xui.playerUI.entityPlayer.IsDead())
			{
				value = "0";
				return true;
			}
			value = (Player.Stats.Health.ModifiedMax / Player.Stats.Health.Max).ToCultureInvariantString();
			return true;
		case "healthcolor":
			if (Player == null)
			{
				value = "";
				return true;
			}
			value = (Player.TwitchEnabled ? twitchHealthColor : defaultHealthColor);
			return true;
		case "partyvisible":
			if (Player == null || base.xui.playerUI.entityPlayer.IsDead())
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "showicon1":
			if (Player == null)
			{
				value = "false";
				return true;
			}
			value = (Player.IsPartyLead() || Player.IsDead() || ((Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || Player.TwitchSafe) && Player.HasTwitchMember())).ToString();
			return true;
		case "showicon2":
			if (Player == null)
			{
				value = "false";
				return true;
			}
			value = ((Player.IsPartyLead() || Player.IsDead()) && (Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || Player.TwitchSafe) && Player.HasTwitchMember()).ToString();
			return true;
		case "showarrow":
			if (Player == null)
			{
				value = "false";
				return true;
			}
			value = Player.IsAlive().ToString();
			return true;
		case "arrowcolor":
		{
			if (Player == null)
			{
				value = "";
				return true;
			}
			int num = Player.Party.MemberList.IndexOf(Player);
			Color32 v2 = Constants.TrackedFriendColors[num % Constants.TrackedFriendColors.Length];
			value = arrowcolorFormatter.Format(v2);
			return true;
		}
		case "icon1":
			if (Player == null || GameStats.GetBool(EnumGameStats.AutoParty))
			{
				value = "";
				return true;
			}
			if (Player.IsDead())
			{
				value = deathIcon;
			}
			else if (Player.IsPartyLead())
			{
				value = leaderIcon;
			}
			else if (Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
			{
				value = twitchDisabledIcon;
			}
			else if (Player.TwitchSafe)
			{
				value = twitchSafeIcon;
			}
			else
			{
				value = "";
			}
			return true;
		case "icon2":
			if (Player == null || GameStats.GetBool(EnumGameStats.AutoParty))
			{
				value = "";
				return true;
			}
			if (!Player.IsDead() && !Player.IsPartyLead())
			{
				value = "";
			}
			else if (Player.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
			{
				value = twitchDisabledIcon;
			}
			else if (Player.TwitchSafe)
			{
				value = twitchSafeIcon;
			}
			else
			{
				value = "";
			}
			return true;
		case "name":
			if (Player == null)
			{
				value = "";
				return true;
			}
			value = GameUtils.SafeStringFormat(Player.PlayerDisplayName);
			return true;
		case "distance":
			if (Player == null)
			{
				value = "";
				return true;
			}
			value = distanceFormatter.Format(distance);
			return true;
		case "distancecolor":
		{
			Color32 v = Color.white;
			if (Player == null)
			{
				value = "";
				return true;
			}
			if (distance > 100f)
			{
				v = Color.grey;
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "voicevisible":
			value = (voiceState != IPartyVoice.EVoiceMemberState.Disabled).ToString();
			return true;
		case "voicemuted":
			value = (voiceState == IPartyVoice.EVoiceMemberState.Muted).ToString();
			return true;
		case "voiceactive":
			value = (voiceState == IPartyVoice.EVoiceMemberState.VoiceActive).ToString();
			return true;
		default:
			return false;
		}
	}

	public XUiC_PartyEntry()
	{
		oldTwitch = false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "death_icon":
			deathIcon = value;
			break;
		case "leader_icon":
			leaderIcon = value;
			break;
		case "twitch_icon":
			twitchActiveIcon = value;
			break;
		case "twitch_disabled_icon":
			twitchDisabledIcon = value;
			break;
		case "twitch_safe_icon":
			twitchSafeIcon = value;
			break;
		case "default_health_color":
			defaultHealthColor = value;
			break;
		case "twitch_health_color":
			twitchHealthColor = value;
			break;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
		RefreshBindings(_forceAll: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		SetPlayer(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ReturnRotation(EntityAlive _self, EntityAlive _other)
	{
		Transform transform = _self.transform;
		Vector3 forward = transform.forward;
		Vector2 vector = new Vector2(forward.x, forward.z);
		Vector3 normalized = (transform.position - _other.transform.position).normalized;
		Vector2 vector2 = new Vector2(normalized.x, normalized.z);
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		float num = Vector2.Angle(vector, vector2);
		if (vector3.z < 0f)
		{
			num = 360f - num;
		}
		return num;
	}
}
