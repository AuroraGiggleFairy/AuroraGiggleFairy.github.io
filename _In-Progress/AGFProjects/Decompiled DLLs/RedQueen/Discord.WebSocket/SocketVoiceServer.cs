using System.Diagnostics;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketVoiceServer
{
	public Cacheable<IGuild, ulong> Guild { get; }

	public string Endpoint { get; }

	public string Token { get; }

	private string DebuggerDisplay => $"SocketVoiceServer ({Guild.Id})";

	internal SocketVoiceServer(Cacheable<IGuild, ulong> guild, string endpoint, string token)
	{
		Guild = guild;
		Endpoint = endpoint;
		Token = token;
	}
}
