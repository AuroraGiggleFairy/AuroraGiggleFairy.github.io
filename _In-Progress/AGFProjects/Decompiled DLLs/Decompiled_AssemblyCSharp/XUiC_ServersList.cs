using System;
using System.Collections.Generic;
using System.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServersList : XUiC_List<XUiC_ServersList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly GameServerInfo gameServerInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_ServersList serversList;

		public ListEntry(GameServerInfo _serverInfo, XUiC_ServersList _serversList)
		{
			gameServerInfo = _serverInfo;
			serversList = _serversList;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return gameServerInfo.LastPlayedLinux.CompareTo(_otherEntry.gameServerInfo.LastPlayedLinux);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "servername":
				_value = GeneratedTextManager.GetDisplayTextImmediately(gameServerInfo.ServerDisplayName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(gameServerInfo.ServerDisplayName) && !GeneratedTextManager.IsFiltered(gameServerInfo.ServerDisplayName))
				{
					GeneratedTextManager.GetDisplayText(gameServerInfo.ServerDisplayName, [PublicizedFrom(EAccessModifier.Private)] (string _) =>
					{
						serversList.RefreshBindingListEntry(this);
					}, _runCallbackIfReadyNow: false, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return true;
			case "servericon":
				_value = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(gameServerInfo.PlayGroup, _fetchGenericIcons: true);
				return true;
			case "servericonatlas":
				_value = "SymbolAtlas";
				return true;
			case "playersonline":
				_value = gameServerInfo.GetValue(GameInfoInt.CurrentPlayers).ToString();
				return true;
			case "playersmax":
				_value = gameServerInfo.GetValue(GameInfoInt.MaxPlayers).ToString();
				return true;
			case "ping":
			{
				int value = gameServerInfo.GetValue(GameInfoInt.Ping);
				_value = ((value >= 0) ? value.ToString() : "N/A");
				return true;
			}
			case "world":
				_value = GeneratedTextManager.GetDisplayTextImmediately(gameServerInfo.ServerWorldName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(gameServerInfo.ServerWorldName) && !GeneratedTextManager.IsFiltered(gameServerInfo.ServerWorldName))
				{
					GeneratedTextManager.GetDisplayText(gameServerInfo.ServerWorldName, [PublicizedFrom(EAccessModifier.Private)] (string _) =>
					{
						serversList.RefreshBindingListEntry(this);
					}, _runCallbackIfReadyNow: false, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return true;
			case "difficulty":
				_value = gameServerInfo.GetValue(GameInfoInt.GameDifficulty).ToString();
				return true;
			case "textcolor":
				_value = (gameServerInfo.IsCompatibleVersion ? textEnabledColor : textDisabledColor);
				return true;
			case "pingcolor":
				_value = ((gameServerInfo.IsCompatibleVersion && gameServerInfo.GetValue(GameInfoInt.Ping) >= 0) ? textEnabledColor : textDisabledColor);
				return true;
			case "anticheatcolor":
				_value = (gameServerInfo.EACEnabled ? iconEnabledColor : iconDisabledColor);
				return true;
			case "passwordcolor":
				_value = (gameServerInfo.GetValue(GameInfoBool.IsPasswordProtected) ? iconEnabledColor : iconDisabledColor);
				return true;
			case "isfavorite":
				_value = gameServerInfo.IsFavorite.ToString();
				return true;
			case "isdedicated":
				_value = gameServerInfo.IsDedicated.ToString();
				return true;
			case "hasentry":
				_value = true.ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return gameServerInfo.GetValue(GameInfoString.GameHost).ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "servername":
			case "playersonline":
			case "playersmax":
			case "ping":
			case "world":
			case "difficulty":
			case "servericon":
			case "servericonatlas":
				_value = "";
				return true;
			case "textcolor":
			case "pingcolor":
			case "anticheatcolor":
			case "passwordcolor":
				_value = "0,0,0";
				return true;
			case "isfavorite":
			case "isdedicated":
			case "hasentry":
				_value = false.ToString();
				return true;
			default:
				return false;
			}
		}
	}

	public class UiServerFilter : IServerListInterface.ServerFilter
	{
		public readonly Func<ListEntry, bool> Func;

		public readonly EnumServerLists ApplyingLists;

		public UiServerFilter(string _name, EnumServerLists _applyingTo, Func<ListEntry, bool> _func = null, EServerFilterType _type = EServerFilterType.Any, int _intMinValue = 0, int _intMaxValue = 0, bool _boolValue = false, string _stringNeedle = null)
			: base(_name, _type, _intMinValue, _intMaxValue, _boolValue, _stringNeedle)
		{
			Func = _func;
			ApplyingLists = _applyingTo;
		}
	}

	[Flags]
	public enum EnumServerLists
	{
		None = 0,
		Dedicated = 1,
		Peer = 2,
		Friends = 4,
		History = 8,
		LAN = 0x10,
		Regular = 3,
		Special = 0x1C,
		All = 0x1F
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EnumColumns
	{
		ServerName,
		World,
		Difficulty,
		Players,
		Ping,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string serverTypeFilterName = "servertype";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string serverNameFilterName = "servername";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textEnabledColor = "255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textDisabledColor = "160,160,160";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string iconEnabledColor = "222,206,163";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string iconDisabledColor = "2,2,2,2";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopListUpdateThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateCurrentList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageUpdateRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPageUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ListEntry> filteredEntriesTmp = new List<ListEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] sortButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortAscending = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<ListEntry, string> sortFuncString;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<ListEntry, int> sortFuncInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<ListEntry, int> sortDefaultFavHistory = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => (!_line.gameServerInfo.IsFavorite) ? _line.gameServerInfo.LastPlayedLinux : int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumServerLists currentServerTypeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, UiServerFilter> currentFilters = new Dictionary<string, UiServerFilter>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly GameServerInfo.UniqueIdEqualityComparer uniqueIdComparer = GameServerInfo.UniqueIdEqualityComparer.Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumServerLists, int> serverCounts = new EnumDictionary<EnumServerLists, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumColumns sortColumn = EnumColumns.Count;

	public EnumServerLists CurrentServerTypeList => currentServerTypeList;

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public event Action<int> OnFilterResultsChanged;

	public event Action CountsChanged;

	public override void Init()
	{
		base.Init();
		sortButtons = new XUiController[5];
		for (int i = 0; i < 5; i++)
		{
			sortButtons[i] = GetChildById("serverlistheader").GetChildById(((EnumColumns)i).ToStringCached());
			if (sortButtons[i] != null)
			{
				sortButtons[i].OnPress += SortButton_OnPress;
				sortButtons[i].OnHover += SortButton_OnHover;
			}
		}
		for (int j = 0; j < listEntryControllers.Length; j++)
		{
			XUiC_ListEntry<ListEntry> obj = listEntryControllers[j];
			obj.OnDoubleClick += EntryDoubleClicked;
			XUiV_Button obj2 = (XUiV_Button)obj.GetChildById("favorite").ViewComponent;
			int rowIndex = j;
			obj2.Controller.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _sender, int _args) =>
			{
				XUiC_ListEntry<ListEntry> obj3 = listEntryControllers[rowIndex];
				GameServerInfo gameServerInfo = obj3.GetEntry().gameServerInfo;
				AddRemoveServerCount(gameServerInfo, _add: false);
				ServerInfoCache.Instance.ToggleFavorite(gameServerInfo);
				AddRemoveServerCount(gameServerInfo, _add: true);
				obj3.IsDirty = true;
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
		stopListUpdateThread = false;
		ThreadManager.StartThread("ServerBrowserListUpdater", null, currentListUpdateThread, null);
	}

	public override void OnClose()
	{
		base.OnClose();
		stopListUpdateThread = true;
	}

	public override void Update(float _dt)
	{
		bool flag = false;
		if (pageUpdateRequired && (double)(Time.realtimeSinceStartup - lastPageUpdate) > 0.1)
		{
			flag = true;
			pageUpdateRequired = false;
			lastPageUpdate = Time.realtimeSinceStartup;
			IsDirty = true;
		}
		base.Update(_dt);
		if (flag)
		{
			this.OnFilterResultsChanged?.Invoke(base.EntryCount);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		lock (serverCounts)
		{
			serverCounts.Clear();
		}
		base.RebuildList(_resetFilter);
	}

	public void RefreshBindingListEntry(ListEntry entry)
	{
		XUiC_ListEntry<ListEntry>[] array = listEntryControllers;
		foreach (XUiC_ListEntry<ListEntry> xUiC_ListEntry in array)
		{
			if (xUiC_ListEntry.GetEntry() == entry)
			{
				xUiC_ListEntry.RefreshBindings();
				break;
			}
		}
	}

	public override void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!string.IsNullOrEmpty(_text))
		{
			SetFilter(new UiServerFilter("servername", EnumServerLists.All, [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _entry) => _entry.MatchesSearch(_text)));
		}
		else
		{
			SetFilter(new UiServerFilter("servername", EnumServerLists.All));
		}
		updateCurrentList = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		this.OnEntryDoubleClicked?.Invoke(_sender, _mouseButton);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "icon_enabled_color"))
		{
			if (_name == "icon_disabled_color")
			{
				iconDisabledColor = _value;
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		iconEnabledColor = _value;
		return true;
	}

	public void SetServerTypeFilter(EnumServerLists _typelist)
	{
		Func<ListEntry, bool> func = _typelist switch
		{
			EnumServerLists.Dedicated => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.IsDedicated, 
			EnumServerLists.Peer => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.IsPeerToPeer, 
			EnumServerLists.Friends => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.IsFriends, 
			EnumServerLists.History => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.IsFavoriteHistory, 
			EnumServerLists.LAN => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.IsLAN, 
			_ => null, 
		};
		currentServerTypeList = _typelist;
		SetFilter(new UiServerFilter("servertype", EnumServerLists.All, func));
		ClearSelection();
	}

	public void SetFilter(UiServerFilter _filter)
	{
		lock (currentFilters)
		{
			if (_filter.Func == null)
			{
				currentFilters.Remove(_filter.Name);
			}
			else
			{
				currentFilters[_filter.Name] = _filter;
			}
		}
		updateCurrentList = true;
	}

	public int GetActiveFilterCount()
	{
		lock (currentFilters)
		{
			return currentFilters.ContainsKey("servertype") ? (currentFilters.Count - 1) : currentFilters.Count;
		}
	}

	public int GetServerCount(EnumServerLists _list)
	{
		lock (serverCounts)
		{
			serverCounts.TryGetValue(_list, out var value);
			return value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListFiltering(ref IEnumerable<ListEntry> _list)
	{
		lock (currentFilters)
		{
			foreach (KeyValuePair<string, UiServerFilter> currentFilter in currentFilters)
			{
				if ((currentServerTypeList & currentFilter.Value.ApplyingLists) != EnumServerLists.None)
				{
					_list = _list.Where(currentFilter.Value.Func);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListSorting(ref IEnumerable<ListEntry> _list)
	{
		if (currentServerTypeList == EnumServerLists.History && sortFuncInt == null && sortFuncString == null)
		{
			_list = _list.OrderByDescending(sortDefaultFavHistory);
		}
		if (sortFuncInt != null)
		{
			_list = (sortAscending ? _list.OrderBy(sortFuncInt) : _list.OrderByDescending(sortFuncInt));
		}
		if (sortFuncString != null)
		{
			_list = (sortAscending ? _list.OrderBy(sortFuncString) : _list.OrderByDescending(sortFuncString));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int indexOfGameServerInfo(GameServerInfo _gameServerInfo)
	{
		if (_gameServerInfo == null)
		{
			return -1;
		}
		int hashCode = uniqueIdComparer.GetHashCode(_gameServerInfo);
		int count = allEntries.Count;
		for (int i = 0; i < count; i++)
		{
			if (uniqueIdComparer.GetHashCode(allEntries[i].gameServerInfo) == hashCode && uniqueIdComparer.Equals(_gameServerInfo, allEntries[i].gameServerInfo))
			{
				return i;
			}
		}
		return -1;
	}

	public bool AddGameServer(GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		int num = indexOfGameServerInfo(_gameServerInfo);
		bool result;
		if (num >= 0)
		{
			GameServerInfo gameServerInfo = allEntries[num].gameServerInfo;
			AddRemoveServerCount(gameServerInfo, _add: false);
			gameServerInfo.Merge(_gameServerInfo, _source);
			AddRemoveServerCount(gameServerInfo, _add: true);
			XUiC_ListEntry<ListEntry> xUiC_ListEntry = IsVisible(allEntries[num]);
			if (xUiC_ListEntry != null)
			{
				xUiC_ListEntry.IsDirty = true;
			}
			result = false;
		}
		else
		{
			lock (allEntries)
			{
				allEntries.Add(new ListEntry(_gameServerInfo, this));
			}
			AddRemoveServerCount(_gameServerInfo, _add: true);
			result = true;
		}
		updateCurrentList = true;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddRemoveServerCount(GameServerInfo _gsi, bool _add)
	{
		if (_gsi.IsDedicated)
		{
			addRemoveCountSingleListType(_gsi, _add, EnumServerLists.Dedicated);
		}
		if (_gsi.IsPeerToPeer)
		{
			addRemoveCountSingleListType(_gsi, _add, EnumServerLists.Peer);
		}
		if (_gsi.IsFriends)
		{
			addRemoveCountSingleListType(_gsi, _add, EnumServerLists.Friends);
		}
		if (_gsi.IsFavoriteHistory)
		{
			addRemoveCountSingleListType(_gsi, _add, EnumServerLists.History);
		}
		if (_gsi.IsLAN)
		{
			addRemoveCountSingleListType(_gsi, _add, EnumServerLists.LAN);
		}
		this.CountsChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveCountSingleListType(GameServerInfo _gsi, bool _add, EnumServerLists _list)
	{
		lock (serverCounts)
		{
			if (serverCounts.TryGetValue(_list, out var value))
			{
				serverCounts[_list] = value + (_add ? 1 : (-1));
			}
			else if (_add)
			{
				serverCounts[_list] = 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentListUpdateThread(ThreadManager.ThreadInfo _tInfo)
	{
		if (!stopListUpdateThread && !_tInfo.TerminationRequested())
		{
			if (updateCurrentList)
			{
				updateCurrentList = false;
				lock (allEntries)
				{
					filteredEntriesTmp.AddRange(allEntries);
				}
				IEnumerable<ListEntry> _list = filteredEntriesTmp;
				updateListFiltering(ref _list);
				updateListSorting(ref _list);
				filteredEntriesTmp = _list.ToList();
				lock (this)
				{
					List<ListEntry> list = filteredEntriesTmp;
					List<ListEntry> list2 = filteredEntries;
					filteredEntries = list;
					filteredEntriesTmp = list2;
				}
				pageUpdateRequired = true;
				filteredEntriesTmp.Clear();
			}
			return 50;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnHover(XUiController _sender, bool _isOver)
	{
		for (int i = 0; i < sortButtons.Length; i++)
		{
			if (_sender != sortButtons[i])
			{
				continue;
			}
			Color color = ((sortColumn == (EnumColumns)i) ? ((Color)new Color32(222, 206, 163, byte.MaxValue)) : (_isOver ? ((Color)new Color32(250, byte.MaxValue, 163, byte.MaxValue)) : Color.white));
			XUiView xUiView = _sender.ViewComponent;
			if (!(xUiView is XUiV_Label xUiV_Label))
			{
				if (xUiView is XUiV_Sprite xUiV_Sprite)
				{
					xUiV_Sprite.Color = color;
				}
			}
			else
			{
				xUiV_Label.Color = color;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < sortButtons.Length; i++)
		{
			if (_sender != sortButtons[i])
			{
				continue;
			}
			updateSortType((EnumColumns)i);
			for (int j = 0; j < sortButtons.Length; j++)
			{
				if (sortButtons[j] != null)
				{
					SortButton_OnHover(sortButtons[j], i == j);
				}
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSortType(EnumColumns _sortType)
	{
		switch (_sortType)
		{
		case EnumColumns.ServerName:
			sortFuncString = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoString.GameHost);
			sortFuncInt = null;
			break;
		case EnumColumns.World:
			sortFuncString = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoString.LevelName);
			sortFuncInt = null;
			break;
		case EnumColumns.Difficulty:
			sortFuncString = null;
			sortFuncInt = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.GameDifficulty);
			break;
		case EnumColumns.Players:
			sortFuncString = null;
			sortFuncInt = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.CurrentPlayers);
			break;
		case EnumColumns.Ping:
			sortFuncString = null;
			sortFuncInt = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.Ping);
			break;
		default:
			sortFuncString = null;
			sortFuncInt = null;
			break;
		}
		if (sortColumn == _sortType)
		{
			if (!sortAscending)
			{
				sortColumn = EnumColumns.Count;
				sortFuncString = null;
				sortFuncInt = null;
				return;
			}
			sortAscending = false;
		}
		else
		{
			sortAscending = true;
		}
		sortColumn = _sortType;
		updateCurrentList = true;
	}
}
