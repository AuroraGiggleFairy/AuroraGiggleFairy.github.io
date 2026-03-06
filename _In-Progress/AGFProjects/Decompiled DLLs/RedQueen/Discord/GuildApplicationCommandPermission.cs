using System.Collections.Generic;

namespace Discord;

internal class GuildApplicationCommandPermission
{
	public ulong CommandId { get; }

	public ulong ApplicationId { get; }

	public ulong GuildId { get; }

	public IReadOnlyCollection<ApplicationCommandPermission> Permissions { get; }

	internal GuildApplicationCommandPermission(ulong commandId, ulong appId, ulong guildId, ApplicationCommandPermission[] permissions)
	{
		CommandId = commandId;
		ApplicationId = appId;
		GuildId = guildId;
		Permissions = permissions;
	}
}
