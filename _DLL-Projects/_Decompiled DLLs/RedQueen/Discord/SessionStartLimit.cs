namespace Discord;

internal class SessionStartLimit
{
	public int Total { get; internal set; }

	public int Remaining { get; internal set; }

	public int ResetAfter { get; internal set; }

	public int MaxConcurrency { get; internal set; }
}
