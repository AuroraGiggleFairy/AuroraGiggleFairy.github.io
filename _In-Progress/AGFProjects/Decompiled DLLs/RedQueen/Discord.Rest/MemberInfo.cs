using System.Runtime.CompilerServices;

namespace Discord.Rest;

internal struct MemberInfo(string nick, bool? deaf, bool? mute)
{
	public string Nickname
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = nick;

	public bool? Deaf
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = deaf;

	public bool? Mute
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = mute;
}
