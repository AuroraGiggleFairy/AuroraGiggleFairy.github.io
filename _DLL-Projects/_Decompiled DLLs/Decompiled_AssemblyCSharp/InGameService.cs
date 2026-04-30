using System;

public class InGameService
{
	public enum InGameServiceTypes
	{
		VendingRent
	}

	public Action<bool> VisibleChangedHandler;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public InGameServiceTypes ServiceType { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Description { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Icon { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Price { get; set; }
}
