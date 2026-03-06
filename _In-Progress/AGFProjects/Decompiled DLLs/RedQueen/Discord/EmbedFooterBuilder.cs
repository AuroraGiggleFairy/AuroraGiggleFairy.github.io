using System;

namespace Discord;

internal class EmbedFooterBuilder
{
	private string _text;

	public const int MaxFooterTextLength = 2048;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (value != null && value.Length > 2048)
			{
				throw new ArgumentException($"Footer text length must be less than or equal to {2048}.", "Text");
			}
			_text = value;
		}
	}

	public string IconUrl { get; set; }

	public EmbedFooterBuilder WithText(string text)
	{
		Text = text;
		return this;
	}

	public EmbedFooterBuilder WithIconUrl(string iconUrl)
	{
		IconUrl = iconUrl;
		return this;
	}

	public EmbedFooter Build()
	{
		return new EmbedFooter(Text, IconUrl, null);
	}

	public static bool operator ==(EmbedFooterBuilder left, EmbedFooterBuilder right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(EmbedFooterBuilder left, EmbedFooterBuilder right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedFooterBuilder embedFooterBuilder)
		{
			return Equals(embedFooterBuilder);
		}
		return false;
	}

	public bool Equals(EmbedFooterBuilder embedFooterBuilder)
	{
		if (_text == embedFooterBuilder?._text)
		{
			return IconUrl == embedFooterBuilder?.IconUrl;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
