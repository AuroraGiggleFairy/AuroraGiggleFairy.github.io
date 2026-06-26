public class ToolTipEvent
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public object Parameter { get; set; }

	public event ToolTipEventHandler EventHandler;

	public void HandleEvent()
	{
		this.EventHandler?.Invoke(Parameter);
	}
}
