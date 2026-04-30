using System.Collections.Generic;
using Twitch;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchEntryListWindow : XUiController
{
	public enum ListTypes
	{
		Actions,
		Votes,
		ActionHistory,
		Leaderboard
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntryList twitchActionEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchVoteInfoEntryList twitchVoteInfoEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionHistoryEntryList twitchActionHistoryEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchLeaderboardEntryList twitchLeaderboardEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchAction> currentActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVote> currentVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionHistoryEntry> currentRedemptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchLeaderboardEntry> currentLeaderboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList voteCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList actionHistoryCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList leaderboardCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInitialized;

	public ListTypes CurrentType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionHistory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAllActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button allActionsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public float UpdateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager tm;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceExtras;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ActionCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string VoteCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ActionHistoryCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LeaderboardCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		twitchActionEntryList = GetChildByType<XUiC_TwitchActionEntryList>();
		twitchActionEntryList.TwitchEntryListWindow = this;
		twitchVoteInfoEntryList = GetChildByType<XUiC_TwitchVoteInfoEntryList>();
		twitchVoteInfoEntryList.TwitchEntryListWindow = this;
		twitchActionHistoryEntryList = GetChildByType<XUiC_TwitchActionHistoryEntryList>();
		twitchActionHistoryEntryList.TwitchEntryListWindow = this;
		twitchLeaderboardEntryList = GetChildByType<XUiC_TwitchLeaderboardEntryList>();
		twitchLeaderboardEntryList.TwitchEntryListWindow = this;
		categoryList = (XUiC_CategoryList)windowGroup.Controller.GetChildById("actioncategories");
		categoryList.CategoryChanged += HandleCategoryChanged;
		voteCategoryList = (XUiC_CategoryList)windowGroup.Controller.GetChildById("votecategories");
		voteCategoryList.CategoryChanged += HandleVoteCategoryChanged;
		actionHistoryCategoryList = (XUiC_CategoryList)windowGroup.Controller.GetChildById("actionHistoryCategories");
		actionHistoryCategoryList.CategoryChanged += HandleActionHistoryCategoryChanged;
		leaderboardCategoryList = (XUiC_CategoryList)windowGroup.Controller.GetChildById("leaderboardCategories");
		leaderboardCategoryList.CategoryChanged += HandleLeaderboardCategoryChanged;
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("searchInput");
		XUiController childById = GetChildById("allactions");
		if (childById != null)
		{
			allActionsButton = childById.ViewComponent as XUiV_Button;
			if (allActionsButton != null)
			{
				childById.OnPress += AllActionsButtonCtrl_OnPress;
			}
		}
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
		ActionCategory = "";
		lblActions = Localization.Get("TwitchInfo_Actions");
		lblVotes = Localization.Get("TwitchInfo_Votes");
		lblActionHistory = Localization.Get("TwitchInfo_ActionHistory");
		lblLeaderboard = Localization.Get("TwitchInfo_Leaderboard");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AllActionsButtonCtrl_OnPress(XUiController _sender, int _mouseButton)
	{
		showAllActions = !showAllActions;
		allActionsButton.Selected = showAllActions;
		IsDirty = true;
	}

	public void SetOpenToActions(bool openExtras = false)
	{
		CurrentType = ListTypes.Actions;
		IsDirty = true;
		forceExtras = openExtras;
	}

	public void SetOpenToLeaderboard()
	{
		CurrentType = ListTypes.Leaderboard;
		IsDirty = true;
	}

	public void SetOpenToVotes()
	{
		CurrentType = ListTypes.Votes;
		IsDirty = true;
	}

	public void SetOpenToHistory()
	{
		CurrentType = ListTypes.ActionHistory;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		ActionCategory = _categoryEntry.CategoryName;
		HandleFilterActions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFilterActions()
	{
		if (currentActions != null && CurrentType == ListTypes.Actions)
		{
			TwitchActionPreset currentPreset = tm.CurrentActionPreset;
			List<TwitchAction> newActionEntryList = (from entry in currentActions
				where (filterText == "" || entry.Title.ContainsCaseInsensitive(filterText) || entry.Command.ContainsCaseInsensitive(filterText)) && ((ActionCategory == "" && entry.DisplayCategory == entry.MainCategory) || entry.DisplayCategory.Name == ActionCategory) && entry.ShowInActionList && (currentPreset.AllowPointGeneration || entry.PointType == TwitchAction.PointTypes.Bits)
				orderby entry.Title
				select entry).ToList();
			twitchActionEntryList.SetTwitchActionList(newActionEntryList, tm.CurrentActionPreset);
			RefreshBindings();
		}
		else
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleVoteCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		VoteCategory = _categoryEntry.CategoryName;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleActionHistoryCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		if (_categoryEntry.CategoryName != ActionHistoryCategory)
		{
			ActionHistoryCategory = _categoryEntry.CategoryName;
			twitchActionHistoryEntryList.SelectedEntry = null;
			twitchActionHistoryEntryList.setFirstEntry = true;
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLeaderboardCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		LeaderboardCategory = _categoryEntry.CategoryName;
		IsDirty = true;
		UpdateDelay = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		filterText = _text;
		if (CurrentType == ListTypes.Actions)
		{
			HandleFilterActions();
		}
		else
		{
			IsDirty = true;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (UpdateDelay > 0f)
		{
			UpdateDelay -= _dt;
		}
		else
		{
			if (!IsDirty)
			{
				return;
			}
			if (tm == null)
			{
				tm = TwitchManager.Current;
			}
			switch (CurrentType)
			{
			case ListTypes.Actions:
			{
				currentActions = (from entry in TwitchActionManager.TwitchActions.Values.ToList()
					where entry != null && entry.ShowInActionList && (entry.IsInPresetForList(tm.CurrentActionPreset) || (showAllActions && !entry.OnlyShowInPreset) || (entry.DisplayCategory != null && entry.DisplayCategory.AlwaysShowInMenu))
					orderby entry.Title
					select entry).ToList();
				twitchActionEntryList.SetTwitchActionList(currentActions, tm.CurrentActionPreset);
				string text = ((categoryList.CurrentCategory != null) ? categoryList.CurrentCategory.CategoryName : "");
				categoryList.SetupCategoriesBasedOnTwitchActions(currentActions);
				if (forceExtras)
				{
					text = "Extras";
					forceExtras = false;
				}
				if (text == "")
				{
					categoryList.SetCategoryToFirst();
					break;
				}
				categoryList.SetCategory(text);
				if (categoryList.CurrentCategory == null || categoryList.CurrentCategory.CategoryName == "")
				{
					categoryList.SetCategoryToFirst();
				}
				break;
			}
			case ListTypes.Votes:
				currentVotes = (from entry in TwitchActionManager.TwitchVotes.Values.ToList()
					where (filterText == "" || entry.Display.ContainsCaseInsensitive(filterText)) && (VoteCategory == "" || (entry.MainVoteType != null && entry.MainVoteType.Name == VoteCategory && entry.MainVoteType.IsInPreset(tm.CurrentVotePreset.Name))) && entry.IsInPreset(tm.CurrentVotePreset)
					orderby entry.VoteDescription
					select entry).ToList();
				twitchVoteInfoEntryList.SetTwitchVoteList(currentVotes);
				break;
			case ListTypes.ActionHistory:
			{
				List<TwitchActionHistoryEntry> list = null;
				switch (ActionHistoryCategory)
				{
				case "action":
					list = tm.ActionHistory;
					break;
				case "vote":
					list = tm.VoteHistory;
					break;
				case "event":
					list = tm.EventHistory;
					break;
				}
				if (list != null)
				{
					currentRedemptions = list.Where([PublicizedFrom(EAccessModifier.Private)] (TwitchActionHistoryEntry entry) => entry != null && entry.IsValid() && (filterText == "" || entry.UserName.ContainsCaseInsensitive(filterText) || entry.Action.Command.ContainsCaseInsensitive(filterText)) && (ActionHistoryCategory == "" || ActionHistoryCategory == entry.HistoryType)).ToList();
					twitchActionHistoryEntryList.SetTwitchActionHistoryList(currentRedemptions);
				}
				break;
			}
			case ListTypes.Leaderboard:
				switch ((leaderboardCategoryList == null) ? "" : leaderboardCategoryList.CurrentCategory.CategoryName)
				{
				case "global_kills":
					currentLeaderboard = (from entry in tm.Leaderboard
						where filterText == "" || entry.UserName.ContainsCaseInsensitive(filterText)
						orderby entry.Kills descending
						select entry).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				case "session_kills":
				{
					Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries4 = TwitchManager.LeaderboardStats.StatEntries;
					currentLeaderboard = (from viewer in statEntries4.Values
						where (filterText == "" || viewer.Name.ContainsCaseInsensitive(filterText)) && viewer.Kills > 0
						orderby viewer.Kills descending
						select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.Kills)).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				}
				case "session_good":
				{
					Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries3 = TwitchManager.LeaderboardStats.StatEntries;
					currentLeaderboard = (from viewer in statEntries3.Values
						where (filterText == "" || viewer.Name.ContainsCaseInsensitive(filterText)) && viewer.GoodActions > 0
						orderby viewer.GoodActions descending
						select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.GoodActions)).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				}
				case "session_bad":
				{
					Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries2 = TwitchManager.LeaderboardStats.StatEntries;
					currentLeaderboard = (from viewer in statEntries2.Values
						where (filterText == "" || viewer.Name.ContainsCaseInsensitive(filterText)) && viewer.BadActions > 0
						orderby viewer.BadActions descending
						select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.BadActions)).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				}
				case "session_bits":
				{
					Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries5 = TwitchManager.LeaderboardStats.StatEntries;
					currentLeaderboard = (from viewer in statEntries5.Values
						where (filterText == "" || viewer.Name.ContainsCaseInsensitive(filterText)) && viewer.BitsUsed > 0
						orderby viewer.BitsUsed descending
						select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.BitsUsed)).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				}
				case "current_good":
				{
					Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries = TwitchManager.LeaderboardStats.StatEntries;
					currentLeaderboard = (from viewer in statEntries.Values
						where (filterText == "" || viewer.Name.ContainsCaseInsensitive(filterText)) && viewer.CurrentActions > 0
						orderby viewer.CurrentGoodActions descending
						select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.CurrentGoodActions)).ToList();
					twitchLeaderboardEntryList.SetTwitchLeaderboardList(currentLeaderboard);
					break;
				}
				}
				UpdateDelay = 1f;
				break;
			}
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!isInitialized)
		{
			player = base.xui.playerUI.entityPlayer;
			categoryList.SetupCategoriesBasedOnTwitchCategories(TwitchActionManager.Current.CategoryList);
			categoryList.SetCategoryToFirst();
			voteCategoryList.SetupCategoriesBasedOnTwitchVoteCategories(TwitchManager.Current.VotingManager.VoteTypes.Values.Where([PublicizedFrom(EAccessModifier.Internal)] (TwitchVoteType v) => v.IsInPreset(TwitchManager.Current.CurrentVotePreset.Name)).ToList());
			voteCategoryList.SetCategoryToFirst();
			actionHistoryCategoryList.SetCategoryEntry(0, "action", "ui_game_symbol_twitch_actions", Localization.Get("TwitchInfo_Actions"));
			actionHistoryCategoryList.SetCategoryEntry(1, "vote", "ui_game_symbol_twitch_vote", Localization.Get("TwitchInfo_Votes"));
			actionHistoryCategoryList.SetCategoryEntry(2, "event", "ui_game_symbol_twitch_custom_actions", Localization.Get("xuiOptionsTwitchEvents"));
			actionHistoryCategoryList.SetCategoryToFirst();
			leaderboardCategoryList.SetCategoryEntry(0, "global_kills", "ui_game_symbol_twitch_top_killer", Localization.Get("TwitchInfo_LeaderboardGlobalKills"));
			leaderboardCategoryList.SetCategoryEntry(1, "session_kills", "ui_game_symbol_skull", Localization.Get("TwitchInfo_LeaderboardSessionKills"));
			leaderboardCategoryList.SetCategoryEntry(2, "session_good", "ui_game_symbol_twitch_top_good", Localization.Get("TwitchInfo_LeaderboardSessionGood"));
			leaderboardCategoryList.SetCategoryEntry(3, "session_bad", "ui_game_symbol_twitch_top_bad", Localization.Get("TwitchInfo_LeaderboardSessionBad"));
			leaderboardCategoryList.SetCategoryEntry(4, "session_bits", "ui_game_symbol_twitch_bits", Localization.Get("TwitchInfo_LeaderboardSessionBits"));
			leaderboardCategoryList.SetCategoryEntry(5, "current_good", "ui_game_symbol_twitch_best_helper", Localization.Get("TwitchInfo_LeaderboardCurrentGood"));
			leaderboardCategoryList.SetCategoryToFirst();
			IsDirty = true;
			isInitialized = true;
		}
		tm = TwitchManager.Current;
		tm.ActionHistoryAdded += Tm_ActionHistoryAdded;
		tm.VoteHistoryAdded += Tm_VoteHistoryAdded;
		tm.EventHistoryAdded += Tm_EventHistoryAdded;
		TwitchManager.LeaderboardStats.LeaderboardChanged += LeaderboardStats_LeaderboardChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LeaderboardStats_LeaderboardChanged()
	{
		if (CurrentType == ListTypes.Leaderboard)
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_ActionHistoryAdded()
	{
		if (CurrentType == ListTypes.ActionHistory && ActionHistoryCategory == "action")
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_VoteHistoryAdded()
	{
		if (CurrentType == ListTypes.ActionHistory && ActionHistoryCategory == "vote")
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_EventHistoryAdded()
	{
		if (CurrentType == ListTypes.ActionHistory && ActionHistoryCategory == "event")
		{
			IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		TwitchManager current = TwitchManager.Current;
		current.ActionHistoryAdded -= Tm_ActionHistoryAdded;
		current.VoteHistoryAdded -= Tm_VoteHistoryAdded;
		current.EventHistoryAdded -= Tm_EventHistoryAdded;
		TwitchManager.LeaderboardStats.LeaderboardChanged -= LeaderboardStats_LeaderboardChanged;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "headertitle":
			switch (CurrentType)
			{
			case ListTypes.Actions:
			{
				string text2 = ((categoryList == null) ? "" : categoryList.CurrentCategory.CategoryDisplayName);
				value = ((text2 != "") ? text2 : Localization.Get("lblAll"));
				break;
			}
			case ListTypes.Votes:
			{
				string text4 = ((voteCategoryList == null) ? "" : voteCategoryList.CurrentCategory.CategoryDisplayName);
				value = ((text4 != "") ? text4 : Localization.Get("lblAll"));
				break;
			}
			case ListTypes.ActionHistory:
			{
				string text3 = ((actionHistoryCategoryList == null) ? "" : actionHistoryCategoryList.CurrentCategory.CategoryDisplayName);
				value = ((text3 != "") ? text3 : Localization.Get("lblAll"));
				break;
			}
			case ListTypes.Leaderboard:
			{
				string text = ((leaderboardCategoryList == null) ? "" : leaderboardCategoryList.CurrentCategory.CategoryDisplayName);
				value = ((text != "") ? text : Localization.Get("lblAll"));
				break;
			}
			}
			return true;
		case "headericon":
			switch (CurrentType)
			{
			case ListTypes.Actions:
				value = "ui_game_symbol_twitch_actions";
				break;
			case ListTypes.Votes:
				value = "ui_game_symbol_twitch_vote";
				break;
			case ListTypes.ActionHistory:
				value = "ui_game_symbol_twitch_history";
				break;
			case ListTypes.Leaderboard:
				value = "ui_game_symbol_twitch_leaderboard_1";
				break;
			}
			return true;
		case "showcategories":
			value = (CurrentType == ListTypes.Actions).ToString();
			return true;
		case "showactions":
			value = (CurrentType == ListTypes.Actions).ToString();
			return true;
		case "showvotes":
			value = (CurrentType == ListTypes.Votes).ToString();
			return true;
		case "showactionhistory":
			value = (CurrentType == ListTypes.ActionHistory).ToString();
			return true;
		case "showleaderboard":
			value = (CurrentType == ListTypes.Leaderboard).ToString();
			return true;
		case "leaderboard_header_value":
			if (CurrentType == ListTypes.Leaderboard)
			{
				switch ((leaderboardCategoryList == null) ? "" : leaderboardCategoryList.CurrentCategory.CategoryName)
				{
				case "global_kills":
				case "session_kills":
					value = Localization.Get("TwitchInfo_KillsHeader");
					break;
				case "session_good":
				case "session_bad":
				case "current_good":
					value = Localization.Get("TwitchInfo_Actions");
					break;
				case "session_bits":
					value = Localization.Get("TwitchPoints_Bits");
					break;
				}
			}
			return true;
		default:
			return false;
		}
	}
}
