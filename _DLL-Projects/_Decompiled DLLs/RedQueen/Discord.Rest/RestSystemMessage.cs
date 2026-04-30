using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestSystemMessage : RestMessage, ISystemMessage, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private string DebuggerDisplay => $"{base.Author}: {base.Content} ({base.Id}, {base.Type})";

	internal RestSystemMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, IUser author)
		: base(discord, id, channel, author, MessageSource.System)
	{
	}

	internal new static RestSystemMessage Create(BaseDiscordClient discord, IMessageChannel channel, IUser author, Message model)
	{
		RestSystemMessage restSystemMessage = new RestSystemMessage(discord, model.Id, channel, author);
		restSystemMessage.Update(model);
		return restSystemMessage;
	}

	internal override void Update(Message model)
	{
		base.Update(model);
	}
}
