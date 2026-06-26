using UnityEngine.Scripting;

[Preserve]
public class XUiC_SelectableEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

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
				if (base.xui.currentSelectedEntry != null)
				{
					base.xui.currentSelectedEntry.SelectedChanged(isSelected: false);
					base.xui.currentSelectedEntry.selected = false;
				}
			}
			else if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.SelectedChanged(isSelected: false);
				base.xui.currentSelectedEntry.selected = false;
				base.xui.currentSelectedEntry = null;
			}
			selected = value;
			if (selected)
			{
				base.xui.currentSelectedEntry = this;
			}
			SelectedChanged(selected);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool isSelected)
	{
	}
}
