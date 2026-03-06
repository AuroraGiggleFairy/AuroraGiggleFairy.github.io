using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestNewsChannel : RestTextChannel, INewsChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	public override int SlowModeInterval
	{
		get
		{
			throw new NotSupportedException("News channels do not support Slow Mode.");
		}
	}

	internal RestNewsChannel(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, guild, id)
	{
	}

	internal new static RestNewsChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestNewsChannel restNewsChannel = new RestNewsChannel(discord, guild, model.Id);
		restNewsChannel.Update(model);
		return restNewsChannel;
	}
}
