namespace Discord;

internal enum ApplicationFlags
{
	GatewayPresence = 0x1000,
	GatewayPresenceLimited = 0x2000,
	GatewayGuildMembers = 0x4000,
	GatewayGuildMembersLimited = 0x8000,
	VerificationPendingGuildLimit = 0x10000,
	Embedded = 0x20000,
	GatewayMessageContent = 0x40000,
	GatewayMessageContentLimited = 0x80000
}
