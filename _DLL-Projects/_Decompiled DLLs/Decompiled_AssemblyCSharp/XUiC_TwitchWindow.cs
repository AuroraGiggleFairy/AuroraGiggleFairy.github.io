using System;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchWindow : XUiController
{
	public static string ID = "";

	public TwitchManager twitchManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color defaultGearColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 gearSpriteSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color gearBlinkColor = new Color32(byte.MaxValue, 180, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float secondRotation = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchCommandList CommandListUI;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button progressionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button optionsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button historyButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button pauseButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window window;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingBitPot;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusWaiting;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusBloodMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusSafe;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStatusQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblVoteLocked;

	public string lblPointsPP;

	public string lblPointsSP;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt trackerheightFormatter = new CachedStringFormatterInt();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		twitchManager = TwitchManager.Current;
		twitchManager.ConnectionStateChanged -= TwitchManager_ConnectionStateChanged;
		twitchManager.ConnectionStateChanged += TwitchManager_ConnectionStateChanged;
		TwitchVotingManager votingManager = twitchManager.VotingManager;
		votingManager.VoteStarted = (OnGameEventVoteAction)Delegate.Remove(votingManager.VoteStarted, new OnGameEventVoteAction(TwitchManager_VoteStarted));
		TwitchVotingManager votingManager2 = twitchManager.VotingManager;
		votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager2.VoteStarted, new OnGameEventVoteAction(TwitchManager_VoteStarted));
		CommandListUI = GetChildByType<XUiC_TwitchCommandList>();
		CommandListUI.Owner = this;
		XUiC_TwitchVoteList[] childrenByType = GetChildrenByType<XUiC_TwitchVoteList>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			childrenByType[i].Owner = this;
		}
		XUiController childById = GetChildById("leftButton");
		childById.OnPress += Left_OnPress;
		childById = GetChildById("rightButton");
		childById.OnPress += Right_OnPress;
		childById = GetChildById("pauseButton");
		childById.OnPress += cooldown_OnPress;
		pauseButton = childById.ViewComponent as XUiV_Button;
		childById = GetChildById("optionsButton");
		childById.OnPress += options_OnPress;
		optionsButton = childById.ViewComponent as XUiV_Button;
		childById = GetChildById("historyButton");
		childById.OnPress += history_OnPress;
		historyButton = childById.ViewComponent as XUiV_Button;
		defaultGearColor = optionsButton.CurrentColor;
		gearSpriteSize = new Vector2(optionsButton.Sprite.width, optionsButton.Sprite.height);
		window = base.ViewComponent as XUiV_Window;
		lblStatusReady = Localization.Get("TwitchCooldownStatus_Ready");
		lblStatusWaiting = Localization.Get("TwitchCooldownStatus_Waiting");
		lblStatusBloodMoon = Localization.Get("TwitchCooldownStatus_BloodMoon");
		lblStatusCooldown = Localization.Get("TwitchCooldownStatus_Cooldown");
		lblStatusPaused = Localization.Get("TwitchCooldownStatus_Paused");
		lblStatusSafe = Localization.Get("TwitchCooldownStatus_Safe");
		lblStatusVote = Localization.Get("TwitchCooldownStatus_Vote");
		lblStatusQuest = Localization.Get("TwitchCooldownStatus_Quest");
		lblVoteLocked = Localization.Get("TwitchCooldownStatus_VoteLocked");
		lblPointsPP = Localization.Get("TwitchPoints_PP");
		lblPointsSP = Localization.Get("TwitchPoints_SP");
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (progressionButton != null)
		{
			progressionButton.Selected = twitchManager.UseProgression;
		}
		if (pauseButton != null)
		{
			pauseButton.Selected = !twitchManager.TwitchActive;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_VoteStarted()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void options_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_TwitchWindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "Actions");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void history_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_TwitchWindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "ActionHistory");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cooldown_OnPress(XUiController _sender, int _mouseButton)
	{
		twitchManager.ToggleTwitchActive();
		if (pauseButton != null)
		{
			pauseButton.Selected = !twitchManager.TwitchActive;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Left_OnPress(XUiController _sender, int _mouseButton)
	{
		CommandListUI.MoveBackward();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Right_OnPress(XUiController _sender, int _mouseButton)
	{
		CommandListUI.MoveForward();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_ConnectionStateChanged(TwitchManager.InitStates oldState, TwitchManager.InitStates newState)
	{
		viewComponent.IsVisible = newState == TwitchManager.InitStates.Ready;
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.time - lastUpdate >= secondRotation)
		{
			if (showingBitPot || (!showingBitPot && twitchManager.BitPot != 0))
			{
				IsDirty = true;
				showingBitPot = !showingBitPot;
			}
			lastUpdate = Time.time;
		}
		if (showingBitPot && twitchManager.BitPot == 0)
		{
			showingBitPot = false;
			IsDirty = true;
			lastUpdate = Time.time;
		}
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		window.IsVisible = twitchManager.InitState == TwitchManager.InitStates.Ready && !localPlayer.IsDead() && !XUi.InGameMenuOpen;
		if (twitchManager != null && twitchManager.LocalPlayerXUi == null)
		{
			twitchManager.LocalPlayerXUi = base.xui;
		}
		if (!window.IsVisible)
		{
			return;
		}
		if (window.TargetAlpha == 0f)
		{
			window.TargetAlpha = 1f;
		}
		if (!twitchManager.HasViewedSettings)
		{
			float num = Mathf.PingPong(Time.time, 0.5f);
			optionsButton.DefaultSpriteColor = Color.Lerp(defaultGearColor, gearBlinkColor, num * 4f);
			float num2 = 1f;
			if (num > 0.25f)
			{
				num2 = 1f + num - 0.25f;
			}
			optionsButton.Sprite.SetDimensions((int)(gearSpriteSize.x * num2), (int)(gearSpriteSize.y * num2));
		}
		else
		{
			optionsButton.DefaultSpriteColor = defaultGearColor;
		}
		if (CommandListUI.IsDirty)
		{
			IsDirty = true;
		}
		if (twitchManager.CooldownTime > 0f)
		{
			IsDirty = true;
		}
		if (twitchManager.UIDirty)
		{
			IsDirty = true;
		}
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (progressionButton != null)
		{
			progressionButton.Selected = twitchManager.UseProgression;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "status_title":
			if (twitchManager == null)
			{
				value = "";
				return true;
			}
			if (twitchManager.InitState == TwitchManager.InitStates.Ready)
			{
				if (!twitchManager.TwitchActive)
				{
					value = lblStatusPaused;
				}
				else if (twitchManager.CooldownTime > 0f && twitchManager.CooldownType == TwitchManager.CooldownTypes.Time)
				{
					value = lblStatusCooldown;
				}
				else if (twitchManager.VotingManager.VotingIsActive)
				{
					value = lblStatusVote;
				}
				else if (twitchManager.CooldownTime > 0f || twitchManager.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Always)
				{
					value = lblStatusCooldown;
				}
				else
				{
					value = lblStatusReady;
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "status":
			if (twitchManager == null)
			{
				value = "";
				return true;
			}
			value = "";
			if (twitchManager.InitState == TwitchManager.InitStates.Ready)
			{
				if ((twitchManager.CooldownTime > 0f && twitchManager.CooldownType == TwitchManager.CooldownTypes.Time) || twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup)
				{
					value = XUiM_PlayerBuffs.ConvertToTimeString(twitchManager.CooldownTime);
				}
				else if (twitchManager.VotingManager.VotingIsActive && twitchManager.VotingManager.CurrentVoteState != TwitchVotingManager.VoteStateTypes.WaitingForActive && twitchManager.VotingManager.CurrentVoteState != TwitchVotingManager.VoteStateTypes.EventActive)
				{
					if (twitchManager.VotingManager.VoteTimeRemaining > 0f)
					{
						value = XUiM_PlayerBuffs.GetCVarValueAsTimeString(twitchManager.VotingManager.VoteTimeRemaining);
					}
				}
				else if (twitchManager.CooldownTime > 0f)
				{
					if (twitchManager.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting)
					{
						value = lblStatusWaiting;
					}
					else if (twitchManager.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || twitchManager.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown)
					{
						value = lblStatusBloodMoon;
					}
					else if (twitchManager.CooldownType == TwitchManager.CooldownTypes.QuestDisabled || twitchManager.CooldownType == TwitchManager.CooldownTypes.QuestCooldown)
					{
						value = lblStatusQuest;
					}
					else if (twitchManager.CooldownType == TwitchManager.CooldownTypes.SafeCooldown)
					{
						value = lblStatusSafe;
					}
					else
					{
						value = XUiM_PlayerBuffs.ConvertToTimeString(twitchManager.CooldownTime);
					}
				}
			}
			return true;
		case "statuscolor":
			value = ((twitchManager != null) ? "[ffffff]" : "[ffffff]");
			return true;
		case "showcontent":
			if (twitchManager == null)
			{
				value = "false";
				return true;
			}
			if (!twitchManager.TwitchActive)
			{
				value = "false";
				return true;
			}
			if (twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "false";
				return true;
			}
			if (twitchManager.CooldownType == TwitchManager.CooldownTypes.Time || twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup)
			{
				value = "false";
				return true;
			}
			if (twitchManager.VotingManager.VotingIsActive)
			{
				value = "true";
				return true;
			}
			if (twitchManager.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || twitchManager.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
			{
				value = "false";
				return true;
			}
			if (twitchManager.VoteLockedLevel == TwitchVoteLockTypes.ActionsLocked)
			{
				value = "false";
				return true;
			}
			if (!twitchManager.AllowActions)
			{
				value = "false";
				return true;
			}
			if (twitchManager.IntegrationSetting == TwitchManager.IntegrationSettings.ExtensionOnly)
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "showcommands":
			value = (twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && !twitchManager.VotingManager.VotingIsActive).ToString();
			return true;
		case "showvotes":
			value = (twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && twitchManager.VotingManager.VotingIsActive).ToString();
			return true;
		case "voteitems":
			value = "3";
			return true;
		case "grouptitle":
			if (twitchManager == null)
			{
				value = "";
				return true;
			}
			if (!twitchManager.TwitchActive)
			{
				value = "";
				return true;
			}
			if (twitchManager.VotingManager.VotingIsActive)
			{
				value = twitchManager.VotingManager.VoteTypeText;
				return true;
			}
			if (twitchManager.VoteLockedLevel == TwitchVoteLockTypes.ActionsLocked)
			{
				value = lblVoteLocked;
				return true;
			}
			if (twitchManager.CooldownTime > 0f && (twitchManager.CooldownType == TwitchManager.CooldownTypes.Time || twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup))
			{
				value = "";
				return true;
			}
			if (!twitchManager.AllowActions)
			{
				value = "";
				return true;
			}
			if (twitchManager.IntegrationSetting == TwitchManager.IntegrationSettings.ExtensionOnly)
			{
				value = Localization.Get("xuiTwitchIntegrationOptionsExtensionOnly");
				return true;
			}
			if (CommandListUI != null)
			{
				value = CommandListUI.CurrentTitle;
				return true;
			}
			value = "";
			return true;
		case "grouptitlevisible":
			if (twitchManager == null)
			{
				value = "false";
				return true;
			}
			if (!twitchManager.TwitchActive)
			{
				value = "false";
				return true;
			}
			if (twitchManager.CooldownType == TwitchManager.CooldownTypes.Time || twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup || twitchManager.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || twitchManager.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
			{
				value = "false";
				return true;
			}
			if (!twitchManager.AllowActions && !twitchManager.VotingManager.VotingIsActive && twitchManager.VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked)
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "commandsheight":
			if (CommandListUI != null)
			{
				value = CommandListUI.GetHeight().ToString();
			}
			return true;
		case "cooldownfill":
			if (twitchManager == null)
			{
				value = "0";
				return true;
			}
			if (twitchManager.CurrentCooldownPreset != null && twitchManager.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.None)
			{
				value = "0";
				return true;
			}
			if (twitchManager.CooldownType != TwitchManager.CooldownTypes.SafeCooldown && twitchManager.CooldownType != TwitchManager.CooldownTypes.SafeCooldownExit && (twitchManager.CooldownTime > 0f || (twitchManager.CurrentCooldownPreset != null && twitchManager.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Always)))
			{
				value = "1";
				return true;
			}
			value = (twitchManager.CurrentCooldownFill / twitchManager.CurrentCooldownPreset.CooldownFillMax).ToString();
			return true;
		case "potbalance":
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "";
				return true;
			}
			if (showingBitPot)
			{
				value = $"[FFB400]{twitchManager.BitPot} BC[-]";
			}
			else
			{
				value = $"{twitchManager.RewardPot} {((twitchManager.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP) ? lblPointsSP : lblPointsPP)}";
			}
			return true;
		case "showpotbalance":
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready || twitchManager.PimpPotType == TwitchManager.PimpPotSettings.Disabled)
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "arrowvisible":
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "true";
				return true;
			}
			if (twitchManager.VotingManager.VotingIsActive || twitchManager.VoteLockedLevel == TwitchVoteLockTypes.ActionsLocked)
			{
				value = "false";
				return true;
			}
			if (!twitchManager.AllowActions)
			{
				value = "false";
				return true;
			}
			if (twitchManager.IntegrationSetting == TwitchManager.IntegrationSettings.ExtensionOnly)
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "vote_tip":
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "";
				return true;
			}
			value = twitchManager.VotingManager.VoteTip;
			return true;
		case "show_vote_tip":
			value = "false";
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "false";
				return true;
			}
			if (twitchManager.VotingManager.WinnerShowing && twitchManager.VotingManager.VoteTip != "")
			{
				value = "true";
			}
			return true;
		case "tip_offset":
			if (twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				value = "0";
				return true;
			}
			value = twitchManager.VotingManager.VoteOffset;
			return true;
		default:
			return false;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (twitchManager != null)
		{
			twitchManager.ConnectionStateChanged -= TwitchManager_ConnectionStateChanged;
		}
	}
}
