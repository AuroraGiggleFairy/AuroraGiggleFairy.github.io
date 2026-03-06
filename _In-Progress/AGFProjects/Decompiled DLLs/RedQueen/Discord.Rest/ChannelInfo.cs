using System.Runtime.CompilerServices;

namespace Discord.Rest;

internal struct ChannelInfo(string name, string topic, int? rateLimit, bool? nsfw, int? bitrate, ChannelType? type)
{
	public string Name
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = name;

	public string Topic
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = topic;

	public int? SlowModeInterval
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = rateLimit;

	public bool? IsNsfw
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = nsfw;

	public int? Bitrate
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = bitrate;

	public ChannelType? ChannelType
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = type;
}
