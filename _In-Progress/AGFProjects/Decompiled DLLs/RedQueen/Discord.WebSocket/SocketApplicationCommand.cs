using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Gateway;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketApplicationCommand : SocketEntity<ulong>, IApplicationCommand, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	public bool IsGlobalCommand => !GuildId.HasValue;

	public ulong ApplicationId { get; private set; }

	public string Name { get; private set; }

	public ApplicationCommandType Type { get; private set; }

	public string Description { get; private set; }

	public bool IsDefaultPermission { get; private set; }

	public bool IsEnabledInDm { get; private set; }

	public GuildPermissions DefaultMemberPermissions { get; private set; }

	public IReadOnlyCollection<SocketApplicationCommandOption> Options { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; private set; }

	public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; private set; }

	public string NameLocalized { get; private set; }

	public string DescriptionLocalized { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public SocketGuild Guild
	{
		get
		{
			if (!GuildId.HasValue)
			{
				return null;
			}
			return base.Discord.GetGuild(GuildId.Value);
		}
	}

	private ulong? GuildId { get; set; }

	IReadOnlyCollection<IApplicationCommandOption> IApplicationCommand.Options => Options;

	internal SocketApplicationCommand(DiscordSocketClient client, ulong id, ulong? guildId)
		: base(client, id)
	{
		GuildId = guildId;
	}

	internal static SocketApplicationCommand Create(DiscordSocketClient client, ApplicationCommandCreatedUpdatedEvent model)
	{
		SocketApplicationCommand socketApplicationCommand = new SocketApplicationCommand(client, model.Id, model.GuildId.ToNullable());
		socketApplicationCommand.Update(model);
		return socketApplicationCommand;
	}

	internal static SocketApplicationCommand Create(DiscordSocketClient client, ApplicationCommand model, ulong? guildId = null)
	{
		SocketApplicationCommand socketApplicationCommand = new SocketApplicationCommand(client, model.Id, guildId);
		socketApplicationCommand.Update(model);
		return socketApplicationCommand;
	}

	internal void Update(ApplicationCommand model)
	{
		ApplicationId = model.ApplicationId;
		Description = model.Description;
		Name = model.Name;
		IsDefaultPermission = model.DefaultPermissions.GetValueOrDefault(defaultValue: true);
		Type = model.Type;
		Options = (model.Options.IsSpecified ? model.Options.Value.Select(SocketApplicationCommandOption.Create).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<SocketApplicationCommandOption>());
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		DescriptionLocalizations = model.DescriptionLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		NameLocalized = model.NameLocalized.GetValueOrDefault();
		DescriptionLocalized = model.DescriptionLocalized.GetValueOrDefault();
		IsEnabledInDm = model.DmPermission.GetValueOrDefault(true) ?? true;
		DefaultMemberPermissions = new GuildPermissions((ulong)(model.DefaultMemberPermission.GetValueOrDefault((GuildPermission)0uL) ?? ((GuildPermission)0uL)));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteUnknownApplicationCommandAsync(base.Discord, GuildId, this, options);
	}

	public Task ModifyAsync(Action<ApplicationCommandProperties> func, RequestOptions options = null)
	{
		return this.ModifyAsync<ApplicationCommandProperties>(func, options);
	}

	public async Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties
	{
		ApplicationCommand applicationCommand = ((!IsGlobalCommand) ? (await InteractionHelper.ModifyGuildCommandAsync(base.Discord, this, GuildId.Value, func, options)) : (await InteractionHelper.ModifyGlobalCommandAsync(base.Discord, this, func, options).ConfigureAwait(continueOnCapturedContext: false)));
		ApplicationCommand model = applicationCommand;
		Update(model);
	}
}
