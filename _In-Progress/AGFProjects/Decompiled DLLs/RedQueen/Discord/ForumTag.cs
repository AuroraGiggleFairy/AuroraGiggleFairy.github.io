using System.Runtime.CompilerServices;

namespace Discord;

internal struct ForumTag
{
	public ulong Id
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string Name
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public IEmote Emoji
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	internal ForumTag(ulong id, string name, ulong? emojiId, string emojiName)
	{
		if (emojiId.HasValue && emojiId.Value != 0L)
		{
			Emoji = new Emote(emojiId.Value, emojiName, animated: false);
		}
		else if (emojiName != null)
		{
			Emoji = new Emoji(name);
		}
		else
		{
			Emoji = null;
		}
		Id = id;
		Name = name;
	}
}
