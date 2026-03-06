using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord;

internal static class EmbedBuilderExtensions
{
	public static EmbedBuilder WithColor(this EmbedBuilder builder, uint rawValue)
	{
		return builder.WithColor(new Color(rawValue));
	}

	public static EmbedBuilder WithColor(this EmbedBuilder builder, byte r, byte g, byte b)
	{
		return builder.WithColor(new Color(r, g, b));
	}

	public static EmbedBuilder WithColor(this EmbedBuilder builder, int r, int g, int b)
	{
		return builder.WithColor(new Color(r, g, b));
	}

	public static EmbedBuilder WithColor(this EmbedBuilder builder, float r, float g, float b)
	{
		return builder.WithColor(new Color(r, g, b));
	}

	public static EmbedBuilder WithAuthor(this EmbedBuilder builder, IUser user)
	{
		return builder.WithAuthor(user.Username + "#" + user.Discriminator, user.GetAvatarUrl(ImageFormat.Auto, 128) ?? user.GetDefaultAvatarUrl());
	}

	public static EmbedBuilder ToEmbedBuilder(this IEmbed embed)
	{
		if (embed.Type != EmbedType.Rich)
		{
			throw new InvalidOperationException("Only Rich embeds may be built.");
		}
		EmbedBuilder embedBuilder = new EmbedBuilder
		{
			Author = new EmbedAuthorBuilder
			{
				Name = embed.Author?.Name,
				IconUrl = embed.Author?.IconUrl,
				Url = embed.Author?.Url
			},
			Color = embed.Color,
			Description = embed.Description,
			Footer = new EmbedFooterBuilder
			{
				Text = embed.Footer?.Text,
				IconUrl = embed.Footer?.IconUrl
			},
			ImageUrl = embed.Image?.Url,
			ThumbnailUrl = embed.Thumbnail?.Url,
			Timestamp = embed.Timestamp,
			Title = embed.Title,
			Url = embed.Url
		};
		System.Collections.Immutable.ImmutableArray<EmbedField>.Enumerator enumerator = embed.Fields.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EmbedField current = enumerator.Current;
			embedBuilder.AddField(current.Name, current.Value, current.Inline);
		}
		return embedBuilder;
	}

	public static EmbedBuilder WithFields(this EmbedBuilder builder, IEnumerable<EmbedFieldBuilder> fields)
	{
		foreach (EmbedFieldBuilder field in fields)
		{
			builder.AddField(field);
		}
		return builder;
	}

	public static EmbedBuilder WithFields(this EmbedBuilder builder, params EmbedFieldBuilder[] fields)
	{
		return builder.WithFields(fields.AsEnumerable());
	}
}
