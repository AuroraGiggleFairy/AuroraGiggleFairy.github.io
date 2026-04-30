using System;

namespace Discord;

[Flags]
internal enum GuildPermission : ulong
{
	CreateInstantInvite = 1uL,
	KickMembers = 2uL,
	BanMembers = 4uL,
	Administrator = 8uL,
	ManageChannels = 0x10uL,
	ManageGuild = 0x20uL,
	ViewGuildInsights = 0x80000uL,
	AddReactions = 0x40uL,
	ViewAuditLog = 0x80uL,
	ViewChannel = 0x400uL,
	SendMessages = 0x800uL,
	SendTTSMessages = 0x1000uL,
	ManageMessages = 0x2000uL,
	EmbedLinks = 0x4000uL,
	AttachFiles = 0x8000uL,
	ReadMessageHistory = 0x10000uL,
	MentionEveryone = 0x20000uL,
	UseExternalEmojis = 0x40000uL,
	Connect = 0x100000uL,
	Speak = 0x200000uL,
	MuteMembers = 0x400000uL,
	DeafenMembers = 0x800000uL,
	MoveMembers = 0x1000000uL,
	UseVAD = 0x2000000uL,
	PrioritySpeaker = 0x100uL,
	Stream = 0x200uL,
	ChangeNickname = 0x4000000uL,
	ManageNicknames = 0x8000000uL,
	ManageRoles = 0x10000000uL,
	ManageWebhooks = 0x20000000uL,
	ManageEmojisAndStickers = 0x40000000uL,
	UseApplicationCommands = 0x80000000uL,
	RequestToSpeak = 0x100000000uL,
	ManageEvents = 0x200000000uL,
	ManageThreads = 0x400000000uL,
	CreatePublicThreads = 0x800000000uL,
	CreatePrivateThreads = 0x1000000000uL,
	UseExternalStickers = 0x2000000000uL,
	SendMessagesInThreads = 0x4000000000uL,
	StartEmbeddedActivities = 0x8000000000uL,
	ModerateMembers = 0x10000000000uL
}
