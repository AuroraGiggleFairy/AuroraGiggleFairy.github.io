using System;

public class OnCountChangedEventArgs : EventArgs
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Count { get; set; }
}
