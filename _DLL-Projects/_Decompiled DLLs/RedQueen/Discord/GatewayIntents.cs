using System;

namespace Discord;

[Flags]
internal enum GatewayIntents
{
	None = 0,
	Guilds = 1,
	GuildMembers = 2,
	GuildBans = 4,
	GuildEmojis = 8,
	GuildIntegrations = 0x10,
	GuildWebhooks = 0x20,
	GuildInvites = 0x40,
	GuildVoiceStates = 0x80,
	GuildPresences = 0x100,
	GuildMessages = 0x200,
	GuildMessageReactions = 0x400,
	GuildMessageTyping = 0x800,
	DirectMessages = 0x1000,
	DirectMessageReactions = 0x2000,
	DirectMessageTyping = 0x4000,
	MessageContent = 0x8000,
	GuildScheduledEvents = 0x10000,
	AllUnprivileged = 0x17EFD,
	All = 0x1FFFF
}
