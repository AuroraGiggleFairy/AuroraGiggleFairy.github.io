public abstract class XUiC_DMBaseList<T> : XUiC_List<T> where T : XUiListEntry<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController hoveredElement;

	public event XUiEvent_OnPressEventHandler OnEntryClicked;

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public event XUiEvent_OnHoverEventHandler OnChildElementHovered;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<T>[] array = listEntryControllers;
		foreach (XUiC_ListEntry<T> obj in array)
		{
			obj.OnPress += EntryClicked;
			obj.OnDoubleClick += EntryDoubleClicked;
			obj.OnHover += ChildElementHovered;
		}
		foreach (XUiController child in pager.Children)
		{
			child.OnHover += ChildElementHovered;
		}
		base.PageContentsChanged += PageContentsChangedHandler;
		searchBox.OnHover += ChildElementHovered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageContentsChangedHandler()
	{
		if (hoveredElement != null && this.OnChildElementHovered != null)
		{
			this.OnChildElementHovered(hoveredElement, _isOver: false);
			this.OnChildElementHovered(hoveredElement, _isOver: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void EntryClicked(XUiController _sender, int _mouseButton)
	{
		this.OnEntryClicked?.Invoke(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		this.OnEntryDoubleClicked?.Invoke(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ChildElementHovered(XUiController _sender, bool _isOver)
	{
		hoveredElement = (_isOver ? _sender : null);
		this.OnChildElementHovered?.Invoke(_sender, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_DMBaseList()
	{
	}
}
