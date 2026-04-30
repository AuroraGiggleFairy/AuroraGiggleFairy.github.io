using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord.Audio;

namespace Discord;

internal interface IGuild : IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	int AFKTimeout { get; }

	bool IsWidgetEnabled { get; }

	DefaultMessageNotifications DefaultMessageNotifications { get; }

	MfaLevel MfaLevel { get; }

	VerificationLevel VerificationLevel { get; }

	ExplicitContentFilterLevel ExplicitContentFilter { get; }

	string IconId { get; }

	string IconUrl { get; }

	string SplashId { get; }

	string SplashUrl { get; }

	string DiscoverySplashId { get; }

	string DiscoverySplashUrl { get; }

	bool Available { get; }

	ulong? AFKChannelId { get; }

	ulong? WidgetChannelId { get; }

	ulong? SystemChannelId { get; }

	ulong? RulesChannelId { get; }

	ulong? PublicUpdatesChannelId { get; }

	ulong OwnerId { get; }

	ulong? ApplicationId { get; }

	string VoiceRegionId { get; }

	IAudioClient AudioClient { get; }

	IRole EveryoneRole { get; }

	IReadOnlyCollection<GuildEmote> Emotes { get; }

	IReadOnlyCollection<ICustomSticker> Stickers { get; }

	GuildFeatures Features { get; }

	IReadOnlyCollection<IRole> Roles { get; }

	PremiumTier PremiumTier { get; }

	string BannerId { get; }

	string BannerUrl { get; }

	string VanityURLCode { get; }

	SystemChannelMessageDeny SystemChannelFlags { get; }

	string Description { get; }

	int PremiumSubscriptionCount { get; }

	int? MaxPresences { get; }

	int? MaxMembers { get; }

	int? MaxVideoChannelUsers { get; }

	int? ApproximateMemberCount { get; }

	int? ApproximatePresenceCount { get; }

	int MaxBitrate { get; }

	string PreferredLocale { get; }

	NsfwLevel NsfwLevel { get; }

	CultureInfo PreferredCulture { get; }

	bool IsBoostProgressBarEnabled { get; }

	ulong MaxUploadLimit { get; }

	Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null);

	Task ModifyWidgetAsync(Action<GuildWidgetProperties> func, RequestOptions options = null);

	Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null);

	Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null);

	Task LeaveAsync(RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(int limit = 1000, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(ulong fromUserId, Direction dir, int limit = 1000, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(IUser fromUser, Direction dir, int limit = 1000, RequestOptions options = null);

	Task<IBan> GetBanAsync(IUser user, RequestOptions options = null);

	Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null);

	Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null);

	Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null);

	Task RemoveBanAsync(IUser user, RequestOptions options = null);

	Task RemoveBanAsync(ulong userId, RequestOptions options = null);

	Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IStageChannel> GetStageChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IStageChannel>> GetStageChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuildChannel> GetWidgetChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> GetRulesChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> GetPublicUpdatesChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IThreadChannel> GetThreadChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IThreadChannel>> GetThreadChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null);

	Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null);

	Task<IStageChannel> CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null);

	Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null);

	Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null);

	Task<IReadOnlyCollection<IIntegration>> GetIntegrationsAsync(RequestOptions options = null);

	Task DeleteIntegrationAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null);

	Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions options = null);

	IRole GetRole(ulong id);

	Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions options = null);

	Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, bool isMentionable = false, RequestOptions options = null);

	Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null);

	Task DisconnectAsync(IGuildUser user);

	Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task DownloadUsersAsync();

	Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null, IEnumerable<ulong> includeRoleIds = null);

	Task<IReadOnlyCollection<IGuildUser>> SearchUsersAsync(string query, int limit = 1000, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null);

	Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null);

	Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(RequestOptions options = null);

	Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null);

	Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default(Optional<IEnumerable<IRole>>), RequestOptions options = null);

	Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null);

	Task MoveAsync(IGuildUser user, IVoiceChannel targetChannel);

	Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null);

	Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Image image, RequestOptions options = null);

	Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, string path, RequestOptions options = null);

	Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Stream stream, string filename, RequestOptions options = null);

	Task<ICustomSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<ICustomSticker>> GetStickersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task DeleteStickerAsync(ICustomSticker sticker, RequestOptions options = null);

	Task<IGuildScheduledEvent> GetEventAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IGuildScheduledEvent>> GetEventsAsync(RequestOptions options = null);

	Task<IGuildScheduledEvent> CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel = GuildScheduledEventPrivacyLevel.Private, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? coverImage = null, RequestOptions options = null);

	Task<IReadOnlyCollection<IApplicationCommand>> GetApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null);

	Task<IApplicationCommand> GetApplicationCommandAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IApplicationCommand> CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null);

	Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options = null);
}
