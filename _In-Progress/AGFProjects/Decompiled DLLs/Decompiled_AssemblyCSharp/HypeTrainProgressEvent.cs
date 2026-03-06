using Newtonsoft.Json;

public class HypeTrainProgressEvent
{
	[JsonProperty("level")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Level { get; set; }
}
