using System.Diagnostics;
using System.Runtime.CompilerServices;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct RestGuildWidget
{
	public bool IsEnabled
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
		private set; }

	public ulong? ChannelId
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
		private set; }

	private string DebuggerDisplay => string.Format("{0} ({1})", ChannelId, IsEnabled ? "Enabled" : "Disabled");

	internal RestGuildWidget(bool isEnabled, ulong? channelId)
	{
		ChannelId = channelId;
		IsEnabled = isEnabled;
	}

	internal static RestGuildWidget Create(GuildWidget model)
	{
		return new RestGuildWidget(model.Enabled, model.ChannelId);
	}

	public override string ToString()
	{
		return ChannelId?.ToString() ?? "Unknown";
	}
}
