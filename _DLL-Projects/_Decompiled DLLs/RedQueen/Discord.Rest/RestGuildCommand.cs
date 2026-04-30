using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestGuildCommand : RestApplicationCommand
{
	public ulong GuildId { get; private set; }

	internal RestGuildCommand(BaseDiscordClient client, ulong id, ulong guildId)
		: base(client, id)
	{
		GuildId = guildId;
	}

	internal static RestGuildCommand Create(BaseDiscordClient client, ApplicationCommand model, ulong guildId)
	{
		RestGuildCommand restGuildCommand = new RestGuildCommand(client, model.Id, guildId);
		restGuildCommand.Update(model);
		return restGuildCommand;
	}

	public override async Task DeleteAsync(RequestOptions options = null)
	{
		await InteractionHelper.DeleteGuildCommandAsync(base.Discord, GuildId, this).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null)
	{
		Update(await InteractionHelper.ModifyGuildCommandAsync(base.Discord, this, GuildId, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task<GuildApplicationCommandPermission> GetCommandPermission(RequestOptions options = null)
	{
		return InteractionHelper.GetGuildCommandPermissionAsync(base.Discord, GuildId, base.Id, options);
	}

	public Task<GuildApplicationCommandPermission> ModifyCommandPermissions(ApplicationCommandPermission[] permissions, RequestOptions options = null)
	{
		return InteractionHelper.ModifyGuildCommandPermissionsAsync(base.Discord, GuildId, base.Id, permissions, options);
	}

	public Task<RestGuild> GetGuild(bool withCounts = false, RequestOptions options = null)
	{
		return ClientHelper.GetGuildAsync(base.Discord, GuildId, withCounts, options);
	}
}
