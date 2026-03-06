using Audio;
using Challenges;
using Platform;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchEntryDescriptionWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntry actionEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchVoteInfoEntry voteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionHistoryEntry historyEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnEnable;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRetry;

	public XUiC_TwitchEntryListWindow OwnerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchAction currentTwitchActionEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVote currentTwitchVoteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionHistoryEntry currentTwitchActionHistoryEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStartGamestage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEndGamestage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointCost;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRandomDaily;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIsPositive;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblVoteEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionHistoryEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboardEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryTargetTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryStateTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryTimeStampTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNoRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNoRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRetryActionUnavailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboardStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblShowBitTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDiscountCost;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIncreasePrice;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDecreasePrice;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableAction_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableAction_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableVote_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableVote_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIncreasePrice_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDecreasePrice_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopKiller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopEvil;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCurrentGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblMostBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalBad;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLargestPimpPot;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTrue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblFalse;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsPP;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsSP;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchLeaderboardStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBitTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string DayTimeFormatString = Localization.Get("xuiDay") + "{0}, {1:00}:{2:00}";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, DayTimeFormatString));

	public TwitchAction CurrentTwitchActionEntry
	{
		get
		{
			return currentTwitchActionEntry;
		}
		set
		{
			currentTwitchActionEntry = value;
			RefreshBindings(_forceAll: true);
		}
	}

	public TwitchVote CurrentTwitchVoteEntry
	{
		get
		{
			return currentTwitchVoteEntry;
		}
		set
		{
			currentTwitchVoteEntry = value;
			RefreshBindings(_forceAll: true);
		}
	}

	public TwitchActionHistoryEntry CurrentTwitchActionHistoryEntry
	{
		get
		{
			return currentTwitchActionHistoryEntry;
		}
		set
		{
			currentTwitchActionHistoryEntry = value;
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = ((XUiC_TwitchInfoWindowGroup)windowGroup.Controller).GetChildByType<XUiC_TwitchHowToWindow>().GetChildById("leftButton");
		XUiController childById2 = windowGroup.Controller.GetChildById("windowTwitchInfoDescription");
		XUiController childById3 = childById2.GetChildById("btnEnable");
		XUiController childById4 = childById2.GetChildById("statClick");
		btnEnable = (XUiV_Button)childById3.GetChildById("clickable").ViewComponent;
		btnEnable.Controller.OnPress += btnEnable_OnPress;
		btnEnable.NavDownTarget = childById.ViewComponent;
		childById3 = childById2.GetChildById("btnRefund");
		btnRefund = (XUiV_Button)childById3.GetChildById("clickable").ViewComponent;
		btnRefund.Controller.OnPress += btnRefund_OnPress;
		btnRetry = childById2.GetChildById("btnRetry").GetChildByType<XUiC_SimpleButton>();
		btnRetry.OnPressed += btnRetry_OnPress;
		btnRetry.ViewComponent.NavDownTarget = childById.ViewComponent;
		childById2.GetChildById("btnIncrease").GetChildByType<XUiC_SimpleButton>().OnPressed += BtnIncrease_OnPressed;
		childById2.GetChildById("btnDecrease").GetChildByType<XUiC_SimpleButton>().OnPressed += BtnDecrease_OnPressed;
		childById4.OnPress += RectStat_OnPress;
		lblStartGamestage = Localization.Get("TwitchInfo_ActionStartGamestage");
		lblEndGamestage = Localization.Get("TwitchInfo_ActionEndGamestage");
		lblPointCost = Localization.Get("TwitchInfo_ActionPointCost");
		lblDiscountCost = Localization.Get("TwitchInfo_ActionDiscountCost");
		lblCooldown = Localization.Get("TwitchInfo_ActionCooldown");
		lblRandomDaily = Localization.Get("TwitchInfo_ActionRandomDaily");
		lblIsPositive = Localization.Get("TwitchInfo_ActionIsPositive");
		lblPointType = Localization.Get("TwitchInfo_ActionPointType");
		lblEnableAction = Localization.Get("TwitchInfo_ActionEnableAction") + " ([action:gui:GUI D-Pad Up])";
		lblDisableAction = Localization.Get("TwitchInfo_ActionDisableAction") + " ([action:gui:GUI D-Pad Up])";
		lblIncreasePrice = Localization.Get("TwitchInfo_IncreasePriceButton") + " ([action:gui:GUI D-Pad Right])";
		lblDecreasePrice = Localization.Get("TwitchInfo_DecreasePriceButton") + " ([action:gui:GUI D-Pad Left])";
		lblEnableVote = Localization.Get("TwitchInfo_ActionEnableVote") + " ([action:gui:GUI D-Pad Up])";
		lblDisableVote = Localization.Get("TwitchInfo_ActionDisableVote") + " ([action:gui:GUI D-Pad Up])";
		lblEnableAction_Controller = Localization.Get("TwitchInfo_ActionEnableAction") + " [action:gui:GUI HalfStack]";
		lblDisableAction_Controller = Localization.Get("TwitchInfo_ActionDisableAction") + " [action:gui:GUI HalfStack]";
		lblIncreasePrice_Controller = Localization.Get("TwitchInfo_IncreasePriceButton") + " [action:gui:GUI Inspect] + [action:gui:GUI D-Pad Right]";
		lblDecreasePrice_Controller = Localization.Get("TwitchInfo_DecreasePriceButton") + " [action:gui:GUI Inspect] + [action:gui:GUI D-Pad Left]";
		lblEnableVote_Controller = Localization.Get("TwitchInfo_ActionEnableVote") + " [action:gui:GUI HalfStack]";
		lblDisableVote_Controller = Localization.Get("TwitchInfo_ActionDisableVote") + " [action:gui:GUI HalfStack]";
		lblActionEmpty = Localization.Get("TwitchInfo_ActionEmpty");
		lblVoteEmpty = Localization.Get("TwitchInfo_VoteEmpty");
		lblActionHistoryEmpty = Localization.Get("TwitchInfo_ActionHistoryEmpty");
		lblLeaderboardEmpty = Localization.Get("TwitchInfo_LeaderboardEmpty");
		lblHistoryTargetTitle = Localization.Get("TwitchInfo_ActionHistoryTarget");
		lblHistoryStateTitle = Localization.Get("xuiLightPropState");
		lblHistoryTimeStampTitle = Localization.Get("ObjectiveTime_keyword");
		lblRefund = Localization.Get("TwitchInfo_ActionHistoryRefund");
		lblNoRefund = Localization.Get("TwitchInfo_ActionHistoryRefundNotAvailable");
		lblRetry = Localization.Get("TwitchInfo_ActionHistoryRetry");
		lblNoRetry = Localization.Get("TwitchInfo_ActionHistoryRetryNotAvailable");
		lblRetryActionUnavailable = Localization.Get("TwitchInfo_ActionHistoryRetryActionUnavailable");
		lblLeaderboardStats = Localization.Get("TwitchInfo_LeaderboardStats");
		lblShowBitTotal = Localization.Get("TwitchInfo_LeaderboardShowBitTotal");
		lblTopKiller = Localization.Get("TwitchInfo_TopKiller");
		lblTopGood = Localization.Get("TwitchInfo_TopGood");
		lblTopEvil = Localization.Get("TwitchInfo_TopEvil");
		lblCurrentGood = Localization.Get("TwitchInfo_CurrentGood");
		lblMostBits = Localization.Get("TwitchInfo_MostBits");
		lblTotalBits = Localization.Get("TwitchInfo_TotalBits");
		lblTotalBad = Localization.Get("TwitchInfo_TotalBad");
		lblTotalGood = Localization.Get("TwitchInfo_TotalGood");
		lblTotalActions = Localization.Get("TwitchInfo_TotalActions");
		lblLargestPimpPot = Localization.Get("TwitchInfo_LargestPimpPot");
		lblTrue = Localization.Get("statTrue");
		lblFalse = Localization.Get("statFalse");
		lblPointsPP = Localization.Get("TwitchPoints_PP");
		lblPointsSP = Localization.Get("TwitchPoints_SP");
		lblPointsBits = Localization.Get("TwitchPoints_Bits");
		RegisterForInputStyleChanges();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!base.ViewComponent.UiTransform.gameObject.activeInHierarchy || (actionEntry == null && voteEntry == null))
		{
			return;
		}
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !base.xui.playerUI.windowManager.IsInputActive())
		{
			if (actionEntry != null)
			{
				if (gUIActions.DPad_Up.WasPressed)
				{
					btnEnable_OnPress(btnEnable.Controller, -1);
				}
				if (gUIActions.DPad_Left.WasPressed)
				{
					BtnDecrease_OnPressed(null, -1);
				}
				if (gUIActions.DPad_Right.WasPressed)
				{
					BtnIncrease_OnPressed(null, -1);
				}
			}
			else if (voteEntry != null && gUIActions.DPad_Up.WasPressed)
			{
				btnEnable_OnPress(btnEnable.Controller, -1);
			}
		}
		else
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				return;
			}
			if (actionEntry != null)
			{
				if (gUIActions.HalfStack.WasPressed)
				{
					btnEnable_OnPress(btnEnable.Controller, -1);
				}
				if (gUIActions.Inspect.IsPressed)
				{
					if (gUIActions.DPad_Left.WasPressed)
					{
						BtnDecrease_OnPressed(null, -1);
					}
					if (gUIActions.DPad_Right.WasPressed)
					{
						BtnIncrease_OnPressed(null, -1);
					}
				}
			}
			else if (voteEntry != null && gUIActions.HalfStack.WasPressed)
			{
				btnEnable_OnPress(btnEnable.Controller, -1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDecrease_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (currentTwitchActionEntry != null)
		{
			currentTwitchActionEntry.DecreaseCost();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnIncrease_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (currentTwitchActionEntry != null)
		{
			currentTwitchActionEntry.IncreaseCost();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RectStat_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!showBitTotal)
		{
			showBitTotal = true;
			RefreshBindings();
			_sender.ViewComponent.EventOnPress = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRefund_OnPress(XUiController _sender, int _mouseButton)
	{
		if (currentTwitchActionHistoryEntry != null)
		{
			currentTwitchActionHistoryEntry.Refund();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRetry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (currentTwitchActionHistoryEntry != null)
		{
			currentTwitchActionHistoryEntry.Retry();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnEnable_OnPress(XUiController _sender, int _mouseButton)
	{
		if (currentTwitchActionEntry != null)
		{
			TwitchManager current = TwitchManager.Current;
			TwitchActionPreset currentActionPreset = current.CurrentActionPreset;
			if (currentTwitchActionEntry.IsInPresetDefault(currentActionPreset))
			{
				if (currentActionPreset.RemovedActions.Contains(currentTwitchActionEntry.Name))
				{
					currentActionPreset.RemovedActions.Remove(currentTwitchActionEntry.Name);
				}
				else
				{
					currentActionPreset.RemovedActions.Add(currentTwitchActionEntry.Name);
				}
			}
			else if (currentActionPreset.AddedActions.Contains(currentTwitchActionEntry.Name))
			{
				currentActionPreset.AddedActions.Remove(currentTwitchActionEntry.Name);
			}
			else
			{
				currentActionPreset.AddedActions.Add(currentTwitchActionEntry.Name);
				if (currentTwitchActionEntry.DisplayCategory.Name == "Extras")
				{
					QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.EnableExtras, currentTwitchActionEntry.DisplayCategory.Name);
				}
			}
			current.HandleChangedPropertyList();
			Manager.PlayInsidePlayerHead("craft_click_craft");
			current.SetupAvailableCommands();
			current.HandleCooldownActionLocking();
			RefreshBindings();
			actionEntry.RefreshBindings();
		}
		else if (currentTwitchVoteEntry != null)
		{
			currentTwitchVoteEntry.Enabled = !currentTwitchVoteEntry.Enabled;
			TwitchManager.Current.HandleChangedPropertyList();
			Manager.PlayInsidePlayerHead("craft_click_craft");
			RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		stats = TwitchManager.LeaderboardStats;
		stats.StatsChanged += Stats_StatsChanged;
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		stats.StatsChanged -= Stats_StatsChanged;
		TwitchManager.Current.HandleChangedPropertyList();
		TwitchManager.Current.ResetPrices();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Stats_StatsChanged()
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "entrytitle":
			if (currentTwitchActionEntry != null)
			{
				value = currentTwitchActionEntry.Title;
			}
			else if (currentTwitchVoteEntry != null)
			{
				value = currentTwitchVoteEntry.VoteDescription;
			}
			else if (currentTwitchActionHistoryEntry != null)
			{
				value = currentTwitchActionHistoryEntry.Title;
			}
			else if (OwnerList != null && OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)
			{
				value = lblLeaderboardStats;
			}
			else
			{
				value = "";
			}
			return true;
		case "entrydescription":
			if (currentTwitchActionEntry != null)
			{
				value = currentTwitchActionEntry.Description;
			}
			else if (currentTwitchVoteEntry != null)
			{
				value = currentTwitchVoteEntry.Description;
			}
			else if (currentTwitchActionHistoryEntry != null)
			{
				value = currentTwitchActionHistoryEntry.Description;
			}
			else
			{
				value = "";
			}
			return true;
		case "showempty":
			value = (currentTwitchActionEntry == null && currentTwitchVoteEntry == null && currentTwitchActionHistoryEntry == null && OwnerList != null && OwnerList.CurrentType != XUiC_TwitchEntryListWindow.ListTypes.Leaderboard).ToString();
			return true;
		case "showstats":
			value = (currentTwitchActionEntry != null || currentTwitchVoteEntry != null || currentTwitchActionHistoryEntry != null || (OwnerList != null && OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)).ToString();
			return true;
		case "showenable":
			value = (currentTwitchActionEntry != null || currentTwitchVoteEntry != null).ToString();
			return true;
		case "emptytext":
			value = "";
			if (OwnerList != null)
			{
				switch (OwnerList.CurrentType)
				{
				case XUiC_TwitchEntryListWindow.ListTypes.Actions:
					value = lblActionEmpty;
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.Votes:
					value = lblVoteEmpty;
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.ActionHistory:
					value = lblActionHistoryEmpty;
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.Leaderboard:
					value = lblLeaderboardEmpty;
					break;
				}
			}
			return true;
		case "actioncommand":
			if (currentTwitchActionEntry != null)
			{
				value = currentTwitchActionEntry.Command;
			}
			else if (currentTwitchActionHistoryEntry != null)
			{
				value = currentTwitchActionHistoryEntry.UserName;
			}
			else
			{
				value = "";
			}
			return true;
		case "actiongamestagetitle":
			value = lblStartGamestage;
			return true;
		case "actiongamestage":
			value = ((currentTwitchActionEntry != null) ? currentTwitchActionEntry.StartGameStage.ToString() : "");
			return true;
		case "actiondefaultcosttitle":
			value = lblPointCost;
			return true;
		case "actiondefaultcost":
			if (currentTwitchActionEntry != null)
			{
				value = currentTwitchActionEntry.ModifiedCost.ToString();
			}
			else if (currentTwitchActionHistoryEntry != null)
			{
				value = currentTwitchActionHistoryEntry.PointsSpent.ToString();
			}
			else
			{
				value = "";
			}
			return true;
		case "actioncostcolor":
			value = "222,206,163,255";
			if (currentTwitchActionEntry != null)
			{
				int num = currentTwitchActionEntry.ModifiedCost - currentTwitchActionEntry.DefaultCost;
				if (num != 0)
				{
					if (num > 0)
					{
						value = "255,0,0,255";
					}
					else if (num < 0)
					{
						value = "0,255,0,255";
					}
				}
			}
			return true;
		case "actiondiscountcost":
			if (currentTwitchActionEntry != null && TwitchManager.Current.BitPriceMultiplier != 1f && currentTwitchActionEntry.PointType == TwitchAction.PointTypes.Bits && !currentTwitchActionEntry.IgnoreDiscount)
			{
				value = $"{currentTwitchActionEntry.GetModifiedDiscountCost()} {lblPointsBits}";
			}
			else
			{
				value = "";
			}
			return true;
		case "actiondiscountcosttitle":
			if (currentTwitchActionEntry != null && TwitchManager.Current.BitPriceMultiplier != 1f && currentTwitchActionEntry.PointType == TwitchAction.PointTypes.Bits && !currentTwitchActionEntry.IgnoreDiscount)
			{
				value = lblDiscountCost;
			}
			else
			{
				value = "";
			}
			return true;
		case "actioncooldowntitle":
			value = lblCooldown;
			return true;
		case "actioncooldown":
			value = ((currentTwitchActionEntry != null) ? XUiM_PlayerBuffs.ConvertToTimeString(currentTwitchActionEntry.Cooldown) : "");
			return true;
		case "actionrandomgrouptitle":
			value = lblRandomDaily;
			return true;
		case "actionrandomgroup":
			value = ((currentTwitchActionEntry == null) ? "" : ((currentTwitchActionEntry.RandomGroup != "") ? lblTrue : lblFalse));
			return true;
		case "actionispositivetitle":
			value = lblIsPositive;
			return true;
		case "actionispositive":
			value = ((currentTwitchActionEntry == null) ? "" : (currentTwitchActionEntry.IsPositive ? lblTrue : lblFalse));
			return true;
		case "actionpointtypetitle":
			value = lblPointType;
			return true;
		case "actionpointcost":
			if (currentTwitchActionEntry != null)
			{
				switch (currentTwitchActionEntry.PointType)
				{
				case TwitchAction.PointTypes.PP:
					value = $"{currentTwitchActionEntry.ModifiedCost} {lblPointsPP}";
					break;
				case TwitchAction.PointTypes.SP:
					value = $"{currentTwitchActionEntry.ModifiedCost} {lblPointsSP}";
					break;
				case TwitchAction.PointTypes.Bits:
					value = $"{currentTwitchActionEntry.ModifiedCost} {lblPointsBits}";
					break;
				}
			}
			else if (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.Action != null)
			{
				value = GetHistoryPointCost();
			}
			else
			{
				value = "";
			}
			return true;
		case "historytargettitle":
			value = lblHistoryTargetTitle;
			return true;
		case "historytarget":
			if (currentTwitchActionHistoryEntry != null)
			{
				value = currentTwitchActionHistoryEntry.Target;
			}
			else
			{
				value = "";
			}
			return true;
		case "historystatetitle":
			value = lblHistoryStateTitle;
			return true;
		case "historystate":
			value = ((currentTwitchActionHistoryEntry != null) ? currentTwitchActionHistoryEntry.EntryState.ToString() : "");
			return true;
		case "historytimestamptitle":
			value = lblHistoryTimeStampTitle;
			return true;
		case "historytimestamp":
			value = ((currentTwitchActionHistoryEntry != null) ? currentTwitchActionHistoryEntry.ActionTime : "");
			return true;
		case "entryicon":
			value = "";
			if (currentTwitchActionEntry != null && currentTwitchActionEntry.MainCategory != null)
			{
				value = currentTwitchActionEntry.MainCategory.Icon;
			}
			else if (currentTwitchVoteEntry != null && currentTwitchVoteEntry.MainVoteType != null)
			{
				value = currentTwitchVoteEntry.MainVoteType.Icon;
			}
			else if (currentTwitchActionHistoryEntry != null)
			{
				if (currentTwitchActionHistoryEntry.Action != null)
				{
					TwitchAction action = currentTwitchActionHistoryEntry.Action;
					if (action.MainCategory != null)
					{
						value = action.MainCategory.Icon;
					}
				}
				else if (currentTwitchActionHistoryEntry.Vote != null)
				{
					TwitchVoteType mainVoteType = currentTwitchActionHistoryEntry.Vote.MainVoteType;
					if (mainVoteType != null)
					{
						value = mainVoteType.Icon;
					}
				}
			}
			return true;
		case "enablebuttontext":
			value = "";
			if (currentTwitchActionEntry != null)
			{
				if (currentTwitchActionEntry.IsInPreset(TwitchManager.Current.CurrentActionPreset))
				{
					value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblDisableAction : lblDisableAction_Controller);
				}
				else
				{
					value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblEnableAction : lblEnableAction_Controller);
				}
			}
			else if (currentTwitchVoteEntry != null)
			{
				if (currentTwitchVoteEntry.Enabled)
				{
					value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblDisableVote : lblDisableVote_Controller);
				}
				else
				{
					value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblEnableVote : lblEnableVote_Controller);
				}
			}
			return true;
		case "increasepricetext":
			value = "";
			if (currentTwitchActionEntry != null)
			{
				value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblIncreasePrice : lblIncreasePrice_Controller);
			}
			return true;
		case "decreasepricetext":
			value = "";
			if (currentTwitchActionEntry != null)
			{
				value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? lblDecreasePrice : lblDecreasePrice_Controller);
			}
			return true;
		case "showaction":
			value = (currentTwitchActionEntry != null).ToString();
			return true;
		case "showvote":
			value = (currentTwitchVoteEntry != null).ToString();
			return true;
		case "showhistory":
			value = (currentTwitchActionHistoryEntry != null).ToString();
			return true;
		case "showhistory_action":
			value = (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.Action != null).ToString();
			return true;
		case "showhistory_vote":
			value = (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.Vote != null).ToString();
			return true;
		case "showhistory_event":
			value = (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.EventEntry != null).ToString();
			return true;
		case "showhistory_retry":
			value = (currentTwitchActionHistoryEntry != null && (currentTwitchActionHistoryEntry.Action != null || currentTwitchActionHistoryEntry.EventEntry != null)).ToString();
			return true;
		case "enablerefund":
			value = (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.CanRefund()).ToString();
			return true;
		case "refundtext":
			if (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.CanRefund())
			{
				value = string.Format(lblRefund, GetHistoryPointCost());
			}
			else
			{
				value = lblNoRefund;
			}
			return true;
		case "enableretry":
			value = (currentTwitchActionHistoryEntry != null && currentTwitchActionHistoryEntry.CanRetry()).ToString();
			return true;
		case "retrytext":
			if (currentTwitchActionHistoryEntry != null)
			{
				if (currentTwitchActionHistoryEntry.CanRetry())
				{
					value = lblRetry;
				}
				else
				{
					value = (currentTwitchActionHistoryEntry.HasRetried ? lblNoRetry : lblRetryActionUnavailable);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "votestartgamestagetitle":
			value = lblStartGamestage;
			return true;
		case "votestartgamestage":
			if (currentTwitchVoteEntry != null && currentTwitchVoteEntry.StartGameStage > 0)
			{
				value = currentTwitchVoteEntry.StartGameStage.ToString();
			}
			else
			{
				value = "";
			}
			return true;
		case "voteendgamestagetitle":
			value = lblEndGamestage;
			return true;
		case "voteendgamestage":
			if (currentTwitchVoteEntry != null && currentTwitchVoteEntry.EndGameStage > 0)
			{
				value = currentTwitchVoteEntry.EndGameStage.ToString();
			}
			else
			{
				value = "";
			}
			return true;
		case "showleaderboard":
			value = (OwnerList != null && OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard).ToString();
			return true;
		case "leaderboard_sessionkiller":
			value = ((stats != null && stats.TopKillerViewer != null) ? $"[{stats.TopKillerViewer.UserColor}]{stats.TopKillerViewer.Name}[-] ({stats.TopKillerViewer.Kills})" : "--");
			return true;
		case "sessionkiller_title":
			value = lblTopKiller;
			return true;
		case "sessiongood_title":
			value = lblTopGood;
			return true;
		case "sessionevil_title":
			value = lblTopEvil;
			return true;
		case "currentgood_title":
			value = ((stats != null) ? string.Format(lblCurrentGood, stats.GoodRewardTime) : "");
			return true;
		case "sessionmostbits_title":
			value = lblMostBits;
			return true;
		case "sessiontotalbits_title":
			value = lblTotalBits;
			return true;
		case "sessiontotalgood_title":
			value = lblTotalGood;
			return true;
		case "sessiontotalbad_title":
			value = lblTotalBad;
			return true;
		case "sessiontotalactions_title":
			value = lblTotalActions;
			return true;
		case "sessionlargestpimppot_title":
			value = lblLargestPimpPot;
			return true;
		case "leaderboard_currentgood":
			value = ((stats != null && stats.CurrentGoodViewer != null) ? $"[{stats.CurrentGoodViewer.UserColor}]{stats.CurrentGoodViewer.Name}[-] ({stats.CurrentGoodViewer.CurrentGoodActions})" : "--");
			return true;
		case "leaderboard_goodperson":
			value = ((stats != null && stats.TopGoodViewer != null) ? $"[{stats.TopGoodViewer.UserColor}]{stats.TopGoodViewer.Name}[-] ({stats.TopGoodViewer.GoodActions})" : "--");
			return true;
		case "leaderboard_badperson":
			value = ((stats != null && stats.TopBadViewer != null) ? $"[{stats.TopBadViewer.UserColor}]{stats.TopBadViewer.Name}[-] ({stats.TopBadViewer.BadActions})" : "--");
			return true;
		case "leaderboard_totalgood":
			value = string.Format("[AFAFFF]{0}[-]", (stats != null) ? stats.TotalGood.ToString() : "0");
			return true;
		case "leaderboard_totalbad":
			value = string.Format("[FFAFAF]{0}[-]", (stats != null) ? stats.TotalBad.ToString() : "0");
			return true;
		case "leaderboard_mostbits":
			value = ((stats != null && stats.MostBitsSpentViewer != null) ? $"[{stats.MostBitsSpentViewer.UserColor}]{stats.MostBitsSpentViewer.Name}[-] ({stats.MostBitsSpentViewer.BitsUsed})" : "--");
			return true;
		case "leaderboard_totalbits":
			if (showBitTotal)
			{
				value = ((stats != null) ? stats.TotalBits.ToString() : "0");
			}
			else
			{
				value = $"<{lblShowBitTotal}>";
			}
			return true;
		case "leaderboard_totalactions":
			value = ((stats != null) ? stats.TotalActions.ToString() : "0");
			return true;
		case "leaderboard_largestpot":
		{
			string arg = ((TwitchManager.Current.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP") : Localization.Get("TwitchPoints_PP"));
			value = $"{((stats != null) ? stats.LargestPimpPot : 0)} {arg}";
			return true;
		}
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetHistoryPointCost()
	{
		if (currentTwitchActionHistoryEntry == null || currentTwitchActionHistoryEntry.Action == null)
		{
			return "";
		}
		return currentTwitchActionHistoryEntry.Action.PointType switch
		{
			TwitchAction.PointTypes.PP => $"{currentTwitchActionHistoryEntry.PointsSpent} {lblPointsPP}", 
			TwitchAction.PointTypes.SP => $"{currentTwitchActionHistoryEntry.PointsSpent} {lblPointsSP}", 
			TwitchAction.PointTypes.Bits => $"{currentTwitchActionHistoryEntry.PointsSpent} {lblPointsBits}", 
			_ => "", 
		};
	}

	public void SetTwitchAction(XUiC_TwitchActionEntry twitchInfoEntry)
	{
		actionEntry = twitchInfoEntry;
		voteEntry = null;
		CurrentTwitchVoteEntry = null;
		historyEntry = null;
		CurrentTwitchActionHistoryEntry = null;
		if (actionEntry != null)
		{
			CurrentTwitchActionEntry = actionEntry.Action;
		}
		else
		{
			CurrentTwitchActionEntry = null;
		}
	}

	public void SetTwitchVote(XUiC_TwitchVoteInfoEntry twitchInfoEntry)
	{
		voteEntry = twitchInfoEntry;
		actionEntry = null;
		CurrentTwitchActionEntry = null;
		historyEntry = null;
		CurrentTwitchActionHistoryEntry = null;
		if (voteEntry != null)
		{
			CurrentTwitchVoteEntry = voteEntry.Vote;
		}
		else
		{
			CurrentTwitchVoteEntry = null;
		}
	}

	public void SetTwitchHistory(XUiC_TwitchActionHistoryEntry twitchInfoEntry)
	{
		historyEntry = twitchInfoEntry;
		actionEntry = null;
		CurrentTwitchActionEntry = null;
		voteEntry = null;
		CurrentTwitchVoteEntry = null;
		if (historyEntry != null)
		{
			CurrentTwitchActionHistoryEntry = historyEntry.HistoryItem;
		}
		else
		{
			CurrentTwitchActionHistoryEntry = null;
		}
	}

	public void ClearEntries()
	{
		actionEntry = null;
		CurrentTwitchActionEntry = null;
		voteEntry = null;
		CurrentTwitchVoteEntry = null;
		historyEntry = null;
		CurrentTwitchActionHistoryEntry = null;
	}
}
