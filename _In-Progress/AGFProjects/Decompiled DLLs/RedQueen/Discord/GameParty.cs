namespace Discord;

internal class GameParty
{
	public string Id { get; internal set; }

	public long Members { get; internal set; }

	public long Capacity { get; internal set; }

	internal GameParty()
	{
	}
}
