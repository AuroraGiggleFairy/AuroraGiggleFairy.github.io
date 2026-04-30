using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace RedQueenMod;

[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
[_003C3a83553a_002D5440_002D4c46_002Da050_002Dad27acd8eda3_003ENullable(0)]
public class ChatHistoryMessage
{
	[JsonProperty("timestamp")]
	public DateTime Timestamp { get; set; }

	[JsonProperty("source")]
	public string Source { get; set; } = "";

	[JsonProperty("username")]
	public string PlayerName { get; set; } = "";

	[JsonProperty("message")]
	public string Message { get; set; } = "";

	[JsonProperty("is_ai_response")]
	public bool IsAI { get; set; }

	[JsonProperty("is_bot")]
	public bool IsBot { get; set; }
}
