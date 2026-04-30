using System;
using System.Collections.Immutable;

namespace Discord;

internal interface IEmbed
{
	string Url { get; }

	string Title { get; }

	string Description { get; }

	EmbedType Type { get; }

	DateTimeOffset? Timestamp { get; }

	Color? Color { get; }

	EmbedImage? Image { get; }

	EmbedVideo? Video { get; }

	EmbedAuthor? Author { get; }

	EmbedFooter? Footer { get; }

	EmbedProvider? Provider { get; }

	EmbedThumbnail? Thumbnail { get; }

	System.Collections.Immutable.ImmutableArray<EmbedField> Fields { get; }
}
