using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal abstract class RestApplicationCommand : RestEntity<ulong>, IApplicationCommand, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	public ulong ApplicationId { get; private set; }

	public ApplicationCommandType Type { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public bool IsDefaultPermission { get; private set; }

	public bool IsEnabledInDm { get; private set; }

	public GuildPermissions DefaultMemberPermissions { get; private set; }

	public IReadOnlyCollection<RestApplicationCommandOption> Options { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; private set; }

	public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; private set; }

	public string NameLocalized { get; private set; }

	public string DescriptionLocalized { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	IReadOnlyCollection<IApplicationCommandOption> IApplicationCommand.Options => Options;

	internal RestApplicationCommand(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal static RestApplicationCommand Create(BaseDiscordClient client, ApplicationCommand model, ulong? guildId)
	{
		if (!guildId.HasValue)
		{
			return RestGlobalCommand.Create(client, model);
		}
		return RestGuildCommand.Create(client, model, guildId.Value);
	}

	internal virtual void Update(ApplicationCommand model)
	{
		Type = model.Type;
		ApplicationId = model.ApplicationId;
		Name = model.Name;
		Description = model.Description;
		IsDefaultPermission = model.DefaultPermissions.GetValueOrDefault(defaultValue: true);
		Options = (model.Options.IsSpecified ? model.Options.Value.Select(RestApplicationCommandOption.Create).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<RestApplicationCommandOption>());
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		DescriptionLocalizations = model.DescriptionLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		NameLocalized = model.NameLocalized.GetValueOrDefault();
		DescriptionLocalized = model.DescriptionLocalized.GetValueOrDefault();
		IsEnabledInDm = model.DmPermission.GetValueOrDefault(true) ?? true;
		DefaultMemberPermissions = new GuildPermissions((ulong)(model.DefaultMemberPermission.GetValueOrDefault((GuildPermission)0uL) ?? ((GuildPermission)0uL)));
	}

	public abstract Task DeleteAsync(RequestOptions options = null);

	public Task ModifyAsync(Action<ApplicationCommandProperties> func, RequestOptions options = null)
	{
		return this.ModifyAsync<ApplicationCommandProperties>(func, options);
	}

	public abstract Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties;
}
