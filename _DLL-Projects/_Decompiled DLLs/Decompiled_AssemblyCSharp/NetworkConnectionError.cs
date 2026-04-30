public enum NetworkConnectionError
{
	InternalDirectConnectFailed = -5,
	EmptyConnectTarget = -4,
	IncorrectParameters = -3,
	CreateSocketOrThreadFailure = -2,
	AlreadyConnectedToAnotherServer = -1,
	NoError = 0,
	ConnectionFailed = 15,
	AlreadyConnectedToServer = 16,
	TooManyConnectedPlayers = 18,
	RSAPublicKeyMismatch = 21,
	ConnectionBanned = 22,
	InvalidPassword = 23,
	NATTargetNotConnected = 69,
	NATTargetConnectionLost = 71,
	NATPunchthroughFailed = 73,
	InvalidPort = 74,
	RestartRequired = 75
}
