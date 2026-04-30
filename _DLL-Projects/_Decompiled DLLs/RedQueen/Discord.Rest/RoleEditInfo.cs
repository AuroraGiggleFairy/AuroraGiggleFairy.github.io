using System.Runtime.CompilerServices;

namespace Discord.Rest;

internal struct RoleEditInfo(Color? color, bool? mentionable, bool? hoist, string name, GuildPermissions? permissions)
{
	public Color? Color
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = color;

	public bool? Mentionable
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = mentionable;

	public bool? Hoist
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = hoist;

	public string Name
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = name;

	public GuildPermissions? Permissions
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	} = permissions;
}
