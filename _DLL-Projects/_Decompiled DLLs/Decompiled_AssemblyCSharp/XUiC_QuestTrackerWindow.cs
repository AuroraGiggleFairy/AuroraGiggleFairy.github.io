using Challenges;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTrackerWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTrackerObjectiveList objectiveList;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge currentChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt trackerheightFormatter = new CachedStringFormatterInt();

	public Quest CurrentQuest
	{
		get
		{
			return currentQuest;
		}
		set
		{
			currentQuest = value;
			questClass = ((value != null) ? QuestClass.GetQuest(currentQuest.ID) : null);
			if (value != null)
			{
				if (currentChallenge != null)
				{
					currentChallenge.OnChallengeStateChanged -= CurrentChallenge_OnChallengeStateChanged;
				}
				currentChallenge = null;
				challengeClass = null;
			}
			RefreshBindings(_forceAll: true);
		}
	}

	public Challenge CurrentChallenge
	{
		get
		{
			return currentChallenge;
		}
		set
		{
			if (currentChallenge != null)
			{
				currentChallenge.OnChallengeStateChanged -= CurrentChallenge_OnChallengeStateChanged;
			}
			currentChallenge = value;
			challengeClass = ((value != null) ? currentChallenge.ChallengeClass : null);
			if (value != null)
			{
				currentQuest = null;
				questClass = null;
				currentChallenge.OnChallengeStateChanged += CurrentChallenge_OnChallengeStateChanged;
			}
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentChallenge_OnChallengeStateChanged(Challenge challenge)
	{
		RefreshBindings();
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		objectiveList = GetChildByType<XUiC_QuestTrackerObjectiveList>();
		RegisterForInputStyleChanges();
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
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && localPlayer.IsDead())
			{
				IsDirty = true;
			}
			else if (!base.ViewComponent.IsVisible && !localPlayer.IsDead())
			{
				IsDirty = true;
			}
			if (currentChallenge != null && currentChallenge.UIDirty)
			{
				IsDirty = true;
				currentChallenge.UIDirty = false;
			}
			if (IsDirty)
			{
				objectiveList.Quest = currentQuest;
				objectiveList.Challenge = currentChallenge;
				RefreshBindings(_forceAll: true);
				IsDirty = false;
			}
		}
		else
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.QuestTracker.OnTrackedQuestChanged += QuestTracker_OnTrackedQuestChanged;
		base.xui.playerUI.entityPlayer.QuestChanged += QuestJournal_QuestChanged;
		base.xui.QuestTracker.OnTrackedChallengeChanged += QuestTracker_OnTrackedChallengeChanged;
		base.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
		if (base.xui.QuestTracker.TrackedQuest != null)
		{
			CurrentQuest = base.xui.QuestTracker.TrackedQuest;
		}
		else if (base.xui.QuestTracker.TrackedChallenge != null)
		{
			CurrentChallenge = base.xui.QuestTracker.TrackedChallenge;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (XUi.IsGameRunning())
		{
			base.xui.QuestTracker.OnTrackedQuestChanged -= QuestTracker_OnTrackedQuestChanged;
			base.xui.playerUI.entityPlayer.QuestChanged -= QuestJournal_QuestChanged;
			base.xui.QuestTracker.OnTrackedChallengeChanged -= QuestTracker_OnTrackedChallengeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedQuestChanged()
	{
		CurrentQuest = base.xui.QuestTracker.TrackedQuest;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedChallengeChanged()
	{
		CurrentChallenge = base.xui.QuestTracker.TrackedChallenge;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		if (CurrentQuest == q)
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "questname":
			if (currentQuest != null)
			{
				value = questClass.Name;
			}
			else if (currentChallenge != null)
			{
				value = challengeClass.Title;
			}
			else
			{
				value = "";
			}
			return true;
		case "questtitle":
			if (currentQuest != null)
			{
				value = questClass.SubTitle;
			}
			else if (currentChallenge != null)
			{
				value = challengeClass.Title;
			}
			else
			{
				value = "";
			}
			return true;
		case "questicon":
			if (currentQuest != null)
			{
				value = questClass.Icon;
			}
			else if (currentChallenge != null)
			{
				value = challengeClass.Icon;
			}
			else
			{
				value = "";
			}
			return true;
		case "showquest":
			value = ((currentQuest != null || currentChallenge != null) && XUi.IsGameRunning() && localPlayer != null && !localPlayer.IsDead()).ToString();
			return true;
		case "showempty":
			value = (currentQuest == null && currentChallenge == null).ToString();
			return true;
		case "questhintavailable":
			if (currentQuest != null)
			{
				value = (questClass.GetCurrentHint(currentQuest.CurrentPhase) != "").ToString();
			}
			else if (currentChallenge != null)
			{
				value = (challengeClass.GetHint(currentChallenge.NeedsPreRequisites) != "").ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "questhintposition":
			if (currentQuest != null)
			{
				value = $"0,{-50 + currentQuest.ActiveObjectives * -27}";
			}
			else if (currentChallenge != null)
			{
				value = $"0,{-50 + currentChallenge.ActiveObjectives * -27}";
			}
			else
			{
				value = "0,0";
			}
			return true;
		case "questhint":
			if (currentQuest != null)
			{
				value = questClass.GetCurrentHint(currentQuest.CurrentPhase);
			}
			else if (currentChallenge != null)
			{
				value = challengeClass.GetHint(currentChallenge.NeedsPreRequisites);
			}
			else
			{
				value = "";
			}
			return true;
		case "trackerheight":
			if (currentQuest != null)
			{
				value = trackerheightFormatter.Format(currentQuest.ActiveObjectives * 27);
			}
			else if (currentChallenge != null)
			{
				value = trackerheightFormatter.Format(currentChallenge.ActiveObjectives * 27);
			}
			else
			{
				value = "0";
			}
			return true;
		default:
			return false;
		}
	}
}
