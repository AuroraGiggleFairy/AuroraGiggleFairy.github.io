using System;

namespace Discord.Interactions;

internal interface IApplicationCommandInfo
{
	string Name { get; }

	ApplicationCommandType CommandType { get; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	bool DefaultPermission { get; }

	bool IsEnabledInDm { get; }

	GuildPermission? DefaultMemberPermissions { get; }
}
