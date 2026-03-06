using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class GuildHelper
{
	public static async Task<Guild> ModifyAsync(IGuild guild, BaseDiscordClient client, Action<GuildProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		GuildProperties guildProperties = new GuildProperties();
		func(guildProperties);
		ModifyGuildParams modifyGuildParams = new ModifyGuildParams
		{
			AfkChannelId = guildProperties.AfkChannelId,
			AfkTimeout = guildProperties.AfkTimeout,
			SystemChannelId = guildProperties.SystemChannelId,
			DefaultMessageNotifications = guildProperties.DefaultMessageNotifications,
			Icon = (guildProperties.Icon.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Icon.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Name = guildProperties.Name,
			Splash = (guildProperties.Splash.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Splash.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Banner = (guildProperties.Banner.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Banner.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			VerificationLevel = guildProperties.VerificationLevel,
			ExplicitContentFilter = guildProperties.ExplicitContentFilter,
			SystemChannelFlags = guildProperties.SystemChannelFlags,
			IsBoostProgressBarEnabled = guildProperties.IsBoostProgressBarEnabled
		};
		if (modifyGuildParams.Banner.IsSpecified)
		{
			guild.Features.EnsureFeature(GuildFeature.Banner);
		}
		if (modifyGuildParams.Splash.IsSpecified)
		{
			guild.Features.EnsureFeature(GuildFeature.InviteSplash);
		}
		if (guildProperties.AfkChannel.IsSpecified)
		{
			modifyGuildParams.AfkChannelId = guildProperties.AfkChannel.Value.Id;
		}
		else if (guildProperties.AfkChannelId.IsSpecified)
		{
			modifyGuildParams.AfkChannelId = guildProperties.AfkChannelId.Value;
		}
		if (guildProperties.SystemChannel.IsSpecified)
		{
			modifyGuildParams.SystemChannelId = guildProperties.SystemChannel.Value.Id;
		}
		else if (guildProperties.SystemChannelId.IsSpecified)
		{
			modifyGuildParams.SystemChannelId = guildProperties.SystemChannelId.Value;
		}
		if (guildProperties.Owner.IsSpecified)
		{
			modifyGuildParams.OwnerId = guildProperties.Owner.Value.Id;
		}
		else if (guildProperties.OwnerId.IsSpecified)
		{
			modifyGuildParams.OwnerId = guildProperties.OwnerId.Value;
		}
		if (guildProperties.Region.IsSpecified)
		{
			modifyGuildParams.RegionId = guildProperties.Region.Value.Id;
		}
		else if (guildProperties.RegionId.IsSpecified)
		{
			modifyGuildParams.RegionId = guildProperties.RegionId.Value;
		}
		if (!modifyGuildParams.Banner.IsSpecified && guild.BannerId != null)
		{
			modifyGuildParams.Banner = new Discord.API.Image(guild.BannerId);
		}
		if (!modifyGuildParams.Splash.IsSpecified && guild.SplashId != null)
		{
			modifyGuildParams.Splash = new Discord.API.Image(guild.SplashId);
		}
		if (!modifyGuildParams.Icon.IsSpecified && guild.IconId != null)
		{
			modifyGuildParams.Icon = new Discord.API.Image(guild.IconId);
		}
		if (guildProperties.ExplicitContentFilter.IsSpecified)
		{
			modifyGuildParams.ExplicitContentFilter = guildProperties.ExplicitContentFilter.Value;
		}
		if (guildProperties.SystemChannelFlags.IsSpecified)
		{
			modifyGuildParams.SystemChannelFlags = guildProperties.SystemChannelFlags.Value;
		}
		if (guildProperties.PreferredLocale.IsSpecified)
		{
			modifyGuildParams.PreferredLocale = guildProperties.PreferredLocale.Value;
		}
		else if (guildProperties.PreferredCulture.IsSpecified)
		{
			modifyGuildParams.PreferredLocale = guildProperties.PreferredCulture.Value.Name;
		}
		return await client.ApiClient.ModifyGuildAsync(guild.Id, modifyGuildParams, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<GuildWidget> ModifyWidgetAsync(IGuild guild, BaseDiscordClient client, Action<GuildWidgetProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		GuildWidgetProperties guildWidgetProperties = new GuildWidgetProperties();
		func(guildWidgetProperties);
		ModifyGuildWidgetParams modifyGuildWidgetParams = new ModifyGuildWidgetParams
		{
			Enabled = guildWidgetProperties.Enabled
		};
		if (guildWidgetProperties.Channel.IsSpecified)
		{
			modifyGuildWidgetParams.ChannelId = guildWidgetProperties.Channel.Value?.Id;
		}
		else if (guildWidgetProperties.ChannelId.IsSpecified)
		{
			modifyGuildWidgetParams.ChannelId = guildWidgetProperties.ChannelId.Value;
		}
		return await client.ApiClient.ModifyGuildWidgetAsync(guild.Id, modifyGuildWidgetParams, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task ReorderChannelsAsync(IGuild guild, BaseDiscordClient client, IEnumerable<ReorderChannelProperties> args, RequestOptions options)
	{
		IEnumerable<ModifyGuildChannelsParams> args2 = args.Select((ReorderChannelProperties x) => new ModifyGuildChannelsParams(x.Id, x.Position));
		await client.ApiClient.ModifyGuildChannelsAsync(guild.Id, args2, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IReadOnlyCollection<Role>> ReorderRolesAsync(IGuild guild, BaseDiscordClient client, IEnumerable<ReorderRoleProperties> args, RequestOptions options)
	{
		IEnumerable<ModifyGuildRolesParams> args2 = args.Select((ReorderRoleProperties x) => new ModifyGuildRolesParams(x.Id, x.Position));
		return await client.ApiClient.ModifyGuildRolesAsync(guild.Id, args2, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task LeaveAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.LeaveGuildAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.DeleteGuildAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static ulong GetUploadLimit(IGuild guild)
	{
		int num = guild.PremiumTier switch
		{
			PremiumTier.Tier2 => 50, 
			PremiumTier.Tier3 => 100, 
			_ => 8, 
		};
		double num2 = Math.Pow(2.0, 20.0);
		return (ulong)((double)num * num2);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(IGuild guild, BaseDiscordClient client, ulong? fromUserId, Direction dir, int limit, RequestOptions options)
	{
		if (dir == Direction.Around && limit > 1000)
		{
			int num = limit / 2;
			if (fromUserId.HasValue)
			{
				return GetBansAsync(guild, client, fromUserId.Value + 1, Direction.Before, num + 1, options).Concat(GetBansAsync(guild, client, fromUserId.Value, Direction.After, num, options));
			}
			return GetBansAsync(guild, client, null, Direction.Before, num + 1, options);
		}
		return new PagedAsyncEnumerable<RestBan>(1000, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildBansParams getGuildBansParams = new GetGuildBansParams
			{
				RelativeDirection = dir,
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildBansParams.RelativeUserId = info.Position.Value;
			}
			IReadOnlyCollection<Ban> obj = await client.ApiClient.GetGuildBansAsync(guild.Id, getGuildBansParams, options).ConfigureAwait(continueOnCapturedContext: false);
			System.Collections.Immutable.ImmutableArray<RestBan>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<RestBan>();
			foreach (Ban item in obj)
			{
				builder.Add(RestBan.Create(client, item));
			}
			return builder.ToImmutable();
		}, delegate(PageInfo info, IReadOnlyCollection<RestBan> lastPage)
		{
			if (lastPage.Count != 1000)
			{
				return false;
			}
			if (dir == Direction.Before)
			{
				info.Position = lastPage.Min((RestBan x) => x.User.Id);
			}
			else
			{
				info.Position = lastPage.Max((RestBan x) => x.User.Id);
			}
			return true;
		}, fromUserId, limit);
	}

	public static async Task<RestBan> GetBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, RequestOptions options)
	{
		Ban ban = await client.ApiClient.GetGuildBanAsync(guild.Id, userId, options).ConfigureAwait(continueOnCapturedContext: false);
		return (ban == null) ? null : RestBan.Create(client, ban);
	}

	public static async Task AddBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, int pruneDays, string reason, RequestOptions options)
	{
		CreateGuildBanParams args = new CreateGuildBanParams
		{
			DeleteMessageDays = pruneDays,
			Reason = reason
		};
		await client.ApiClient.CreateGuildBanAsync(guild.Id, userId, args, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, RequestOptions options)
	{
		await client.ApiClient.RemoveGuildBanAsync(guild.Id, userId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<RestGuildChannel> GetChannelAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Channel channel = await client.ApiClient.GetChannelAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (channel != null)
		{
			return RestGuildChannel.Create(client, guild, channel);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<RestGuildChannel>> GetChannelsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildChannelsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Channel x) => RestGuildChannel.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<RestTextChannel> CreateTextChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<TextChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		TextChannelProperties textChannelProperties = new TextChannelProperties();
		func?.Invoke(textChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Text)
		{
			CategoryId = textChannelProperties.CategoryId,
			Topic = textChannelProperties.Topic,
			IsNsfw = textChannelProperties.IsNsfw,
			Position = textChannelProperties.Position,
			SlowModeInterval = textChannelProperties.SlowModeInterval,
			Overwrites = (textChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)textChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestTextChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestVoiceChannel> CreateVoiceChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<VoiceChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		VoiceChannelProperties voiceChannelProperties = new VoiceChannelProperties();
		func?.Invoke(voiceChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Voice)
		{
			CategoryId = voiceChannelProperties.CategoryId,
			Bitrate = voiceChannelProperties.Bitrate,
			UserLimit = voiceChannelProperties.UserLimit,
			Position = voiceChannelProperties.Position,
			Overwrites = (voiceChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)voiceChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestVoiceChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestStageChannel> CreateStageChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<VoiceChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		VoiceChannelProperties voiceChannelProperties = new VoiceChannelProperties();
		func?.Invoke(voiceChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Stage)
		{
			CategoryId = voiceChannelProperties.CategoryId,
			Bitrate = voiceChannelProperties.Bitrate,
			UserLimit = voiceChannelProperties.UserLimit,
			Position = voiceChannelProperties.Position,
			Overwrites = (voiceChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)voiceChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestStageChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestCategoryChannel> CreateCategoryChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<GuildChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		GuildChannelProperties guildChannelProperties = new GuildChannelProperties();
		func?.Invoke(guildChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Category)
		{
			Position = guildChannelProperties.Position,
			Overwrites = (guildChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)guildChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestCategoryChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildVoiceRegionsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestIntegration>> GetIntegrationsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetIntegrationsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Integration x) => RestIntegration.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task DeleteIntegrationAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		await client.ApiClient.DeleteIntegrationAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> GetSlashCommandsAsync(IGuild guild, BaseDiscordClient client, bool withLocalizations, string locale, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildApplicationCommandsAsync(guild.Id, withLocalizations, locale, options)).Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guild.Id)).ToImmutableArray();
	}

	public static async Task<RestGuildCommand> GetSlashCommandAsync(IGuild guild, ulong id, BaseDiscordClient client, RequestOptions options)
	{
		return RestGuildCommand.Create(client, await client.ApiClient.GetGuildApplicationCommandAsync(guild.Id, id, options), guild.Id);
	}

	public static async Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildInvitesAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((InviteMetadata x) => RestInviteMetadata.Create(client, guild, null, x)).ToImmutableArray();
	}

	public static async Task<RestInviteMetadata> GetVanityInviteAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		InviteVanity vanityModel = await client.ApiClient.GetVanityInviteAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (vanityModel == null)
		{
			throw new InvalidOperationException("This guild does not have a vanity URL.");
		}
		InviteMetadata inviteMetadata = await client.ApiClient.GetInviteAsync(vanityModel.Code, options).ConfigureAwait(continueOnCapturedContext: false);
		inviteMetadata.Uses = vanityModel.Uses;
		return RestInviteMetadata.Create(client, guild, null, inviteMetadata);
	}

	public static async Task<RestRole> CreateRoleAsync(IGuild guild, BaseDiscordClient client, string name, GuildPermissions? permissions, Color? color, bool isHoisted, bool isMentionable, RequestOptions options)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		ModifyGuildRoleParams obj = new ModifyGuildRoleParams
		{
			Color = (((Optional<uint>?)color?.RawValue) ?? Optional.Create<uint>()),
			Hoist = isHoisted,
			Mentionable = isMentionable,
			Name = name
		};
		string text = permissions?.RawValue.ToString();
		obj.Permissions = ((text != null) ? ((Optional<string>)text) : Optional.Create<string>());
		ModifyGuildRoleParams args = obj;
		return RestRole.Create(client, guild, await client.ApiClient.CreateGuildRoleAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestGuildUser> AddGuildUserAsync(IGuild guild, BaseDiscordClient client, ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		AddGuildUserProperties addGuildUserProperties = new AddGuildUserProperties();
		func?.Invoke(addGuildUserProperties);
		if (addGuildUserProperties.Roles.IsSpecified)
		{
			IEnumerable<ulong> enumerable = addGuildUserProperties.Roles.Value.Select((IRole r) => r.Id);
			if (addGuildUserProperties.RoleIds.IsSpecified)
			{
				addGuildUserProperties.RoleIds = Optional.Create(addGuildUserProperties.RoleIds.Value.Concat(enumerable));
			}
			else
			{
				addGuildUserProperties.RoleIds = Optional.Create(enumerable);
			}
		}
		AddGuildMemberParams args = new AddGuildMemberParams
		{
			AccessToken = accessToken,
			Nickname = addGuildUserProperties.Nickname,
			IsDeafened = addGuildUserProperties.Deaf,
			IsMuted = addGuildUserProperties.Mute,
			RoleIds = (addGuildUserProperties.RoleIds.IsSpecified ? ((Optional<ulong[]>)addGuildUserProperties.RoleIds.Value.Distinct().ToArray()) : Optional.Create<ulong[]>())
		};
		GuildMember guildMember = await client.ApiClient.AddGuildMemberAsync(guild.Id, userId, args, options);
		return (guildMember == null) ? null : RestGuildUser.Create(client, guild, guildMember);
	}

	public static async Task AddGuildUserAsync(ulong guildId, BaseDiscordClient client, ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		AddGuildUserProperties addGuildUserProperties = new AddGuildUserProperties();
		func?.Invoke(addGuildUserProperties);
		if (addGuildUserProperties.Roles.IsSpecified)
		{
			IEnumerable<ulong> enumerable = addGuildUserProperties.Roles.Value.Select((IRole r) => r.Id);
			if (addGuildUserProperties.RoleIds.IsSpecified)
			{
				addGuildUserProperties.RoleIds.Value.Concat(enumerable);
			}
			else
			{
				addGuildUserProperties.RoleIds = Optional.Create(enumerable);
			}
		}
		AddGuildMemberParams args = new AddGuildMemberParams
		{
			AccessToken = accessToken,
			Nickname = addGuildUserProperties.Nickname,
			IsDeafened = addGuildUserProperties.Deaf,
			IsMuted = addGuildUserProperties.Mute,
			RoleIds = (addGuildUserProperties.RoleIds.IsSpecified ? ((Optional<ulong[]>)addGuildUserProperties.RoleIds.Value.Distinct().ToArray()) : Optional.Create<ulong[]>())
		};
		await client.ApiClient.AddGuildMemberAsync(guildId, userId, args, options);
	}

	public static async Task<RestGuildUser> GetUserAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		GuildMember guildMember = await client.ApiClient.GetGuildMemberAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildMember != null)
		{
			return RestGuildUser.Create(client, guild, guildMember);
		}
		return null;
	}

	public static async Task<RestGuildUser> GetCurrentUserAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return await GetUserAsync(guild, client, client.CurrentUser.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(IGuild guild, BaseDiscordClient client, ulong? fromUserId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestGuildUser>(1000, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildMembersParams getGuildMembersParams = new GetGuildMembersParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildMembersParams.AfterUserId = info.Position.Value;
			}
			return (await client.ApiClient.GetGuildMembersAsync(guild.Id, getGuildMembersParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildMember x) => RestGuildUser.Create(client, guild, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestGuildUser> lastPage)
		{
			if (lastPage.Count != 1000)
			{
				return false;
			}
			info.Position = lastPage.Max((RestGuildUser x) => x.Id);
			return true;
		}, fromUserId, limit);
	}

	public static async Task<int> PruneUsersAsync(IGuild guild, BaseDiscordClient client, int days, bool simulate, RequestOptions options, IEnumerable<ulong> includeRoleIds)
	{
		GuildPruneParams args = new GuildPruneParams(days, includeRoleIds?.ToArray());
		GetGuildPruneCountResponse getGuildPruneCountResponse = ((!simulate) ? (await client.ApiClient.BeginGuildPruneAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)) : (await client.ApiClient.GetGuildPruneCountAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)));
		return getGuildPruneCountResponse.Pruned;
	}

	public static async Task<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(IGuild guild, BaseDiscordClient client, string query, int? limit, RequestOptions options)
	{
		SearchGuildMembersParams args = new SearchGuildMembersParams
		{
			Query = query,
			Limit = (((Optional<int>?)limit) ?? Optional.Create<int>())
		};
		return (await client.ApiClient.SearchGuildMembersAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildMember x) => RestGuildUser.Create(client, guild, x)).ToImmutableArray();
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> GetAuditLogsAsync(IGuild guild, BaseDiscordClient client, ulong? from, int? limit, RequestOptions options, ulong? userId = null, ActionType? actionType = null)
	{
		return new PagedAsyncEnumerable<RestAuditLogEntry>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetAuditLogsParams getAuditLogsParams = new GetAuditLogsParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getAuditLogsParams.BeforeEntryId = info.Position.Value;
			}
			if (userId.HasValue)
			{
				getAuditLogsParams.UserId = userId.Value;
			}
			if (actionType.HasValue)
			{
				getAuditLogsParams.ActionType = (int)actionType.Value;
			}
			AuditLog model = await client.ApiClient.GetAuditLogsAsync(guild.Id, getAuditLogsParams, options);
			return model.Entries.Select((AuditLogEntry x) => RestAuditLogEntry.Create(client, model, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestAuditLogEntry> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Min((RestAuditLogEntry x) => x.Id);
			return true;
		}, from, limit);
	}

	public static async Task<RestWebhook> GetWebhookAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Webhook webhook = await client.ApiClient.GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook == null)
		{
			return null;
		}
		return RestWebhook.Create(client, guild, webhook);
	}

	public static async Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildWebhooksAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Webhook x) => RestWebhook.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildEmotesAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Discord.API.Emoji x) => x.ToEntity()).ToImmutableArray();
	}

	public static async Task<GuildEmote> GetEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildEmoteAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static async Task<GuildEmote> CreateEmoteAsync(IGuild guild, BaseDiscordClient client, string name, Image image, Optional<IEnumerable<IRole>> roles, RequestOptions options)
	{
		CreateGuildEmoteParams createGuildEmoteParams = new CreateGuildEmoteParams
		{
			Name = name,
			Image = image.ToModel()
		};
		if (roles.IsSpecified)
		{
			createGuildEmoteParams.RoleIds = roles.Value?.Select((IRole xr) => xr.Id).ToArray();
		}
		return (await client.ApiClient.CreateGuildEmoteAsync(guild.Id, createGuildEmoteParams, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static async Task<GuildEmote> ModifyEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, Action<EmoteProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		EmoteProperties emoteProperties = new EmoteProperties();
		func(emoteProperties);
		ModifyGuildEmoteParams modifyGuildEmoteParams = new ModifyGuildEmoteParams
		{
			Name = emoteProperties.Name
		};
		if (emoteProperties.Roles.IsSpecified)
		{
			modifyGuildEmoteParams.RoleIds = emoteProperties.Roles.Value?.Select((IRole xr) => xr.Id).ToArray();
		}
		return (await client.ApiClient.ModifyGuildEmoteAsync(guild.Id, id, modifyGuildEmoteParams, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static Task DeleteEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		return client.ApiClient.DeleteGuildEmoteAsync(guild.Id, id, options);
	}

	public static async Task<Discord.API.Sticker> CreateStickerAsync(BaseDiscordClient client, IGuild guild, string name, string description, IEnumerable<string> tags, Image image, RequestOptions options = null)
	{
		Preconditions.NotNull(name, "name");
		Preconditions.NotNull(description, "description");
		Preconditions.AtLeast(name.Length, 2, "name");
		Preconditions.AtLeast(description.Length, 2, "description");
		Preconditions.AtMost(name.Length, 30, "name");
		Preconditions.AtMost(description.Length, 100, "name");
		CreateStickerParams args = new CreateStickerParams
		{
			Name = name,
			Description = description,
			File = image.Stream,
			Tags = string.Join(", ", tags)
		};
		return await client.ApiClient.CreateGuildStickerAsync(args, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<Discord.API.Sticker> CreateStickerAsync(BaseDiscordClient client, IGuild guild, string name, string description, IEnumerable<string> tags, Stream file, string filename, RequestOptions options = null)
	{
		Preconditions.NotNull(name, "name");
		Preconditions.NotNull(description, "description");
		Preconditions.NotNull(file, "file");
		Preconditions.NotNull(filename, "filename");
		Preconditions.AtLeast(name.Length, 2, "name");
		Preconditions.AtLeast(description.Length, 2, "description");
		Preconditions.AtMost(name.Length, 30, "name");
		Preconditions.AtMost(description.Length, 100, "name");
		CreateStickerParams args = new CreateStickerParams
		{
			Name = name,
			Description = description,
			File = file,
			Tags = string.Join(", ", tags),
			FileName = filename
		};
		return await client.ApiClient.CreateGuildStickerAsync(args, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<Discord.API.Sticker> ModifyStickerAsync(BaseDiscordClient client, ulong guildId, ISticker sticker, Action<StickerProperties> func, RequestOptions options = null)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		StickerProperties stickerProperties = new StickerProperties();
		func(stickerProperties);
		ModifyStickerParams args = new ModifyStickerParams
		{
			Description = stickerProperties.Description,
			Name = stickerProperties.Name,
			Tags = (stickerProperties.Tags.IsSpecified ? ((Optional<string>)string.Join(", ", stickerProperties.Tags.Value)) : Optional<string>.Unspecified)
		};
		return await client.ApiClient.ModifyStickerAsync(args, guildId, sticker.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteStickerAsync(BaseDiscordClient client, ulong guildId, ISticker sticker, RequestOptions options = null)
	{
		await client.ApiClient.DeleteStickerAsync(guildId, sticker.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, int limit = 100, RequestOptions options = null)
	{
		return (await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, limit, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEventUser x) => RestUser.Create(client, guildEvent.Guild, x)).ToImmutableArray();
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, ulong? fromUserId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestUser>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetEventUsersParams getEventUsersParams = new GetEventUsersParams
			{
				Limit = info.PageSize,
				RelativeDirection = Direction.After
			};
			if (info.Position.HasValue)
			{
				getEventUsersParams.RelativeUserId = info.Position.Value;
			}
			return (await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, getEventUsersParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEventUser x) => RestUser.Create(client, guildEvent.Guild, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestUser x) => x.Id);
			return true;
		}, fromUserId, limit);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, ulong? fromUserId, Direction dir, int limit, RequestOptions options = null)
	{
		if (dir == Direction.Around && limit > 100)
		{
			int num = limit / 2;
			if (fromUserId.HasValue)
			{
				return GetEventUsersAsync(client, guildEvent, fromUserId.Value + 1, Direction.Before, num + 1, options).Concat(GetEventUsersAsync(client, guildEvent, fromUserId, Direction.After, num, options));
			}
			return GetEventUsersAsync(client, guildEvent, null, Direction.Before, num + 1, options);
		}
		return new PagedAsyncEnumerable<RestUser>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetEventUsersParams getEventUsersParams = new GetEventUsersParams
			{
				RelativeDirection = dir,
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getEventUsersParams.RelativeUserId = info.Position.Value;
			}
			GuildScheduledEventUser[] obj = await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, getEventUsersParams, options).ConfigureAwait(continueOnCapturedContext: false);
			System.Collections.Immutable.ImmutableArray<RestUser>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<RestUser>();
			GuildScheduledEventUser[] array = obj;
			foreach (GuildScheduledEventUser model in array)
			{
				builder.Add(RestUser.Create(client, guildEvent.Guild, model));
			}
			return builder.ToImmutable();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			if (dir == Direction.Before)
			{
				info.Position = lastPage.Min((RestUser x) => x.Id);
			}
			else
			{
				info.Position = lastPage.Max((RestUser x) => x.Id);
			}
			return true;
		}, fromUserId, limit);
	}

	public static async Task<GuildScheduledEvent> ModifyGuildEventAsync(BaseDiscordClient client, Action<GuildScheduledEventsProperties> func, IGuildScheduledEvent guildEvent, RequestOptions options = null)
	{
		GuildScheduledEventsProperties guildScheduledEventsProperties = new GuildScheduledEventsProperties();
		func(guildScheduledEventsProperties);
		if (guildScheduledEventsProperties.Status.IsSpecified)
		{
			switch (guildScheduledEventsProperties.Status.Value)
			{
			case GuildScheduledEventStatus.Active:
				if (guildEvent.Status == GuildScheduledEventStatus.Scheduled)
				{
					break;
				}
				goto IL_0089;
			case GuildScheduledEventStatus.Completed:
				if (guildEvent.Status == GuildScheduledEventStatus.Active)
				{
					break;
				}
				goto IL_0089;
			case GuildScheduledEventStatus.Cancelled:
				{
					if (guildEvent.Status == GuildScheduledEventStatus.Scheduled)
					{
						break;
					}
					goto IL_0089;
				}
				IL_0089:
				throw new ArgumentException($"Cannot set event to {guildScheduledEventsProperties.Status.Value} when events status is {guildEvent.Status}");
			}
		}
		if (guildScheduledEventsProperties.Type.IsSpecified && guildScheduledEventsProperties.Type.Value == GuildScheduledEventType.External)
		{
			if (!guildScheduledEventsProperties.Location.IsSpecified)
			{
				throw new ArgumentException("Location must be specified for external events.");
			}
			if (!guildScheduledEventsProperties.EndTime.IsSpecified)
			{
				throw new ArgumentException("End time must be specified for external events.");
			}
			if (!guildScheduledEventsProperties.ChannelId.IsSpecified)
			{
				throw new ArgumentException("Channel id must be set to null!");
			}
			if (guildScheduledEventsProperties.ChannelId.Value.HasValue)
			{
				throw new ArgumentException("Channel id must be set to null!");
			}
		}
		ModifyGuildScheduledEventParams modifyGuildScheduledEventParams = new ModifyGuildScheduledEventParams
		{
			ChannelId = guildScheduledEventsProperties.ChannelId,
			Description = guildScheduledEventsProperties.Description,
			EndTime = guildScheduledEventsProperties.EndTime,
			Name = guildScheduledEventsProperties.Name,
			PrivacyLevel = guildScheduledEventsProperties.PrivacyLevel,
			StartTime = guildScheduledEventsProperties.StartTime,
			Status = guildScheduledEventsProperties.Status,
			Type = guildScheduledEventsProperties.Type,
			Image = ((!guildScheduledEventsProperties.CoverImage.IsSpecified) ? Optional<Discord.API.Image?>.Unspecified : (guildScheduledEventsProperties.CoverImage.Value.HasValue ? ((Optional<Discord.API.Image?>)guildScheduledEventsProperties.CoverImage.Value.Value.ToModel()) : ((Optional<Discord.API.Image?>)null)))
		};
		if (guildScheduledEventsProperties.Location.IsSpecified)
		{
			modifyGuildScheduledEventParams.EntityMetadata = new GuildScheduledEventEntityMetadata
			{
				Location = guildScheduledEventsProperties.Location
			};
		}
		return await client.ApiClient.ModifyGuildScheduledEventAsync(modifyGuildScheduledEventParams, guildEvent.Id, guildEvent.Guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<RestGuildEvent> GetGuildEventAsync(BaseDiscordClient client, ulong id, IGuild guild, RequestOptions options = null)
	{
		GuildScheduledEvent guildScheduledEvent = await client.ApiClient.GetGuildScheduledEventAsync(id, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildScheduledEvent == null)
		{
			return null;
		}
		return RestGuildEvent.Create(client, guild, guildScheduledEvent);
	}

	public static async Task<IReadOnlyCollection<RestGuildEvent>> GetGuildEventsAsync(BaseDiscordClient client, IGuild guild, RequestOptions options = null)
	{
		return (await client.ApiClient.ListGuildScheduledEventsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEvent x) => RestGuildEvent.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<RestGuildEvent> CreateGuildEventAsync(BaseDiscordClient client, IGuild guild, string name, GuildScheduledEventPrivacyLevel privacyLevel, DateTimeOffset startTime, GuildScheduledEventType type, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? bannerImage = null, RequestOptions options = null)
	{
		if (location != null)
		{
			Preconditions.AtMost(location.Length, 100, "location");
		}
		switch (type)
		{
		case GuildScheduledEventType.Stage:
		case GuildScheduledEventType.Voice:
			if (!channelId.HasValue)
			{
				throw new ArgumentException(string.Format("{0} must not be null when type is {1}", "channelId", type), "channelId");
			}
			break;
		case GuildScheduledEventType.External:
			if (channelId.HasValue)
			{
				throw new ArgumentException("channelId must be null when using external event type", "channelId");
			}
			if (location == null)
			{
				throw new ArgumentException("location must not be null when using external event type", "location");
			}
			if (!endTime.HasValue)
			{
				throw new ArgumentException("endTime must not be null when using external event type", "endTime");
			}
			break;
		}
		if (startTime <= DateTimeOffset.Now)
		{
			throw new ArgumentOutOfRangeException("startTime", "The start time for an event cannot be in the past");
		}
		if (endTime.HasValue && endTime <= startTime)
		{
			throw new ArgumentOutOfRangeException("endTime", "endTime cannot be before the start time");
		}
		CreateGuildScheduledEventParams obj = new CreateGuildScheduledEventParams
		{
			ChannelId = (((Optional<ulong>?)channelId) ?? Optional<ulong>.Unspecified)
		};
		obj.Description = ((description != null) ? ((Optional<string>)description) : Optional<string>.Unspecified);
		obj.EndTime = ((Optional<DateTimeOffset>?)endTime) ?? Optional<DateTimeOffset>.Unspecified;
		obj.Name = name;
		obj.PrivacyLevel = privacyLevel;
		obj.StartTime = startTime;
		obj.Type = type;
		obj.Image = (bannerImage.HasValue ? ((Optional<Discord.API.Image>)bannerImage.Value.ToModel()) : Optional<Discord.API.Image>.Unspecified);
		CreateGuildScheduledEventParams createGuildScheduledEventParams = obj;
		if (location != null)
		{
			createGuildScheduledEventParams.EntityMetadata = new GuildScheduledEventEntityMetadata
			{
				Location = location
			};
		}
		GuildScheduledEvent model = await client.ApiClient.CreateGuildScheduledEventAsync(createGuildScheduledEventParams, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestGuildEvent.Create(client, guild, client.CurrentUser, model);
	}

	public static async Task DeleteEventAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, RequestOptions options = null)
	{
		await client.ApiClient.DeleteGuildScheduledEventAsync(guildEvent.Id, guildEvent.Guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
