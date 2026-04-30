using System;
using System.Collections.Generic;

namespace Discord.Interactions;

internal class DefaultChannelConverter<T> : DefaultEntityTypeConverter<T> where T : class, IChannel
{
	private readonly List<ChannelType> _channelTypes;

	public DefaultChannelConverter()
	{
		Type typeFromHandle = typeof(T);
		_channelTypes = (typeof(IStageChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.Stage } : (typeof(IVoiceChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.Voice } : (typeof(IDMChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.DM } : (typeof(IGroupChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.Group } : (typeof(ICategoryChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.Category } : (typeof(INewsChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType> { ChannelType.News } : (typeof(IThreadChannel).IsAssignableFrom(typeFromHandle) ? new List<ChannelType>
		{
			ChannelType.PublicThread,
			ChannelType.PrivateThread,
			ChannelType.NewsThread
		} : ((!typeof(ITextChannel).IsAssignableFrom(typeFromHandle)) ? null : new List<ChannelType> { ChannelType.Text }))))))));
	}

	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.Channel;
	}

	public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
	{
		if (_channelTypes != null)
		{
			properties.ChannelTypes = _channelTypes;
		}
	}
}
