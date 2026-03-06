namespace Discord.API.Gateway;

internal enum GatewayOpCode : byte
{
	Dispatch,
	Heartbeat,
	Identify,
	PresenceUpdate,
	VoiceStateUpdate,
	VoiceServerPing,
	Resume,
	Reconnect,
	RequestGuildMembers,
	InvalidSession,
	Hello,
	HeartbeatAck,
	GuildSync
}
