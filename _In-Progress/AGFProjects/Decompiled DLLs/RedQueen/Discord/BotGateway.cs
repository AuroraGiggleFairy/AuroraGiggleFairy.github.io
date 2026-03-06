namespace Discord;

internal class BotGateway
{
	public string Url { get; internal set; }

	public int Shards { get; internal set; }

	public SessionStartLimit SessionStartLimit { get; internal set; }
}
