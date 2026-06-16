public abstract class XUiC_DMBaseList<T> : XUiC_List<T> where T : XUiListEntry<T>
{
	public delegate void ListEntryHoveredDelegate(T _entry, bool _isOver);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController hoveredElement;

	public event ListEntryHoveredDelegate OnEntryHovered;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry[] array = listEntryControllers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnHover += childElementHovered;
		}
		foreach (XUiController child in pager.Children)
		{
			child.OnHover += childElementHovered;
		}
		base.PageContentsChanged += pageContentsChangedHandler;
		searchBox.OnHover += childElementHovered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void pageContentsChangedHandler(XUiC_List<T> _list)
	{
		if (hoveredElement != null && this.OnEntryHovered != null)
		{
			T entry = null;
			if (hoveredElement is XUiC_ListEntry xUiC_ListEntry)
			{
				entry = xUiC_ListEntry.GetEntry();
			}
			this.OnEntryHovered(entry, _isOver: false);
			this.OnEntryHovered(entry, _isOver: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void childElementHovered(XUiController _sender, bool _isOver)
	{
		hoveredElement = (_isOver ? _sender : null);
		XUiC_ListEntry xUiC_ListEntry = _sender as XUiC_ListEntry;
		this.OnEntryHovered?.Invoke((xUiC_ListEntry != null) ? xUiC_ListEntry.GetEntry() : null, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_DMBaseList()
	{
	}
}
