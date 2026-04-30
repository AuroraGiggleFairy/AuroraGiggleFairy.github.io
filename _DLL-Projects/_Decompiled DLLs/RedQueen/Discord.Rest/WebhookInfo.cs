using System.Runtime.CompilerServices;

namespace Discord.Rest;

internal struct WebhookInfo(string name, ulong? channelId, string avatar)
{
	public string Name
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = name;

	public ulong? ChannelId
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = channelId;

	public string Avatar
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = avatar;
}
