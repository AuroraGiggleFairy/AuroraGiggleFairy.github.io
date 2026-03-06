namespace Discord.WebSocket;

internal enum VoiceCloseCode
{
	UnknownOpcode = 4001,
	DecodeFailure = 4002,
	NotAuthenticated = 4003,
	AuthenticationFailed = 4004,
	AlreadyAuthenticated = 4005,
	SessionNolongerValid = 4006,
	SessionTimeout = 4009,
	ServerNotFound = 4011,
	UnknownProtocol = 4012,
	Disconnected = 4014,
	VoiceServerCrashed = 4015,
	UnknownEncryptionMode = 4016
}
