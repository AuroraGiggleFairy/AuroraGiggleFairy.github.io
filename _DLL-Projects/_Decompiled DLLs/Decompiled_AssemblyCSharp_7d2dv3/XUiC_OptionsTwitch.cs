using System;
using System.Collections.Generic;
using Challenges;
using Platform;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsTwitch : XUiC_OptionsDialogBase
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

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

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
	public string authCode = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string authVerificationUrl = "";

	public TwitchActionPreset ActionPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return twitchManager.ActionPresets[comboAllowActions.SelectedIndex];
		}
	}

	public TwitchVotePreset VotePreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return twitchManager.VotePresets[comboAllowVotes.SelectedIndex];
		}
	}

	public TwitchEventPreset EventPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return twitchManager.EventPresets[comboAllowEvents.SelectedIndex];
		}
	}

	[XuiXmlBinding("twitchstatus")]
	public string TwitchStatus
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return twitchManager?.StateText ?? "";
		}
	}

	[XuiXmlBinding("twitchbuttontext")]
	public string TwitchButtonText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				if (twitchManager.InitState != TwitchManager.InitStates.Ready)
				{
					return Localization.Get("xuiOptionsTwitchLoginTwitch");
				}
				return Localization.Get("xuiOptionsTwitchDisconnect");
			}
			return "";
		}
	}

	[XuiXmlBinding("onlyconnected")]
	public bool OnlyConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj == null)
			{
				return false;
			}
			return obj.InitState == TwitchManager.InitStates.Ready;
		}
	}

	[XuiXmlBinding("notconnected")]
	public bool NotConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj == null)
			{
				return true;
			}
			return obj.InitState != TwitchManager.InitStates.Ready;
		}
	}

	[XuiXmlBinding("notconnecting_console")]
	public bool NotConnectingConsole
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.WaitingForOAuth)
			{
				return (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
			}
			return true;
		}
	}

	[XuiXmlBinding("connecting_console")]
	public bool ConnectingConsole
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.WaitingForOAuth)
			{
				return !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
			}
			return false;
		}
	}

	[XuiXmlBinding("connecting_standalone")]
	public bool ConnectingStandalone
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.WaitingForOAuth)
			{
				return (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
			}
			return false;
		}
	}

	[XuiXmlBinding("auth_devicecode")]
	public string AuthDeviceCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return authCode ?? "";
		}
	}

	[XuiXmlBinding("auth_verificationUrl")]
	public string AuthVerificationUrl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return authVerificationUrl ?? "";
		}
	}

	[XuiXmlBinding("show_devicecode")]
	public string ShowDeviceCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!showDeviceCode)
			{
				return Localization.Get("xuiOptionsTwitchDeviceCodeShow");
			}
			return authCode;
		}
	}

	[XuiXmlBinding("hascustomevents")]
	public bool HasCustomEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return twitchManager.HasCustomEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("allowactions")]
	public bool AllowActions
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return !ActionPreset.IsEmpty;
			}
			return false;
		}
	}

	[XuiXmlBinding("allowvotes")]
	public bool AllowVotes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return !VotePreset.IsEmpty;
			}
			return false;
		}
	}

	[XuiXmlBinding("allowevents")]
	public bool AllowEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return !EventPreset.IsEmpty;
			}
			return false;
		}
	}

	[XuiXmlBinding("subvalues")]
	public string SubValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return twitchManager.GetSubTierRewards(tempSubModifier);
			}
			return "";
		}
	}

	[XuiXmlBinding("giftsubvalues")]
	public string GiftSubValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return twitchManager.GetGiftSubTierRewards(tempGiftSubModifier);
			}
			return "";
		}
	}

	[XuiXmlBinding("votedaytimerange")]
	public string VoteDayTimeRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return twitchManager.VotingManager.GetDayTimeRange(tempVoteDayTimeRange);
			}
			return "";
		}
	}

	[XuiXmlBinding("action_description")]
	public string ActionDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return "[DECEA3]" + ActionPreset.Title + "[-]\n" + ActionPreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("vote_description")]
	public string VoteDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return "[DECEA3]" + VotePreset.Title + "[-]\n" + VotePreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("event_description")]
	public string EventDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (twitchManager != null)
			{
				return "[DECEA3]" + EventPreset.Title + "[-]\n" + EventPreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("hasbitevents")]
	public bool HasBitEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasBitEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hassubevents")]
	public bool HasSubEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasSubEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hasgiftsubevents")]
	public bool HasGiftSubEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasGiftSubEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hascharityevents")]
	public bool HasCharityEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasCharityEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hasraidevents")]
	public bool HasRaidEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasRaidEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hashypetrainevents")]
	public bool HasHypeTrainEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasHypeTrainEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("hascreatorgoalevents")]
	public bool HasCreatorGoalEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasCreatorGoalEvents;
			}
			return false;
		}
	}

	[XuiXmlBinding("haschannelpointevents")]
	public bool HasChannelPointEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager obj = twitchManager;
			if (obj != null && obj.InitState == TwitchManager.InitStates.Ready)
			{
				return EventPreset.HasChannelPointEvents;
			}
			return false;
		}
	}

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
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
		if (GetChildById("btnLoginTwitch") is XUiC_Button xUiC_Button)
		{
			xUiC_Button.OnPress += BtnLoginTwitch_OnPressed;
		}
		if (GetChildById("btnShowExtras") is XUiC_Button xUiC_Button2)
		{
			xUiC_Button2.OnPress += BtnShowExtras_OnPressed;
		}
		if (GetChildById("btnEnableAllExtras") is XUiC_Button xUiC_Button3)
		{
			xUiC_Button3.OnPress += BtnEnableAllExtras_OnPressed;
		}
		if (GetChildById("btnDisableAllExtras") is XUiC_Button xUiC_Button4)
		{
			xUiC_Button4.OnPress += BtnDisableAllExtras_OnPressed;
		}
		if (GetChildById("btnResetPrices") is XUiC_Button xUiC_Button5)
		{
			xUiC_Button5.OnPress += BtnResetPrices_OnPressed;
		}
		if (GetChildById("btnShowDeviceCode") is XUiC_Button xUiC_Button6)
		{
			xUiC_Button6.OnPress += BtnShowDeviceCode_OnPressed;
		}
		qrCodeTexControl = GetChildById("qrCodeTex").ViewComponent as XUiV_Texture;
		TwitchAuthentication.QRCodeGenerated += TwitchAuthentication_QRCodeGenerated;
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
		SetChanged();
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
		SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboPreset_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoteDayTimeRange_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		SetChanged();
		tempVoteDayTimeRange = comboVoteDayTimeRange.SelectedIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubPoints_OnValueChanged(XUiController _sender, PointsGenerationOptions _oldValue, PointsGenerationOptions _newValue)
	{
		SetChanged();
		tempSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GiftSubPoints_OnValueChanged(XUiController _sender, PointsGenerationOptions _oldValue, PointsGenerationOptions _newValue)
	{
		SetChanged();
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
		xui.playerUI.windowManager.Close(windowGroup);
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doSaveChangesInternal()
	{
		base.doSaveChangesInternal();
		applyChanges();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doResetToDefaultsInternal()
	{
		base.doResetToDefaultsInternal();
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
		SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchAuthentication_QRCodeGenerated(Texture2D _qrCodeTex, string _userCode, string _verificationUrl)
	{
		qrCodeTexControl.Texture = _qrCodeTex;
		authCode = _userCode;
		authVerificationUrl = _verificationUrl;
		RefreshBindings();
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
		comboBitPrices.SelectedIndex = (origBitPricesIndex = getBitPriceModifier(twitchManager.BitPriceMultiplier));
		comboBitPotPercent.SelectedIndex = (origBitPotPercentIndex = Mathf.FloorToInt((1f - twitchManager.BitPotPercentage) / 0.05f));
		comboViewerStartingPoints.SelectedIndex = (origViewerStartingPoints = getIndexFromCombobox(twitchManager.ViewerData.StartingPoints, comboViewerStartingPoints));
		comboBitPointsAdd.Value = (origBitPointModifier = (BitAddOptions)twitchManager.BitPointModifier);
		comboSubPointsAdd.Value = (origSubPointModifier = (PointsGenerationOptions)twitchManager.SubPointModifier);
		comboGiftSubPointsAdd.Value = (origGiftSubPointModifier = (PointsGenerationOptions)twitchManager.GiftSubPointModifier);
		comboRaidPointsAdd.SelectedIndex = (origRaidPointAmountIndex = getIndexFromCombobox(twitchManager.RaidPointAdd, comboRaidPointsAdd));
		comboRaidViewerMinimum.SelectedIndex = (origRaidViewerMinimumIndex = getIndexFromCombobox(twitchManager.RaidViewerMinimum, comboRaidViewerMinimum));
		comboHelperRewardAmount.SelectedIndex = (origHelperRewardAmountIndex = getIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardAmount, comboHelperRewardAmount));
		comboHelperRewardTime.SelectedIndex = (origHelperRewardTimeIndex = getIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardTime, comboHelperRewardTime));
		comboMaxDailyVotes.SelectedIndex = (origMaxDailyVotes = getIndexFromCombobox(votingManager.MaxDailyVotes, comboMaxDailyVotes));
		comboVoteTime.SelectedIndex = (origVoteTime = getIndexFromCombobox((int)votingManager.VoteTime, comboVoteTime));
		comboVoteDayTimeRange.SelectedIndex = (origVoteDayTimeRange = votingManager.CurrentVoteDayTimeRange);
		comboVoteViewerDefeatReward.SelectedIndex = (origVoteViewerDefeatReward = getIndexFromCombobox(votingManager.ViewerDefeatReward, comboVoteViewerDefeatReward));
		comboActionSpamDelay.SelectedIndex = (origActionSpamDelay = getActionSpamDelay(twitchManager.ViewerData.ActionSpamDelay));
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
	public int getIndexFromCombobox(int _value, XUiC_ComboBoxList<string> _cboList)
	{
		string text = _value.ToString();
		for (int i = 0; i < _cboList.Elements.Count; i++)
		{
			if (_cboList.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getActionSpamDelay(float _actionSpamDelay)
	{
		string text = ((int)_actionSpamDelay).ToString();
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
	public int getBitPriceModifier(float _bitPriceModifier)
	{
		if (_bitPriceModifier <= 0.5f)
		{
			if (_bitPriceModifier == 0.25f)
			{
				return 3;
			}
			if (_bitPriceModifier == 0.5f)
			{
				return 2;
			}
		}
		else
		{
			if (_bitPriceModifier == 0.75f)
			{
				return 1;
			}
			if (_bitPriceModifier == 1f)
			{
				return 0;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float setBitPriceModifier(int _index)
	{
		return _index switch
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
		XUiC_OptionsTwitch.OnSettingsChanged?.Invoke();
	}

	public override void OnOpen()
	{
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
		setupComboBoxListString(comboViewerStartingPoints, new string[7] { "0", "50", "100", "150", "200", "250", "500" }, twitchManager.ViewerData.StartingPoints);
		setupComboBoxListString(comboRaidPointsAdd, new string[6] { "0", "500", "1000", "1500", "2000", "2500" }, twitchManager.RaidPointAdd);
		setupComboBoxListString(comboRaidViewerMinimum, new string[9] { "1", "3", "5", "10", "15", "20", "25", "50", "100" }, twitchManager.RaidViewerMinimum);
		setupComboBoxListString(comboMaxDailyVotes, new string[12]
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
			"11", "12"
		}, twitchManager.VotingManager.MaxDailyVotes);
		setupComboBoxListString(comboVoteTime, new string[4] { "30", "60", "90", "120" }, (int)twitchManager.VotingManager.VoteTime);
		setupComboBoxListString(comboHelperRewardAmount, new string[7] { "100", "250", "500", "1000", "1250", "1500", "2000" }, TwitchManager.LeaderboardStats.GoodRewardAmount);
		setupComboBoxListString(comboHelperRewardTime, new string[4] { "15", "30", "45", "60" }, TwitchManager.LeaderboardStats.GoodRewardTime);
		comboVoteDayTimeRange.OnValueChanged -= ComboVoteDayTimeRange_OnValueChanged;
		comboVoteDayTimeRange.Elements.Clear();
		for (int m = 0; m < twitchManager.VotingManager.VoteDayTimeRanges.Count; m++)
		{
			comboVoteDayTimeRange.Elements.Add(twitchManager.VotingManager.VoteDayTimeRanges[m].Name);
		}
		comboVoteDayTimeRange.SelectedIndex = twitchManager.VotingManager.CurrentVoteDayTimeRange;
		comboVoteDayTimeRange.OnValueChanged += ComboVoteDayTimeRange_OnValueChanged;
		setupComboBoxListString(comboVoteViewerDefeatReward, new string[8] { "0", "100", "200", "250", "500", "1000", "2500", "5000" }, twitchManager.VotingManager.ViewerDefeatReward);
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
		comboActionSpamDelay.Value = comboActionSpamDelay.Elements[getActionSpamDelay(twitchManager.ViewerData.ActionSpamDelay)];
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
		comboBitPrices.Value = comboBitPrices.Elements[getBitPriceModifier(twitchManager.BitPriceMultiplier)];
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
		startedSinceOpened = false;
		XUi.InGameMenuOpen = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupComboBoxListString(XUiC_ComboBoxList<string> _comboBox, string[] _values, int _currentValue, XUiC_ComboBox<string>.XUiEvent_ValueChanged _valueChanged = null)
	{
		if (_valueChanged == null)
		{
			_valueChanged = ComboBoxListString_OnValueChanged;
		}
		_comboBox.OnValueChanged -= _valueChanged;
		_comboBox.Elements.Clear();
		_comboBox.Elements.AddRange(_values);
		_comboBox.Value = _comboBox.Elements[getIndexFromCombobox(_currentValue, _comboBox)];
		_comboBox.OnValueChanged += _valueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_ConnectionStateChanged(TwitchManager.InitStates _oldState, TwitchManager.InitStates _newState)
	{
		RefreshBindings();
		if (_oldState != TwitchManager.InitStates.Ready && _newState == TwitchManager.InitStates.Ready)
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
		twitchManager.BitPriceMultiplier = setBitPriceModifier(origBitPricesIndex);
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
}
