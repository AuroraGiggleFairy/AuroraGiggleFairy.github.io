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
		public readonly GameServerInfo GameServerInfo;

		public ListEntry(GameServerInfo _serverInfo, XUiC_ServersList _serversList)
		{
			GameServerInfo = _serverInfo;
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return GameServerInfo.LastPlayedLinux.CompareTo(_otherEntry.GameServerInfo.LastPlayedLinux);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return GameServerInfo.GetValue(GameInfoString.GameHost).ContainsCaseInsensitive(_searchString);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_ListEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_ServersList parentList;

		[XuiXmlBinding("servername")]
		public string ServerName
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (entryData == null)
				{
					return "";
				}
				string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(entryData.GameServerInfo.ServerDisplayName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(entryData.GameServerInfo.ServerDisplayName) && !GeneratedTextManager.IsFiltered(entryData.GameServerInfo.ServerDisplayName))
				{
					GeneratedTextManager.GetDisplayText(entryData.GameServerInfo.ServerDisplayName, [PublicizedFrom(EAccessModifier.Private)] (string _) =>
					{
						parentList.RefreshBindingListEntry(entryData);
					}, _runCallbackIfReadyNow: false, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return displayTextImmediately;
			}
		}

		[XuiXmlBinding("servericon")]
		public string ServerIcon
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (entryData != null)
				{
					return PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(entryData.GameServerInfo.PlayGroup, _fetchGenericIcons: true);
				}
				return "";
			}
		}

		[XuiXmlBinding("servericonatlas")]
		public string ServerIconAtlas
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return "SymbolAtlas";
			}
		}

		[XuiXmlBinding("playersonline")]
		public int PlayersOnline
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.GetValue(GameInfoInt.CurrentPlayers) ?? 0;
			}
		}

		[XuiXmlBinding("playersmax")]
		public int PlayersMax
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.GetValue(GameInfoInt.MaxPlayers) ?? 0;
			}
		}

		[XuiXmlBinding("ping")]
		public string Ping
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (entryData == null)
				{
					return "";
				}
				int value = entryData.GameServerInfo.GetValue(GameInfoInt.Ping);
				if (value < 0)
				{
					return "N/A";
				}
				return value.ToString();
			}
		}

		[XuiXmlBinding("world")]
		public string World
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (entryData == null)
				{
					return "";
				}
				string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(entryData.GameServerInfo.ServerWorldName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(entryData.GameServerInfo.ServerWorldName) && !GeneratedTextManager.IsFiltered(entryData.GameServerInfo.ServerWorldName))
				{
					GeneratedTextManager.GetDisplayText(entryData.GameServerInfo.ServerWorldName, [PublicizedFrom(EAccessModifier.Private)] (string _) =>
					{
						parentList.RefreshBindingListEntry(entryData);
					}, _runCallbackIfReadyNow: false, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return displayTextImmediately;
			}
		}

		[XuiXmlBinding("anticheatprotected")]
		public bool AntiCheatProtected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.EACEnabled ?? false;
			}
		}

		[XuiXmlBinding("passwordprotected")]
		public bool PasswordColorProtected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.GetValue(GameInfoBool.IsPasswordProtected) ?? false;
			}
		}

		[XuiXmlBinding("isfavorite")]
		public bool IsFavorite
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.IsFavorite ?? false;
			}
		}

		[XuiXmlBinding("isdedicated")]
		public bool IsDedicated
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.IsDedicated ?? false;
			}
		}

		[XuiXmlBinding("hasping")]
		public bool HasPing
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (entryData != null && entryData.GameServerInfo.IsCompatibleVersion)
				{
					return entryData.GameServerInfo.GetValue(GameInfoInt.Ping) >= 0;
				}
				return false;
			}
		}

		[XuiXmlBinding("sameversion")]
		public bool SameVersion
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return entryData?.GameServerInfo.IsCompatibleVersion ?? true;
			}
		}

		public override void Init()
		{
			base.Init();
			parentList = (XUiC_ServersList)list;
			GetChildById("favorite").OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				GameServerInfo gameServerInfo = entryData.GameServerInfo;
				((XUiC_ServersList)list).addRemoveServerCount(gameServerInfo, _add: false);
				ServerInfoCache.Instance.ToggleFavorite(gameServerInfo);
				((XUiC_ServersList)list).addRemoveServerCount(gameServerInfo, _add: true);
				IsDirty = true;
			};
		}

		public override void Update(float _dt)
		{
			IsDirty = true;
			base.Update(_dt);
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
	public const string ServerTypeFilterName = "servertype";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ServerNameFilterName = "servername";

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
	public readonly XUiC_Button[] sortButtons = new XUiC_Button[5];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortAscending = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<ListEntry, string> sortFuncString;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<ListEntry, int> sortFuncInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<ListEntry, int> sortDefaultFavHistory = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => (!_line.GameServerInfo.IsFavorite) ? _line.GameServerInfo.LastPlayedLinux : int.MaxValue;

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

	[XuiXmlBinding("sortcolumn")]
	public EnumColumns SortColumn
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sortColumn;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != sortColumn)
			{
				sortColumn = value;
				IsDirty = true;
			}
		}
	}

	public event Action<int> OnFilterResultsChanged;

	public event Action CountsChanged;

	public override void Init()
	{
		base.Init();
		if (!TryGetChildController<XUiController>("serverlistheader", out var _child))
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (_child.TryGetChildController<XUiC_Button>(((EnumColumns)i).ToStringCached(), out sortButtons[i]))
			{
				sortButtons[i].OnPress += SortButton_OnPress;
			}
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

	public void RefreshBindingListEntry(ListEntry _entry)
	{
		XUiC_ListEntry[] array = listEntryControllers;
		foreach (XUiC_ListEntry xUiC_ListEntry in array)
		{
			if (xUiC_ListEntry.GetEntry() == _entry)
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

	public void SetServerTypeFilter(EnumServerLists _typelist)
	{
		Func<ListEntry, bool> func = _typelist switch
		{
			EnumServerLists.Dedicated => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.IsDedicated, 
			EnumServerLists.Peer => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.IsPeerToPeer, 
			EnumServerLists.Friends => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.IsFriends, 
			EnumServerLists.History => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.IsFavoriteHistory, 
			EnumServerLists.LAN => [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.IsLAN, 
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
			if (uniqueIdComparer.GetHashCode(allEntries[i].GameServerInfo) == hashCode && uniqueIdComparer.Equals(_gameServerInfo, allEntries[i].GameServerInfo))
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
			GameServerInfo gameServerInfo = allEntries[num].GameServerInfo;
			addRemoveServerCount(gameServerInfo, _add: false);
			gameServerInfo.Merge(_gameServerInfo, _source);
			addRemoveServerCount(gameServerInfo, _add: true);
			XUiC_ListEntry xUiC_ListEntry = IsVisible(allEntries[num]);
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
			addRemoveServerCount(_gameServerInfo, _add: true);
			result = true;
		}
		updateCurrentList = true;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveServerCount(GameServerInfo _gsi, bool _add)
	{
		if (_gsi.IsDedicated)
		{
			addRemoveCountSingleListType(_add, EnumServerLists.Dedicated);
		}
		if (_gsi.IsPeerToPeer)
		{
			addRemoveCountSingleListType(_add, EnumServerLists.Peer);
		}
		if (_gsi.IsFriends)
		{
			addRemoveCountSingleListType(_add, EnumServerLists.Friends);
		}
		if (_gsi.IsFavoriteHistory)
		{
			addRemoveCountSingleListType(_add, EnumServerLists.History);
		}
		if (_gsi.IsLAN)
		{
			addRemoveCountSingleListType(_add, EnumServerLists.LAN);
		}
		this.CountsChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveCountSingleListType(bool _add, EnumServerLists _list)
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
		if (stopListUpdateThread || _tInfo.TerminationRequested())
		{
			return -1;
		}
		if (!updateCurrentList)
		{
			return 50;
		}
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
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < sortButtons.Length; i++)
		{
			if (_sender == sortButtons[i])
			{
				updateSortType((EnumColumns)i);
				break;
			}
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSortType(EnumColumns _sortType)
	{
		switch (_sortType)
		{
		case EnumColumns.ServerName:
			sortFuncString = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoString.GameHost);
			sortFuncInt = null;
			break;
		case EnumColumns.World:
			sortFuncString = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoString.LevelName);
			sortFuncInt = null;
			break;
		case EnumColumns.Difficulty:
			sortFuncString = null;
			sortFuncInt = null;
			break;
		case EnumColumns.Players:
			sortFuncString = null;
			sortFuncInt = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoInt.CurrentPlayers);
			break;
		case EnumColumns.Ping:
			sortFuncString = null;
			sortFuncInt = [PublicizedFrom(EAccessModifier.Internal)] (ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoInt.Ping);
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
