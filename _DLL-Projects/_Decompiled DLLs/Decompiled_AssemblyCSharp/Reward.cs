using Newtonsoft.Json;

public class Reward
{
	[JsonProperty("id")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Id { get; set; } = "";

	[JsonProperty("title")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Title { get; set; } = "";

	[JsonProperty("cost")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Cost { get; set; }
}
