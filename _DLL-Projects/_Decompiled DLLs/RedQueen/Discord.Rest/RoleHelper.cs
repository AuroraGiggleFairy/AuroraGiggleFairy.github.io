using System;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class RoleHelper
{
	public static async Task DeleteAsync(IRole role, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.DeleteGuildRoleAsync(role.Guild.Id, role.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<Role> ModifyAsync(IRole role, BaseDiscordClient client, Action<RoleProperties> func, RequestOptions options)
	{
		RoleProperties args = new RoleProperties();
		func(args);
		if (args.Icon.IsSpecified || args.Emoji.IsSpecified)
		{
			role.Guild.Features.EnsureFeature(GuildFeature.RoleIcons);
			if (args.Icon.IsSpecified && args.Icon.Value.HasValue && args.Emoji.IsSpecified && args.Emoji.Value != null)
			{
				throw new ArgumentException("Emoji and Icon properties cannot be present on a role at the same time.");
			}
		}
		ModifyGuildRoleParams modifyGuildRoleParams = new ModifyGuildRoleParams
		{
			Color = (args.Color.IsSpecified ? ((Optional<uint>)args.Color.Value.RawValue) : Optional.Create<uint>()),
			Hoist = args.Hoist,
			Mentionable = args.Mentionable,
			Name = args.Name,
			Permissions = (args.Permissions.IsSpecified ? ((Optional<string>)args.Permissions.Value.RawValue.ToString()) : Optional.Create<string>()),
			Icon = (args.Icon.IsSpecified ? ((Optional<Discord.API.Image?>)(args.Icon.Value?.ToModel())) : Optional<Discord.API.Image?>.Unspecified),
			Emoji = (args.Emoji.IsSpecified ? ((Optional<string>)(args.Emoji.Value?.Name ?? "")) : Optional.Create<string>())
		};
		if (args.Icon.IsSpecified && args.Icon.Value.HasValue && role.Emoji != null)
		{
			modifyGuildRoleParams.Emoji = "";
		}
		if (args.Emoji.IsSpecified && args.Emoji.Value != null && !string.IsNullOrEmpty(role.Icon))
		{
			modifyGuildRoleParams.Icon = Optional<Discord.API.Image?>.Unspecified;
		}
		Role model = await client.ApiClient.ModifyGuildRoleAsync(role.Guild.Id, role.Id, modifyGuildRoleParams, options).ConfigureAwait(continueOnCapturedContext: false);
		if (args.Position.IsSpecified)
		{
			ModifyGuildRolesParams[] args2 = new ModifyGuildRolesParams[1]
			{
				new ModifyGuildRolesParams(role.Id, args.Position.Value)
			};
			await client.ApiClient.ModifyGuildRolesAsync(role.Guild.Id, args2, options).ConfigureAwait(continueOnCapturedContext: false);
			model.Position = args.Position.Value;
		}
		return model;
	}
}
