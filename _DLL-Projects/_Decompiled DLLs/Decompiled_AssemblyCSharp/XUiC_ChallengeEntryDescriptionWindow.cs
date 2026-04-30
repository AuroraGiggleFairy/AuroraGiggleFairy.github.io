using Audio;
using Challenges;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryDescriptionWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge currentChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTrack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController gotoButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string DayTimeFormatString = Localization.Get("xuiDay") + " {0}, {1:00}:{2:00}";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, DayTimeFormatString));

	public Challenge CurrentChallengeEntry
	{
		get
		{
			return currentChallenge;
		}
		set
		{
			currentChallenge = value;
			challengeClass = ((currentChallenge != null) ? currentChallenge.ChallengeClass : null);
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		btnTrack = GetChildById("btnTrack").GetChildByType<XUiC_SimpleButton>();
		btnTrack.OnPressed += BtnTrack_OnPressed;
		btnComplete = GetChildById("btnComplete").GetChildByType<XUiC_SimpleButton>();
		btnComplete.OnPressed += BtnComplete_OnPressed;
		gotoButton = GetChildById("gotoButton");
		if (gotoButton != null)
		{
			gotoButton.OnPress += GotoButton_OnPress;
		}
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GotoButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (currentChallenge != null)
		{
			BaseChallengeObjective navObjective = currentChallenge.GetNavObjective();
			if (navObjective is ChallengeObjectiveCraft challengeObjectiveCraft)
			{
				_ = challengeObjectiveCraft.itemRecipe;
				XUiC_RecipeList xUiC_RecipeList = null;
				XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "crafting");
				base.xui.GetChildByType<XUiC_RecipeList>()?.SetRecipeDataByItem(challengeObjectiveCraft.itemRecipe.itemValueType);
			}
			else if (navObjective is ChallengeObjectiveTwitch)
			{
				XUiC_TwitchWindowSelector.OpenSelectorAndWindow(GameManager.Instance.World.GetPrimaryPlayer(), "Actions", _extras: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty || (currentChallenge != null && currentChallenge.NeedsUIUpdate))
		{
			RefreshBindings();
			IsDirty = false;
		}
		if (!base.ViewComponent.UiTransform.gameObject.activeInHierarchy || currentChallenge == null)
		{
			return;
		}
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !base.xui.playerUI.windowManager.IsInputActive())
		{
			if (gUIActions.DPad_Up.WasPressed)
			{
				BtnComplete_OnPressed(btnComplete, -1);
			}
			if (gUIActions.DPad_Left.WasPressed)
			{
				BtnTrack_OnPressed(btnTrack, -1);
			}
		}
	}

	public void TrackCurrentChallenege()
	{
		if (currentChallenge.IsActive)
		{
			if (base.xui.QuestTracker.TrackedChallenge == currentChallenge)
			{
				base.xui.QuestTracker.TrackedChallenge = null;
			}
			else
			{
				base.xui.QuestTracker.TrackedChallenge = currentChallenge;
				Manager.PlayInsidePlayerHead("ui_challenge_track");
			}
			entry.Owner.MarkDirty();
		}
	}

	public void CompleteCurrentChallenege()
	{
		if (currentChallenge != null && currentChallenge.ReadyToComplete)
		{
			currentChallenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
			currentChallenge.Redeem();
			QuestEventManager.Current.ChallengeCompleted(challengeClass, isRedeemed: true);
			currentChallenge = currentChallenge.Owner.GetNextRedeemableChallenge(currentChallenge);
			XUiC_ChallengeEntry selectedEntry = entry.Owner.SelectedEntry;
			if (selectedEntry != null && selectedEntry.Entry != currentChallenge)
			{
				entry.Owner.SetEntryByChallenge(currentChallenge);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTrack_OnPressed(XUiController _sender, int _mouseButton)
	{
		TrackCurrentChallenege();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnComplete_OnPressed(XUiController _sender, int _mouseButton)
	{
		CompleteCurrentChallenege();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
		RefreshButtonLabels(PlatformManager.NativePlatform.Input.CurrentInputStyle);
		QuestEventManager.Current.ChallengeComplete += Current_ChallengeComplete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ChallengeComplete(ChallengeClass challenge, bool isRedeemed)
	{
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		QuestEventManager.Current.ChallengeComplete -= Current_ChallengeComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "entrytitle":
			value = ((currentChallenge != null) ? challengeClass.Title : "");
			return true;
		case "entrygroup":
			value = ((currentChallenge != null) ? currentChallenge.ChallengeGroup.Title : "");
			return true;
		case "entryshortdescription":
			value = ((currentChallenge != null) ? challengeClass.ShortDescription : "");
			return true;
		case "entrydescription":
			value = ((currentChallenge != null) ? challengeClass.GetDescription() : "");
			return true;
		case "entryicon":
			value = ((currentChallenge != null) ? challengeClass.Icon : "");
			return true;
		case "rewardtitle":
			value = Localization.Get("xuiRewards");
			return true;
		case "rewardtext":
			value = ((currentChallenge != null) ? challengeClass.RewardText : "");
			return true;
		case "hasreward":
			value = ((currentChallenge != null) ? (challengeClass.RewardEvent != "").ToString() : "false");
			return true;
		case "objective1":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 0) ? currentChallenge.ObjectiveList[0].ObjectiveText : "");
			}
			else
			{
				value = "";
			}
			return true;
		case "objective2":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 1) ? currentChallenge.ObjectiveList[1].ObjectiveText : "");
			}
			else
			{
				value = "";
			}
			return true;
		case "objective3":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 2) ? currentChallenge.ObjectiveList[2].ObjectiveText : "");
			}
			else
			{
				value = "";
			}
			return true;
		case "objective4":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 3) ? currentChallenge.ObjectiveList[3].ObjectiveText : "");
			}
			else
			{
				value = "";
			}
			return true;
		case "objective5":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 4) ? currentChallenge.ObjectiveList[4].ObjectiveText : "");
			}
			else
			{
				value = "";
			}
			return true;
		case "hasobjective1":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count > 0).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "hasobjective2":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count > 1).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "hasobjective3":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count > 2).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "hasobjective4":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count > 3).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "hasobjective5":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count > 4).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "has1objective":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count == 1).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "has2objective":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count == 2).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "has3objective":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count == 3).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "has4objective":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count == 4).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "has5objective":
			if (currentChallenge != null)
			{
				value = (currentChallenge.ObjectiveList.Count == 5).ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "objectivefill1":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 0) ? currentChallenge.ObjectiveList[0].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		case "objectivefill2":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 1) ? currentChallenge.ObjectiveList[1].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		case "objectivefill3":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 2) ? currentChallenge.ObjectiveList[2].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		case "objectivefill4":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 3) ? currentChallenge.ObjectiveList[3].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		case "objectivefill5":
			if (currentChallenge != null)
			{
				value = ((currentChallenge.ObjectiveList.Count > 4) ? currentChallenge.ObjectiveList[4].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		case "adjustedheight":
			if (currentChallenge != null)
			{
				switch (currentChallenge.ObjectiveList.Count)
				{
				case 1:
					value = "276";
					return true;
				case 2:
					value = "236";
					return true;
				case 3:
					value = "196";
					return true;
				case 4:
					value = "156";
					return true;
				case 5:
					value = "116";
					return true;
				}
			}
			value = "196";
			return true;
		case "showempty":
			value = (currentChallenge == null).ToString();
			return true;
		case "haschallenge":
			value = (currentChallenge != null).ToString();
			return true;
		case "enabletrack":
			value = (currentChallenge != null && currentChallenge.ChallengeState == Challenge.ChallengeStates.Active).ToString();
			return true;
		case "enableredeem":
			value = (currentChallenge != null && currentChallenge.ReadyToComplete).ToString();
			return true;
		case "showgoto":
			value = (currentChallenge != null && currentChallenge.ChallengeClass.HasNavType).ToString();
			return true;
		default:
			return false;
		}
	}

	public void SetChallenge(XUiC_ChallengeEntry challengeEntry)
	{
		entry = challengeEntry;
		if (entry != null)
		{
			CurrentChallengeEntry = entry.Entry;
		}
		else
		{
			CurrentChallengeEntry = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		RefreshButtonLabels(_style);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtonLabels(PlayerInputManager.InputStyle _style)
	{
		if (_style == PlayerInputManager.InputStyle.Keyboard)
		{
			(btnTrack.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalTrack"), LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Left.GetBindingString(_forController: false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets));
			(btnComplete.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalComplete"), LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Up.GetBindingString(_forController: false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets));
		}
		else
		{
			(btnTrack.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalTrack"), LocalPlayerUI.primaryUI.playerInput.GUIActions.HalfStack.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle));
			(btnComplete.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalComplete"), LocalPlayerUI.primaryUI.playerInput.GUIActions.Inspect.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle));
		}
	}
}
