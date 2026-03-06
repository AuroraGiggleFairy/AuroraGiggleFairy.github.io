using System;

namespace Discord;

internal class EmbedAuthorBuilder
{
	private string _name;

	public const int MaxAuthorNameLength = 256;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != null && value.Length > 256)
			{
				throw new ArgumentException($"Author name length must be less than or equal to {256}.", "Name");
			}
			_name = value;
		}
	}

	public string Url { get; set; }

	public string IconUrl { get; set; }

	public EmbedAuthorBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public EmbedAuthorBuilder WithUrl(string url)
	{
		Url = url;
		return this;
	}

	public EmbedAuthorBuilder WithIconUrl(string iconUrl)
	{
		IconUrl = iconUrl;
		return this;
	}

	public EmbedAuthor Build()
	{
		return new EmbedAuthor(Name, Url, IconUrl, null);
	}

	public static bool operator ==(EmbedAuthorBuilder left, EmbedAuthorBuilder right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(EmbedAuthorBuilder left, EmbedAuthorBuilder right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedAuthorBuilder embedAuthorBuilder)
		{
			return Equals(embedAuthorBuilder);
		}
		return false;
	}

	public bool Equals(EmbedAuthorBuilder embedAuthorBuilder)
	{
		if (_name == embedAuthorBuilder?._name && Url == embedAuthorBuilder?.Url)
		{
			return IconUrl == embedAuthorBuilder?.IconUrl;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
