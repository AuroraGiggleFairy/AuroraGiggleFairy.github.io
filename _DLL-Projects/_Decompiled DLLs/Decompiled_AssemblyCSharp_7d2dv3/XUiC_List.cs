using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_List<T> : XUiController where T : XUiListEntry<T>
{
	public enum EPagingStepSize
	{
		Page,
		SingleEntry
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class XUiC_ListEntry : XUiController
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isHovered;

		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly XUiC_List<T> list;

		[PublicizedFrom(EAccessModifier.Protected)]
		public T entryData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool selected;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool forceHovered;

		[XuiXmlBinding("hasentry")]
		public bool HasEntry
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return entryData != null;
			}
		}

		[XuiXmlBinding("hovered")]
		public bool IsHovered
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				if (!isHovered)
				{
					return forceHovered;
				}
				return true;
			}
		}

		[XuiXmlBinding("isselected")]
		public new bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				if (value)
				{
					if (list.SelectedEntry != null)
					{
						list.SelectedEntry.selected = false;
					}
				}
				else if (list.SelectedEntry == this)
				{
					selected = false;
					list.ClearSelection();
				}
				selected = value;
				if (selected)
				{
					list.SelectedEntry = this;
				}
				IsDirty = true;
			}
		}

		public bool ForceHovered
		{
			get
			{
				return forceHovered;
			}
			set
			{
				if (value != forceHovered)
				{
					forceHovered = value;
					IsDirty = true;
				}
			}
		}

		public override void Init()
		{
			base.Init();
			base.ViewComponent.Enabled = HasEntry;
			IsDirty = true;
		}

		[XuiBindEvent("OnScroll", null)]
		[PublicizedFrom(EAccessModifier.Private)]
		public void Event_OnScroll(XUiController _sender, float _delta)
		{
			list.HandleOnScroll(this, _delta);
		}

		[XuiBindEvent("OnPress", null)]
		public void Event_OnPress(XUiController _sender, int _mouseButton)
		{
			if (!Selected)
			{
				Selected = true;
			}
			list.OnListEntryClicked(entryData);
		}

		[XuiBindEvent("OnDoubleClick", null)]
		public void Event_OnDoubleClick(XUiController _sender, int _mouseButton)
		{
			list.OnListEntryDoubleClicked(entryData);
		}

		[XuiBindEvent("OnHover", null)]
		public void Event_OnHover(XUiController _sender, bool _isOver)
		{
			list.OnListEntryHovered(entryData, _isOver);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			isHovered = _isOver;
			IsDirty = true;
			base.OnHovered(_isOver);
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			T val = entryData;
			if (val != null && val.UiDirty)
			{
				IsDirty = true;
			}
			if (handleDirtyUpdateDefault() && entryData != null)
			{
				entryData.UiDirty = false;
			}
		}

		public virtual void SetEntry(T _data)
		{
			if (_data != entryData)
			{
				entryData = _data;
				base.ViewComponent.Enabled = HasEntry;
			}
			IsDirty = true;
		}

		public T GetEntry()
		{
			return entryData;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> filteredEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public T currentSelectedEntry;

	[XuiBindComponent("list.", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_ListEntry[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntryIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool updateSelectedItemByIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pageSizeOverride = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public EPagingStepSize pagingStepSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pagingOverscroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[XuiBindComponent("searchInput", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_TextInput searchBox;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_CaptionedTextBox searchBox2;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<T> allEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string previousMatch = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreSelectionChange;

	public int PageLength
	{
		get
		{
			if (PageSizeOverride <= 0)
			{
				XUiC_ListEntry[] array = listEntryControllers;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
			return PageSizeOverride;
		}
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, LastPage);
			if (num != page)
			{
				page = num;
				updatePageLabel();
				IsDirty = true;
				ClearSelection();
				CurrentBaseIndex = page * PageLength;
				this.PageNumberChanged?.Invoke(this, page);
			}
		}
	}

	public int LastPage => Math.Max(0, Mathf.CeilToInt((float)filteredEntries.Count / (float)PageLength) - 1);

	public XUiC_ListEntry SelectedEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return selectedEntry;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value == selectedEntry)
			{
				return;
			}
			T val = currentSelectedEntry;
			XUiC_ListEntry xUiC_ListEntry = selectedEntry;
			selectedEntry = null;
			if (xUiC_ListEntry != null)
			{
				xUiC_ListEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				currentSelectedEntry = selectedEntry.GetEntry();
				for (int i = 0; i < listEntryControllers.Length; i++)
				{
					if (selectedEntry == listEntryControllers[i])
					{
						selectedEntryIndex = CurrentBaseIndex + i;
						break;
					}
				}
			}
			else
			{
				currentSelectedEntry = null;
				selectedEntryIndex = -1;
			}
			if (val != currentSelectedEntry)
			{
				OnSelectionChanged(xUiC_ListEntry, selectedEntry);
			}
			IsDirty = true;
		}
	}

	public T SelectedEntryData
	{
		get
		{
			XUiC_ListEntry xUiC_ListEntry = SelectedEntry;
			if (xUiC_ListEntry == null)
			{
				return null;
			}
			return xUiC_ListEntry.GetEntry();
		}
	}

	public int SelectedEntryIndex
	{
		get
		{
			return selectedEntryIndex;
		}
		set
		{
			if (value >= 0 && value < EntryCount && selectedEntryIndex != value)
			{
				forceEntryVisible(value);
				selectedEntryIndex = value;
				updateSelectedItemByIndex = true;
				updateCurrentPageContents();
				updateSelectedItemByIndex = false;
				SelectedEntry = listEntryControllers[value - CurrentBaseIndex];
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("page_size_override", false)]
	public int PageSizeOverride
	{
		get
		{
			return pageSizeOverride;
		}
		set
		{
			if (value != pageSizeOverride)
			{
				pageSizeOverride = value;
				if (PagingStepSize == EPagingStepSize.Page)
				{
					Page = 0;
				}
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("paging_step_size", false)]
	public EPagingStepSize PagingStepSize
	{
		get
		{
			return pagingStepSize;
		}
		set
		{
			if (value != pagingStepSize)
			{
				pagingStepSize = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("paging_overscroll", false)]
	public Vector2i PagingOverscroll
	{
		get
		{
			return pagingOverscroll;
		}
		set
		{
			if (!(value == pagingOverscroll))
			{
				pagingOverscroll = value;
				IsDirty = true;
			}
		}
	}

	public int CurrentBaseIndex
	{
		get
		{
			return minIndex;
		}
		set
		{
			if (minIndex != value)
			{
				if (value < 0)
				{
					value = Mathf.Max(value, PagingOverscroll.x);
				}
				if (value > EntryCount - PageLength)
				{
					value = PagingStepSize switch
					{
						EPagingStepSize.Page => Mathf.Min(value, EntryCount - 1), 
						EPagingStepSize.SingleEntry => Mathf.Min(value, EntryCount - PageLength + PagingOverscroll.y), 
						_ => throw new ArgumentOutOfRangeException(), 
					};
				}
				minIndex = value;
				ClearSelection();
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("current_min_index")]
	public int CurrentPageMinIndex => Mathf.Max(minIndex, 0);

	[XuiXmlBinding("current_max_index")]
	public int CurrentPageMaxIndex => Mathf.Min(minIndex + PageLength, EntryCount) - 1;

	[XuiXmlBinding("hasselection")]
	public bool HasSelection => SelectedEntryData != null;

	[XuiXmlBinding("entrycountfiltered")]
	public int EntryCount => filteredEntries.Count;

	[XuiXmlBinding("entrycounttotal")]
	public int UnfilteredEntryCount => allEntries.Count;

	[XuiXmlAttribute("clear_selection_on_open", false)]
	public bool ClearSelectionOnOpenClose { get; set; } = true;

	[XuiXmlAttribute("clear_searchtext_on_open", false)]
	public bool ClearSearchTextOnOpenClose { get; set; }

	[XuiXmlAttribute("selectable", false)]
	public bool SelectableEntries { get; set; } = true;

	[XuiXmlAttribute("cursor_controllable", false)]
	public bool CursorControllable { get; set; }

	public event XUiEvent_ListSelectionChangedEventHandler<T> SelectionChanged;

	public event XUiEvent_ListPageNumberChangedEventHandler<T> PageNumberChanged;

	public event XUiEvent_ListEntryClickedEventHandler<T> ListEntryClicked;

	public event XUiEvent_ListEntryDoubleClickedEventHandler<T> ListEntryDoubleClicked;

	public event XUiEvent_ListEntryHoveredEventHandler<T> ListEntryHovered;

	public event XUiEvent_PageContentsChangedEventHandler<T> PageContentsChanged;

	public T GetEntry(int _index)
	{
		return filteredEntries[_index];
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("list");
		if (childById == null)
		{
			Log.Warning("[XUi] List controller without a 'list' child element! (window group '" + base.WindowGroup.Id + "')");
		}
		else if (CursorControllable)
		{
			if (childById.ViewComponent is XUiV_Grid xUiV_Grid)
			{
				if (xUiV_Grid.Arrangement == UIGrid.Arrangement.Horizontal)
				{
					columns = xUiV_Grid.Columns;
					rows = listEntryControllers.Length / columns;
				}
				else
				{
					rows = xUiV_Grid.Rows;
					columns = listEntryControllers.Length / rows;
				}
			}
			if (childById.ViewComponent is XUiV_Table xUiV_Table)
			{
				columns = xUiV_Table.Columns;
				rows = listEntryControllers.Length / columns;
			}
		}
		RebuildList(_resetFilter: true);
	}

	public virtual void RebuildList(bool _resetFilter = false)
	{
		ClearSelection();
		filteredEntries?.Clear();
		RefreshView(_resetFilter);
	}

	public virtual void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
		if (_resetFilter)
		{
			if (searchBox != null)
			{
				searchBox.Text = "";
			}
			if (searchBox2 != null)
			{
				searchBox2.Text = "";
			}
		}
		OnSearchInputChanged(this, (searchBox != null) ? searchBox.Text : searchBox2?.Text, _changeFromCode: true);
		if (_resetPage)
		{
			switch (PagingStepSize)
			{
			case EPagingStepSize.Page:
				Page = 0;
				break;
			case EPagingStepSize.SingleEntry:
				CurrentBaseIndex = PagingOverscroll.x;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry IsVisible(T _value)
	{
		XUiC_ListEntry[] array = listEntryControllers;
		foreach (XUiC_ListEntry xUiC_ListEntry in array)
		{
			T entry = xUiC_ListEntry.GetEntry();
			if (entry != null && entry == _value)
			{
				return xUiC_ListEntry;
			}
		}
		return null;
	}

	[XuiBindEvent("OnPageChanged", "pager")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnPageChanged()
	{
		Page = pager.CurrentPageNumber;
	}

	[XuiBindEvent("OnScroll", null)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		ChangePage((!(_delta > 0f)) ? 1 : (-1));
	}

	public void ChangePage(int _offset)
	{
		switch (PagingStepSize)
		{
		case EPagingStepSize.Page:
			Page += _offset;
			break;
		case EPagingStepSize.SingleEntry:
			CurrentBaseIndex += _offset;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[XuiBindEvent("OnSubmitHandler", "searchBox")]
	[XuiBindEvent("OnSubmitHandler", "searchBox2")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSearchInputSubmit(XUiController _sender, string _text)
	{
		OnSearchInputChanged(_sender, _text, _changeFromCode: false);
	}

	[XuiBindEvent("OnChangeHandler", "searchBox")]
	[XuiBindEvent("OnChangeHandler", "searchBox2")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		FilterResults(_text);
		IsDirty = true;
	}

	public IReadOnlyList<T> AllEntries()
	{
		return allEntries;
	}

	public T GetEntryByIndex(int _index)
	{
		if (_index < 0 || _index >= filteredEntries.Count)
		{
			return null;
		}
		return filteredEntries[_index];
	}

	public void GetCurrentPageEntries(List<T> _entries)
	{
		if (_entries == null)
		{
			throw new ArgumentNullException("_entries");
		}
		_entries.Clear();
		for (int i = 0; i < PageLength; i++)
		{
			int num = i + CurrentBaseIndex;
			if (num < filteredEntries.Count)
			{
				_entries.Add(filteredEntries[num]);
			}
		}
	}

	public void SetFilter(string _filter)
	{
		if (searchBox != null)
		{
			searchBox.Text = _filter;
			IsDirty = true;
		}
		else if (searchBox2 != null)
		{
			searchBox2.Text = _filter;
			IsDirty = true;
		}
		else
		{
			Log.Error("Can not set filter without search box");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FilterResults(string _textMatch)
	{
		if (_textMatch == null)
		{
			filteredEntries.Clear();
			filteredEntries.AddRange(allEntries);
		}
		else
		{
			if (_textMatch == previousMatch && filteredEntries.Count == allEntries.Count)
			{
				return;
			}
			previousMatch = _textMatch;
			filteredEntries.Clear();
			if (_textMatch.Length > 0)
			{
				foreach (T allEntry in allEntries)
				{
					if (allEntry.MatchesSearch(_textMatch))
					{
						filteredEntries.Add(allEntry);
					}
				}
				return;
			}
			filteredEntries.AddRange(allEntries);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSelectionChanged(XUiC_ListEntry _previousEntry, XUiC_ListEntry _newEntry)
	{
		if (!ignoreSelectionChange)
		{
			this.SelectionChanged?.Invoke(this, (_previousEntry != null) ? _previousEntry.GetEntry() : null, (_newEntry != null) ? _newEntry.GetEntry() : null);
			if (!SelectableEntries)
			{
				ignoreSelectionChange = true;
				ClearSelection();
				ignoreSelectionChange = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void goToPageOfCurrentlySelectedEntry()
	{
		if (currentSelectedEntry == null)
		{
			return;
		}
		T val = currentSelectedEntry;
		bool flag = false;
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i] == currentSelectedEntry)
			{
				forceEntryVisible(i);
				flag = true;
				break;
			}
		}
		currentSelectedEntry = (flag ? val : null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void forceEntryVisible(int _index)
	{
		if (_index >= 0 && _index < filteredEntries.Count && (_index < CurrentBaseIndex || _index > CurrentPageMaxIndex))
		{
			switch (PagingStepSize)
			{
			case EPagingStepSize.Page:
				Page = _index / PageLength;
				break;
			case EPagingStepSize.SingleEntry:
				CurrentBaseIndex = _index;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		if (filteredEntries == null)
		{
			Log.Error("filteredEntries is null!");
			return;
		}
		for (int i = 0; i < listEntryControllers.Length; i++)
		{
			int num = i + CurrentBaseIndex;
			XUiC_ListEntry xUiC_ListEntry = listEntryControllers[i];
			if (xUiC_ListEntry == null)
			{
				Log.Error("listEntry is null! {0} items in listEntryControllers", listEntryControllers.Length);
				break;
			}
			if (num >= 0 && num < filteredEntries.Count && i < PageLength)
			{
				xUiC_ListEntry.SetEntry(filteredEntries[num]);
			}
			else
			{
				xUiC_ListEntry.SetEntry(null);
				if (xUiC_ListEntry.Selected)
				{
					xUiC_ListEntry.Selected = false;
				}
			}
			if (!updateSelectedItemByIndex && currentSelectedEntry != null && currentSelectedEntry == xUiC_ListEntry.GetEntry())
			{
				SelectedEntry = xUiC_ListEntry;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePageLabel()
	{
		if (pager != null)
		{
			pager.LastPageNumber = LastPage;
			pager.CurrentPageNumber = page;
		}
	}

	public override void Update(float _dt)
	{
		if (SelectableEntries && CursorControllable && columns > 0 && rows > 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !xui.playerUI.windowManager.IsInputActive())
		{
			PlayerActionsGUI gUIActions = windowGroup.playerUI.playerInput.GUIActions;
			if (gUIActions.Left.WasPressed)
			{
				SelectedEntryIndex = Math.Max(0, SelectedEntryIndex - rows);
			}
			if (gUIActions.Right.WasPressed)
			{
				SelectedEntryIndex = Math.Min(EntryCount - 1, SelectedEntryIndex + rows);
			}
			if (gUIActions.Up.WasPressed)
			{
				SelectedEntryIndex = Math.Max(0, SelectedEntryIndex - 1);
			}
			if (gUIActions.Down.WasPressed)
			{
				SelectedEntryIndex = Math.Min(EntryCount - 1, SelectedEntryIndex + 1);
			}
		}
		if (IsDirty)
		{
			goToPageOfCurrentlySelectedEntry();
			if (page > LastPage)
			{
				Page = LastPage;
			}
			updateCurrentPageContents();
			if (SelectedEntryData != currentSelectedEntry)
			{
				ClearSelection();
			}
			updatePageLabel();
			this.PageContentsChanged?.Invoke(this);
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ClearSelectionOnOpenClose)
		{
			ClearSelection();
		}
		if (ClearSearchTextOnOpenClose)
		{
			if (searchBox != null)
			{
				searchBox.Text = string.Empty;
				OnSearchInputChanged(this, string.Empty, _changeFromCode: true);
			}
			if (searchBox2 != null)
			{
				searchBox2.Text = string.Empty;
				OnSearchInputChanged(this, string.Empty, _changeFromCode: true);
			}
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (ClearSelectionOnOpenClose)
		{
			ClearSelection();
		}
		if (ClearSearchTextOnOpenClose)
		{
			if (searchBox != null)
			{
				searchBox.Text = "";
			}
			if (searchBox2 != null)
			{
				searchBox2.Text = "";
			}
		}
	}

	public void ClearSelection()
	{
		SelectedEntry = null;
	}

	public void SelectCursorElementForSelectedEntry()
	{
		SelectedEntry?.SelectCursorElement(_withDelay: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryClicked(T _entry)
	{
		this.ListEntryClicked?.Invoke(this, _entry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryDoubleClicked(T _entry)
	{
		this.ListEntryDoubleClicked?.Invoke(this, _entry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryHovered(T _entry, bool _isOver)
	{
		this.ListEntryHovered?.Invoke(this, _entry, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List()
	{
	}
}
