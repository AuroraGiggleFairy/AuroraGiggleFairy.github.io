using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IApplicationCommand : ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	ulong ApplicationId { get; }

	ApplicationCommandType Type { get; }

	string Name { get; }

	string Description { get; }

	bool IsDefaultPermission { get; }

	bool IsEnabledInDm { get; }

	GuildPermissions DefaultMemberPermissions { get; }

	IReadOnlyCollection<IApplicationCommandOption> Options { get; }

	IReadOnlyDictionary<string, string> NameLocalizations { get; }

	IReadOnlyDictionary<string, string> DescriptionLocalizations { get; }

	string NameLocalized { get; }

	string DescriptionLocalized { get; }

	Task ModifyAsync(Action<ApplicationCommandProperties> func, RequestOptions options = null);

	Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties;
}
