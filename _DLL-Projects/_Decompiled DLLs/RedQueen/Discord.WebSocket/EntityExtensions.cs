using System;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal static class EntityExtensions
{
	public static IActivity ToEntity(this Discord.API.Game model)
	{
		if (model.Id.IsSpecified && model.Id.Value == "custom")
		{
			return new CustomStatusGame
			{
				Type = ActivityType.CustomStatus,
				Name = model.Name,
				State = (model.State.IsSpecified ? model.State.Value : null),
				Emote = (model.Emoji.IsSpecified ? model.Emoji.Value.ToIEmote() : null),
				CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(model.CreatedAt.Value)
			};
		}
		if (model.SyncId.IsSpecified)
		{
			GameAsset[] obj = model.Assets.GetValueOrDefault()?.ToEntity();
			string albumTitle = ((obj == null) ? null : obj[1]?.Text);
			string text = ((obj == null) ? null : obj[1]?.ImageId?.Replace("spotify:", ""));
			GameTimestamps gameTimestamps = (model.Timestamps.IsSpecified ? model.Timestamps.Value.ToEntity() : null);
			SpotifyGame spotifyGame = new SpotifyGame();
			spotifyGame.Name = model.Name;
			spotifyGame.SessionId = model.SessionId.GetValueOrDefault();
			spotifyGame.TrackId = model.SyncId.Value;
			spotifyGame.TrackUrl = CDN.GetSpotifyDirectUrl(model.SyncId.Value);
			spotifyGame.AlbumTitle = albumTitle;
			spotifyGame.TrackTitle = model.Details.GetValueOrDefault();
			spotifyGame.Artists = (from x in model.State.GetValueOrDefault()?.Split(';')
				select x?.Trim()).ToImmutableArray();
			spotifyGame.StartedAt = gameTimestamps?.Start;
			spotifyGame.EndsAt = gameTimestamps?.End;
			spotifyGame.Duration = gameTimestamps?.End - gameTimestamps?.Start;
			spotifyGame.AlbumArtUrl = ((text != null) ? CDN.GetSpotifyAlbumArtUrl(text) : null);
			spotifyGame.Type = ActivityType.Listening;
			spotifyGame.Flags = model.Flags.GetValueOrDefault();
			return spotifyGame;
		}
		if (model.ApplicationId.IsSpecified)
		{
			ulong value = model.ApplicationId.Value;
			GameAsset[] array = model.Assets.GetValueOrDefault()?.ToEntity(value);
			return new RichGame
			{
				ApplicationId = value,
				Name = model.Name,
				Details = model.Details.GetValueOrDefault(),
				State = model.State.GetValueOrDefault(),
				SmallAsset = ((array != null) ? array[0] : null),
				LargeAsset = ((array != null) ? array[1] : null),
				Party = (model.Party.IsSpecified ? model.Party.Value.ToEntity() : null),
				Secrets = (model.Secrets.IsSpecified ? model.Secrets.Value.ToEntity() : null),
				Timestamps = (model.Timestamps.IsSpecified ? model.Timestamps.Value.ToEntity() : null),
				Flags = model.Flags.GetValueOrDefault()
			};
		}
		if (model.StreamUrl.IsSpecified)
		{
			return new StreamingGame(model.Name, model.StreamUrl.Value)
			{
				Flags = model.Flags.GetValueOrDefault(),
				Details = model.Details.GetValueOrDefault()
			};
		}
		return new Game(model.Name, model.Type.GetValueOrDefault().GetValueOrDefault(), model.Flags.IsSpecified ? model.Flags.Value : ActivityProperties.None, model.Details.GetValueOrDefault());
	}

	public static GameAsset[] ToEntity(this GameAssets model, ulong? appId = null)
	{
		return new GameAsset[2]
		{
			model.SmallImage.IsSpecified ? new GameAsset
			{
				ApplicationId = appId,
				ImageId = model.SmallImage.GetValueOrDefault(),
				Text = model.SmallText.GetValueOrDefault()
			} : null,
			model.LargeImage.IsSpecified ? new GameAsset
			{
				ApplicationId = appId,
				ImageId = model.LargeImage.GetValueOrDefault(),
				Text = model.LargeText.GetValueOrDefault()
			} : null
		};
	}

	public static GameParty ToEntity(this Discord.API.GameParty model)
	{
		long members = 0L;
		long capacity = 0L;
		long[] size = model.Size;
		if (size != null && size.Length == 2)
		{
			members = model.Size[0];
			capacity = model.Size[1];
		}
		return new GameParty
		{
			Id = model.Id,
			Members = members,
			Capacity = capacity
		};
	}

	public static GameSecrets ToEntity(this Discord.API.GameSecrets model)
	{
		return new GameSecrets(model.Match, model.Join, model.Spectate);
	}

	public static GameTimestamps ToEntity(this Discord.API.GameTimestamps model)
	{
		return new GameTimestamps(model.Start.ToNullable(), model.End.ToNullable());
	}
}
