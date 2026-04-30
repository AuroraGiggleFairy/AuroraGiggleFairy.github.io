using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal sealed class ChannelTypesAttribute : Attribute
{
	public IReadOnlyCollection<ChannelType> ChannelTypes { get; }

	public ChannelTypesAttribute(params ChannelType[] channelTypes)
	{
		if (channelTypes == null)
		{
			throw new ArgumentNullException("channelTypes");
		}
		ChannelTypes = channelTypes.ToImmutableArray();
	}
}
