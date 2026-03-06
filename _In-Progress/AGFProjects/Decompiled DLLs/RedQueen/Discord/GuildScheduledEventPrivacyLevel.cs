using System;

namespace Discord;

internal enum GuildScheduledEventPrivacyLevel
{
	[Obsolete("This event type isn't supported yet! check back later.", true)]
	Public = 1,
	Private
}
