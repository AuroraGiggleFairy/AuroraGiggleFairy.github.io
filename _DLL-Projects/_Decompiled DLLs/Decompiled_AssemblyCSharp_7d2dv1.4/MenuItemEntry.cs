public class MenuItemEntry
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Text { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string IconName { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsEnabled { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public object Tag { get; set; }

	public event XUiEvent_MenuItemClicked ItemClicked;

	public void HandleItemClicked()
	{
		if (this.ItemClicked != null)
		{
			this.ItemClicked(this);
		}
	}
}
