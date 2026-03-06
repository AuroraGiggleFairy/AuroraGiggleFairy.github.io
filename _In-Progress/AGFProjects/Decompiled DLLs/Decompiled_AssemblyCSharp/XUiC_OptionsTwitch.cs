using System;
using System.Collections.Generic;
using Challenges;
using Platform;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsTwitch : XUiController
{
	public enum TwitchBloodMoonOptions
	{
		Disabled,
		Standard,
		CooldownOnly
	}

	public enum PointsGenerationOptions
	{
		Disabled,
		Standard,
		Double,
		Triple
	}

	public enum BitAddOptions
	{
		Standard = 1,
		Double,
		Triple
	}

	public static string ID = "";

	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoginTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnShowExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnResetPrices;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEnableAllExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDisableAllExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboOptOutTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<TwitchBloodMoonOptions> comboAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<TwitchBloodMoonOptions> comboAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<PointsGenerationOptions> comboPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboCooldownPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVisionEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboActionSpamDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings> comboActionTwitchIntegrationType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<BitAddOptions> comboBitPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<PointsGenerationOptions> comboSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<PointsGenerationOptions> comboGiftSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidViewerMinimum;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowBitEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowGiftSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCharityEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowRaidEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowHypeTrainEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCreatorGoalEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowChannelPointRedeemEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPrices;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPotPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchBloodMoonOptions origAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchBloodMoonOptions origAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public PointsGenerationOptions origPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origCooldownPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowTwitchOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVisionEffects = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionSpamDelay = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager.IntegrationSettings origIntegrationSetting;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVotePresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origEventPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPricesIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPotPercentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public BitAddOptions origBitPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public PointsGenerationOptions origSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public PointsGenerationOptions origGiftSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidPointAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidViewerMinimumIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardTimeIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startedSinceOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	public TwitchManager twitchManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempGiftSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempVoteDayTimeRange = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowBitEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowGiftSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCharityEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowRaidEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowHypeTrainEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCreatorGoalEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowChannelPointRedemptions = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showDeviceCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture qrCodeTexControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string Auth_Code = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string Auth_VerificationUrl = "";

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		tabs = GetChildByType<XUiC_TabSelector>();
		tabs.OnTabChanged += Tabs_OnTabChanged;
		comboAllowActionsDuringBloodmoon = GetChildById("AllowActionsDuringBloodmoon").GetChildByType<XUiC_ComboBoxEnum<TwitchBloodMoonOptions>>();
		comboAllowActionsDuringQuests = GetChildById("AllowActionsDuringQuests").GetChildByType<XUiC_ComboBoxEnum<TwitchBloodMoonOptions>>();
		comboPointsGeneration = GetChildById("PointGeneration").GetChildByType<XUiC_ComboBoxEnum<PointsGenerationOptions>>();
		comboCooldownPreset = GetChildById("CooldownPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		comboUseProgression = GetChildById("UseProgression").GetChildByType<XUiC_ComboBoxBool>();
		comboOptOutTwitch = GetChildById("OptOutTwitch").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowVisionEffects = GetChildById("AllowVisual").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowCrateSharing = GetChildById("AllowCrateSharing").GetChildByType<XUiC_ComboBoxBool>();
		comboActionSpamDelay = GetChildById("ActionSpamDelay").GetChildByType<XUiC_ComboBoxList<string>>();
		comboActionTwitchIntegrationType = GetChildById("ActionTwitchIntegrationType").GetChildByType<XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings>>();
		comboAllowActions = GetChildById("AllowActions").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAllowVotes = GetChildById("AllowVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAllowEvents = GetChildById("AllowEvents").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAllowVotesDuringBloodmoon = GetChildById("AllowVotesDuringBloodmoon").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowVotesDuringQuests = GetChildById("AllowVotesDuringQuests").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowVotesInSafeZone = GetChildById("AllowVotesInSafeZone").GetChildByType<XUiC_ComboBoxBool>();
		comboBitPrices = GetChildById("BitPrices").GetChildByType<XUiC_ComboBoxList<string>>();
		comboBitPotPercent = GetChildById("BitPotPercent").GetChildByType<XUiC_ComboBoxList<string>>();
		comboViewerStartingPoints = GetChildById("ViewerStartingPoints").GetChildByType<XUiC_ComboBoxList<string>>();
		comboBitPointsAdd = GetChildById("BitPointsAdd").GetChildByType<XUiC_ComboBoxEnum<BitAddOptions>>();
		comboSubPointsAdd = GetChildById("SubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<PointsGenerationOptions>>();
		comboGiftSubPointsAdd = GetChildById("GiftSubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<PointsGenerationOptions>>();
		comboRaidPointsAdd = GetChildById("RaidPointsAdd").GetChildByType<XUiC_ComboBoxList<string>>();
		comboRaidViewerMinimum = GetChildById("RaidViewerMinimum").GetChildByType<XUiC_ComboBoxList<string>>();
		comboHelperRewardAmount = GetChildById("HelperRewardAmount").GetChildByType<XUiC_ComboBoxList<string>>();
		comboHelperRewardTime = GetChildById("HelperRewardTime").GetChildByType<XUiC_ComboBoxList<string>>();
		comboMaxDailyVotes = GetChildById("MaxDailyVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		comboVoteTime = GetChildById("VoteTime").GetChildByType<XUiC_ComboBoxList<string>>();
		comboVoteDayTimeRange = GetChildById("VoteDayTimeRange").GetChildByType<XUiC_ComboBoxList<string>>();
		comboVoteViewerDefeatReward = GetChildById("ViewerDefeatReward").GetChildByType<XUiC_ComboBoxList<string>>();
		comboAllowBitEvents = GetChildById("AllowBitEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowSubEvents = GetChildById("AllowSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowGiftSubEvents = GetChildById("AllowGiftSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowCharityEvents = GetChildById("AllowCharityEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowRaidEvents = GetChildById("AllowRaidEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowHypeTrainEvents = GetChildById("AllowHypeTrainEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowCreatorGoalEvents = GetChildById("AllowCreatorGoalEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowChannelPointRedeemEvents = GetChildById("AllowChannelPointRedeemEvents").GetChildByType<XUiC_ComboBoxBool>();
		comboAllowActionsDuringBloodmoon.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowActionsDuringQuests.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboPointsGeneration.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboUseProgression.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboOptOutTwitch.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowVisionEffects.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowCrateSharing.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboActionTwitchIntegrationType.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowActions.OnValueChanged += ComboPreset_OnValueChanged;
		comboAllowVotes.OnValueChanged += ComboPreset_OnValueChanged;
		comboAllowEvents.OnValueChanged += ComboPreset_OnValueChanged;
		comboAllowVotesDuringBloodmoon.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowVotesDuringQuests.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowVotesInSafeZone.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboBitPointsAdd.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboSubPointsAdd.OnValueChanged += SubPoints_OnValueChanged;
		comboGiftSubPointsAdd.OnValueChanged += GiftSubPoints_OnValueChanged;
		comboAllowBitEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowSubEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowGiftSubEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowCharityEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowRaidEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowHypeTrainEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowCreatorGoalEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAllowChannelPointRedeemEvents.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboBitPrices.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboBitPotPercent.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		btnLoginTwitch = GetChildById("btnLoginTwitch") as XUiC_SimpleButton;
		btnLoginTwitch.OnPressed += BtnLoginTwitch_OnPressed;
		btnShowExtras = GetChildById("btnShowExtras") as XUiC_SimpleButton;
		btnShowExtras.OnPressed += BtnShowExtras_OnPressed;
		btnEnableAllExtras = GetChildById("btnEnableAllExtras") as XUiC_SimpleButton;
		btnEnableAllExtras.OnPressed += BtnEnableAllExtras_OnPressed;
		btnDisableAllExtras = GetChildById("btnDisableAllExtras") as XUiC_SimpleButton;
		btnDisableAllExtras.OnPressed += BtnDisableAllExtras_OnPressed;
		btnResetPrices = GetChildById("btnResetPrices") as XUiC_SimpleButton;
		btnResetPrices.OnPressed += BtnResetPrices_OnPressed;
		btnBack = GetChildById("btnBack") as XUiC_SimpleButton;
		btnDefaults = GetChildById("btnDefaults") as XUiC_SimpleButton;
		btnApply = GetChildById("btnApply") as XUiC_SimpleButton;
		btnBack.OnPressed += BtnBack_OnPressed;
		btnDefaults.OnPressed += BtnDefaults_OnOnPressed;
		btnApply.OnPressed += BtnApply_OnPressed;
		RefreshApplyLabel();
		RegisterForInputStyleChanges();
		qrCodeTexControl = GetChildById("qrCodeTex").ViewComponent as XUiV_Texture;
		TwitchAuthentication.QRCodeGenerated += TwitchAuthentication_QRCodeGenerated;
		(GetChildById("btnShowDeviceCode") as XUiC_SimpleButton).OnPressed += BtnShowDeviceCode_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowDeviceCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		showDeviceCode = !showDeviceCode;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tabs_OnTabChanged(int _i, XUiC_TabSelectorTab _tab)
	{
		if (_i != 0 && twitchManager.InitState != TwitchManager.InitStates.Ready)
		{
			tabs.SelectedTabIndex = 0;
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboBoxListString_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboPreset_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		btnApply.Enabled = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoteDayTimeRange_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		btnApply.Enabled = true;
		IsDirty = true;
		tempVoteDayTimeRange = comboVoteDayTimeRange.SelectedIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubPoints_OnValueChanged(XUiController _sender, PointsGenerationOptions _oldValue, PointsGenerationOptions _newValue)
	{
		btnApply.Enabled = true;
		IsDirty = true;
		tempSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GiftSubPoints_OnValueChanged(XUiController _sender, PointsGenerationOptions _oldValue, PointsGenerationOptions _newValue)
	{
		btnApply.Enabled = true;
		IsDirty = true;
		tempGiftSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoginTwitch_OnPressed(XUiController _sender, int _mouseButton)
	{
		switch (twitchManager.InitState)
		{
		case TwitchManager.InitStates.None:
		case TwitchManager.InitStates.PermissionDenied:
		case TwitchManager.InitStates.WaitingForOAuth:
		case TwitchManager.InitStates.ExtensionNotInstalled:
		case TwitchManager.InitStates.Failed:
			startedSinceOpened = true;
			showDeviceCode = false;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				twitchManager.StartTwitchIntegration();
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTwitchAccess>().Setup());
				twitchManager.WaitForPermission();
			}
			if (!localPlayer.PlayerUI.windowManager.IsWindowOpen("twitch"))
			{
				localPlayer.PlayerUI.windowManager.Open("twitch", _bModal: false);
			}
			origAllowTwitchOptions = true;
			localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			break;
		case TwitchManager.InitStates.Ready:
			twitchManager.StopTwitchIntegration();
			break;
		case TwitchManager.InitStates.WaitingForPermission:
		case TwitchManager.InitStates.Authenticating:
		case TwitchManager.InitStates.Authenticated:
		case TwitchManager.InitStates.CheckingForExtension:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges();
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (GameManager.Instance.IsPaused() && GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			GameManager.Instance.Pause(_bOn: false);
		}
		XUiC_TwitchWindowSelector.OpenSelectorAndWindow(GameManager.Instance.World.GetPrimaryPlayer(), "Actions", _extras: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEnableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList()
			where entry.DisplayCategory.Name == "Extras"
			orderby entry.Title
			select entry).ToList();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction item in list)
		{
			if (!currentActionPreset.AddedActions.Contains(item.Name))
			{
				currentActionPreset.AddedActions.Add(item.Name);
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.EnableExtras, item.DisplayCategory.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDisableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList()
			where entry.DisplayCategory.Name == "Extras"
			orderby entry.Title
			select entry).ToList();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction item in list)
		{
			if (currentActionPreset.AddedActions.Contains(item.Name))
			{
				currentActionPreset.AddedActions.Remove(item.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnResetPrices_OnPressed(XUiController _sender, int _mouseButton)
	{
		twitchManager.ResetPricesToDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges();
		btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		twitchManager.UseActionsDuringBloodmoon = 2;
		twitchManager.UseActionsDuringQuests = 2;
		twitchManager.ViewerData.PointRate = 1f;
		twitchManager.SetToDefaultCooldown();
		twitchManager.SetUseProgression(useProgression: true);
		twitchManager.AllowCrateSharing = false;
		twitchManager.ViewerData.ActionSpamDelay = 3f;
		TwitchVotingManager votingManager = twitchManager.VotingManager;
		twitchManager.SetToDefaultActionPreset();
		twitchManager.SetToDefaultVotePreset();
		twitchManager.SetToDefaultEventPreset();
		twitchManager.ViewerData.StartingPoints = 100;
		twitchManager.BitPointModifier = 1;
		twitchManager.SubPointModifier = 1;
		twitchManager.GiftSubPointModifier = 2;
		twitchManager.RaidPointAdd = 1000;
		twitchManager.RaidViewerMinimum = 10;
		twitchManager.BitPotPercentage = 0.25f;
		TwitchManager.LeaderboardStats.GoodRewardAmount = 1000;
		TwitchManager.LeaderboardStats.GoodRewardTime = 15;
		localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
		votingManager.MaxDailyVotes = 4;
		votingManager.VoteTime = 60f;
		votingManager.CurrentVoteDayTimeRange = 2;
		votingManager.ViewerDefeatReward = 250;
		votingManager.AllowVotesDuringBloodmoon = false;
		votingManager.AllowVotesDuringQuests = false;
		votingManager.AllowVotesInSafeZone = false;
		updateOptions();
		applyChanges();
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchAuthentication_QRCodeGenerated(Texture2D qrCodeTex, string userCode, string verificationUrl)
	{
		qrCodeTexControl.Texture = qrCodeTex;
		Auth_Code = userCode;
		Auth_VerificationUrl = verificationUrl;
		RefreshBindings();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			RefreshApplyLabel();
			IsDirty = false;
		}
		if (btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed)
		{
			BtnApply_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		if (GameStats.GetBool(EnumGameStats.TwitchBloodMoonAllowed))
		{
			comboAllowActionsDuringBloodmoon.Value = (origAllowActionsDuringBloodmoon = (TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringBloodmoon);
			comboAllowActionsDuringBloodmoon.Enabled = true;
		}
		else
		{
			comboAllowActionsDuringBloodmoon.Value = TwitchBloodMoonOptions.Disabled;
			comboAllowActionsDuringBloodmoon.Enabled = false;
			twitchManager.UseActionsDuringBloodmoon = 0;
		}
		comboAllowActionsDuringQuests.Value = (origAllowActionsDuringQuests = (TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringQuests);
		comboPointsGeneration.Value = (origPointsGeneration = (PointsGenerationOptions)twitchManager.ViewerData.PointRate);
		comboCooldownPreset.SelectedIndex = (origCooldownPresetIndex = twitchManager.CooldownPresetIndex);
		comboUseProgression.Value = (origUseProgression = twitchManager.UseProgression);
		comboAllowVisionEffects.Value = (origAllowVisionEffects = !localPlayer.TwitchVisionDisabled);
		comboOptOutTwitch.Value = (origAllowTwitchOptions = localPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled);
		comboAllowCrateSharing.Value = (origAllowCrateSharing = twitchManager.AllowCrateSharing);
		comboActionTwitchIntegrationType.Value = (origIntegrationSetting = twitchManager.IntegrationSetting);
		TwitchVotingManager votingManager = twitchManager.VotingManager;
		comboAllowActions.SelectedIndex = (origActionPresetIndex = twitchManager.ActionPresetIndex);
		comboAllowVotes.SelectedIndex = (origVotePresetIndex = twitchManager.VotePresetIndex);
		comboAllowEvents.SelectedIndex = (origEventPresetIndex = twitchManager.EventPresetIndex);
		comboBitPrices.SelectedIndex = (origBitPricesIndex = GetBitPriceModifier(twitchManager.BitPriceMultiplier));
		comboBitPotPercent.SelectedIndex = (origBitPotPercentIndex = Mathf.FloorToInt((1f - twitchManager.BitPotPercentage) / 0.05f));
		comboViewerStartingPoints.SelectedIndex = (origViewerStartingPoints = GetIndexFromCombobox(twitchManager.ViewerData.StartingPoints, comboViewerStartingPoints));
		comboBitPointsAdd.Value = (origBitPointModifier = (BitAddOptions)twitchManager.BitPointModifier);
		comboSubPointsAdd.Value = (origSubPointModifier = (PointsGenerationOptions)twitchManager.SubPointModifier);
		comboGiftSubPointsAdd.Value = (origGiftSubPointModifier = (PointsGenerationOptions)twitchManager.GiftSubPointModifier);
		comboRaidPointsAdd.SelectedIndex = (origRaidPointAmountIndex = GetIndexFromCombobox(twitchManager.RaidPointAdd, comboRaidPointsAdd));
		comboRaidViewerMinimum.SelectedIndex = (origRaidViewerMinimumIndex = GetIndexFromCombobox(twitchManager.RaidViewerMinimum, comboRaidViewerMinimum));
		comboHelperRewardAmount.SelectedIndex = (origHelperRewardAmountIndex = GetIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardAmount, comboHelperRewardAmount));
		comboHelperRewardTime.SelectedIndex = (origHelperRewardTimeIndex = GetIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardTime, comboHelperRewardTime));
		comboMaxDailyVotes.SelectedIndex = (origMaxDailyVotes = GetIndexFromCombobox(votingManager.MaxDailyVotes, comboMaxDailyVotes));
		comboVoteTime.SelectedIndex = (origVoteTime = GetIndexFromCombobox((int)votingManager.VoteTime, comboVoteTime));
		comboVoteDayTimeRange.SelectedIndex = (origVoteDayTimeRange = votingManager.CurrentVoteDayTimeRange);
		comboVoteViewerDefeatReward.SelectedIndex = (origVoteViewerDefeatReward = GetIndexFromCombobox(votingManager.ViewerDefeatReward, comboVoteViewerDefeatReward));
		comboActionSpamDelay.SelectedIndex = (origActionSpamDelay = GetActionSpamDelay(twitchManager.ViewerData.ActionSpamDelay));
		comboAllowVotesDuringBloodmoon.Value = (origAllowVotesDuringBloodmoon = votingManager.AllowVotesDuringBloodmoon);
		comboAllowVotesDuringQuests.Value = (origAllowVotesDuringQuests = votingManager.AllowVotesDuringQuests);
		comboAllowVotesInSafeZone.Value = (origAllowVotesInSafeZone = votingManager.AllowVotesInSafeZone);
		comboAllowBitEvents.Value = (origAllowBitEvents = twitchManager.AllowBitEvents);
		comboAllowSubEvents.Value = (origAllowSubEvents = twitchManager.AllowSubEvents);
		comboAllowGiftSubEvents.Value = (origAllowGiftSubEvents = twitchManager.AllowGiftSubEvents);
		comboAllowCharityEvents.Value = (origAllowCharityEvents = twitchManager.AllowCharityEvents);
		comboAllowRaidEvents.Value = (origAllowRaidEvents = twitchManager.AllowRaidEvents);
		comboAllowHypeTrainEvents.Value = (origAllowHypeTrainEvents = twitchManager.AllowHypeTrainEvents);
		comboAllowCreatorGoalEvents.Value = (origAllowCreatorGoalEvents = twitchManager.AllowCreatorGoalEvents);
		comboAllowChannelPointRedeemEvents.Value = (origAllowChannelPointRedemptions = twitchManager.AllowChannelPointRedemptions);
		tempSubModifier = (int)origSubPointModifier;
		tempGiftSubModifier = (int)origGiftSubPointModifier;
		tempVoteDayTimeRange = origVoteDayTimeRange;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetIndexFromCombobox(int value, XUiC_ComboBoxList<string> cboList)
	{
		string text = value.ToString();
		for (int i = 0; i < cboList.Elements.Count; i++)
		{
			if (cboList.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetActionSpamDelay(float actionSpamDelay)
	{
		string text = ((int)actionSpamDelay).ToString();
		for (int i = 0; i < comboActionSpamDelay.Elements.Count; i++)
		{
			if (i == 0)
			{
				if (text == "0")
				{
					return i;
				}
			}
			else if (comboActionSpamDelay.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetBitPriceModifier(float bitPriceModifier)
	{
		if (bitPriceModifier <= 0.5f)
		{
			if (bitPriceModifier != 0.25f)
			{
				if (bitPriceModifier == 0.5f)
				{
					return 2;
				}
				return 0;
			}
			return 3;
		}
		if (bitPriceModifier != 0.75f)
		{
			_ = 1f;
			return 0;
		}
		return 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float SetBitPriceModifier(int index)
	{
		return index switch
		{
			0 => 1f, 
			1 => 0.75f, 
			2 => 0.5f, 
			3 => 0.25f, 
			_ => 1f, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		origAllowActionsDuringBloodmoon = comboAllowActionsDuringBloodmoon.Value;
		origAllowActionsDuringQuests = comboAllowActionsDuringQuests.Value;
		origPointsGeneration = comboPointsGeneration.Value;
		origCooldownPresetIndex = comboCooldownPreset.SelectedIndex;
		origUseProgression = comboUseProgression.Value;
		origAllowVisionEffects = comboAllowVisionEffects.Value;
		origAllowTwitchOptions = comboOptOutTwitch.Value;
		origAllowCrateSharing = comboAllowCrateSharing.Value;
		origActionSpamDelay = comboActionSpamDelay.SelectedIndex;
		origIntegrationSetting = comboActionTwitchIntegrationType.Value;
		origActionPresetIndex = comboAllowActions.SelectedIndex;
		origVotePresetIndex = comboAllowVotes.SelectedIndex;
		origEventPresetIndex = comboAllowEvents.SelectedIndex;
		origViewerStartingPoints = comboViewerStartingPoints.SelectedIndex;
		origBitPointModifier = comboBitPointsAdd.Value;
		origSubPointModifier = comboSubPointsAdd.Value;
		origGiftSubPointModifier = comboGiftSubPointsAdd.Value;
		origRaidPointAmountIndex = comboRaidPointsAdd.SelectedIndex;
		origRaidViewerMinimumIndex = comboRaidViewerMinimum.SelectedIndex;
		origHelperRewardAmountIndex = comboHelperRewardAmount.SelectedIndex;
		origHelperRewardTimeIndex = comboHelperRewardTime.SelectedIndex;
		origBitPricesIndex = comboBitPrices.SelectedIndex;
		origBitPotPercentIndex = comboBitPotPercent.SelectedIndex;
		origMaxDailyVotes = comboMaxDailyVotes.SelectedIndex;
		origVoteTime = comboVoteTime.SelectedIndex;
		origVoteDayTimeRange = comboVoteDayTimeRange.SelectedIndex;
		origVoteViewerDefeatReward = comboVoteViewerDefeatReward.SelectedIndex;
		origAllowVotesDuringBloodmoon = comboAllowVotesDuringBloodmoon.Value;
		origAllowVotesDuringQuests = comboAllowVotesDuringQuests.Value;
		origAllowVotesInSafeZone = comboAllowVotesInSafeZone.Value;
		origAllowBitEvents = comboAllowBitEvents.Value;
		origAllowSubEvents = comboAllowSubEvents.Value;
		origAllowGiftSubEvents = comboAllowGiftSubEvents.Value;
		origAllowCharityEvents = comboAllowCharityEvents.Value;
		origAllowRaidEvents = comboAllowRaidEvents.Value;
		origAllowHypeTrainEvents = comboAllowHypeTrainEvents.Value;
		origAllowCreatorGoalEvents = comboAllowCreatorGoalEvents.Value;
		origAllowChannelPointRedemptions = comboAllowChannelPointRedeemEvents.Value;
		if (XUiC_OptionsTwitch.OnSettingsChanged != null)
		{
			XUiC_OptionsTwitch.OnSettingsChanged();
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		twitchManager = TwitchManager.Current;
		twitchManager.ConnectionStateChanged -= TwitchManager_ConnectionStateChanged;
		twitchManager.ConnectionStateChanged += TwitchManager_ConnectionStateChanged;
		comboCooldownPreset.OnValueChanged -= ComboBoxListString_OnValueChanged;
		comboCooldownPreset.Elements.Clear();
		for (int i = 0; i < twitchManager.CooldownPresets.Count; i++)
		{
			comboCooldownPreset.Elements.Add(twitchManager.CooldownPresets[i].Title);
		}
		if (twitchManager.CooldownPresetIndex >= comboCooldownPreset.Elements.Count)
		{
			twitchManager.SetToDefaultCooldown();
		}
		comboCooldownPreset.Value = comboCooldownPreset.Elements[twitchManager.CooldownPresetIndex];
		comboCooldownPreset.OnValueChanged += ComboBoxListString_OnValueChanged;
		localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		comboAllowActions.OnValueChanged -= ComboPreset_OnValueChanged;
		comboAllowActions.Elements.Clear();
		for (int j = 0; j < twitchManager.ActionPresets.Count; j++)
		{
			TwitchActionPreset twitchActionPreset = twitchManager.ActionPresets[j];
			if (twitchActionPreset.IsEnabled)
			{
				comboAllowActions.Elements.Add(twitchActionPreset.Title);
			}
		}
		if (twitchManager.ActionPresetIndex >= comboAllowActions.Elements.Count)
		{
			twitchManager.SetToDefaultActionPreset();
		}
		comboAllowActions.Value = comboAllowActions.Elements[twitchManager.ActionPresetIndex];
		comboAllowActions.OnValueChanged += ComboPreset_OnValueChanged;
		comboAllowVotes.OnValueChanged -= ComboPreset_OnValueChanged;
		comboAllowVotes.Elements.Clear();
		for (int k = 0; k < twitchManager.VotePresets.Count; k++)
		{
			comboAllowVotes.Elements.Add(twitchManager.VotePresets[k].Title);
		}
		if (twitchManager.VotePresetIndex >= comboAllowVotes.Elements.Count)
		{
			twitchManager.SetToDefaultVotePreset();
		}
		comboAllowVotes.Value = comboAllowVotes.Elements[twitchManager.VotePresetIndex];
		comboAllowVotes.OnValueChanged += ComboPreset_OnValueChanged;
		comboAllowEvents.OnValueChanged -= ComboPreset_OnValueChanged;
		comboAllowEvents.Elements.Clear();
		for (int l = 0; l < twitchManager.EventPresets.Count; l++)
		{
			comboAllowEvents.Elements.Add(twitchManager.EventPresets[l].Title);
		}
		if (twitchManager.EventPresetIndex >= comboAllowEvents.Elements.Count)
		{
			twitchManager.SetToDefaultEventPreset();
		}
		comboAllowEvents.Value = comboAllowEvents.Elements[twitchManager.EventPresetIndex];
		comboAllowEvents.OnValueChanged += ComboPreset_OnValueChanged;
		SetupComboBoxListString(comboViewerStartingPoints, new string[7] { "0", "50", "100", "150", "200", "250", "500" }, twitchManager.ViewerData.StartingPoints);
		SetupComboBoxListString(comboRaidPointsAdd, new string[6] { "0", "500", "1000", "1500", "2000", "2500" }, twitchManager.RaidPointAdd);
		SetupComboBoxListString(comboRaidViewerMinimum, new string[9] { "1", "3", "5", "10", "15", "20", "25", "50", "100" }, twitchManager.RaidViewerMinimum);
		SetupComboBoxListString(comboMaxDailyVotes, new string[12]
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
			"11", "12"
		}, twitchManager.VotingManager.MaxDailyVotes);
		SetupComboBoxListString(comboVoteTime, new string[4] { "30", "60", "90", "120" }, (int)twitchManager.VotingManager.VoteTime);
		SetupComboBoxListString(comboHelperRewardAmount, new string[7] { "100", "250", "500", "1000", "1250", "1500", "2000" }, TwitchManager.LeaderboardStats.GoodRewardAmount);
		SetupComboBoxListString(comboHelperRewardTime, new string[4] { "15", "30", "45", "60" }, TwitchManager.LeaderboardStats.GoodRewardTime);
		comboVoteDayTimeRange.OnValueChanged -= ComboVoteDayTimeRange_OnValueChanged;
		comboVoteDayTimeRange.Elements.Clear();
		for (int m = 0; m < twitchManager.VotingManager.VoteDayTimeRanges.Count; m++)
		{
			comboVoteDayTimeRange.Elements.Add(twitchManager.VotingManager.VoteDayTimeRanges[m].Name);
		}
		comboVoteDayTimeRange.SelectedIndex = twitchManager.VotingManager.CurrentVoteDayTimeRange;
		comboVoteDayTimeRange.OnValueChanged += ComboVoteDayTimeRange_OnValueChanged;
		SetupComboBoxListString(comboVoteViewerDefeatReward, new string[8] { "0", "100", "200", "250", "500", "1000", "2500", "5000" }, twitchManager.VotingManager.ViewerDefeatReward);
		comboActionSpamDelay.OnValueChanged -= ComboBoxListString_OnValueChanged;
		comboActionSpamDelay.Elements.Clear();
		comboActionSpamDelay.Elements.AddRange(new string[6]
		{
			Localization.Get("xuiLightPropShadowsNone"),
			"1",
			"2",
			"3",
			"4",
			"5"
		});
		comboActionSpamDelay.Value = comboActionSpamDelay.Elements[GetActionSpamDelay(twitchManager.ViewerData.ActionSpamDelay)];
		comboActionSpamDelay.OnValueChanged += ComboBoxListString_OnValueChanged;
		comboBitPrices.OnValueChanged -= ComboBoxListString_OnValueChanged;
		comboBitPrices.Elements.Clear();
		comboBitPrices.Elements.AddRange(new string[4]
		{
			Localization.Get("xuiDefault"),
			"75%",
			"50%",
			"25%"
		});
		comboBitPrices.Value = comboBitPrices.Elements[GetBitPriceModifier(twitchManager.BitPriceMultiplier)];
		comboBitPrices.OnValueChanged += ComboBoxListString_OnValueChanged;
		comboBitPotPercent.OnValueChanged -= ComboBoxListString_OnValueChanged;
		comboBitPotPercent.Elements.Clear();
		comboBitPotPercent.Elements.AddRange(new string[20]
		{
			"100%", "95%", "90%", "85%", "80%", "75%", "70%", "65%", "60%", "55%",
			"50%", "45%", "40%", "35%", "30%", "25%", "20%", "15%", "10%", "0%"
		});
		comboBitPotPercent.Value = comboBitPotPercent.Elements[Mathf.FloorToInt((1f - twitchManager.BitPotPercentage) / 0.05f)];
		comboBitPotPercent.OnValueChanged += ComboBoxListString_OnValueChanged;
		if (tabs != null)
		{
			tabs.SelectedTabIndex = 0;
		}
		updateOptions();
		base.OnOpen();
		btnApply.Enabled = false;
		RefreshBindings();
		startedSinceOpened = false;
		XUi.InGameMenuOpen = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupComboBoxListString(XUiC_ComboBoxList<string> comboBox, string[] values, int currentValue, XUiC_ComboBox<string>.XUiEvent_ValueChanged valueChanged = null)
	{
		if (valueChanged == null)
		{
			valueChanged = ComboBoxListString_OnValueChanged;
		}
		comboBox.OnValueChanged -= valueChanged;
		comboBox.Elements.Clear();
		comboBox.Elements.AddRange(values);
		comboBox.Value = comboBox.Elements[GetIndexFromCombobox(currentValue, comboBox)];
		comboBox.OnValueChanged += valueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_ConnectionStateChanged(TwitchManager.InitStates oldState, TwitchManager.InitStates newState)
	{
		RefreshBindings();
		if (oldState != TwitchManager.InitStates.Ready && newState == TwitchManager.InitStates.Ready)
		{
			localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			comboOptOutTwitch.Value = true;
			origAllowTwitchOptions = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		twitchManager.UseActionsDuringBloodmoon = (int)origAllowActionsDuringBloodmoon;
		twitchManager.UseActionsDuringQuests = (int)origAllowActionsDuringQuests;
		twitchManager.ViewerData.PointRate = (float)origPointsGeneration;
		twitchManager.BitPriceMultiplier = SetBitPriceModifier(origBitPricesIndex);
		twitchManager.BitPotPercentage = 1f - (float)origBitPotPercentIndex * 0.05f;
		twitchManager.SetCooldownPreset(origCooldownPresetIndex);
		twitchManager.SetUseProgression(origUseProgression);
		twitchManager.AllowCrateSharing = origAllowCrateSharing;
		twitchManager.ViewerData.ActionSpamDelay = origActionSpamDelay;
		twitchManager.IntegrationSetting = origIntegrationSetting;
		TwitchVotingManager votingManager = twitchManager.VotingManager;
		bool allowChannelPointRedemptions = twitchManager.AllowChannelPointRedemptions;
		twitchManager.AllowBitEvents = origAllowBitEvents;
		twitchManager.AllowSubEvents = origAllowSubEvents;
		twitchManager.AllowGiftSubEvents = origAllowGiftSubEvents;
		twitchManager.AllowCharityEvents = origAllowCharityEvents;
		twitchManager.AllowRaidEvents = origAllowRaidEvents;
		twitchManager.AllowHypeTrainEvents = origAllowHypeTrainEvents;
		twitchManager.AllowCreatorGoalEvents = origAllowCreatorGoalEvents;
		twitchManager.AllowChannelPointRedemptions = origAllowChannelPointRedemptions;
		twitchManager.SetTwitchActionPreset(origActionPresetIndex);
		twitchManager.SetTwitchVotePreset(origVotePresetIndex);
		twitchManager.SetTwitchEventPreset(origEventPresetIndex, allowChannelPointRedemptions);
		twitchManager.ViewerData.StartingPoints = StringParsers.ParseSInt32(comboViewerStartingPoints.Elements[origViewerStartingPoints]);
		twitchManager.BitPointModifier = (int)origBitPointModifier;
		twitchManager.SubPointModifier = (int)origSubPointModifier;
		twitchManager.GiftSubPointModifier = (int)origGiftSubPointModifier;
		twitchManager.RaidPointAdd = StringParsers.ParseSInt32(comboRaidPointsAdd.Elements[origRaidPointAmountIndex]);
		twitchManager.RaidViewerMinimum = StringParsers.ParseSInt32(comboRaidViewerMinimum.Elements[origRaidViewerMinimumIndex]);
		TwitchManager.LeaderboardStats.GoodRewardAmount = StringParsers.ParseSInt32(comboHelperRewardAmount.Elements[origHelperRewardAmountIndex]);
		TwitchManager.LeaderboardStats.GoodRewardTime = StringParsers.ParseSInt32(comboHelperRewardTime.Elements[origHelperRewardTimeIndex]);
		votingManager.MaxDailyVotes = StringParsers.ParseSInt32(comboMaxDailyVotes.Elements[origMaxDailyVotes]);
		votingManager.VoteTime = StringParsers.ParseFloat(comboVoteTime.Elements[origVoteTime]);
		votingManager.CurrentVoteDayTimeRange = origVoteDayTimeRange;
		votingManager.ViewerDefeatReward = StringParsers.ParseSInt32(comboVoteViewerDefeatReward.Elements[origVoteViewerDefeatReward]);
		votingManager.AllowVotesDuringBloodmoon = origAllowVotesDuringBloodmoon;
		votingManager.AllowVotesDuringQuests = origAllowVotesDuringQuests;
		votingManager.AllowVotesInSafeZone = origAllowVotesInSafeZone;
		twitchManager.HasDataChanges = true;
		if (origAllowTwitchOptions)
		{
			if (localPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled)
			{
				localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			}
		}
		else if (localPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
		{
			localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Disabled;
		}
		localPlayer.TwitchVisionDisabled = !origAllowVisionEffects;
		twitchManager.ConnectionStateChanged -= TwitchManager_ConnectionStateChanged;
		if (twitchManager.CurrentCooldownPreset == null)
		{
			twitchManager.GetCooldownMax();
		}
		if (startedSinceOpened)
		{
			if (twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup && twitchManager.CurrentCooldownPreset.StartCooldownTime > 0)
			{
				twitchManager.SetCooldown(twitchManager.CurrentCooldownPreset.StartCooldownTime, TwitchManager.CooldownTypes.Startup, displayToChannel: false, playCooldownSound: false);
			}
			twitchManager.InitialCooldownSet = true;
		}
		if (twitchManager.InitState == TwitchManager.InitStates.Ready && twitchManager.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
		{
			twitchManager.SetCooldown(0f, TwitchManager.CooldownTypes.None, displayToChannel: false, playCooldownSound: false);
		}
		twitchManager.ExtensionCheckTime = 0f;
		twitchManager = null;
		localPlayer = null;
		XUi.InGameMenuOpen = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "notingame":
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		case "notinlinux":
			_value = "true";
			return true;
		case "twitchstatus":
			if (twitchManager != null)
			{
				_value = twitchManager.StateText;
			}
			else
			{
				_value = "";
			}
			return true;
		case "twitchbuttontext":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = Localization.Get("xuiOptionsTwitchDisconnect");
				}
				else
				{
					_value = Localization.Get("xuiOptionsTwitchLoginTwitch");
				}
			}
			else
			{
				_value = "";
			}
			return true;
		case "onlyconnected":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = "true";
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "notconnected":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = "false";
				}
				else
				{
					_value = "true";
				}
			}
			else
			{
				_value = "true";
			}
			return true;
		case "notconnecting_console":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
				{
					_value = "false";
				}
				else
				{
					_value = "true";
				}
			}
			else
			{
				_value = "true";
			}
			return true;
		case "connecting_console":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
				{
					_value = "true";
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "connecting_standalone":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
				{
					_value = "true";
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "auth_devicecode":
			_value = Auth_Code;
			return true;
		case "auth_verificationUrl":
			_value = Auth_VerificationUrl;
			return true;
		case "show_devicecode":
			_value = (showDeviceCode ? Auth_Code : Localization.Get("xuiOptionsTwitchDeviceCodeShow"));
			return true;
		case "hascustomevents":
			if (twitchManager != null)
			{
				_value = (twitchManager.InitState == TwitchManager.InitStates.Ready && twitchManager.HasCustomEvents).ToString();
			}
			else
			{
				_value = "false";
			}
			return true;
		case "allowactions":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = (!twitchManager.ActionPresets[comboAllowActions.SelectedIndex].IsEmpty).ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "allowvotes":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = (!twitchManager.VotePresets[comboAllowVotes.SelectedIndex].IsEmpty).ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "allowevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = (!twitchManager.EventPresets[comboAllowEvents.SelectedIndex].IsEmpty).ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "subvalues":
			_value = "";
			if (twitchManager != null)
			{
				_value = twitchManager.GetSubTierRewards(tempSubModifier);
			}
			return true;
		case "giftsubvalues":
			_value = "";
			if (twitchManager != null)
			{
				_value = twitchManager.GetGiftSubTierRewards(tempGiftSubModifier);
			}
			return true;
		case "votedaytimerange":
			_value = "";
			if (twitchManager != null)
			{
				_value = twitchManager.VotingManager.GetDayTimeRange(tempVoteDayTimeRange);
			}
			return true;
		case "action_description":
			_value = "";
			if (twitchManager != null)
			{
				TwitchActionPreset twitchActionPreset = twitchManager.ActionPresets[comboAllowActions.SelectedIndex];
				_value = $"[DECEA3]{twitchActionPreset.Title}[-]\n{twitchActionPreset.Description}";
			}
			return true;
		case "vote_description":
			_value = "";
			if (twitchManager != null)
			{
				TwitchVotePreset twitchVotePreset = twitchManager.VotePresets[comboAllowVotes.SelectedIndex];
				_value = $"[DECEA3]{twitchVotePreset.Title}[-]\n{twitchVotePreset.Description}";
			}
			return true;
		case "event_description":
			_value = "";
			if (twitchManager != null)
			{
				TwitchEventPreset twitchEventPreset = twitchManager.EventPresets[comboAllowEvents.SelectedIndex];
				_value = $"[DECEA3]{twitchEventPreset.Title}[-]\n{twitchEventPreset.Description}";
			}
			return true;
		case "hasbitevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasBitEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hassubevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasSubEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hasgiftsubevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasGiftSubEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hascharityevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasCharityEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hasraidevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasRaidEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hashypetrainevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasHypeTrainEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "hascreatorgoalevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasCreatorGoalEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		case "haschannelpointevents":
			if (twitchManager != null)
			{
				if (twitchManager.InitState == TwitchManager.InitStates.Ready)
				{
					_value = twitchManager.EventPresets[comboAllowEvents.SelectedIndex].HasChannelPointEvents.ToString();
				}
				else
				{
					_value = "false";
				}
			}
			else
			{
				_value = "false";
			}
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
