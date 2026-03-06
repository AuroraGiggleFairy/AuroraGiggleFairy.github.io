using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public abstract class XUiC_List<T> : XUiController where T : XUiListEntry<T>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> filteredEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public T CurrentSelectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry<T>[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry<T> selectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntryIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool updateSelectedItemByIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_TextInput searchBox;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<T> allEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string previousMatch = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignore_selection_change;

	public int PageLength
	{
		get
		{
			if (listEntryControllers == null)
			{
				return 0;
			}
			return listEntryControllers.Length;
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
			int num = Math.Max(0, Math.Min(value, LastPage));
			if (num != page)
			{
				page = num;
				pager?.SetPage(page);
				IsDirty = true;
				SelectedEntry = null;
				this.PageNumberChanged?.Invoke(page);
			}
		}
	}

	public int LastPage => Math.Max(0, Mathf.CeilToInt((float)filteredEntries.Count / (float)PageLength) - 1);

	public XUiC_ListEntry<T> SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (value == selectedEntry)
			{
				return;
			}
			T currentSelectedEntry = CurrentSelectedEntry;
			XUiC_ListEntry<T> xUiC_ListEntry = selectedEntry;
			selectedEntry = null;
			if (xUiC_ListEntry != null)
			{
				xUiC_ListEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				CurrentSelectedEntry = selectedEntry.GetEntry();
				for (int i = 0; i < listEntryControllers.Length; i++)
				{
					if (selectedEntry == listEntryControllers[i])
					{
						selectedEntryIndex = page * PageLength + i;
						break;
					}
				}
			}
			else
			{
				CurrentSelectedEntry = null;
				selectedEntryIndex = -1;
			}
			if (currentSelectedEntry != CurrentSelectedEntry)
			{
				OnSelectionChanged(xUiC_ListEntry, selectedEntry);
			}
			IsDirty = true;
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
				Page = value / PageLength;
				selectedEntryIndex = value;
				updateSelectedItemByIndex = true;
				updateCurrentPageContents();
				updateSelectedItemByIndex = false;
				SelectedEntry = listEntryControllers[selectedEntryIndex % PageLength];
				IsDirty = true;
			}
		}
	}

	public int EntryCount => filteredEntries.Count;

	public int UnfilteredEntryCount => allEntries.Count;

	public bool ClearSelectionOnOpenClose { get; set; } = true;

	public bool ClearSearchTextOnOpenClose { get; set; }

	public bool SelectableEntries { get; set; } = true;

	public bool CursorControllable { get; set; }

	public event XUiEvent_ListSelectionChangedEventHandler<T> SelectionChanged;

	public event XUiEvent_ListPageNumberChangedEventHandler PageNumberChanged;

	public event XUiEvent_ListEntryClickedEventHandler<T> ListEntryClicked;

	public event XUiEvent_PageContentsChangedEventHandler PageContentsChanged;

	public T GetEntry(int _index)
	{
		return filteredEntries[_index];
	}

	public override void Init()
	{
		base.Init();
		base.OnScroll += HandleOnScroll;
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
		XUiController childById = GetChildById("list");
		if (childById == null)
		{
			Log.Warning("[XUi] List controller without a 'list' child element! (window group '" + base.WindowGroup.ID + "')");
			listEntryControllers = Array.Empty<XUiC_ListEntry<T>>();
		}
		else
		{
			listEntryControllers = new XUiC_ListEntry<T>[childById.Children.Count];
			for (int num = 0; num < childById.Children.Count; num++)
			{
				listEntryControllers[num] = childById.Children[num] as XUiC_ListEntry<T>;
				if (listEntryControllers[num] != null)
				{
					listEntryControllers[num].OnScroll += HandleOnScroll;
					listEntryControllers[num].List = this;
				}
				else
				{
					Log.Warning("[XUi] List elements do not have the correct controller set (should be \"XUiC_ListEntry<" + typeof(T).FullName + ">\")");
				}
			}
			if (CursorControllable)
			{
				if (childById.ViewComponent is XUiV_Grid xUiV_Grid)
				{
					if (xUiV_Grid.Arrangement == UIGrid.Arrangement.Horizontal)
					{
						columns = xUiV_Grid.Columns;
						rows = PageLength / columns;
					}
					else
					{
						rows = xUiV_Grid.Rows;
						columns = PageLength / rows;
					}
				}
				if (childById.ViewComponent is XUiV_Table xUiV_Table)
				{
					columns = xUiV_Table.Columns;
					rows = PageLength / columns;
				}
			}
		}
		searchBox = GetChildById("searchInput") as XUiC_TextInput;
		if (searchBox != null)
		{
			searchBox.OnChangeHandler += OnSearchInputChanged;
			searchBox.OnSubmitHandler += OnSearchInputSubmit;
		}
		RebuildList(_resetFilter: true);
	}

	public virtual void RebuildList(bool _resetFilter = false)
	{
		SelectedEntry = null;
		if (filteredEntries != null)
		{
			filteredEntries.Clear();
		}
		RefreshView(_resetFilter);
	}

	public virtual void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
		if (_resetFilter && searchBox != null)
		{
			searchBox.Text = "";
		}
		OnSearchInputChanged(this, searchBox?.Text, _changeFromCode: true);
		if (_resetPage)
		{
			Page = 0;
		}
	}

	public XUiC_ListEntry<T> IsVisible(T _value)
	{
		XUiC_ListEntry<T>[] array = listEntryControllers;
		foreach (XUiC_ListEntry<T> xUiC_ListEntry in array)
		{
			T entry = xUiC_ListEntry.GetEntry();
			if (entry != null && entry == _value)
			{
				return xUiC_ListEntry;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSearchInputSubmit(XUiController _sender, string _text)
	{
		OnSearchInputChanged(_sender, _text, _changeFromCode: false);
	}

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
	public virtual void OnSelectionChanged(XUiC_ListEntry<T> _previousEntry, XUiC_ListEntry<T> _newEntry)
	{
		if (!ignore_selection_change && this.SelectionChanged != null)
		{
			this.SelectionChanged(_previousEntry, _newEntry);
			if (!SelectableEntries)
			{
				ignore_selection_change = true;
				SelectedEntry = null;
				ignore_selection_change = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void goToPageOfCurrentlySelectedEntry()
	{
		if (CurrentSelectedEntry == null)
		{
			return;
		}
		T currentSelectedEntry = CurrentSelectedEntry;
		bool flag = false;
		for (int i = 0; i < filteredEntries.Count; i++)
		{
			if (filteredEntries[i] == CurrentSelectedEntry)
			{
				Page = i / PageLength;
				flag = true;
				break;
			}
		}
		CurrentSelectedEntry = (flag ? currentSelectedEntry : null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		if (filteredEntries == null)
		{
			Log.Error("filteredEntries is null!");
			return;
		}
		for (int i = 0; i < PageLength; i++)
		{
			int num = i + PageLength * page;
			XUiC_ListEntry<T> xUiC_ListEntry = listEntryControllers[i];
			if (xUiC_ListEntry == null)
			{
				Log.Error("listEntry is null! {0} items in listEntryControllers", listEntryControllers.Length);
				break;
			}
			if (num < filteredEntries.Count)
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
			if (!updateSelectedItemByIndex && CurrentSelectedEntry != null && CurrentSelectedEntry == xUiC_ListEntry.GetEntry() && SelectedEntry != xUiC_ListEntry)
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
			pager.CurrentPageNumber = page;
			pager.LastPageNumber = LastPage;
		}
	}

	public override void Update(float _dt)
	{
		if (SelectableEntries && CursorControllable && columns > 0 && rows > 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !base.xui.playerUI.windowManager.IsInputActive())
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
			if (SelectedEntry != null && SelectedEntry.GetEntry() != CurrentSelectedEntry)
			{
				ClearSelection();
			}
			updatePageLabel();
			this.PageContentsChanged?.Invoke();
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "hasselection":
		{
			XUiC_ListEntry<T> xUiC_ListEntry = SelectedEntry;
			_value = (((xUiC_ListEntry != null) ? xUiC_ListEntry.GetEntry() : null) != null).ToString();
			return true;
		}
		case "entrycounttotal":
			_value = allEntries.Count.ToString();
			return true;
		case "entrycountfiltered":
			_value = filteredEntries.Count.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (base.ParseAttribute(_name, _value, _parent))
		{
			return true;
		}
		switch (_name)
		{
		case "selectable":
			SelectableEntries = StringParsers.ParseBool(_value);
			break;
		case "clear_selection_on_open":
			ClearSelectionOnOpenClose = StringParsers.ParseBool(_value);
			break;
		case "clear_searchtext_on_open":
			ClearSearchTextOnOpenClose = StringParsers.ParseBool(_value);
			break;
		case "cursor_controllable":
			CursorControllable = StringParsers.ParseBool(_value);
			break;
		default:
			return false;
		}
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ClearSelectionOnOpenClose)
		{
			ClearSelection();
		}
		if (ClearSearchTextOnOpenClose && searchBox != null)
		{
			searchBox.Text = string.Empty;
			OnSearchInputChanged(this, string.Empty, _changeFromCode: true);
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
		if (ClearSearchTextOnOpenClose && searchBox != null)
		{
			searchBox.Text = "";
		}
	}

	public void ClearSelection()
	{
		SelectedEntry = null;
	}

	public virtual void OnListEntryClicked(XUiC_ListEntry<T> _entry)
	{
		this.ListEntryClicked?.Invoke(_entry);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List()
	{
	}
}
