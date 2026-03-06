using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Embed : IEmbed
{
	public EmbedType Type { get; }

	public string Description { get; internal set; }

	public string Url { get; internal set; }

	public string Title { get; internal set; }

	public DateTimeOffset? Timestamp { get; internal set; }

	public Color? Color { get; internal set; }

	public EmbedImage? Image { get; internal set; }

	public EmbedVideo? Video { get; internal set; }

	public EmbedAuthor? Author { get; internal set; }

	public EmbedFooter? Footer { get; internal set; }

	public EmbedProvider? Provider { get; internal set; }

	public EmbedThumbnail? Thumbnail { get; internal set; }

	public System.Collections.Immutable.ImmutableArray<EmbedField> Fields { get; internal set; }

	public int Length
	{
		get
		{
			int num = Title?.Length ?? 0;
			int valueOrDefault = (Author?.Name?.Length).GetValueOrDefault();
			int num2 = Description?.Length ?? 0;
			int valueOrDefault2 = (Footer?.Text?.Length).GetValueOrDefault();
			int valueOrDefault3 = Fields.Sum((EmbedField f) => f.Name?.Length + f.Value?.ToString().Length).GetValueOrDefault();
			return num + valueOrDefault + num2 + valueOrDefault2 + valueOrDefault3;
		}
	}

	private string DebuggerDisplay => $"{Title} ({Type})";

	internal Embed(EmbedType type)
	{
		Type = type;
		Fields = System.Collections.Immutable.ImmutableArray.Create<EmbedField>();
	}

	internal Embed(EmbedType type, string title, string description, string url, DateTimeOffset? timestamp, Color? color, EmbedImage? image, EmbedVideo? video, EmbedAuthor? author, EmbedFooter? footer, EmbedProvider? provider, EmbedThumbnail? thumbnail, System.Collections.Immutable.ImmutableArray<EmbedField> fields)
	{
		Type = type;
		Title = title;
		Description = description;
		Url = url;
		Color = color;
		Timestamp = timestamp;
		Image = image;
		Video = video;
		Author = author;
		Footer = footer;
		Provider = provider;
		Thumbnail = thumbnail;
		Fields = fields;
	}

	public override string ToString()
	{
		return Title;
	}

	public static bool operator ==(Embed left, Embed right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(Embed left, Embed right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is Embed embed)
		{
			return Equals(embed);
		}
		return false;
	}

	public bool Equals(Embed embed)
	{
		return GetHashCode() == embed?.GetHashCode();
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 23 + (Type, Title, Description, Timestamp, Color, Image, Video, Author, Footer, Provider, Thumbnail).GetHashCode();
		System.Collections.Immutable.ImmutableArray<EmbedField>.Enumerator enumerator = Fields.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = num * 23 + enumerator.Current.GetHashCode();
		}
		return num;
	}
}
