using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestGuild : RestEntity<ulong>, IGuild, IDeletable, ISnowflakeEntity, IEntity<ulong>, IUpdateable
{
	private ImmutableDictionary<ulong, RestRole> _roles;

	private System.Collections.Immutable.ImmutableArray<GuildEmote> _emotes;

	private System.Collections.Immutable.ImmutableArray<CustomSticker> _stickers;

	public string Name { get; private set; }

	public int AFKTimeout { get; private set; }

	public bool IsWidgetEnabled { get; private set; }

	public VerificationLevel VerificationLevel { get; private set; }

	public MfaLevel MfaLevel { get; private set; }

	public DefaultMessageNotifications DefaultMessageNotifications { get; private set; }

	public ExplicitContentFilterLevel ExplicitContentFilter { get; private set; }

	public ulong? AFKChannelId { get; private set; }

	public ulong? WidgetChannelId { get; private set; }

	public ulong? SystemChannelId { get; private set; }

	public ulong? RulesChannelId { get; private set; }

	public ulong? PublicUpdatesChannelId { get; private set; }

	public ulong OwnerId { get; private set; }

	public string VoiceRegionId { get; private set; }

	public string IconId { get; private set; }

	public string SplashId { get; private set; }

	public string DiscoverySplashId { get; private set; }

	internal bool Available { get; private set; }

	public ulong? ApplicationId { get; private set; }

	public PremiumTier PremiumTier { get; private set; }

	public string BannerId { get; private set; }

	public string VanityURLCode { get; private set; }

	public SystemChannelMessageDeny SystemChannelFlags { get; private set; }

	public string Description { get; private set; }

	public int PremiumSubscriptionCount { get; private set; }

	public string PreferredLocale { get; private set; }

	public int? MaxPresences { get; private set; }

	public int? MaxMembers { get; private set; }

	public int? MaxVideoChannelUsers { get; private set; }

	public int? ApproximateMemberCount { get; private set; }

	public int? ApproximatePresenceCount { get; private set; }

	public int MaxBitrate => PremiumTier switch
	{
		PremiumTier.Tier1 => 128000, 
		PremiumTier.Tier2 => 256000, 
		PremiumTier.Tier3 => 384000, 
		_ => 96000, 
	};

	public ulong MaxUploadLimit => GuildHelper.GetUploadLimit(this);

	public NsfwLevel NsfwLevel { get; private set; }

	public bool IsBoostProgressBarEnabled { get; private set; }

	public CultureInfo PreferredCulture { get; private set; }

	public GuildFeatures Features { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetGuildIconUrl(base.Id, IconId);

	public string SplashUrl => CDN.GetGuildSplashUrl(base.Id, SplashId);

	public string DiscoverySplashUrl => CDN.GetGuildDiscoverySplashUrl(base.Id, DiscoverySplashId);

	public string BannerUrl => CDN.GetGuildBannerUrl(base.Id, BannerId, ImageFormat.Auto);

	public RestRole EveryoneRole => GetRole(base.Id);

	public IReadOnlyCollection<RestRole> Roles => _roles.ToReadOnlyCollection();

	public IReadOnlyCollection<GuildEmote> Emotes => _emotes;

	public IReadOnlyCollection<CustomSticker> Stickers => _stickers;

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	bool IGuild.Available => Available;

	IAudioClient IGuild.AudioClient => null;

	IRole IGuild.EveryoneRole => EveryoneRole;

	IReadOnlyCollection<IRole> IGuild.Roles => Roles;

	IReadOnlyCollection<ICustomSticker> IGuild.Stickers => Stickers;

	internal RestGuild(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal static RestGuild Create(BaseDiscordClient discord, Guild model)
	{
		RestGuild restGuild = new RestGuild(discord, model.Id);
		restGuild.Update(model);
		return restGuild;
	}

	internal void Update(Guild model)
	{
		AFKChannelId = model.AFKChannelId;
		if (model.WidgetChannelId.IsSpecified)
		{
			WidgetChannelId = model.WidgetChannelId.Value;
		}
		SystemChannelId = model.SystemChannelId;
		RulesChannelId = model.RulesChannelId;
		PublicUpdatesChannelId = model.PublicUpdatesChannelId;
		AFKTimeout = model.AFKTimeout;
		if (model.WidgetEnabled.IsSpecified)
		{
			IsWidgetEnabled = model.WidgetEnabled.Value;
		}
		IconId = model.Icon;
		Name = model.Name;
		OwnerId = model.OwnerId;
		VoiceRegionId = model.Region;
		SplashId = model.Splash;
		DiscoverySplashId = model.DiscoverySplash;
		VerificationLevel = model.VerificationLevel;
		MfaLevel = model.MfaLevel;
		DefaultMessageNotifications = model.DefaultMessageNotifications;
		ExplicitContentFilter = model.ExplicitContentFilter;
		ApplicationId = model.ApplicationId;
		PremiumTier = model.PremiumTier;
		VanityURLCode = model.VanityURLCode;
		BannerId = model.Banner;
		SystemChannelFlags = model.SystemChannelFlags;
		Description = model.Description;
		PremiumSubscriptionCount = model.PremiumSubscriptionCount.GetValueOrDefault();
		NsfwLevel = model.NsfwLevel;
		if (model.MaxPresences.IsSpecified)
		{
			MaxPresences = model.MaxPresences.Value ?? 25000;
		}
		if (model.MaxMembers.IsSpecified)
		{
			MaxMembers = model.MaxMembers.Value;
		}
		if (model.MaxVideoChannelUsers.IsSpecified)
		{
			MaxVideoChannelUsers = model.MaxVideoChannelUsers.Value;
		}
		PreferredLocale = model.PreferredLocale;
		PreferredCulture = new CultureInfo(PreferredLocale);
		if (model.ApproximateMemberCount.IsSpecified)
		{
			ApproximateMemberCount = model.ApproximateMemberCount.Value;
		}
		if (model.ApproximatePresenceCount.IsSpecified)
		{
			ApproximatePresenceCount = model.ApproximatePresenceCount.Value;
		}
		if (model.IsBoostProgressBarEnabled.IsSpecified)
		{
			IsBoostProgressBarEnabled = model.IsBoostProgressBarEnabled.Value;
		}
		if (model.Emojis != null)
		{
			System.Collections.Immutable.ImmutableArray<GuildEmote>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<GuildEmote>(model.Emojis.Length);
			for (int i = 0; i < model.Emojis.Length; i++)
			{
				builder.Add(model.Emojis[i].ToEntity());
			}
			_emotes = builder.ToImmutableArray();
		}
		else
		{
			_emotes = System.Collections.Immutable.ImmutableArray.Create<GuildEmote>();
		}
		Features = model.Features;
		ImmutableDictionary<ulong, RestRole>.Builder builder2 = ImmutableDictionary.CreateBuilder<ulong, RestRole>();
		if (model.Roles != null)
		{
			for (int j = 0; j < model.Roles.Length; j++)
			{
				builder2[model.Roles[j].Id] = RestRole.Create(base.Discord, this, model.Roles[j]);
			}
		}
		_roles = builder2.ToImmutable();
		if (model.Stickers != null)
		{
			System.Collections.Immutable.ImmutableArray<CustomSticker>.Builder builder3 = System.Collections.Immutable.ImmutableArray.CreateBuilder<CustomSticker>();
			for (int k = 0; k < model.Stickers.Length; k++)
			{
				global::Discord.API.Sticker sticker = model.Stickers[k];
				CustomSticker item = CustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
				builder3.Add(item);
			}
			_stickers = builder3.ToImmutable();
		}
		else
		{
			_stickers = System.Collections.Immutable.ImmutableArray.Create<CustomSticker>();
		}
		Available = true;
	}

	internal void Update(GuildWidget model)
	{
		WidgetChannelId = model.ChannelId;
		IsWidgetEnabled = model.Enabled;
	}

	public async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetGuildAsync(base.Id, withCounts: false, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task UpdateAsync(bool withCounts, RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetGuildAsync(base.Id, withCounts, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteAsync(this, base.Discord, options);
	}

	public async Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task ModifyWidgetAsync(Action<GuildWidgetProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyWidgetAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null)
	{
		ReorderChannelProperties[] args2 = args.ToArray();
		await GuildHelper.ReorderChannelsAsync(this, base.Discord, args2, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null)
	{
		foreach (Role item in await GuildHelper.ReorderRolesAsync(this, base.Discord, args, options).ConfigureAwait(continueOnCapturedContext: false))
		{
			GetRole(item.Id)?.Update(item);
		}
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return GuildHelper.LeaveAsync(this, base.Discord, options);
	}

	public Task DeleteSlashCommandsAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteAllGuildCommandsAsync(base.Discord, base.Id, options);
	}

	public Task<IReadOnlyCollection<RestGuildCommand>> GetSlashCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		return GuildHelper.GetSlashCommandsAsync(this, base.Discord, withLocalizations, locale, options);
	}

	public Task<RestGuildCommand> GetSlashCommandAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetSlashCommandAsync(this, id, base.Discord, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, null, Direction.Before, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(ulong fromUserId, Direction dir, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, fromUserId, dir, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(IUser fromUser, Direction dir, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, fromUser.Id, dir, limit, options);
	}

	public Task<RestBan> GetBanAsync(IUser user, RequestOptions options = null)
	{
		return GuildHelper.GetBanAsync(this, base.Discord, user.Id, options);
	}

	public Task<RestBan> GetBanAsync(ulong userId, RequestOptions options = null)
	{
		return GuildHelper.GetBanAsync(this, base.Discord, userId, options);
	}

	public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, user.Id, pruneDays, reason, options);
	}

	public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, userId, pruneDays, reason, options);
	}

	public Task RemoveBanAsync(IUser user, RequestOptions options = null)
	{
		return GuildHelper.RemoveBanAsync(this, base.Discord, user.Id, options);
	}

	public Task RemoveBanAsync(ulong userId, RequestOptions options = null)
	{
		return GuildHelper.RemoveBanAsync(this, base.Discord, userId, options);
	}

	public Task<IReadOnlyCollection<RestGuildChannel>> GetChannelsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetChannelsAsync(this, base.Discord, options);
	}

	public Task<RestGuildChannel> GetChannelAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetChannelAsync(this, base.Discord, id, options);
	}

	public async Task<RestTextChannel> GetTextChannelAsync(ulong id, RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false)) as RestTextChannel;
	}

	public async Task<IReadOnlyCollection<RestTextChannel>> GetTextChannelsAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelsAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).OfType<RestTextChannel>().ToImmutableArray();
	}

	public async Task<RestThreadChannel> GetThreadChannelAsync(ulong id, RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false)) as RestThreadChannel;
	}

	public async Task<IReadOnlyCollection<RestThreadChannel>> GetThreadChannelsAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelsAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).OfType<RestThreadChannel>().ToImmutableArray();
	}

	public async Task<RestVoiceChannel> GetVoiceChannelAsync(ulong id, RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false)) as RestVoiceChannel;
	}

	public async Task<IReadOnlyCollection<RestVoiceChannel>> GetVoiceChannelsAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelsAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).OfType<RestVoiceChannel>().ToImmutableArray();
	}

	public async Task<RestStageChannel> GetStageChannelAsync(ulong id, RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false)) as RestStageChannel;
	}

	public async Task<IReadOnlyCollection<RestStageChannel>> GetStageChannelsAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelsAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).OfType<RestStageChannel>().ToImmutableArray();
	}

	public async Task<IReadOnlyCollection<RestCategoryChannel>> GetCategoryChannelsAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetChannelsAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).OfType<RestCategoryChannel>().ToImmutableArray();
	}

	public async Task<RestVoiceChannel> GetAFKChannelAsync(RequestOptions options = null)
	{
		ulong? aFKChannelId = AFKChannelId;
		if (aFKChannelId.HasValue)
		{
			return (await GuildHelper.GetChannelAsync(this, base.Discord, aFKChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false)) as RestVoiceChannel;
		}
		return null;
	}

	public async Task<RestTextChannel> GetDefaultChannelAsync(RequestOptions options = null)
	{
		IReadOnlyCollection<RestTextChannel> channels = await GetTextChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		RestGuildUser user = await GetCurrentUserAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		return (from c in channels
			where user.GetPermissions(c).ViewChannel
			orderby c.Position
			select c).FirstOrDefault();
	}

	public async Task<RestGuildChannel> GetWidgetChannelAsync(RequestOptions options = null)
	{
		ulong? widgetChannelId = WidgetChannelId;
		if (widgetChannelId.HasValue)
		{
			return await GuildHelper.GetChannelAsync(this, base.Discord, widgetChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	public async Task<RestTextChannel> GetSystemChannelAsync(RequestOptions options = null)
	{
		ulong? systemChannelId = SystemChannelId;
		if (systemChannelId.HasValue)
		{
			return (await GuildHelper.GetChannelAsync(this, base.Discord, systemChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false)) as RestTextChannel;
		}
		return null;
	}

	public async Task<RestTextChannel> GetRulesChannelAsync(RequestOptions options = null)
	{
		ulong? rulesChannelId = RulesChannelId;
		if (rulesChannelId.HasValue)
		{
			return (await GuildHelper.GetChannelAsync(this, base.Discord, rulesChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false)) as RestTextChannel;
		}
		return null;
	}

	public async Task<RestTextChannel> GetPublicUpdatesChannelAsync(RequestOptions options = null)
	{
		ulong? publicUpdatesChannelId = PublicUpdatesChannelId;
		if (publicUpdatesChannelId.HasValue)
		{
			return (await GuildHelper.GetChannelAsync(this, base.Discord, publicUpdatesChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false)) as RestTextChannel;
		}
		return null;
	}

	public Task<RestTextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateTextChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateVoiceChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestStageChannel> CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateStageChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestCategoryChannel> CreateCategoryChannelAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateCategoryChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetVoiceRegionsAsync(this, base.Discord, options);
	}

	public Task<IReadOnlyCollection<RestIntegration>> GetIntegrationsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetIntegrationsAsync(this, base.Discord, options);
	}

	public Task DeleteIntegrationAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.DeleteIntegrationAsync(this, base.Discord, id, options);
	}

	public Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		return GuildHelper.GetInvitesAsync(this, base.Discord, options);
	}

	public Task<RestInviteMetadata> GetVanityInviteAsync(RequestOptions options = null)
	{
		return GuildHelper.GetVanityInviteAsync(this, base.Discord, options);
	}

	public RestRole GetRole(ulong id)
	{
		if (_roles.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public async Task<RestRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, bool isMentionable = false, RequestOptions options = null)
	{
		RestRole restRole = await GuildHelper.CreateRoleAsync(this, base.Discord, name, permissions, color, isHoisted, isMentionable, options).ConfigureAwait(continueOnCapturedContext: false);
		_roles = _roles.Add(restRole.Id, restRole);
		return restRole;
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(RequestOptions options = null)
	{
		return GuildHelper.GetUsersAsync(this, base.Discord, null, null, options);
	}

	public Task<RestGuildUser> AddGuildUserAsync(ulong id, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.AddGuildUserAsync(this, base.Discord, id, accessToken, func, options);
	}

	public Task<RestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetUserAsync(this, base.Discord, id, options);
	}

	public Task<RestGuildUser> GetCurrentUserAsync(RequestOptions options = null)
	{
		return GuildHelper.GetUserAsync(this, base.Discord, base.Discord.CurrentUser.Id, options);
	}

	public Task<RestGuildUser> GetOwnerAsync(RequestOptions options = null)
	{
		return GuildHelper.GetUserAsync(this, base.Discord, OwnerId, options);
	}

	public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null, IEnumerable<ulong> includeRoleIds = null)
	{
		return GuildHelper.PruneUsersAsync(this, base.Discord, days, simulate, options, includeRoleIds);
	}

	public Task<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(string query, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.SearchUsersAsync(this, base.Discord, query, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> GetAuditLogsAsync(int limit, RequestOptions options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null)
	{
		return GuildHelper.GetAuditLogsAsync(this, base.Discord, beforeId, limit, options, userId, actionType);
	}

	public Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetWebhookAsync(this, base.Discord, id, options);
	}

	public Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		return GuildHelper.GetWebhooksAsync(this, base.Discord, options);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> GetApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		return await ClientHelper.GetGuildApplicationCommandsAsync(base.Discord, base.Id, withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestGuildCommand> GetApplicationCommandAsync(ulong id, RequestOptions options = null)
	{
		return await ClientHelper.GetGuildApplicationCommandAsync(base.Discord, id, base.Id, options);
	}

	public async Task<RestGuildCommand> CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null)
	{
		ApplicationCommand model = await InteractionHelper.CreateGuildCommandAsync(base.Discord, base.Id, properties, options);
		return RestGuildCommand.Create(base.Discord, model, base.Id);
	}

	public async Task<IReadOnlyCollection<RestGuildCommand>> BulkOverwriteApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		return (await InteractionHelper.BulkOverwriteGuildCommandsAsync(base.Discord, base.Id, properties, options)).Select((ApplicationCommand x) => RestGuildCommand.Create(base.Discord, x, base.Id)).ToImmutableArray();
	}

	public override string ToString()
	{
		return Name;
	}

	public Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(RequestOptions options = null)
	{
		return GuildHelper.GetEmotesAsync(this, base.Discord, options);
	}

	public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetEmoteAsync(this, base.Discord, id, options);
	}

	public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default(Optional<IEnumerable<IRole>>), RequestOptions options = null)
	{
		return GuildHelper.CreateEmoteAsync(this, base.Discord, name, image, roles, options);
	}

	public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null)
	{
		return GuildHelper.ModifyEmoteAsync(this, base.Discord, emote.Id, func, options);
	}

	public Task MoveAsync(IGuildUser user, IVoiceChannel targetChannel)
	{
		return user.ModifyAsync(delegate(GuildUserProperties x)
		{
			x.Channel = new Optional<IVoiceChannel>(targetChannel);
		});
	}

	public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
	{
		return GuildHelper.DeleteEmoteAsync(this, base.Discord, emote.Id, options);
	}

	public async Task<CustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Image image, RequestOptions options = null)
	{
		global::Discord.API.Sticker sticker = await GuildHelper.CreateStickerAsync(base.Discord, this, name, description, tags, image, options).ConfigureAwait(continueOnCapturedContext: false);
		return CustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
	}

	public Task<CustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, string path, RequestOptions options = null)
	{
		FileStream fileStream = File.OpenRead(path);
		return CreateStickerAsync(name, description, tags, fileStream, Path.GetFileName(fileStream.Name), options);
	}

	public async Task<CustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Stream stream, string filename, RequestOptions options = null)
	{
		global::Discord.API.Sticker sticker = await GuildHelper.CreateStickerAsync(base.Discord, this, name, description, tags, stream, filename, options).ConfigureAwait(continueOnCapturedContext: false);
		return CustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
	}

	public async Task<CustomSticker> GetStickerAsync(ulong id, RequestOptions options = null)
	{
		global::Discord.API.Sticker sticker = await base.Discord.ApiClient.GetGuildStickerAsync(base.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (sticker == null)
		{
			return null;
		}
		return CustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
	}

	public async Task<IReadOnlyCollection<CustomSticker>> GetStickersAsync(RequestOptions options = null)
	{
		global::Discord.API.Sticker[] array = await base.Discord.ApiClient.ListGuildStickersAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (array.Length == 0)
		{
			return null;
		}
		List<CustomSticker> list = new List<CustomSticker>();
		global::Discord.API.Sticker[] array2 = array;
		foreach (global::Discord.API.Sticker sticker in array2)
		{
			CustomSticker item = CustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
			list.Add(item);
		}
		return list.ToImmutableArray();
	}

	public Task DeleteStickerAsync(CustomSticker sticker, RequestOptions options = null)
	{
		return sticker.DeleteAsync(options);
	}

	public Task<RestGuildEvent> GetEventAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetGuildEventAsync(base.Discord, id, this, options);
	}

	public Task<IReadOnlyCollection<RestGuildEvent>> GetEventsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetGuildEventsAsync(base.Discord, this, options);
	}

	public Task<RestGuildEvent> CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel = GuildScheduledEventPrivacyLevel.Private, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? coverImage = null, RequestOptions options = null)
	{
		return GuildHelper.CreateGuildEventAsync(base.Discord, this, name, privacyLevel, startTime, type, description, endTime, channelId, location, coverImage, options);
	}

	async Task<IGuildScheduledEvent> IGuild.CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel, string description, DateTimeOffset? endTime, ulong? channelId, string location, Image? coverImage, RequestOptions options)
	{
		return await CreateEventAsync(name, startTime, type, privacyLevel, description, endTime, channelId, location, coverImage, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuildScheduledEvent> IGuild.GetEventAsync(ulong id, RequestOptions options)
	{
		return await GetEventAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IGuildScheduledEvent>> IGuild.GetEventsAsync(RequestOptions options)
	{
		return await GetEventsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(int limit, RequestOptions options)
	{
		return GetBansAsync(limit, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(ulong fromUserId, Direction dir, int limit, RequestOptions options)
	{
		return GetBansAsync(fromUserId, dir, limit, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(IUser fromUser, Direction dir, int limit, RequestOptions options)
	{
		return GetBansAsync(fromUser, dir, limit, options);
	}

	async Task<IBan> IGuild.GetBanAsync(IUser user, RequestOptions options)
	{
		return await GetBanAsync(user, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IBan> IGuild.GetBanAsync(ulong userId, RequestOptions options)
	{
		return await GetBanAsync(userId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IGuildChannel>> IGuild.GetChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IGuildChannel>();
	}

	async Task<IGuildChannel> IGuild.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<ITextChannel>> IGuild.GetTextChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetTextChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<ITextChannel>();
	}

	async Task<ITextChannel> IGuild.GetTextChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetTextChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IThreadChannel> IGuild.GetThreadChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetThreadChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IGuild.GetThreadChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetThreadChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IVoiceChannel>> IGuild.GetVoiceChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetVoiceChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IVoiceChannel>();
	}

	async Task<IReadOnlyCollection<ICategoryChannel>> IGuild.GetCategoriesAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetCategoryChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IStageChannel> IGuild.GetStageChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetStageChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IStageChannel>> IGuild.GetStageChannelsAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetStageChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IVoiceChannel> IGuild.GetVoiceChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetVoiceChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IVoiceChannel> IGuild.GetAFKChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetAFKChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<ITextChannel> IGuild.GetDefaultChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetDefaultChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IGuildChannel> IGuild.GetWidgetChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetWidgetChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<ITextChannel> IGuild.GetSystemChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetSystemChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<ITextChannel> IGuild.GetRulesChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetRulesChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<ITextChannel> IGuild.GetPublicUpdatesChannelAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetPublicUpdatesChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<ITextChannel> IGuild.CreateTextChannelAsync(string name, Action<TextChannelProperties> func, RequestOptions options)
	{
		return await CreateTextChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceChannel> IGuild.CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
	{
		return await CreateVoiceChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IStageChannel> IGuild.CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
	{
		return await CreateStageChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<ICategoryChannel> IGuild.CreateCategoryAsync(string name, Action<GuildChannelProperties> func, RequestOptions options)
	{
		return await CreateCategoryChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IGuild.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IIntegration>> IGuild.GetIntegrationsAsync(RequestOptions options)
	{
		return await GetIntegrationsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IGuild.DeleteIntegrationAsync(ulong id, RequestOptions options)
	{
		await DeleteIntegrationAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IInviteMetadata>> IGuild.GetInvitesAsync(RequestOptions options)
	{
		return await GetInvitesAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInviteMetadata> IGuild.GetVanityInviteAsync(RequestOptions options)
	{
		return await GetVanityInviteAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IRole IGuild.GetRole(ulong id)
	{
		return GetRole(id);
	}

	async Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted, RequestOptions options)
	{
		return await CreateRoleAsync(name, permissions, color, isHoisted, isMentionable: false, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted, bool isMentionable, RequestOptions options)
	{
		return await CreateRoleAsync(name, permissions, color, isHoisted, isMentionable, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuildUser> IGuild.AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		return await AddGuildUserAsync(userId, accessToken, func, options);
	}

	async Task IGuild.DisconnectAsync(IGuildUser user)
	{
		await user.ModifyAsync(delegate(GuildUserProperties x)
		{
			x.Channel = null;
		});
	}

	async Task<IGuildUser> IGuild.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IGuildUser> IGuild.GetCurrentUserAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetCurrentUserAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IGuildUser> IGuild.GetOwnerAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetOwnerAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	async Task<IReadOnlyCollection<IGuildUser>> IGuild.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return (await GetUsersAsync(options).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
		}
		return System.Collections.Immutable.ImmutableArray.Create<IGuildUser>();
	}

	Task IGuild.DownloadUsersAsync()
	{
		throw new NotSupportedException();
	}

	async Task<IReadOnlyCollection<IGuildUser>> IGuild.SearchUsersAsync(string query, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await SearchUsersAsync(query, limit, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return System.Collections.Immutable.ImmutableArray.Create<IGuildUser>();
	}

	async Task<IReadOnlyCollection<IAuditLogEntry>> IGuild.GetAuditLogsAsync(int limit, CacheMode cacheMode, RequestOptions options, ulong? beforeId, ulong? userId, ActionType? actionType)
	{
		if (cacheMode == CacheMode.AllowDownload)
		{
			return (await GetAuditLogsAsync(limit, options, beforeId, userId, actionType).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
		}
		return System.Collections.Immutable.ImmutableArray.Create<IAuditLogEntry>();
	}

	async Task<IWebhook> IGuild.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return await GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IWebhook>> IGuild.GetWebhooksAsync(RequestOptions options)
	{
		return await GetWebhooksAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IGuild.GetApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return await GetApplicationCommandsAsync(withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, string description, IEnumerable<string> tags, Image image, RequestOptions options)
	{
		return await CreateStickerAsync(name, description, tags, image, options);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, string description, IEnumerable<string> tags, Stream stream, string filename, RequestOptions options)
	{
		return await CreateStickerAsync(name, description, tags, stream, filename, options);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, string description, IEnumerable<string> tags, string path, RequestOptions options)
	{
		return await CreateStickerAsync(name, description, tags, path, options);
	}

	async Task<ICustomSticker> IGuild.GetStickerAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return null;
		}
		return await GetStickerAsync(id, options);
	}

	async Task<IReadOnlyCollection<ICustomSticker>> IGuild.GetStickersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return null;
		}
		return await GetStickersAsync(options);
	}

	Task IGuild.DeleteStickerAsync(ICustomSticker sticker, RequestOptions options)
	{
		return sticker.DeleteAsync();
	}

	async Task<IApplicationCommand> IGuild.CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options)
	{
		return await CreateApplicationCommandAsync(properties, options);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IGuild.BulkOverwriteApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options)
	{
		return await BulkOverwriteApplicationCommandsAsync(properties, options);
	}

	async Task<IApplicationCommand> IGuild.GetApplicationCommandAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetApplicationCommandAsync(id, options);
		}
		return null;
	}
}
