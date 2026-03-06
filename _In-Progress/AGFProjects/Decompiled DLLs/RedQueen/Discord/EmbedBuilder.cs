using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Utils;
using Newtonsoft.Json;

namespace Discord;

internal class EmbedBuilder
{
	private string _title;

	private string _description;

	private EmbedImage? _image;

	private EmbedThumbnail? _thumbnail;

	private List<EmbedFieldBuilder> _fields;

	public const int MaxFieldCount = 25;

	public const int MaxTitleLength = 256;

	public const int MaxDescriptionLength = 4096;

	public const int MaxEmbedLength = 6000;

	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != null && value.Length > 256)
			{
				throw new ArgumentException($"Title length must be less than or equal to {256}.", "Title");
			}
			_title = value;
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != null && value.Length > 4096)
			{
				throw new ArgumentException($"Description length must be less than or equal to {4096}.", "Description");
			}
			_description = value;
		}
	}

	public string Url { get; set; }

	public string ThumbnailUrl
	{
		get
		{
			return _thumbnail?.Url;
		}
		set
		{
			_thumbnail = new EmbedThumbnail(value, null, null, null);
		}
	}

	public string ImageUrl
	{
		get
		{
			return _image?.Url;
		}
		set
		{
			_image = new EmbedImage(value, null, null, null);
		}
	}

	public List<EmbedFieldBuilder> Fields
	{
		get
		{
			return _fields;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Fields", "Cannot set an embed builder's fields collection to null.");
			}
			if (value.Count > 25)
			{
				throw new ArgumentException($"Field count must be less than or equal to {25}.", "Fields");
			}
			_fields = value;
		}
	}

	public DateTimeOffset? Timestamp { get; set; }

	public Color? Color { get; set; }

	public EmbedAuthorBuilder Author { get; set; }

	public EmbedFooterBuilder Footer { get; set; }

	public int Length
	{
		get
		{
			int num = Title?.Length ?? 0;
			int valueOrDefault = (Author?.Name?.Length).GetValueOrDefault();
			int num2 = Description?.Length ?? 0;
			int valueOrDefault2 = (Footer?.Text?.Length).GetValueOrDefault();
			int num3 = Fields.Sum((EmbedFieldBuilder f) => f.Name.Length + (f.Value?.ToString()?.Length).GetValueOrDefault());
			return num + valueOrDefault + num2 + valueOrDefault2 + num3;
		}
	}

	public EmbedBuilder()
	{
		Fields = new List<EmbedFieldBuilder>();
	}

	public static bool TryParse(string json, out EmbedBuilder builder)
	{
		builder = new EmbedBuilder();
		try
		{
			Embed embed = JsonConvert.DeserializeObject<Embed>(json);
			if ((object)embed != null)
			{
				builder = embed.ToEmbedBuilder();
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static EmbedBuilder Parse(string json)
	{
		try
		{
			Embed embed = JsonConvert.DeserializeObject<Embed>(json);
			if ((object)embed != null)
			{
				return embed.ToEmbedBuilder();
			}
			return new EmbedBuilder();
		}
		catch
		{
			throw;
		}
	}

	public EmbedBuilder WithTitle(string title)
	{
		Title = title;
		return this;
	}

	public EmbedBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public EmbedBuilder WithUrl(string url)
	{
		Url = url;
		return this;
	}

	public EmbedBuilder WithThumbnailUrl(string thumbnailUrl)
	{
		ThumbnailUrl = thumbnailUrl;
		return this;
	}

	public EmbedBuilder WithImageUrl(string imageUrl)
	{
		ImageUrl = imageUrl;
		return this;
	}

	public EmbedBuilder WithCurrentTimestamp()
	{
		Timestamp = DateTimeOffset.UtcNow;
		return this;
	}

	public EmbedBuilder WithTimestamp(DateTimeOffset dateTimeOffset)
	{
		Timestamp = dateTimeOffset;
		return this;
	}

	public EmbedBuilder WithColor(Color color)
	{
		Color = color;
		return this;
	}

	public EmbedBuilder WithAuthor(EmbedAuthorBuilder author)
	{
		Author = author;
		return this;
	}

	public EmbedBuilder WithAuthor(Action<EmbedAuthorBuilder> action)
	{
		EmbedAuthorBuilder embedAuthorBuilder = new EmbedAuthorBuilder();
		action(embedAuthorBuilder);
		Author = embedAuthorBuilder;
		return this;
	}

	public EmbedBuilder WithAuthor(string name, string iconUrl = null, string url = null)
	{
		EmbedAuthorBuilder author = new EmbedAuthorBuilder
		{
			Name = name,
			IconUrl = iconUrl,
			Url = url
		};
		Author = author;
		return this;
	}

	public EmbedBuilder WithFooter(EmbedFooterBuilder footer)
	{
		Footer = footer;
		return this;
	}

	public EmbedBuilder WithFooter(Action<EmbedFooterBuilder> action)
	{
		EmbedFooterBuilder embedFooterBuilder = new EmbedFooterBuilder();
		action(embedFooterBuilder);
		Footer = embedFooterBuilder;
		return this;
	}

	public EmbedBuilder WithFooter(string text, string iconUrl = null)
	{
		EmbedFooterBuilder footer = new EmbedFooterBuilder
		{
			Text = text,
			IconUrl = iconUrl
		};
		Footer = footer;
		return this;
	}

	public EmbedBuilder AddField(string name, object value, bool inline = false)
	{
		EmbedFieldBuilder field = new EmbedFieldBuilder().WithIsInline(inline).WithName(name).WithValue(value);
		AddField(field);
		return this;
	}

	public EmbedBuilder AddField(EmbedFieldBuilder field)
	{
		if (Fields.Count >= 25)
		{
			throw new ArgumentException($"Field count must be less than or equal to {25}.", "field");
		}
		Fields.Add(field);
		return this;
	}

	public EmbedBuilder AddField(Action<EmbedFieldBuilder> action)
	{
		EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
		action(embedFieldBuilder);
		AddField(embedFieldBuilder);
		return this;
	}

	public Embed Build()
	{
		if (Length > 6000)
		{
			throw new InvalidOperationException($"Total embed length must be less than or equal to {6000}.");
		}
		if (!string.IsNullOrEmpty(Url))
		{
			UrlValidation.Validate(Url, allowAttachments: true);
		}
		if (!string.IsNullOrEmpty(ThumbnailUrl))
		{
			UrlValidation.Validate(ThumbnailUrl, allowAttachments: true);
		}
		if (!string.IsNullOrEmpty(ImageUrl))
		{
			UrlValidation.Validate(ImageUrl, allowAttachments: true);
		}
		if (Author != null)
		{
			if (!string.IsNullOrEmpty(Author.Url))
			{
				UrlValidation.Validate(Author.Url, allowAttachments: true);
			}
			if (!string.IsNullOrEmpty(Author.IconUrl))
			{
				UrlValidation.Validate(Author.IconUrl, allowAttachments: true);
			}
		}
		if (Footer != null && !string.IsNullOrEmpty(Footer.IconUrl))
		{
			UrlValidation.Validate(Footer.IconUrl, allowAttachments: true);
		}
		System.Collections.Immutable.ImmutableArray<EmbedField>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<EmbedField>(Fields.Count);
		for (int i = 0; i < Fields.Count; i++)
		{
			builder.Add(Fields[i].Build());
		}
		return new Embed(EmbedType.Rich, Title, Description, Url, Timestamp, Color, _image, null, Author?.Build(), Footer?.Build(), null, _thumbnail, builder.ToImmutable());
	}

	public static bool operator ==(EmbedBuilder left, EmbedBuilder right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(EmbedBuilder left, EmbedBuilder right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedBuilder embedBuilder)
		{
			return Equals(embedBuilder);
		}
		return false;
	}

	public bool Equals(EmbedBuilder embedBuilder)
	{
		if ((object)embedBuilder == null)
		{
			return false;
		}
		if (Fields.Count != embedBuilder.Fields.Count)
		{
			return false;
		}
		for (int i = 0; i < _fields.Count; i++)
		{
			if (_fields[i] != embedBuilder._fields[i])
			{
				return false;
			}
		}
		if (_title == embedBuilder?._title && _description == embedBuilder?._description && _image == embedBuilder?._image && _thumbnail == embedBuilder?._thumbnail && Timestamp == embedBuilder?.Timestamp)
		{
			Color? color = Color;
			Color? color2 = embedBuilder?.Color;
			if (color.HasValue == color2.HasValue && (!color.HasValue || color.GetValueOrDefault() == color2.GetValueOrDefault()) && Author == embedBuilder?.Author && Footer == embedBuilder?.Footer)
			{
				return Url == embedBuilder?.Url;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
