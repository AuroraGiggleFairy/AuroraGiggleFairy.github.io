using System;

namespace Discord;

internal static class CDN
{
	public static string GetTeamIconUrl(ulong teamId, string iconId)
	{
		if (iconId == null)
		{
			return null;
		}
		return string.Format("{0}team-icons/{1}/{2}.jpg", "https://cdn.discordapp.com/", teamId, iconId);
	}

	public static string GetApplicationIconUrl(ulong appId, string iconId)
	{
		if (iconId == null)
		{
			return null;
		}
		return string.Format("{0}app-icons/{1}/{2}.jpg", "https://cdn.discordapp.com/", appId, iconId);
	}

	public static string GetUserAvatarUrl(ulong userId, string avatarId, ushort size, ImageFormat format)
	{
		if (avatarId == null)
		{
			return null;
		}
		string text = FormatToExtension(format, avatarId);
		return string.Format("{0}avatars/{1}/{2}.{3}?size={4}", "https://cdn.discordapp.com/", userId, avatarId, text, size);
	}

	public static string GetGuildUserAvatarUrl(ulong userId, ulong guildId, string avatarId, ushort size, ImageFormat format)
	{
		if (avatarId == null)
		{
			return null;
		}
		string text = FormatToExtension(format, avatarId);
		return string.Format("{0}guilds/{1}/users/{2}/avatars/{3}.{4}?size={5}", "https://cdn.discordapp.com/", guildId, userId, avatarId, text, size);
	}

	public static string GetUserBannerUrl(ulong userId, string bannerId, ushort size, ImageFormat format)
	{
		if (bannerId == null)
		{
			return null;
		}
		string text = FormatToExtension(format, bannerId);
		return string.Format("{0}banners/{1}/{2}.{3}?size={4}", "https://cdn.discordapp.com/", userId, bannerId, text, size);
	}

	public static string GetDefaultUserAvatarUrl(ushort discriminator)
	{
		return string.Format("{0}embed/avatars/{1}.png", "https://cdn.discordapp.com/", discriminator % 5);
	}

	public static string GetGuildIconUrl(ulong guildId, string iconId)
	{
		if (iconId == null)
		{
			return null;
		}
		return string.Format("{0}icons/{1}/{2}.jpg", "https://cdn.discordapp.com/", guildId, iconId);
	}

	public static string GetGuildRoleIconUrl(ulong roleId, string roleHash)
	{
		if (roleHash == null)
		{
			return null;
		}
		return string.Format("{0}role-icons/{1}/{2}.png", "https://cdn.discordapp.com/", roleId, roleHash);
	}

	public static string GetGuildSplashUrl(ulong guildId, string splashId)
	{
		if (splashId == null)
		{
			return null;
		}
		return string.Format("{0}splashes/{1}/{2}.jpg", "https://cdn.discordapp.com/", guildId, splashId);
	}

	public static string GetGuildDiscoverySplashUrl(ulong guildId, string discoverySplashId)
	{
		if (discoverySplashId == null)
		{
			return null;
		}
		return string.Format("{0}discovery-splashes/{1}/{2}.jpg", "https://cdn.discordapp.com/", guildId, discoverySplashId);
	}

	public static string GetChannelIconUrl(ulong channelId, string iconId)
	{
		if (iconId == null)
		{
			return null;
		}
		return string.Format("{0}channel-icons/{1}/{2}.jpg", "https://cdn.discordapp.com/", channelId, iconId);
	}

	public static string GetGuildBannerUrl(ulong guildId, string bannerId, ImageFormat format, ushort? size = null)
	{
		if (string.IsNullOrEmpty(bannerId))
		{
			return null;
		}
		string text = FormatToExtension(format, bannerId);
		return string.Format("{0}banners/{1}/{2}.{3}", "https://cdn.discordapp.com/", guildId, bannerId, text) + (size.HasValue ? $"?size={size}" : string.Empty);
	}

	public static string GetEmojiUrl(ulong emojiId, bool animated)
	{
		return string.Format("{0}emojis/{1}.{2}", "https://cdn.discordapp.com/", emojiId, animated ? "gif" : "png");
	}

	public static string GetRichAssetUrl(ulong appId, string assetId, ushort size, ImageFormat format)
	{
		string text = FormatToExtension(format, "");
		return string.Format("{0}app-assets/{1}/{2}.{3}?size={4}", "https://cdn.discordapp.com/", appId, assetId, text, size);
	}

	public static string GetSpotifyAlbumArtUrl(string albumArtId)
	{
		return "https://i.scdn.co/image/" + albumArtId;
	}

	public static string GetSpotifyDirectUrl(string trackId)
	{
		return "https://open.spotify.com/track/" + trackId;
	}

	public static string GetStickerUrl(ulong stickerId, StickerFormatType format = StickerFormatType.Png)
	{
		return string.Format("{0}stickers/{1}.{2}", "https://cdn.discordapp.com/", stickerId, FormatToExtension(format));
	}

	public static string GetEventCoverImageUrl(ulong guildId, ulong eventId, string assetId, ImageFormat format = ImageFormat.Auto, ushort size = 1024)
	{
		return string.Format("{0}guild-events/{1}/{2}/{3}.{4}?size={5}", "https://cdn.discordapp.com/", guildId, eventId, assetId, FormatToExtension(format, assetId), size);
	}

	private static string FormatToExtension(StickerFormatType format)
	{
		switch (format)
		{
		case StickerFormatType.None:
		case StickerFormatType.Png:
		case StickerFormatType.Apng:
			return "png";
		case StickerFormatType.Lottie:
			return "lottie";
		default:
			throw new ArgumentException("format");
		}
	}

	private static string FormatToExtension(ImageFormat format, string imageId)
	{
		if (format == ImageFormat.Auto)
		{
			format = (imageId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png);
		}
		return format switch
		{
			ImageFormat.Gif => "gif", 
			ImageFormat.Jpeg => "jpeg", 
			ImageFormat.Png => "png", 
			ImageFormat.WebP => "webp", 
			_ => throw new ArgumentException("format"), 
		};
	}
}
