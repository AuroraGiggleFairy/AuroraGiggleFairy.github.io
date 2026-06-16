public class XUiC_SelectableEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			if (value)
			{
				if (xui.CurrentSelectedEntry != null)
				{
					xui.CurrentSelectedEntry.SelectedChanged(_isSelected: false);
					xui.CurrentSelectedEntry.isSelected = false;
				}
			}
			else if (xui.CurrentSelectedEntry == this)
			{
				xui.CurrentSelectedEntry.SelectedChanged(_isSelected: false);
				xui.CurrentSelectedEntry.isSelected = false;
				xui.CurrentSelectedEntry = null;
			}
			isSelected = value;
			if (isSelected)
			{
				xui.CurrentSelectedEntry = this;
			}
			SelectedChanged(isSelected);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool _isSelected)
	{
	}
}
