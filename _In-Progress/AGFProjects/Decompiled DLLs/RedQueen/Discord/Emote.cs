using System;
using System.Diagnostics;
using System.Globalization;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Emote : IEmote, ISnowflakeEntity, IEntity<ulong>
{
	public string Name { get; }

	public ulong Id { get; }

	public bool Animated { get; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public string Url => CDN.GetEmojiUrl(Id, Animated);

	private string DebuggerDisplay => $"{Name} ({Id})";

	internal Emote(ulong id, string name, bool animated)
	{
		Id = id;
		Name = name;
		Animated = animated;
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!(other is Emote emote))
		{
			return false;
		}
		return Id == emote.Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public static Emote Parse(string text)
	{
		if (TryParse(text, out var result))
		{
			return result;
		}
		throw new ArgumentException("Invalid emote format.", "text");
	}

	public static bool TryParse(string text, out Emote result)
	{
		result = null;
		if (text == null)
		{
			return false;
		}
		if (text.Length >= 4 && text[0] == '<' && (text[1] == ':' || (text[1] == 'a' && text[2] == ':')) && text[text.Length - 1] == '>')
		{
			bool flag = text[1] == 'a';
			int num = (flag ? 3 : 2);
			int num2 = text.IndexOf(':', num);
			if (num2 == -1)
			{
				return false;
			}
			if (!ulong.TryParse(text.Substring(num2 + 1, text.Length - num2 - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var result2))
			{
				return false;
			}
			string name = text.Substring(num, num2 - num);
			result = new Emote(result2, name, flag);
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format("<{0}:{1}:{2}>", Animated ? "a" : "", Name, Id);
	}

	public static implicit operator Emote(string s)
	{
		return Parse(s);
	}
}
